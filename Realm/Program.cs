using Realm.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realm
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Realm Server | ver. 0.1 beta alpha";
            Console.WriteLine("Realm Server started...");
            Console.WriteLine();

            ServerObject server = new ServerObject();
            Task serverTask = new Task(server.StartServer);
            serverTask.Start();
            serverTask.Wait();

            Console.WriteLine("Realm Server stoped!");
            Console.ReadKey();
        }
    }
}
