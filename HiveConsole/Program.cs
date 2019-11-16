using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpochHive;

namespace HiveConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.Connect(new Config() { Host = "localhost", User = "root", Password = "", Schema = "epoch107testdb" });
            //var r = Database.StoreVehicle("1","Goose","Lada","lada","1","[]","[]","1","0","","","12345","123","0");
            var r = Database.GetVehicleForSpawn("4", "[1,2,3]", "123");
            if (r.Success)
                Console.WriteLine(r.Result);
            else
                Console.WriteLine(r.Exception);
        }
    }
}
