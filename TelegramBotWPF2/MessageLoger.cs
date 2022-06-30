using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System;

namespace TelegramBotWPF2
{
    /// <summary>
    /// Структура, содержащая информацию о сообщении, пришедшем от пользователя.
    /// Используется для сериализации информации в .json файл.
    /// </summary>
    /// <summary>
    /// Структура содержит информацию из объекта message,
    /// которая будет сохранена в историю сообщений.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="firstName"></param>
    /// <param name="lastName"></param>
    /// <param name="username"></param>
    /// <param name="messageTime"></param>
    /// <param name="text"></param>
    struct MessageFromUser
    {
        public MessageFromUser(long id, string firstName, string lastName, string username, string text)
        {
            this.id = id;
            this.firstName = firstName;
            this.lastName = lastName;
            this.username = username;
            messageTime = DateTime.Now.ToString("g");
            this.text = text;
        }
        private long id;
        public long Id { get { return id; } }

        private string firstName;
        public string FirstName { get { return firstName; } }
        private string lastName;
        public string LastName { get { return lastName; } }
        private string username;
        public string Username { get { return username; } }

        private string messageTime;
        public string MessageTime { get { return messageTime; } }

        private string text;
        public string Text { get { return text; } }
    }

    /// <summary>
    /// Класс, содержащий методы для логирования сообщений.
    /// </summary>
    internal static class MessageLoger
    {

        private static string path = @"Logs\";
        public static string Path { get { return path; } }
        /// <summary>
        /// Метод сохраняет сообщение, пришедшее от пользователя в файл .json.
        /// </summary>
        /// <param name="message">Сообщение от пользователя.</param>
        /// <returns></returns>
        public static async Task SaveMessageToHistory(Message message)
        {
            MessageFromUser msgFromUser = new(message.Chat.Id, message.From.FirstName, message.From.LastName, message.From.Username, message.Text);

            using(FileStream fs = new FileStream(Path + $"{message.Chat.Id}" + ".json", FileMode.Append))
            {
                await JsonSerializer.SerializeAsync<MessageFromUser>(fs, msgFromUser, options: new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                });
            }
        }
    }
}
