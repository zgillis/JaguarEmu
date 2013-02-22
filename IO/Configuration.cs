using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Jaguar.IO
{
    class Configuration
    {
        public int hotel_port;
        public string hotel_name;
        public string mysql_host;
        public string mysql_user;
        public string mysql_pass;
        public int mysql_port;
        public string mysql_database;
        public int new_user_credits;

        public Configuration()
        {
            string[] lines = null;
            try
            {
                lines = File.ReadAllLines(Directory.GetCurrentDirectory() + "/config.ini");
            }
            catch
            {
                write("Unable to open config.ini.", true, true);
            }
            try
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string temp = lines[i];
                    string[] keyvalstr = temp.Split('=');
                    if (keyvalstr.Length != 2)
                        continue;
                    string key = keyvalstr[0];
                    string value = keyvalstr[1];
                    switch (key)
                    {
                        case "hotel_port":
                            hotel_port = int.Parse(value);
                            break;
                        case "hotel_name":
                            hotel_name = value;
                            break;
                        case "mysql_host":
                            mysql_host = value;
                            break;
                        case "mysql_user":
                            mysql_user = value;
                            break;
                        case "mysql_pass":
                            mysql_pass = value;
                            break;
                        case "mysql_port":
                            mysql_port = int.Parse(value);
                            break;
                        case "mysql_database":
                            mysql_database = value;
                            break;
                        case "new_user_credits":
                            new_user_credits = int.Parse(value);
                            break;
                        default:
                            write("Ignored unrecognized config line: \"" + temp + "\"\n");
                            break;
                    }
                }
            }
            catch
            {
                write("Unable to load configuration. 'config.ini' contains invalid information.", true, true);
            }
        }

        void write(string s, bool error = false, bool fatal = false)
        {
            CoreFunctions.Write("Configuration", s, error, fatal);
        }
    }
}
