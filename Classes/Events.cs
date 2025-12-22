using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerTelegramBot_True.Classes
{
    public class Events
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }

        // Поля для повторяющихся задач
        public bool IsRecurring { get; set; }
        public string RecurrenceDays { get; set; } = ""; // "Monday,Wednesday,Sunday"
        public string RecurrenceTime { get; set; } = ""; // "21:00"

        // Конструктор для обычных задач
        public Events(DateTime time, string message)
        {
            Time = time;
            Message = message;
            IsRecurring = false;
        }

        // Конструктор для повторяющихся задач
        public Events(DateTime time, string message, bool isRecurring, string recurrenceDays, string recurrenceTime)
        {
            Time = time;
            Message = message;
            IsRecurring = isRecurring;
            RecurrenceDays = recurrenceDays;
            RecurrenceTime = recurrenceTime;
        }
    }
}