namespace TelegramBot
{
    public class Person
    {
        public string UserName { get; set; } = string.Empty;                // Получение имени пользователя для базы данных
        public string UserTaskMessage { get; set; } = string.Empty;         // Получение сообщений из чата
        public int FlagAddTask { get; set; } = 0;                           // Добавление задачи
        public int FlagChangeTask { get; set; } = 0;                        // Изменение задачи шаг 1 - получение id задачи
        public int FlagAddChangeTask { get; set; } = 0;                     // Изменение задачи шаг 2 - получение и изменение задачи
        public int FlagRemoveTask { get; set; } = 0;                        // Удаление задачи
        public int IdTask { get; set; } = 0;                                // Получение id задачи для изменения методом ChangeTaskAsync
    }
}