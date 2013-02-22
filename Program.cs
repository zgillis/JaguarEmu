using System;
using Jaguar.IO;
using Jaguar.Networking;
using Jaguar.Database;

namespace Jaguar
{
    class Program
    {
        const float VERSION = 0.1f;
        public static Configuration config;
        public static DBManager database;
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Clear();
            Console.Write("JAGUAR EMULATOR\nBETA VERSION {0}\nCODED BY ZGILLIS\n\n", VERSION);
            config = new Configuration();
            Console.WriteLine("Starting server for {0}...\n", config.hotel_name);
            database = new DBManager(config.mysql_host,
                                     config.mysql_port,
                                     config.mysql_user,
                                     config.mysql_pass,
                                     config.mysql_database);
            new GreetServer(config.hotel_port);
            while (true) ;
        }

        public static void exit(int ec)
        {
            try
            {
                if (database.connection != null)
                    database.connection.Close();
            }
            catch { }
            Environment.Exit(ec);
        }
    }
}
