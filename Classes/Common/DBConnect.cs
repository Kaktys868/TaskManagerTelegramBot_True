using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerTelegramBot_True.Classes.Common
{
    public class DBConnect : DbContext
    {
        public DbSet<BDUsere> BDUseres { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                    "server=127.0.0.1;database=pr9;user=root;port=3307",
                    new MySqlServerVersion(new Version(8, 0, 11)));
        }
    }
}
