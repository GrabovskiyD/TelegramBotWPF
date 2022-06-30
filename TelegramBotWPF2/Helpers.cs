using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TelegramBotWPF2
{
    public static class Helpers
    {
        /// <summary>
        /// Метод возвращает список всех файлов, полученных от пользователя.
        /// </summary>
        /// <param name="basePath">Базовый путь к папке с загрузками.</param>
        /// <returns>Возвращается список загруженых файлов.</returns>
        public static List<string> GetListOfDownloadedFiles(string basePath)
        {
            List<string> downloadedFiles = new List<string>();
            foreach (string file in Directory.GetFiles(basePath))
            {
                downloadedFiles.Add(file.Split('\\').Last());
            }
            return downloadedFiles;
        }

        /// <summary>
        /// Метод создаёт все необходимые для работы бота директории.
        /// </summary>
        public static void CreateNeccesaryDirectories(string downloadPath)
        {
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }

            if (!Directory.Exists(MessageLoger.Path))
            {
                Directory.CreateDirectory(MessageLoger.Path);
            }
        }

        /// <summary>
        /// Метод осуществляет переименование файла, если его имя равно null.
        /// </summary>
        /// <param name="messageType">Тип сообщения.</param>
        /// <returns></returns>
        private static string FileRenamer(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Video => $"Video {DateTime.Now.ToShortDateString().Replace('.', '_')}_{DateTime.Now.ToShortTimeString().Replace(':', '_')}.mp4",
                MessageType.Audio => $"Auido {DateTime.Now.ToShortDateString().Replace('.', '_')}_{DateTime.Now.ToShortTimeString().Replace(':', '_')}.mp3",
                MessageType.Photo => $"Photo {DateTime.Now.ToShortDateString().Replace('.', '_')}_{DateTime.Now.ToShortTimeString().Replace(':', '_')}.jpg",
                _ => $"File {DateTime.Now.ToShortDateString().Replace('.', '_')}_{DateTime.Now.ToShortTimeString().Replace(':', '_')}"
            };
        }

        /// <summary>
        /// Метод осуществляет загрузку файла в личную папку пользователя.
        /// </summary>
        /// <param name="botClient">Телеграм-бот.</param>
        /// <param name="message">Сообщение от пользователя.</param>
        /// <param name="basePath">Базовый путь к папке с загрузками.</param>
        /// <returns></returns>
        public static async Task FileDownloader(ITelegramBotClient botClient, Message message, string basePath)
        {
            string path;
            switch (message.Type)
            {
                case MessageType.Document:
                    path = basePath + message.Document.FileName;
                    await Download(botClient, message.Document.FileId, path);
                    Debug.WriteLine($"Документ сохранён по адресу: {path}");
                    break;
                case MessageType.Audio:
                    string audioFileName = message.Audio.FileName;
                    if (audioFileName == null)
                    {
                        audioFileName = FileRenamer(MessageType.Audio);
                    }
                    Debug.WriteLine($"Название аудиофайла: {audioFileName}, размер: {message.Audio.FileSize}.");
                    path = basePath + audioFileName;
                    await Download(botClient, message.Audio.FileId, path);
                    Debug.WriteLine($"Аудиофайл сохранён по адресу: {path}");
                    break;
                case MessageType.Video:
                    string videoFileName = message.Video.FileName;
                    if (videoFileName == null)
                    {
                        videoFileName = FileRenamer(MessageType.Video);
                    }
                    Debug.WriteLine($"Название видеофайла: {videoFileName}, размер: {message.Video.FileSize}.");
                    path = basePath + videoFileName;
                    await Download(botClient, message.Video.FileId, path);
                    Debug.WriteLine($"Видеофайл сохранён по адресу: {path}");
                    break;
                case MessageType.Photo:
                    string photoName = FileRenamer(MessageType.Photo);
                    Debug.WriteLine($"Получено фото {photoName}");
                    path = basePath + photoName;
                    await Download(botClient, message.Photo[0].FileId, path);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Метод для загрузки файлов из чата.
        /// </summary>
        /// <param name="botClient">Телеграм-бот.</param>
        /// <param name="fileID">Идентификатор файла.</param>
        /// <param name="downloadPath">Путь сохранения файла.</param>
        /// <returns></returns>
        private static async Task Download(ITelegramBotClient botClient, string fileID, string downloadPath)
        {
            var file = await botClient.GetFileAsync(fileID);
            FileStream fs = new FileStream($"{downloadPath}", FileMode.Create);
            await botClient.DownloadFileAsync(file.FilePath, fs);
            fs.Close();
        }
    }
}
