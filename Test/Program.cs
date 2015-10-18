using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using easysql;
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            MySqlDatabase db = new MySqlDatabase("server=localhost;database=easyweb;Persist Security Info=False;uid=root;pwd=youotech;");
            var dt = db.QueryDataTable("select * from user where id>{0}", 1);
            db.Dispose();
            Console.WriteLine(dt);
        }
    }
}
