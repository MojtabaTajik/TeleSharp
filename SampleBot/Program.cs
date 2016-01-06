using System;
using System.IO;
using TeleSharp.Entities;

namespace SampleBot
{
    internal class Program
    {
        public static TeleSharp.TeleSharp Bot;

        private static void Main(string[] args)
        {
            Bot = new TeleSharp.TeleSharp("Bot Authentication Token");
            Bot.OnMessageReceived += HandleMessage;

            Console.WriteLine(@"TeleSharp initialized");

            Console.WriteLine($"Hi, My Name is : {Bot.Me.Username}");

            Console.ReadLine();
        }

        /// <summary>
        /// Read received messages of bot in infinity loop
        /// </summary>
        private static void HandleMessage(Message message)
        {
            // Get mesage sender information
            MessageSender sender = (MessageSender) message.Chat ?? message.From;

            // If user joined to bot say welcome
            if ((!string.IsNullOrEmpty(message.Text)) && (message.Text == "/start"))
            {
                string welcomeMessage =
                    $"Welcome {message.From.Username} !{Environment.NewLine}My name is {Bot.Me.Username}{Environment.NewLine}I made using TeleBot : http://www.github.com/Fel0ny/TeleBot";
                Bot.SendMessage(sender, welcomeMessage);
                return;
            }

            string baseStoragePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            // If any file exists in message download it
            DownloadFileFromMessage(message, baseStoragePath);

            if (string.IsNullOrEmpty(message.Text) || string.IsNullOrEmpty(baseStoragePath))
                return;

            try
            {
                string sampleData = Path.Combine(baseStoragePath, "SampleData");
                switch (message.Text.ToLower())
                {
                    case "time":
                    {
                        Bot.SendMessage(sender, DateTime.Now.ToLongDateString(), false);
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


                    default:
                    {
                        Bot.SendMessage(sender, "Unknown command !", true, message);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Bot.SendMessage(sender, ex.Message, true, message);
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
            if (message.Voice != null)
                fileInfo = Bot.DownloadFileById(message.Voice.FileId, savePath);


            // Download photo if exists
            if (message.Photo != null)
                foreach (PhotoSize photoSize in message.Photo)
                    fileInfo = Bot.DownloadFileById(photoSize.FileId, savePath);

            // Download sticker if exists
            if (message.Sticker != null)
                fileInfo = Bot.DownloadFileById(message.Sticker.FileId, savePath);

            if (fileInfo != null)
                Console.WriteLine($"File : {fileInfo.FilePath} Size : {fileInfo.FileSize} was downloaded successfully");
        }
    }
}
