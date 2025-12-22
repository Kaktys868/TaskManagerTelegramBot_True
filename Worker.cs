using System.Reflection.Metadata.Ecma335;
using TaskManagerTelegramBot_True.Classes;
using TaskManagerTelegramBot_True.Classes.Common;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TaskManagerTelegramBot_True
{
    public class Worker : BackgroundService
    {
        private Dictionary<string, DayOfWeek> dayMapping = new Dictionary<string, DayOfWeek>
        {
            ["понедельник"] = DayOfWeek.Monday,
            ["вторник"] = DayOfWeek.Tuesday,
            ["среда"] = DayOfWeek.Wednesday,
            ["четверг"] = DayOfWeek.Thursday,
            ["пятница"] = DayOfWeek.Friday,
            ["суббота"] = DayOfWeek.Saturday,
            ["воскресенье"] = DayOfWeek.Sunday,
            ["воскресение"] = DayOfWeek.Sunday
        };

        readonly string Token = "8448214301:AAF0Fcz1NiXXbnCAUrlpK3itwDoMs3gFR80";
        TelegramBotClient TelegramBotClient;
        List<Users> Users = new List<Users>();
        static List<Events> Events = new List<Events>();
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
            TelegramBotClient = new TelegramBotClient(Token);
            TelegramBotClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                null,
                new CancellationTokenSource().Token
                );
            TimerCallback TimerCallback = new TimerCallback(Tick);
            Timer = new Timer(TimerCallback, 0, 0, 60 * 1000);
        }
        public bool CheckFormatDateTime(string value, out DateTime time)
        {
            return DateTime.TryParse(value, out time);
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
        public static InlineKeyboardMarkup DeleteEvent(int IdEvent)
        {
            List<InlineKeyboardButton> inlineKeyboards = new List<InlineKeyboardButton>();
            inlineKeyboards.Add(new InlineKeyboardButton("Удалить") { CallbackData = Events.Find(x=>x.Id==IdEvent).Message});
            return new InlineKeyboardMarkup(inlineKeyboards);
        }
        public async void SendMessage(long chatId, int typeMessage)
        {
            if (typeMessage != 3)
            {
                await TelegramBotClient.SendMessage(
                    chatId,
                    Messages[typeMessage],
                    parseMode: ParseMode.Html,
                    replyMarkup: GetButtons());

            }
            else if (typeMessage == 3)
            {
                await TelegramBotClient.SendMessage(
                    chatId,
                    $"Указанное вами время и дата не могут быть установлены, " +
                    $"потому что сейчас уже: {DateTime.Now.ToString("HH:mm dd.MM.yyyy")}",
                    replyMarkup: GetButtons());
            }
        }
        public async void Command(long chatId, string command)
        {
            if (command.ToLower() == "/start") SendMessage(chatId, 0);
            else if (command.ToLower() == "/create_task") { 
                SendMessage(chatId, 1);
            }
            else if (command.ToLower() == "/list_tasks")
            {
                Users User = Users.Find(x => x.IdUser == chatId);
                if (User == null) SendMessage(chatId, 3);
                else if (User.Events.Count == 0) SendMessage(chatId, 3);
                else
                {
                    foreach (Events Event in User.Events)
                    {
                        string messageText = $"⏰ {Event.Time.ToString("HH:mm dd.MM.yyyy")}\n" +
                                           $"📝 {Event.Message}";

                        if (Event.IsRecurring)
                        {
                            messageText += $"\n🔁 Повторяется: {Event.RecurrenceTime} ({Event.RecurrenceDays.Replace(",", ", ")})";
                        }

                        await TelegramBotClient.SendMessage(
                            chatId,
                            messageText,
                            replyMarkup: DeleteEvent(Event.Id)
                        );
                    }
                }
            }
        }
        public async void GetMessages(Message message)
        {
            Console.WriteLine("Получено сообщение: " + message.Text + " от пользователя: " + message.Chat.Username);
            long IdUser = message.Chat.Id;
            string MessageUser = message.Text;



            if (message.Text.Contains("/")) Command(message.Chat.Id, message.Text);
            else if (message.Text.Equals("Удалить все задачи"))
            {
                Users User = Users.Find(x => x.IdUser == message.Chat.Id);
                if (User == null) SendMessage(message.Chat.Id, 3);
                else if (User.Events.Count == 0) SendMessage(User.IdUser, 3);
                else
                {
                    User.Events = new List<Events>();
                    SendMessage(User.IdUser, 5);
                }
            }
            else if (message.Text.Equals("Показать все задачи"))
            {
                Users User = Users.Find(x => x.IdUser == message.Chat.Id);
                if (User == null) SendMessage(message.Chat.Id, 3);
                else if (User.Events.Count == 0) SendMessage(message.Chat.Id, 3);
                else
                {
                    foreach (Events Event in User.Events)
                    {
                        TelegramBotClient.SendMessage(
                            message.Chat.Id,
                            $"Уведомить пользователя: {Event.Time.ToString("HH:mm dd.MM.yyyy")}" +
                            $"\nСообщение: {Event.Message}",
                            replyMarkup: DeleteEvent(Event.Id)
                        );
                    }
                }
            }
            else
            {
                Users User = Users.Find(x => x.IdUser == message.Chat.Id);
                if (User == null)
                {
                    User = new Users(message.Chat.Id);
                    Users.Add(User);
                }

                string[] Info = message.Text.Split('\n');
                if (Info.Length < 2)
                {
                    SendMessage(message.Chat.Id, 2);
                    return;
                }

                // ========== ПРОВЕРКА НА ПОВТОРЯЮЩУЮСЯ ЗАДАЧУ ==========
                var firstLineParts = Info[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (firstLineParts.Length >= 3 && firstLineParts[1].ToLower() == "каждый")
                {
                    // Это повторяющаяся задача в формате: "21:00 каждый среда воскресенье"
                    try
                    {
                        // Парсим время
                        if (!TimeSpan.TryParse(firstLineParts[0], out TimeSpan timeOfDay))
                        {
                            SendMessage(message.Chat.Id, 2);
                            return;
                        }

                        // Собираем дни недели
                        List<DayOfWeek> daysOfWeek = new List<DayOfWeek>();
                        for (int i = 2; i < firstLineParts.Length; i++)
                        {
                            string dayName = firstLineParts[i].ToLower();
                            if (dayMapping.ContainsKey(dayName))
                            {
                                daysOfWeek.Add(dayMapping[dayName]);
                            }
                        }

                        if (daysOfWeek.Count == 0)
                        {
                            SendMessage(message.Chat.Id, 2);
                            return;
                        }

                        // Находим ближайший подходящий день
                        DateTime nextDate = FindNextOccurrence(timeOfDay, daysOfWeek);

                        // Сообщение без первой строки
                        string taskMessage = message.Text.Replace(Info[0] + "\n", "");

                        // Создаем задачу
                        User.Events.Add(new Events(
                            nextDate,
                            taskMessage,
                            isRecurring: true,
                            recurrenceDays: string.Join(",", daysOfWeek.Select(d => d.ToString())),
                            recurrenceTime: timeOfDay.ToString(@"hh\:mm")));

                        // Сохраняем в БД если нужно
                        using (var connect = new DBConnect())
                        {
                            connect.BDUseres.Add(new BDUsere(message.Chat.Id, $"Повторяющаяся: {taskMessage}"));
                            connect.SaveChanges();
                        }

                        await TelegramBotClient.SendMessage(
                            message.Chat.Id,
                            $"✅ Создано повторяющееся напоминание!\n" +
                            $"Следующий раз: {nextDate:HH:mm dd.MM.yyyy}\n" +
                            $"Повтор: каждый {string.Join(", ", daysOfWeek.Select(d => dayMapping.First(x => x.Value == d).Key))} в {timeOfDay:hh\\:mm}",
                            replyMarkup: GetButtons()
                        );
                        return;
                    }
                    catch
                    {
                        SendMessage(message.Chat.Id, 2);
                        return;
                    }
                }

                // ========== ОБЫЧНАЯ РАЗОВАЯ ЗАДАЧА ==========
                DateTime Time;
                if (CheckFormatDateTime(Info[0], out Time) == false)
                {
                    SendMessage(message.Chat.Id, 2);
                    return;
                }

                if (Time < DateTime.Now) SendMessage(message.Chat.Id, 3);

                User.Events.Add(new Events(
                    Time,
                    message.Text.Replace(Time.ToString("HH:mm dd.MM.yyyy") + "\n", "")));


                using (var connect = new DBConnect())
                {
                    connect.BDUseres.Add(new BDUsere(IdUser, MessageUser));
                    Console.WriteLine("Сохранено в БД");
                    connect.SaveChanges();
                }
            }
        }
        private DateTime FindNextOccurrence(TimeSpan timeOfDay, List<DayOfWeek> daysOfWeek)
        {
            DateTime now = DateTime.Now;
            DateTime todayWithTime = now.Date.Add(timeOfDay);

            // Проверяем сегодня
            if (daysOfWeek.Contains(now.DayOfWeek) && now < todayWithTime)
                return todayWithTime;

            // Ищем ближайший подходящий день
            for (int i = 1; i <= 7; i++)
            {
                DateTime nextDay = now.AddDays(i);
                if (daysOfWeek.Contains(nextDay.DayOfWeek))
                {
                    return nextDay.Date.Add(timeOfDay);
                }
            }

            return todayWithTime.AddDays(1); // На всякий случай
        }
        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
                GetMessages(update.Message);

            else if (update.Type == UpdateType.CallbackQuery)
            {
                CallbackQuery query = update.CallbackQuery;
                Users User = Users.Find(x => x.IdUser == query.Message.Chat.Id);
                Events Event = User.Events.Find(x => x.Message == query.Data);
                User.Events.Remove(Event);
                SendMessage(query.Message.Chat.Id, 4);
            }
        }
        private async Task HandleErrorAsync(
            ITelegramBotClient client,
            Exception exception,
            CancellationToken token)
        {
            Console.WriteLine("Ошибка: " + exception.Message);
        }
        public async void Tick(object obj)
        {
            DateTime now = DateTime.Now;
            string TimeNow = now.ToString("HH:mm dd.MM.yyyy");

            foreach (Users User in Users)
            {
                for (int i = User.Events.Count - 1; i >= 0; i--)
                {
                    if (User.Events[i].Time.ToString("HH:mm dd.MM.yyyy") != TimeNow)
                        continue;

                    // Отправляем напоминание
                    await TelegramBotClient.SendMessage(
                        User.IdUser,
                        "Напоминание: " + User.Events[i].Message
                    );

                    // ========== ОБРАБОТКА ПОВТОРЯЮЩЕЙСЯ ЗАДАЧИ ==========
                    if (User.Events[i].IsRecurring &&
                        !string.IsNullOrEmpty(User.Events[i].RecurrenceDays) &&
                        !string.IsNullOrEmpty(User.Events[i].RecurrenceTime))
                    {
                        try
                        {
                            // Парсим дни и время
                            var days = User.Events[i].RecurrenceDays
                                .Split(",")
                                .Select(d => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), d))
                                .ToList();

                            TimeSpan timeOfDay = TimeSpan.Parse(User.Events[i].RecurrenceTime);

                            // Находим следующую дату
                            DateTime nextDate = FindNextOccurrence(timeOfDay, days);

                            // Создаем новую задачу
                            User.Events.Add(new Events(
                                nextDate,
                                User.Events[i].Message,
                                isRecurring: true,
                                recurrenceDays: User.Events[i].RecurrenceDays,
                                recurrenceTime: User.Events[i].RecurrenceTime));

                            Console.WriteLine($"Создано следующее повторение: {nextDate:HH:mm dd.MM.yyyy}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка обработки повторяющейся задачи: {ex.Message}");
                        }
                    }

                    // Удаляем текущую задачу
                    User.Events.RemoveAt(i);
                }
            }
        }
    }
}