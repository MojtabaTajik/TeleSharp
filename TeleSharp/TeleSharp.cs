using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using TeleSharp.Entities;
using TeleSharp.Entities.Inline;
using TeleSharp.Entities.SendEntities;
using TeleSharp.Properties;
using File = TeleSharp.Entities.File;

namespace TeleSharp
{
    public class TeleSharp
    {
        private readonly int Timeout = 5;
        private readonly string _authToken;
        private readonly RestClient _botClient;
        private User _me;
        private int _lastFetchedMessageId;

        #region Events
        public delegate void MessageDelegate(Message message);
        public event MessageDelegate OnMessage;

        public delegate void InlineDelegate(InlineQuery inlineQuery);
        public event InlineDelegate OnInlineQuery;

        public delegate void ChooseInlineResultDelegate(ChosenInlineResult inlineResult);
        public event ChooseInlineResultDelegate OnChooseInlineResult;
        public delegate void CallbackQueryDelegate(CallbackQuery CallbackQuery);
        public event CallbackQueryDelegate OnCallbackQuery;
        #endregion

        /// <summary>
        /// Sets up a new bot with the given authtoken.
        /// </summary>
        /// <param name="authenticationToken">The authorization token for your bot</param>
        public TeleSharp(string authenticationToken)
        {
            if (string.IsNullOrWhiteSpace(authenticationToken))
                throw new ArgumentNullException(nameof(authenticationToken));

            _authToken = authenticationToken;
            _botClient = new RestClient(Resources.TelegramAPIUrl + _authToken);
            _botClient.AddDefaultHeader(Resources.HttpContentType, Resources.TeleSharpHttpClientContentType);

            SimpleJson.CurrentJsonSerializerStrategy = new SnakeCaseSerializationStrategy();

            // Receive messages
            new Task(HandleMessages).Start();
        }

        /// <summary>
        /// Gets the current bot basic information
        /// </summary>
        /// <returns>Basic information about the bot</returns>
        public User Me
        {
            get
            {
                if (_me != null) return _me;

                return _me = _botClient.Execute<User>(Utils.GenerateRestRequest(Resources.Method_GetMe, Method.POST)).Data;
            }
        }

        /// <summary>
        /// Get messages sent to the bot by all 
        /// </summary>
        /// <returns>A list of all the messages since the last request.</returns>
        public async Task<List<Update>> PollMessages()
        {
            try
            {
                var request = Utils.GenerateRestRequest(Resources.Method_GetUpdates, Method.POST, null,
                    new Dictionary<string, object>
                    {
                        {Resources.Param_Timeout, Timeout},
                        {Resources.Param_Offset, _lastFetchedMessageId + 1},
                    });

                IRestResponse<List<Update>> response = null;

                while (response?.Data == null)
                    response = await _botClient.ExecuteTaskAsync<List<Update>>(request);

                if (!response.Data.Any()) return new List<Update>();

                _lastFetchedMessageId = response.Data.Last().UpdateId;
                //var rawData = response.Data.Select(d => d.Message);

                return response.Data;
                //return rawData.Select(d => (d.Chat.Title == null ? d.AsUserMessage() : d)).ToList();
            }
            catch (Exception ex)
            {
                return new List<Update>();
            }
        }

        /// <summary>
        /// Pass messages to receive message event
        /// </summary>
        private async void HandleMessages()
        {
            while (true)
            {
                var updates = await PollMessages();

                foreach (Update update in updates)
                {
                    if (update.Message != null)
                    {
                        OnMessage?.Invoke(update.Message);
                        continue;
                    }

                    if (update.InlineQuery != null)
                    {
                        OnInlineQuery?.Invoke(update.InlineQuery);
                        continue;
                    }

                    if (update.ChosenInlineResult != null)
                        OnChooseInlineResult?.Invoke(update.ChosenInlineResult);
                    if (update.CallbackQuery != null)
                        OnCallbackQuery?.Invoke(update.CallbackQuery);
                }
            }
        }

        /// <summary>
        /// Sends a message to the given sender (user or group chat)
        /// </summary>
        /// <param name="messageParams"/>Information of message to send/param>
        /// <returns>Message that was sent</returns>
        public Message SendMessage(SendMessageParams messageParams)
        {
            if (messageParams == null)
                throw new ArgumentNullException(nameof(messageParams));

            var request = Utils.GenerateRestRequest(Resources.Method_SendMessage, Method.POST, null,
                new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, messageParams.ChatId},
                    {Resources.Param_Text, messageParams.Text},
                    {Resources.Param_ParseMode, messageParams.ParseMode},
                    {Resources.Param_DisableWebPagePreview, messageParams.DisableWebPagePreview},
                });

            if (messageParams.ReplyToMessage != null)
                request.AddParameter(Resources.Param_ReplyToMmessageId, messageParams.ReplyToMessage.MessageId);

            if (messageParams.CustomKeyboard != null)
                request.AddParameter(Resources.Param_ReplyMarkup,
                    new RestRequest().JsonSerializer.Serialize(messageParams.CustomKeyboard));
            if (messageParams.InlineKeyboard != null)
                request.AddParameter(Resources.Param_ReplyMarkup,
                    new RestRequest().JsonSerializer.Serialize(messageParams.InlineKeyboard));
            if (messageParams.ReplyKeyboardRemove != null)
                request.AddParameter(Resources.Param_ReplyMarkup,
                   new RestRequest().JsonSerializer.Serialize(messageParams.ReplyKeyboardRemove));

            var result = _botClient.Execute<Message>(request);
            return result.Data;
        }
        public Message AnswerCallbackQuery(CallbackQuery sender, string text = null, bool? show_alert = null, string url = null, int? cache_time = null)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));
            var request = Utils.GenerateRestRequest(Resources.Method_AnswerCallbackQuery, Method.POST,
               new Dictionary<string, string>
               {
                    {Resources.HttpContentType, Resources.HttpMultiPartFormData}
               }
               , new Dictionary<string, object>
               {
                    {Resources.Param_CallbackQueryId, sender.Id},
               });
            if (!string.IsNullOrEmpty(text))
                request.Parameters.Add(new Parameter { Name = Resources.Param_Text, Value = text, Type = ParameterType.GetOrPost });
            if (show_alert != null)
                request.Parameters.Add(new Parameter { Name = Resources.Param_show_alert, Value = show_alert, Type = ParameterType.GetOrPost });
            if (!string.IsNullOrEmpty(url))
                request.Parameters.Add(new Parameter { Name = Resources.Param_url, Value = url, Type = ParameterType.GetOrPost });
            if (cache_time != null)
                request.Parameters.Add(new Parameter { Name = Resources.Param_CacheTime, Value = cache_time, Type = ParameterType.GetOrPost });
            return _botClient.Execute<Message>(request).Data;
        }
        public Message EditMessageText(SendMessageParams messageParams)
        {
            if (messageParams == null)
                throw new ArgumentNullException(nameof(messageParams));

            var request = Utils.GenerateRestRequest(Resources.Method_EditMessageText, Method.POST, null,
                new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, messageParams.ChatId},
                    {Resources.Param_MessageId,messageParams.MessageId },
                    {Resources.Param_InlineMessageId,messageParams.InlineMessageId },
                    {Resources.Param_Text, messageParams.Text},
                    {Resources.Param_ParseMode, messageParams.ParseMode},
                    {Resources.Param_DisableWebPagePreview, messageParams.DisableWebPagePreview},
                });
            if (messageParams.InlineKeyboard != null)
                request.AddParameter(Resources.Param_ReplyMarkup,
                    new RestRequest().JsonSerializer.Serialize(messageParams.InlineKeyboard));
            var result = _botClient.Execute<Message>(request);
            return result.Data;
        }
        /// <summary>
        /// Indicates that the bot is doing a specified action
        /// </summary>
        /// <param name="sender">The sender to indicate towards</param>
        /// <param name="action">The action the bot is doing (from the ChatAction class)</param>
        public void SetCurrentAction(MessageSender sender, ChatAction action)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            string actionName = string.Empty;
            switch (action)
            {
                case ChatAction.Typing:
                    actionName = Resources.Action_typing;
                    break;
                case ChatAction.FindLocation:
                    actionName = Resources.Action_FindLocation;
                    break;
                case ChatAction.RecordVideo:
                    actionName = Resources.Action_RecordVideo;
                    break;
                case ChatAction.RecordAudio:
                    actionName = Resources.Action_RecordAudio;
                    break;
                case ChatAction.UploadPhoto:
                    actionName = Resources.Action_UploadPhoto;
                    break;
                case ChatAction.UploadVideo:
                    actionName = Resources.Action_UploadVideo;
                    break;
                case ChatAction.UploadAudio:
                    actionName = Resources.Action_UploadAudio;
                    break;
                case ChatAction.UploadDocument:
                    actionName = Resources.Action_UploadDocument;
                    break;
            }

            if (string.IsNullOrEmpty(actionName))
                return;

            var request = Utils.GenerateRestRequest(Resources.Method_SendChatAction, Method.POST, null,
                new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, sender.Id},
                    {Resources.Param_Action, actionName},
                });

            _botClient.Execute(request);
        }

        /// <summary>
        /// Forward a message from one chat to another.
        /// </summary>
        /// <param name="message">Message to forwar.</param>
        /// <param name="sender">User/group to send to</param>
        /// <returns>Message that was forwarded</returns>
        public Message ForwardMessage(Message message, MessageSender sender)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            var request = Utils.GenerateRestRequest(Resources.Method_ForwardMessage, Method.POST,
                new Dictionary<string, string>
                {
                    {Resources.HttpContentType, Resources.HttpMultiPartFormData}
                }
                , new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, sender.Id},
                    {Resources.Param_FromChatId, message.Chat?.Id ?? message.From.Id},
                    {Resources.Param_MessageId, message.MessageId}
                });

            return _botClient.Execute<Message>(request).Data;
        }

        /// <summary>
        /// Sends a photo to the given sender (user or group chat)
        /// </summary>
        /// <param name="sender">User or GroupChat</param>
        /// <param name="imageBuffer">Stream containing the data of the picture</param>
        /// <param name="filename">Filename to send to Telegram</param>
        /// <param name="caption">Caption for the image</param>
        /// <returns>Sent message</returns>
        public Message SendPhoto(MessageSender sender, byte[] imageBuffer, string filename = null, string caption = null)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            if (imageBuffer == null)
                throw new ArgumentNullException(nameof(imageBuffer));

            var request = Utils.GenerateRestRequest(Resources.Method_SendPhoto, Method.POST,
                new Dictionary<string, string>
                {
                    {Resources.HttpContentType, Resources.HttpMultiPartFormData}
                }
                , new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, sender.Id},
                    {Resources.Param_Caption, caption}
                },
                new List<Tuple<string, byte[], string>>
                {
                    new Tuple<string, byte[], string>(Resources.PhotoFile, imageBuffer,
                        filename ?? Utils.ComputeFileMd5Hash(imageBuffer) + ".jpg")
                });

            return _botClient.Execute<Message>(request).Data;
        }

        /// <summary>
        /// Sends a video to the given sender (user or group chat)
        /// </summary>
        /// <param name="sender">User or GroupChat</param>
        /// <param name="videoBuffer">Stream containing the data of the video</param>
        /// <param name="filename">Filename to send to Telegram</param>
        /// <param name="caption">Caption for the video</param>
        /// <returns>Sent message</returns>
        public Message SendVideo(MessageSender sender, byte[] videoBuffer, string filename = null, string caption = null)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            if (videoBuffer == null)
                throw new ArgumentNullException(nameof(videoBuffer));

            var request = Utils.GenerateRestRequest(Resources.Method_SendVideo, Method.POST,
                new Dictionary<string, string>
                {
                    {Resources.HttpContentType, Resources.HttpMultiPartFormData}
                }
                , new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, sender.Id},
                    {Resources.Param_Caption, caption}
                },
                new List<Tuple<string, byte[], string>>
                {
                    new Tuple<string, byte[], string>(Resources.VideoFile, videoBuffer,
                        filename ?? Utils.ComputeFileMd5Hash(videoBuffer) + ".mp4")
                });

            return _botClient.Execute<Message>(request).Data;
        }

        /// <summary>
        /// Sends a audio to the given sender (user or group chat)
        /// </summary>
        /// <param name="sender">User or GroupChat</param>
        /// <param name="audioBuffer">Stream containing the data of the audio</param>
        /// <param name="filename">Filename to send to Telegram</param>
        /// <returns>Sent message</returns>
        public Message SendAudio(MessageSender sender, byte[] audioBuffer, string filename = null)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            if (audioBuffer == null)
                throw new ArgumentNullException(nameof(audioBuffer));

            var request = Utils.GenerateRestRequest(Resources.Method_SendAudio, Method.POST,
                new Dictionary<string, string>
                {
                    {Resources.HttpContentType, Resources.HttpMultiPartFormData}
                }
                , new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, sender.Id}
                },
                new List<Tuple<string, byte[], string>>
                {
                    new Tuple<string, byte[], string>(Resources.AudioFile, audioBuffer,
                        filename ?? Utils.ComputeFileMd5Hash(audioBuffer) + ".mp3")
                });

            return _botClient.Execute<Message>(request).Data;
        }

        public Message SendDocument(MessageSender sender, byte[] fileBuffer, string filename = null)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            if (fileBuffer == null)
                throw new ArgumentNullException(nameof(fileBuffer));

            var request = Utils.GenerateRestRequest(Resources.Method_SendDocument, Method.POST,
                new Dictionary<string, string>
                {
                    {Resources.HttpContentType, Resources.HttpMultiPartFormData}
                }
                , new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, sender.Id}
                },
                new List<Tuple<string, byte[], string>>
                {
                    new Tuple<string, byte[], string>(Resources.DocumentFile, fileBuffer,
                        filename ?? Utils.ComputeFileMd5Hash(fileBuffer))
                });

            return _botClient.Execute<Message>(request).Data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="stickerBuffer"></param>
        /// <returns>Sent message</returns>
        public Message SendSticker(MessageSender sender, byte[] stickerBuffer)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            if (stickerBuffer == null)
                throw new ArgumentNullException(nameof(stickerBuffer));

            var request = Utils.GenerateRestRequest(Resources.Method_SendSticker, Method.POST,
                new Dictionary<string, string>
                {
                    {Resources.HttpContentType, Resources.HttpMultiPartFormData}
                }
                , new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, sender.Id}
                },
                new List<Tuple<string, byte[], string>>
                {
                    new Tuple<string, byte[], string>(Resources.StickerFile, stickerBuffer, Guid.NewGuid().ToString())
                });

            return _botClient.Execute<Message>(request).Data;
        }

        /// <summary>
        /// Send specified location to user that requested the action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="latitude">latitude positon of location</param>
        /// <param name="longitude">longitude position of location</param>
        /// <returns>Sent message</returns>
        public Message SendLocation(MessageSender sender, string latitude, string longitude)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            var request = Utils.GenerateRestRequest(Resources.Method_SendLocation, Method.POST,
                new Dictionary<string, string>
                {
                    {Resources.HttpContentType, Resources.HttpMultiPartFormData}
                }
                , new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, sender.Id},
                    {Resources.Param_Latitude, latitude},
                    {Resources.Param_Longitude, longitude}
                });

            return _botClient.Execute<Message>(request).Data;
        }

        public bool AnswerInlineQuery(AnswerInlineQuery answer)
        {
            if (answer == null)
                throw new ArgumentNullException(nameof(answer));

            var request = Utils.GenerateRestRequest(Resources.Method_AnswerInlineQuery, Method.POST,
                new Dictionary<string, string>
                {
                    {Resources.HttpContentType, Resources.HttpMultiPartFormData}
                }
                , new Dictionary<string, object>
                {
                    {Resources.Param_InlineQueryid, answer.InlineQueryId},
                    {Resources.Param_Results, new RestRequest().JsonSerializer.Serialize(answer.Results)},
                    {Resources.Param_CacheTime, answer.CacheTime},
                    {Resources.Param_IsPersonal, answer.IsPersonal},
                    {Resources.Param_NextOffset, answer.NextOffset}
                });

            return _botClient.Execute<bool>(request).Data;
        }

        /// <summary>
        /// Get profile picture's information of specified user
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>Profile picture's information of specified user</returns>
        public PhotoSizeArray GetUserProfilePhotos(int userId)
        {
            if (userId <= 0)
                throw new ArgumentNullException(nameof(userId));

            var request = Utils.GenerateRestRequest(Resources.Method_GetUserProfilePhotos, Method.POST, null
                , new Dictionary<string, object>
                {
                    {Resources.Param_UserId, userId},
                });

            return _botClient.Execute<PhotoSizeArray>(request).Data;
        }

        /// <summary>
        /// Get file information using ID of it
        /// </summary>
        /// <param name="fileId">ID of file to fetch information of it</param>
        /// <returns>File information of given ID</returns>
        public File GetFileInfoByFileId(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                throw new ArgumentNullException(nameof(fileId));

            try
            {
                var request = Utils.GenerateRestRequest(Resources.Method_GetFile, Method.POST, null,
                    new Dictionary<string, object>
                    {
                        {Resources.Param_FileId, fileId},
                    });

                return _botClient.Execute<File>(request).Data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Download given file id from Telegram server to memory
        /// </summary>
        /// <param name="fileId">Identifier of file to download</param>
        /// <returns>Downloaded buffer</returns>
        public FileDownloadResult DownloadFileById(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
                throw new ArgumentNullException(nameof(fileId));

            try
            {
                var fileInfo = GetFileInfoByFileId(fileId);

                if (string.IsNullOrEmpty(fileInfo.FilePath))
                    return null;

                using (var wc = new WebClient())
                {
                    string downloadUrl = string.Format(Resources.TelegramDownloadDocumentUrl, _authToken,
                        fileInfo.FilePath);
                    using (var fileStream = new MemoryStream(wc.DownloadData(downloadUrl)))
                        return new FileDownloadResult
                        {
                            FileId = fileInfo.FileId,
                            FilePath = fileInfo.FilePath,
                            FileExtension = Path.GetExtension(fileInfo.FilePath),
                            FileSize = fileInfo.FileSize,
                            Buffer = fileStream.ToArray()
                        };
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Download given file id from Telegram server and save it in physical path
        /// </summary>
        /// <param name="fileId">File id that must download</param>
        /// <param name="savePath">Path that file must save on it</param>
        /// <returns>Determine operation compelete successfully or not</returns>
        public FileDownloadResult DownloadFileById(string fileId, string savePath)
        {
            if (string.IsNullOrEmpty(fileId))
                throw new ArgumentNullException(nameof(fileId));

            if (string.IsNullOrEmpty(savePath))
                throw new ArgumentNullException(nameof(savePath));

            try
            {
                FileDownloadResult fileDownload = DownloadFileById(fileId);

                if (fileDownload == null)
                    return null;

                string filePath = Path.Combine(savePath, fileId) + fileDownload.FileExtension;
                System.IO.File.WriteAllBytes(filePath, fileDownload.Buffer);

                return fileDownload;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}