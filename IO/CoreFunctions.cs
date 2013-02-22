using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Jaguar.IO
{
    class CoreFunctions
    {
        public static void Write(string c, string s, bool error = false, bool fatal=false)
        {
            if (error)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[{0}.", c);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("] ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(s);
                Console.ForegroundColor = ConsoleColor.Gray;
                if (fatal)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("\nERROR IS FATAL. PRESS ENTER TO EXIT...");
                    Console.ReadLine();
                    Program.exit(0);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[{0}] ", c);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(s);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }

        public static int Base64Decode(string data)
        { 
            try 
            { 
                char[] val = data.ToCharArray(); 
                int intTot = 0; 
                int y = 0; 
                for (int x = (val.Length - 1); x >= 0; x--) 
                { 
                    int intTmp = (int)(byte)((val[x] - 64)); 
                    if (y > 0) 
                        intTmp = intTmp * (int)(Math.Pow(64, y)); 
                    intTot += intTmp; 
                    y++; 
                } 
                return intTot; 
            }     
            catch (Exception e) 
            {
                write("Invalid B64 header from packet. Packet ignored.", true);
                return -1;
            }
        }

        static void write(string s, bool error=false, bool fatal=false)
        {
            IO.CoreFunctions.Write("CoreFunctions", s, error, fatal);
        }

        public static bool IsConnected(Socket s)
        {
            try
            {
                return !(s.Poll(1, SelectMode.SelectRead) && s.Available == 0);
            }
            catch
            {
                return false;
            }
        }

        public static string[] GetPacketStrings(string packet, int argc)
        {
            string[] packetStrings = new string[argc];
            int currLoc = 0;
            for (int i = 0; i < argc; i++)
            {
                int strLen = Base64Decode(packet.Substring(currLoc, 2));
                currLoc += 2;
                packetStrings[i] = packet.Substring(currLoc, strLen);
                currLoc += strLen;
            }
            return packetStrings;
        }
    }
}
