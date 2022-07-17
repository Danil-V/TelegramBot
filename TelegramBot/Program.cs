using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    class Program
    {
        // Создание экземпляра класса MyTgBot:
        static HelperBot myTgBot;

        static async Task Main()
        {
            Console.WriteLine("Запуск");
            // Получаем API ключ:
            var apiKey = System.IO.File.ReadAllText(@"C:\\C#\\Telegram\\Bot\\apiKey.txt");   // t.me/Helper_B0t_bot - ссылка для работы с ботом.

            // Создаем подключение к БД:
            string connectionString = "Data Source=UsersTasksData.db";
            SqliteConnection connection = new SqliteConnection(connectionString);

            // Cоздаём клиента(бота):
            var bot = new TelegramBotClient(apiKey);

            //Cоздаем базу данных:
            SQLiteBD? sqlBd = new SQLiteBD(connection);
            await sqlBd.GreateSqliteBdAsync(); // Если БД создана, пишем в консоль исключение.
            myTgBot = new HelperBot(sqlBd);

            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            // Выбираем типы обновлений, которые будем получать (только сообщения от пользователя):
            var allowedUpdates = new UpdateType[] { UpdateType.Message, UpdateType.CallbackQuery };
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = allowedUpdates
            };

            // Начинаем принимать обновления:
            bot.StartReceiving(UpdateHandlerAsync, ErrorHandlerAsync, receiverOptions, CancellationToken.None);

            Console.Read();
        }

        // Ссылка на метод который получает ошибки:
        static async Task ErrorHandlerAsync(ITelegramBotClient bot, Exception exc, CancellationToken token) => Console.WriteLine(exc.Message);

        // Ссылка на метод который получает сообщения:
        static async Task UpdateHandlerAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Message != null)
                await myTgBot.ProcessAsync(bot, update, update.Message, token);
            else
                Console.WriteLine(update.CallbackQuery.Data);
        }
    }
}