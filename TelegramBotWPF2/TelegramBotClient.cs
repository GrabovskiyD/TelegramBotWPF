using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using System.Diagnostics;
using Telegram.Bot.Exceptions;
using System.Collections.ObjectModel;
using Telegram.Bot.Types.InputFiles;

namespace TelegramBotWPF2
{
    public class TgBot
    {
        private MainWindow mainWindow;

        private string downloadPath = @"Downloads\";
        public string DownloadPath { get => downloadPath; }

        private TelegramBotClient botClient;

        private ObservableCollection<TelegramUser> users;
        public ObservableCollection<TelegramUser> Users { get => this.users; }

        public TgBot(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            users = new ObservableCollection<TelegramUser>();

            botClient = new TelegramBotClient("5308130432:AAFuba3_1oZrIfQoRXRSfKSnmmoG7rZQ9Qw");

            using CancellationTokenSource cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            Helpers.CreateNeccesaryDirectories(downloadPath);

            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: cts.Token, receiverOptions: receiverOptions);
        }

        /// <summary>
        /// Обработчик обновлений, пришедших от бота.
        /// </summary>
        /// <param name="botClient">Телеграм-бот.</param>
        /// <param name="update">Объект-обновление.</param>
        /// <param name="cancellationToken">Завершающий токен.</param>
        /// <returns></returns>
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                if (update.Message.Text != null)
                {
                    AddUserToCollection(update.Message);
                    await MessageLoger.SaveMessageToHistory(update.Message);
                    await HandleMessageAsync(botClient, update.Message);
                    return;
                }
                else if (update.Message.Video != null || update.Message.Audio != null || update.Message.Document != null)
                {
                    await HandleDigitalContentAsync(botClient, update.Message, DownloadPath);
                    return;
                }
            }
        }

        /// <summary>
        /// Обработчик сообщений.
        /// </summary>
        /// <param name="botClient">Телеграм-бот.</param>
        /// <param name="message">Пришедшее сообщение.</param>
        /// <returns></returns>
        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                TelegramUser newUser = new TelegramUser(message.Chat.Id, message.From.FirstName, message.From.LastName, message.From.Username);

                if (!users.Contains(newUser))
                {
                    users.Add(newUser);
                    users[users.IndexOf(newUser)].AddMessage($"{newUser.Username}: {message.Text}");
                }
            });

            switch (message.Text)
            {
                case "/start":
                    await StartMessageReplyAsync(botClient, message);
                    break;
                case "/files":
                    await ShowUploadFilesAsync(botClient, message, DownloadPath);
                    break;
                default:
                    await SentFileToUserAsync(botClient, message, DownloadPath);
                    break;
            }
        }

        /// <summary>
        /// Метод осуществляет отправку пользовтелю файла, если файл существует.
        /// </summary>
        /// <param name="botClient">Телеграм-бот.</param>
        /// <param name="message">Сообщение от пользователя.</param>
        /// <param name="basePath">Базовый путь к папке с загрузками.</param>
        private async Task SentFileToUserAsync(ITelegramBotClient botClient, Message message, string basePath)
        {
            string pathFile = basePath + $@"{message.From.FirstName} {message.From.LastName}\{message.Text}";
            if (System.IO.File.Exists(pathFile))
            {
                await using Stream stream = System.IO.File.OpenRead(pathFile);
                await botClient.SendDocumentAsync(message.Chat.Id,
                                                document: new InputOnlineFile(content: stream,
                                                fileName: pathFile.Split(@"\").Last()));
            }
        }

        /// <summary>
        /// Ответ на сообщение /start.
        /// </summary>
        /// <param name="botClient">Телеграм-бот.</param>
        /// <param name="message">Сообщение от пользователя.</param>
        /// <returns></returns>
        private async Task StartMessageReplyAsync(ITelegramBotClient botClient, Message message)
        {
            string replyText = $"Привет, {message.From.FirstName}!\n" +
                        $"Этот бот может использоваться как файлообменник:\n" +
                        $"сохранять и загружать аудио- и видеофайлы, документы, фото (в виде документов);\n" +
                        $"\n" +
                        $"Чтобы просмотерть список ранее отправленных файлов введите:\n" +
                        $"/files\n" +
                        $"Чтобы получить ранее отправленный файл введите его имя.";
            await botClient.SendTextMessageAsync(message.Chat.Id, replyText);
            return;

        }

        /// <summary>
        /// Обработка получения контента (аудио, фото, видео).
        /// </summary>
        /// <param name="botClient">Телеграм-бот.</param>
        /// <param name="message">Сообщение от пользователя.</param>
        /// <returns></returns>
        private async Task HandleDigitalContentAsync(ITelegramBotClient botClient, Message message, string basePath)
        {
            Debug.WriteLine($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}." +
                $" От пользователя {message.From.FirstName} {message.From.LastName} получен файл типа {message.Type}.");
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            string path = basePath + @$"{message.From.FirstName} {message.From.LastName}\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            await Helpers.FileDownloader(botClient, message, path);
        }

        /// <summary>
        /// Отправка файла, запрашиваемого пользователем.
        /// </summary>
        /// <param name="botClient">Телеграм-бот.</param>
        /// <param name="message">Сообщение от пользователя.</param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        private async Task ShowUploadFilesAsync(ITelegramBotClient botClient, Message message, string basePath)
        {
            string path = basePath + @$"{message.From.FirstName} {message.From.LastName}\";
            if (Directory.Exists(path))
            {
                List<string> downloadedFiles = Helpers.GetListOfDownloadedFiles(path);
                StringBuilder answerString = new StringBuilder();
                if (downloadedFiles.Count != 0)
                {
                    answerString.Append("Список загруженных файлов:\n\n");
                    foreach (string downloadedFile in downloadedFiles)
                    {
                        answerString.Append(downloadedFile);
                        answerString.Append("\n");
                    }
                }
                else
                {
                    answerString.Append("Загруженных файлов нет.");
                }
                await botClient.SendTextMessageAsync(message.Chat.Id, answerString.ToString());
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Загруженных файлов нет.");
            }
        }

        /// <summary>
        /// Метод добавляет нового пользователя в коллекцию пользователей.
        /// </summary>
        /// <param name="message">Сообщений от пользователя.</param>
        /// <returns></returns>
        private void AddUserToCollection(Message message)
        {
            mainWindow.Dispatcher.Invoke(() =>
            {
                MessageFromUser msg = new(
                                    message.Chat.Id,
                                    message.From.FirstName,
                                    message.From.LastName,
                                    message.From.Username,
                                    message.Text);

                TelegramUser user = new(msg.Id, msg.FirstName, msg.LastName, msg.Username);
                if (!Users.Contains(user))
                {
                    Users.Add(user);
                }
                Users[Users.IndexOf(user)].AddMessage(
                        $"{msg.MessageTime} {msg.FirstName} {msg.LastName}: {msg.Text}");
            }); 
        }

        /// <summary>
        /// Отправка сообщения в чат с пользователем вручную.
        /// </summary>
        /// <returns></returns>
        public async Task SendMessage()
        {
            TelegramUser selectedUser = users[users.IndexOf(mainWindow.usersList.SelectedItem as TelegramUser)];
            string responseMessage = $"{mainWindow.messageBox.Text}";
            selectedUser.AddMessage($"Bot message + {responseMessage}");
            await botClient.SendTextMessageAsync(selectedUser.Id, responseMessage);
            mainWindow.messageBox.Text = String.Empty;
        }

        /// <summary>
        /// Обработчик ошибок
        /// </summary>
        /// <param name="botClient">Телеграм-бот.</param>
        /// <param name="exception">Пойманное исключение.</param>
        /// <param name="cancellationToken">Завершающий токен.</param>
        /// <returns></returns>
        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Ошибка телеграм API:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString(),
            };
            Debug.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
