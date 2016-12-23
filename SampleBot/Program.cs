using System;
using System.Collections.Generic;
using System.IO;
using TeleSharp.Entities;
using TeleSharp.Entities.Inline;
using TeleSharp.Entities.SendEntities;

namespace SampleBot
{
    internal class Program
    {
        public static TeleSharp.TeleSharp Bot;

        private static void Main(string[] args)
        {
            Bot = new TeleSharp.TeleSharp("152529427:AAFOizfzWycWHnJoQghmRAbN5IlBInd-wSe8");
            Bot.SendMessage(new SendMessageParams
            {
                ChatId = "39699831",
                Text = "Test msg !",
                InlineKeyboard = new InlineKeyboardMarkup
                {
                    InlineKeyboard = new List<List<InlineKeyboardButton>>
                    {
                        new List<InlineKeyboardButton>
                        {
                             new InlineKeyboardButton {Text="CallbackData",CallbackData="Ok",SwitchInlineQuery=string.Empty,SwitchInlineQueryCurrentChat=string.Empty,Url=string.Empty },
                        },
                        new List<InlineKeyboardButton>
                        {
                             new InlineKeyboardButton {Text="SwitchInlineQueryCurrentChat",CallbackData=string.Empty,SwitchInlineQuery=string.Empty,SwitchInlineQueryCurrentChat="OK",Url=string.Empty },
                        },
                         new List<InlineKeyboardButton>
                        {
                             new InlineKeyboardButton {Text="Url",CallbackData=string.Empty,SwitchInlineQuery=string.Empty,SwitchInlineQueryCurrentChat=string.Empty,Url="http://dualp.ir" },
                        },
                        new List<InlineKeyboardButton>
                        {
                              new InlineKeyboardButton {Text="SwitchInlineQuery",SwitchInlineQuery="سلام",Url=string.Empty,CallbackData=string.Empty,SwitchInlineQueryCurrentChat=string.Empty }
                        }
                    }
                }
            });
            Bot.OnMessage += OnMessage;
            Bot.OnInlineQuery += OnInlineQuery;
            Bot.OnCallbackQuery += Bot_OnCallbackQuery;



            Console.WriteLine(@"TeleSharp initialized");

            Console.WriteLine($"Hi, My Name is : {Bot.Me.Username}");

            Console.ReadLine();
        }

        private static void Bot_OnCallbackQuery(CallbackQuery CallbackQuery)
        {
            Bot.AnswerCallbackQuery(CallbackQuery, "Hello");
            Bot.EditMessageText(new SendMessageParams
            {
                ChatId = CallbackQuery.Message.Chat.Id.ToString(),
                MessageId = CallbackQuery.Message.MessageId.ToString(),
                Text=CallbackQuery.Message.Text,
                InlineKeyboard = new InlineKeyboardMarkup
                {
                    InlineKeyboard = new List<List<InlineKeyboardButton>>
                     {
                         new List<InlineKeyboardButton>
                         {
                             new InlineKeyboardButton {Text="Test",CallbackData="OK",SwitchInlineQuery=string.Empty,SwitchInlineQueryCurrentChat=string.Empty,Url=string.Empty }
                         }
                     }
                }
            });
        }

        private static void OnInlineQuery(InlineQuery inlinequery)
        {
            Bot.AnswerInlineQuery(new AnswerInlineQuery
            {
                InlineQueryId = inlinequery.Id,
                Results = new List<InlineQueryResult>
                {
                    new InlineQueryResultArticle
                    {
                        Id = inlinequery.Query,
                        Title = DateTime.Now.ToLongDateString(),
                        MessageText = Guid.NewGuid().ToString(),
                        ParseMode = "",
                        Url = "",
                        DisableWebPagePreview = false,
                        Description = "",
                        HideUrl = false,
                        ThumbHeight = 0,
                        ThumbWidth = 0,
                        ThumbUrl = ""
                    }
                },
                IsPersonal = false,
                CacheTime = 300,
                NextOffset = "0"
            });
        }

        /// <summary>
        /// Read received messages of bot in infinity loop
        /// </summary>
        private static void OnMessage(Message message)
        {
            // Get mesage sender information
            MessageSender sender = (MessageSender)message.Chat ?? message.From;
            
            Console.WriteLine(message.Text ?? "");
            // If user joined to bot say welcome
            if ((!string.IsNullOrEmpty(message.Text)) && (message.Text == "/start"))
            {
                string welcomeMessage =
                    $"Welcome {message.From.Username} !{Environment.NewLine}My name is {Bot.Me.Username}{Environment.NewLine}I made using TeleBot : http://www.github.com/Fel0ny/TeleSharp";

                Bot.SendMessage(new SendMessageParams
                {
                    ChatId = sender.Id.ToString(),
                    Text = welcomeMessage
                });
                return;
            }

            string baseStoragePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            // If any file exists in message download it
            DownloadFileFromMessage(message, baseStoragePath);
            // If Send Location or Contact
            GetLocationContactFromMessage(message, sender);

            if (string.IsNullOrEmpty(message.Text) || string.IsNullOrEmpty(baseStoragePath))
                return;

            try
            {
                string sampleData = Path.Combine(baseStoragePath, "SampleData");

                if (!string.IsNullOrEmpty(message.Text))
                    switch (message.Text.ToLower())
                    {
                        case "time":
                            {
                                Bot.SendMessage(new SendMessageParams
                                {
                                    ChatId = sender.Id.ToString(),
                                    Text = DateTime.Now.ToLongDateString()
                                });
                                break;
                            }

                        case "location":
                            {
                                Bot.SendLocation(sender, "50.69421", "3.17456");
                                break;
                            }

                        case "sticker":
                            {
                                Bot.SendSticker(sender, System.IO.File.ReadAllBytes(Path.Combine(sampleData, "sticker.png")));
                                break;
                            }

                        case "photo":
                            {
                                string photoFilePath = Path.Combine(sampleData, "sticker.png");

                                Bot.SetCurrentAction(sender, ChatAction.UploadPhoto);
                                Bot.SendPhoto(sender, System.IO.File.ReadAllBytes(photoFilePath),
                                    Path.GetFileName(photoFilePath), "This is sample photo");
                                break;
                            }

                        case "video":
                            {
                                string videoFilePath = Path.Combine(sampleData, "video.mp4");

                                Bot.SetCurrentAction(sender, ChatAction.UploadVideo);
                                Bot.SendVideo(sender, System.IO.File.ReadAllBytes(videoFilePath),
                                    Path.GetFileName(videoFilePath), "This is sample video");
                                break;
                            }

                        case "audio":
                            {
                                string audioFilePath = Path.Combine(sampleData, "audio.mp3");

                                Bot.SetCurrentAction(sender, ChatAction.UploadAudio);
                                Bot.SendAudio(sender, System.IO.File.ReadAllBytes(audioFilePath),
                                    Path.GetFileName(audioFilePath));
                                break;
                            }

                        case "document":
                            {
                                string documentFilePath = Path.Combine(sampleData, "Document.txt");

                                Bot.SetCurrentAction(sender, ChatAction.UploadDocument);
                                Bot.SendDocument(sender, System.IO.File.ReadAllBytes(documentFilePath),
                                    Path.GetFileName(documentFilePath));
                                break;
                            }

                        case "keyboard":
                            {
                                Bot.SendMessage(new SendMessageParams
                                {
                                    ChatId = sender.Id.ToString(),
                                    Text = "This is sample keyboard :",
                                    CustomKeyboard = new ReplyKeyboardMarkup
                                    {
                                        Keyboard = new List<List<KeyboardButton>>
                                {
                                    new List<KeyboardButton>
                                    {
                                        new KeyboardButton { Text="send location",RequestContact=false,RequestLocation=true }
                                        ,   new KeyboardButton {Text="cancel",RequestContact=false,RequestLocation=false }

                                    }
                                },
                                        ResizeKeyboard = true
                                    },
                                    ReplyToMessage = message
                                });

                                break;
                            }
                        case "cancel":
                            {
                                Bot.SendMessage(new SendMessageParams
                                {
                                    ChatId = sender.Id.ToString(),
                                    Text = $"You choose keyboard command : {message.Text}",
                                });
                                break;
                            }

                        default:
                            {
                                Bot.SendMessage(new SendMessageParams
                                {
                                    ChatId = sender.Id.ToString(),
                                    Text = "Unknown command !",
                                });

                                break;
                            }
                    }
            }
            catch (Exception ex)
            {

            }
        }

        private static void GetLocationContactFromMessage(Message message, MessageSender sender)
        {
            if (message.Location != null)
            {
                Console.WriteLine($"Location :({message.Location.Latitude},{message.Location.Longitude})");
                Bot.SendMessage(new SendMessageParams
                {
                    ChatId = sender.Id.ToString(),
                    Text = $"You Send location",
                    ReplyToMessage = message
                });
            }
            else if (message.Contact != null)
            {
                Console.WriteLine($"Contact :({message.Contact.FirstName},{message.Contact.LastName},{message.Contact.PhoneNumber})");
                Bot.SendMessage(new SendMessageParams
                {
                    ChatId = sender.Id.ToString(),
                    Text = $"You Send Contact",
                    ReplyToMessage = message
                });
            }
        }

        public static void DownloadFileFromMessage(Message message, string savePath)
        {
            // Make storage path
            savePath = Path.Combine(savePath, "Storage");
            savePath = Path.Combine(savePath, message.From.Username ?? message.From.Id.ToString());
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            FileDownloadResult fileInfo = null;
            if (message.Document != null)
                fileInfo = Bot.DownloadFileById(message.Document.FileId, savePath);


            // Download video if exists
            if (message.Video != null)
                fileInfo = Bot.DownloadFileById(message.Video.FileId, savePath);


            // Download audio if exists
            if (message.Audio != null)
                fileInfo = Bot.DownloadFileById(message.Audio.FileId, savePath);


            // Download photo if exists
            if (message.Photo != null)
                foreach (PhotoSize photoSize in message.Photo)
                    fileInfo = Bot.DownloadFileById(photoSize.FileId, savePath);

            // Download sticker if exists
            if (message.Sticker != null)
                fileInfo = Bot.DownloadFileById(message.Sticker.FileId, savePath);
            //
            if (fileInfo != null)
                Console.WriteLine($"File : {fileInfo.FilePath} Size : {fileInfo.FileSize} was downloaded successfully");
        }
    }
}
