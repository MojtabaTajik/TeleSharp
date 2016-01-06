using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using TeleSharp.Entities;
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

        public delegate void MessageDelegate(Message message);
        public event MessageDelegate OnMessageReceived;

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
        public async Task<List<Message>> PollMessages()
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

            if (!response.Data.Any()) return new List<Message>();

            _lastFetchedMessageId = response.Data.Last().UpdateId;
            var rawData = response.Data.Select(d => d.Message);

            return rawData.Select(d => (d.Chat.Title == null ? d.AsUserMessage() : d)).ToList();
        }

        /// <summary>
        /// Pass messages to receive message event
        /// </summary>
        private async void HandleMessages()
        {
            while (true)
            {
                var messages = await PollMessages();

                foreach (Message message in messages)
                    OnMessageReceived?.Invoke(message);
            }
        }

        /// <summary>
        /// Sends a message to the given sender (user or group chat)
        /// </summary>
        /// <param name="sender">User or GroupChat</param>
        /// <param name="messageText">Body of the message</param>
        /// <param name="disableLinkPreview">disable link previews or not</param>
        /// <param name="replyTarget">Message to reply to</param>
        /// <returns>Message that was sen</returns>
        public Message SendMessage(MessageSender sender, string messageText, bool disableLinkPreview = false,
            Message replyTarget = null)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            var request = Utils.GenerateRestRequest(Resources.Method_SendMessage, Method.POST, null,
                new Dictionary<string, object>
                {
                    {Resources.Param_ChatId, sender.Id},
                    {Resources.Param_Text, messageText},
                    {Resources.Param_DisableWebPagePreview, disableLinkPreview}
                });

            if (replyTarget != null)
                request.AddParameter(Resources.Param_ReplyToMmessageId, replyTarget.MessageId);

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
        /// <param name="stickerName">Name of sticker</param>
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