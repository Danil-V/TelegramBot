using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace TelegramBot
{
    public class HelperBot
    {
        SQLiteBD _sqlBd;
        public HelperBot(SQLiteBD sqlBD)
        {
            _sqlBd = sqlBD;
        }

        // Вывод сообщений в консоль:
        public async Task ProcessAsync(ITelegramBotClient bot, Update update, Message message, CancellationToken token)
        {
            Person person = new();

            // Вывод сообщений в консоль:
            Console.WriteLine(message.Text);

            // Отправка клавиатуры и работа с меню:
            switch (message.Text)
            {
                case "/start":
                    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing); // отправление сообщения - живой бот (печатает).
                    await Task.Delay(500);                                             // Видимость печати:
                    await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Для корректной работы бота, укажите в настройках Telegram: Имя пользователя", cancellationToken: token);
                    await SendReplyKeyboardAsync(bot, message, token); // при /start добавляем клавиатуру
                    break;

                case "/menu":
                    await SendReplyKeyboardAsync(bot, message, token); // при /menu добавляем клавиатуру
                    break;

                case "Создать задачу":
                    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing); // отправление сообщения - живой бот (печатает).
                    await Task.Delay(500);                                             // Видимость печати:

                    await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                         text: "Введите название задачи:",                                                         
                                                         cancellationToken: token);
                    person.UserName = update.Message.Chat.Username;
                    person.FlagAddTask = 1;                                     // Активация флага для метода AddTaskAsync();
                    await _sqlBd.AddPersonAsync(bot, message, person, token);    // Добавление пользователя в БД таблицу Persons
                    await _sqlBd.UpdatePersonAsync(bot, message, person, token); // Если пользователь есть, обновлем
                    update.Message.Text = null;                                 // Очистка.
                    break;

                case "Изменить задачу":
                    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);  // отправление сообщения - живой бот (печатает).
                    await Task.Delay(500);
                    await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Введите id задачи: ", cancellationToken: token);

                    person.UserName = update.Message.Chat.Username;                 // Получение имени пользователя.
                    person.FlagChangeTask = 1;                                      // Активация флага для метода ChangeTaskAsync() - шаг 1.
                    await _sqlBd.UpdatePersonAsync(bot, message, person, token);     // Если пользователь есть, обновлем
                    update.Message.Text = null;                                     // Очистка данных для корректной работы метода.
                    break;

                case "Завершить задачу":
                    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);  // отправление сообщения - живой бот (печатает).
                    await Task.Delay(500);                                              // Видимость печати
                    await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                         text: "Для удаления задачи, введи ее id:",                                                        
                                                         cancellationToken: token);

                    person.UserName = update.Message.Chat.Username;             // Получение имени пользователя.
                    person.FlagRemoveTask = 1;                                  // Активация флага для метода RemoveTaskAsync();                                                                          
                    await _sqlBd.UpdatePersonAsync(bot, message, person, token); // Если пользователь есть, обновлем
                    update.Message.Text = null;                                 // Очистка.
                    break;

                case "Посмотреть список оставшихся задач":
                    person.UserName = update.Message.Chat.Username;     // Получение имени пользователя.
                    await _sqlBd.PrintAllTasksAsync(bot, message, person.UserName, token);
                    break;

                case "/RemoveAll":
                    person.UserName = update.Message.Chat.Username;     // Получение имени пользователя.
                    await _sqlBd.RemoveAllTasksAsync(bot, message, person.UserName, token); // удаление всех задач.
                    await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Задачи удалены.", cancellationToken: token);
                    break;

                case "Скрыть меню":
                    await RemoveKeyboardAsync(bot, message, token); // удаление клавиатуры вызываемой при /start
                    break;

                case "Досуг":
                    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing); // отправление сообщения - живой бот (печатает).
                    await Task.Delay(500);                                             // Видимость печати:
                    await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Когда-нибудь я обязательно напишу этот раздел :)", cancellationToken: token);
                    break;

                default:                    
                    break;
            }
            await AddTaskAsync(bot, update, message, token);            // Добавление задачи.         
            await ChangeTaskAsync(bot, update, message, token);         // Изменение задачи шаг 1.
            await AddChangeTaskAsync(bot, update, message, token);      // Изменение задачи шаг 2.
            await RemoveTaskAsync(bot, update, message, token);         // Удаление задачи.
        }


        // Добавление клавиатуры и отправка в чат:
        public async Task<Message> SendReplyKeyboardAsync(ITelegramBotClient bot, Message message, CancellationToken token)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                    new KeyboardButton[] { "Создать задачу"},
                    new KeyboardButton[] { "Изменить задачу"},
                    new KeyboardButton[] { "Завершить задачу" },
                    new KeyboardButton[] { "Посмотреть список оставшихся задач" },
                    new KeyboardButton[] { "Досуг" },
                    new KeyboardButton[] { "Скрыть меню" }
                }
                )
            {
                ResizeKeyboard = true
            };
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                         text: "Выберите варинат:",
                                                         replyMarkup: replyKeyboardMarkup, // Добавляем клавиатуру
                                                         cancellationToken: token);
        }


        // Удаление клавиатуры:
        public async Task<Message> RemoveKeyboardAsync(ITelegramBotClient bot, Message message, CancellationToken token)
        {
            return await bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                                          text: "Для возврата меню отправьте /menu",
                                                          replyMarkup: new ReplyKeyboardRemove(), // Удаляем клавиатуру
                                                          cancellationToken: token);
        }


        //Добавление задачи в БД:
        public async Task AddTaskAsync(ITelegramBotClient bot, Update update, Message message, CancellationToken token)
        {
            Person newPerson = await _sqlBd.GetPersonAsync(update.Message.Chat.Username);            // получаем данные из БД для Person  
            
            if (newPerson.FlagAddTask == 1 && update.Message.Text != null)
            {
                if (update.Message.Text == "/start")
                    await SendReplyKeyboardAsync(bot, message, token);
                else
                {
                    newPerson.UserTaskMessage = update.Message.Text;                                                     // Получаем задачу.
                    await _sqlBd.AddAsync(bot, message, newPerson.UserName, newPerson.UserTaskMessage, token);           // Добавляем задачу.

                    // Обновляем параметры для дальнейшей работы:
                    newPerson.FlagAddTask = 0;
                    await _sqlBd.UpdatePersonAsync(bot, message, newPerson, token);                                      // Обновление состояния БД.                   
                    await SendReplyKeyboardAsync(bot, message, token);
                }
            }

        }


        // Корректировка задачи  шаг 1 (получение id задачи):
        public async Task ChangeTaskAsync(ITelegramBotClient bot, Update update, Message message, CancellationToken token)
        {
            Person newPerson = await _sqlBd.GetPersonAsync(update.Message.Chat.Username);    // получаем данные из БД для Person
            bool flag = false;

            if (newPerson.FlagChangeTask == 1 && update.Message.Text != null)
            {
                newPerson.UserTaskMessage = update.Message.Text;               
                flag = int.TryParse(newPerson.UserTaskMessage, out int idTask); // если считали id, переходим к шагу 2.
                newPerson.IdTask = idTask;  
            }            

            if (flag == true)
            {
                await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);  // отправление сообщения - типо живой бот (печатает).
                await Task.Delay(500);
                await bot.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Введите новую задачу: ", cancellationToken: token);

                // Обновляем параметры для дальнейшей работы:                
                newPerson.FlagChangeTask = 0;                                    // сброс флага для шага 1
                newPerson.FlagAddChangeTask = 1;                                 // активая флага для перехода к шагу 2
                await _sqlBd.UpdatePersonAsync(bot, message, newPerson, token);  // обновление состояния БД.
                update.Message.Text = null;                                      // Очистка.
            }
        }


        // Корректировка задачи  шаг 2 (получение новой задачи):
        public async Task AddChangeTaskAsync(ITelegramBotClient bot, Update update, Message message, CancellationToken token)
        {
            Person newPerson = await _sqlBd.GetPersonAsync(update.Message.Chat.Username);    // получаем данные из БД для Person

            if (newPerson.FlagAddChangeTask == 1 && update.Message.Text != null)
            {
                newPerson.UserTaskMessage = update.Message.Text;
                await _sqlBd.ChangeTaskAsync(bot, message, newPerson.UserTaskMessage, newPerson.UserName, newPerson.IdTask, token); // Добавление обновленной задачи в бд.

                // Обновляем параметры для дальнейшей работы:
                newPerson.FlagAddChangeTask = 0;      // сброс флага для шага 2.                
                newPerson.IdTask = 0;                 // сброс id задачи.
                await _sqlBd.UpdatePersonAsync(bot, message, newPerson, token);
                await SendReplyKeyboardAsync(bot, message, token);
            }
        }


        // Завершение задачи:
        public async Task RemoveTaskAsync(ITelegramBotClient bot, Update update, Message message, CancellationToken token)
        {
            Person newPerson = await _sqlBd.GetPersonAsync(update.Message.Chat.Username);    // получаем данные из БД для Person

            if (newPerson.FlagRemoveTask == 1 && update.Message.Text != null)
            {
                newPerson.UserTaskMessage = update.Message.Text;
                _ = int.TryParse(newPerson.UserTaskMessage, out var key);

                await _sqlBd.RemoveTaskAsync(bot, message, newPerson.UserName, key, token);
                newPerson.FlagRemoveTask = 0;                                   // сброс флага для удаления задачи.
                await _sqlBd.UpdatePersonAsync(bot, message, newPerson, token);  // обновление состояния флага для удаления задачи в БД.
                await SendReplyKeyboardAsync(bot, message, token);

            }
        }
    }
}