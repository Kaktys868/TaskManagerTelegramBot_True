using System.Reflection.Metadata.Ecma335;
using TaskManagerTelegramBot_True.Classes;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TaskManagerTelegramBot_True
{
    public class Worker : BackgroundService
    {
        readonly string Token = "полученный телеграм токен";
        TelegramBotClient TelegramBotClient;
        List<Users> Users = new List<Users>();
        Timer Timer;
        List<string> Messages = new List<string>()
        {
            "Здравствуйте!" +
            "\nРад приветствовать вас в Telegram-боте «Напоминатор»!" +
            "\nНаш бот создан для того, чтобы напоминать вам о важных событиях и мероприятиях. С ним вы точно не пропустите ничего важного!" +
            "\nНе забудьте добавить бота в список своих контактов и настроить уведомления. Тогда вы всегда будете в курсе событий!",

            "Укажите дату и время напоминания в следующем формате:" +
            "\n<i><b>12:51 26.04.2025</b></i>" +
            "\nНапомни о том что я хотел сходить в магазин.",

            "Кажется, что-то не получилось." +
            "\nУкажите дату и время напоминания в следующем формате:" +
            "\n<i><b>12:51 26.04.2025</b></i>" +
            "\nНапомни о том что я хотел сходить в магазин.",
            
            "Задачи пользователя не найдены.",

            "Событие удалено.",

            "Все события удалены."
        };
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
        public bool CheckFormatDateTime(string value, out DateTime time)
        {
            return DateTime.TryParse(value,out time);
        }
        private static ReplyKeyboardMarkup GetButtons()
        {
            List<KeyboardButton> keyboardButtons = new List<KeyboardButton>();
            keyboardButtons.Add(new KeyboardButton("Удалить все задачи"));
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>> { keyboardButtons }
            };
        }
        public static InlineKeyboardMarkup DeleteEvent(string Message)
        {
            List<InlineKeyboardButton> inlineKeyboards = new List<InlineKeyboardButton>();
            inlineKeyboards.Add(new InlineKeyboardButton("Удалить") { CallbackData = Message });
            return new InlineKeyboardMarkup(inlineKeyboards);
        }
    }
}
