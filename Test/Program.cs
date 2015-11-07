using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using easysql;
namespace Test
{
    public class UserModel
    {
        public int id { get; set; }
        public String username { get; set; }
        public String password { get; set; }
        public String test { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            //MySqlDatabase db = new MySqlDatabase("server=localhost;database=easyweb;Persist Security Info=False;uid=root;pwd=youotech;");
            //var dt = db.QueryDataTable("select * from user where id>{0}", 1);
            //db.Dispose();
            //Console.WriteLine(dt);

            //DateTime time1 = default(DateTime);
            //DateTime time2 = default(DateTime);

            //DHF.CreateSqlServerDBHelper(connString).QueryDataTable("select top 20 * from [user]");


            //Console.WriteLine("初始化完成");

            //time1 = DateTime.Now;

            //BaseDatabase db = DatabaseFactory.CreateSqlServerDatabase(connString);
            //for (var i = 0; i < 100; i++)
            //{
            //    db.QueryDataTable("select top 20 * from [user]");
            //}
            //db.Dispose();

            //time2 = DateTime.Now;
            //Console.WriteLine("耗时:" + (time2.Ticks - time1.Ticks) / 10000);


            //time1 = DateTime.Now;

            //BaseDBHelper dh = DHF.CreateSqlServerDBHelper(connString);
            //for (var i = 0; i < 100; i++)
            //{
            //    dh.QueryDataTable("select top 20 * from [user]");
            //}
            //time2 = DateTime.Now;
            //Console.WriteLine("耗时:" + (time2.Ticks - time1.Ticks) / 10000);


            //    BaseDatabase db = DatabaseFactory.CreateMySqlDatabase("server=localhost;database=easyweb;Persist Security Info=False;uid=root;pwd=youotech;");
            //    var list = db.Query<UserModel>("select * from user");
            String connString = "user id=sa;password=youotech;initial catalog=test;Server=139.129.20.1";

            BaseDBHelper dh = DHF.CreateSqlServerDBHelper(connString);

            //var list = dh.CreateDatabase().Query<UserModel>("select top 10 * from [user]");
            //      var list = dh.CreateDatabase().Query<UserModel>("tb_user", null, Restrain.Order("id"),Restrain.MaxResult(20));
            //  var dt  = dh.CreateDatabase().QueryDataTable(10, 20, "select distinct * from tb_user");

            var dt = dh.QueryDataTable(10, 200, "select * from tb_user where id>{0}", 20);

            Console.ReadKey();

        }
    }
}
