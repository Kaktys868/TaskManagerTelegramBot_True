using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerTelegramBot_True.Classes
{
    public class BDUsere
    {
        public int Id { get; set; }
        public long IdUser { get; set; }
        public string Message { get; set; }
        public BDUsere() { }
        public BDUsere(long idUser, string Message)
        {
            IdUser = idUser;
            this.Message = Message;
        }
    }
}
