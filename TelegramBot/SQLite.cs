using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    public class SQLiteBD
    {
        // Сохраняем состояние подключения к БД:
        private SqliteConnection Connection { get; set; }

        // Открывает соединение с БД:
        public SQLiteBD(SqliteConnection connection)
        {
            connection.Open();
            Connection = connection;
        }


        // Создание бд, таблиц и столбцов: 
        public async Task GreateSqliteBdAsync()
        {
            try
            {                
                SqliteCommand command = new();
                command.Connection = Connection;
                command.CommandText = "CREATE TABLE Tasks(Id INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, Username TEXT NOT NULL, UserTasks TEXT NOT NULL)";  //IdTask INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE
                await command.ExecuteNonQueryAsync();
                Console.WriteLine("Таблица Tasks создана");
                command.CommandText = "CREATE TABLE Persons(Id INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, Username TEXT NOT NULL, FlagAddTask INTEGER NOT NULL, FlagChangeTask INTEGER NOT NULL, FlagAddChangeTask INTEGER NOT NULL, FlagRemoveTask INTEGER NOT NULL, IdTask INTEGER NOT NULL)";
                await command.ExecuteNonQueryAsync();
                Console.WriteLine("Таблица Persons создана");                
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); } // Если база данных есть, просто выкидывает исключение.
        }


        // Работа с Пользователем (Таблица Persons)
        // Получение данных пользователя (помещаем в БД, таблица Persons)        
        public async Task AddPersonAsync(ITelegramBotClient bot, Message message, Person person, CancellationToken token)
        {
            try
            {
                string sqlExpression = $"SELECT * FROM Persons WHERE Username='{person.UserName}'";
                string name = string.Empty;
                
                SqliteCommand command = new SqliteCommand(sqlExpression, Connection);
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {

                    if (reader.HasRows) // если есть данные
                    {
                        while (reader.Read())   // построчно считываем данные
                        {
                            var id = reader.GetValue(0);
                            var userName = reader.GetValue(1);
                            name = userName.ToString();
                        }
                    }
                }

                if (name == person.UserName) { } // если пользователь создан, ничего не делаем, если пользователь отсутствует, создаем нового пользователя
                else
                {
                    command.Connection = Connection;
                    command.CommandText =
                        $"INSERT INTO Persons (Username, FlagAddTask, FlagChangeTask, FlagAddChangeTask, FlagRemoveTask, IdTask) " +
                        $"VALUES ('{person.UserName}', '{person.FlagAddTask}', '{person.FlagChangeTask}', '{person.FlagAddChangeTask}', '{person.FlagRemoveTask}', '{person.IdTask}')";
                    int number = await command.ExecuteNonQueryAsync();

                    Console.WriteLine($"В таблицу Persons добавлено пользователей: {number}");
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }


        // Обновление данных пользователя в БД (Таблица Persons)
        public async Task UpdatePersonAsync(ITelegramBotClient bot, Message message, Person person, CancellationToken token)
        {
            try
            {
                string sqlExpression = $"SELECT * FROM Persons WHERE Username='{person.UserName}'";
                string name = string.Empty;
                
                SqliteCommand command = new SqliteCommand(sqlExpression, Connection);
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {

                    if (reader.HasRows) // если есть данные
                    {
                        while (reader.Read())   // построчно считываем данные
                        {
                            var id = reader.GetValue(0);
                            var userName = reader.GetValue(1);
                            name = userName.ToString();
                        }
                    }
                }

                if (name == person.UserName) // если пользователь есть, обновляем данные
                {
                    sqlExpression = $"UPDATE Persons " +
                                    $"SET Username='{person.UserName}', FlagAddTask='{person.FlagAddTask}', FlagChangeTask='{person.FlagChangeTask}', FlagAddChangeTask='{person.FlagAddChangeTask}', FlagRemoveTask='{person.FlagRemoveTask}', IdTask='{person.IdTask}' " +
                                    $"WHERE Username='{name}'";

                    command = new SqliteCommand(sqlExpression, Connection);
                    int number = await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"Обновлено данных пользователя: {number}");
                }                
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }


        // Получение данных по пользователю из БД (Таблица Persons)
        public async Task<Person> GetPersonAsync(string name)
        {
            var newPerson = new Person();
            try
            {
                string sqlExpression = $"SELECT * FROM Persons WHERE Username='{name}'";
                
                SqliteCommand command = new SqliteCommand(sqlExpression, Connection);
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows) // если есть данные
                    {
                        while (reader.Read())   // построчно считываем данные
                        {
                            // Получаем данные для newPerson
                            int id = reader.GetInt32(0);
                            string userName = reader.GetString(1);
                            int flagAddTask = reader.GetInt32(2);
                            int flagChangeTask = reader.GetInt32(3);
                            int flagAddChangeTask = reader.GetInt32(4);
                            int flagRemoveTask = reader.GetInt32(5);
                            int idTask = reader.GetInt32(6);

                            newPerson.UserName = userName;
                            newPerson.FlagAddTask = flagAddTask;
                            newPerson.FlagChangeTask = flagChangeTask;
                            newPerson.FlagAddChangeTask = flagAddChangeTask;
                            newPerson.FlagRemoveTask = flagRemoveTask;
                            newPerson.IdTask = idTask;
                        }
                    }
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
            return newPerson;
        }


        // Работа с БД (Таблица Tasks)
        // Добавление данных в БД (Таблица Tasks):
        public async Task AddAsync(ITelegramBotClient bot, Message message, string? username, string? task, CancellationToken token)
        {
            try
            {
                SqliteCommand command = new SqliteCommand();
                command.Connection = Connection;
                command.CommandText = $"INSERT INTO Tasks (Username, UserTasks) VALUES ('{username}', '{task}')";
                int number = await command.ExecuteNonQueryAsync();

                Console.WriteLine($"В таблицу Tasks добавлено объектов: {number}");
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                await Task.Delay(500);
                await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Добавлено задач: {number}", cancellationToken: token);
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }


        // Корректировка задач в БД (Таблица Tasks): 
        public async Task ChangeTaskAsync(ITelegramBotClient bot, Message message, string userTask, string username, int id, CancellationToken token)
        {
            try
            {
                string sqlExpression = $"UPDATE Tasks SET UserTasks='{userTask}' WHERE Username='{username}' AND id='{id}'";

                var command = new SqliteCommand(sqlExpression, Connection);
                int number = await command.ExecuteNonQueryAsync();
                Console.WriteLine($"Обновлено объектов: {number}");
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                await Task.Delay(500);
                await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Обновлено задач: {number}", cancellationToken: token);
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }


        // Удаление объектов из БД (Таблица Tasks):
        public async Task RemoveTaskAsync(ITelegramBotClient bot, Message message, string username, int idTask, CancellationToken token)
        {
            try
            {
                string sqlExpression = $"DELETE FROM Tasks WHERE Username='{username}' AND Id={idTask}";
                
                var command = new SqliteCommand(sqlExpression, Connection);
                int number = await command.ExecuteNonQueryAsync();

                Console.WriteLine($"Удалено объектов: {number}");
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                await Task.Delay(500);
                await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Удаленно задач: {number}", cancellationToken: token);                
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }


        // Удаление всех задач у пользователя из БД (Таблица Tasks):
        public async Task RemoveAllTasksAsync(ITelegramBotClient bot, Message message, string username, CancellationToken token)
        {
            try
            {
                string sqlExpression = $"DELETE FROM Tasks WHERE Username='{username}'";
                
                var command = new SqliteCommand(sqlExpression, Connection);
                int number = await command.ExecuteNonQueryAsync();

                Console.WriteLine($"Удалено объектов: {number}");
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                await Task.Delay(500);
                await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Удаленно задач: {number}", cancellationToken: token);                
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }


        // Вывод всех задач пользователю из БД (Таблица Tasks):
        public async Task PrintAllTasksAsync(ITelegramBotClient bot, Message message, string username, CancellationToken token)
        {
            try
            {
                string sqlExpression = $"SELECT * FROM Tasks WHERE Username='{username}'";
                
                SqliteCommand command = new SqliteCommand(sqlExpression, Connection);
                using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows) // если есть данные
                    {
                        while (reader.Read())   // построчно считываем данные
                        {
                            var id = reader.GetValue(0);
                            var userName = reader.GetValue(1);
                            var userTasks = reader.GetValue(2);

                            await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Id: {id} - \t{userTasks}.", cancellationToken: token);
                            Console.WriteLine($"{id} \t {userName} \t {userTasks}");
                        }
                        await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Для удаления всех задач отправьте: /RemoveAll", cancellationToken: token);
                    }
                    else
                        await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Список задач пуст.", cancellationToken: token);
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex.Message); }
        }
    }
}