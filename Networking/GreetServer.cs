using System;
using System.Net;
using System.Net.Sockets;

namespace Jaguar.Networking
{
    class GreetServer
    {
        IPEndPoint serverEP;
        Socket greeter;

        public GreetServer(int port)
        {
            serverEP = new IPEndPoint(IPAddress.Any, port);
            try
            {
                greeter = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                greeter.Bind(serverEP);
                greeter.Listen(10);
                write("Successfully bound to port " + port + ".\n");
            }
            catch(Exception e)
            {
                write("FATAL: Unable to bind to port " + port + ".\n", true);
                Console.ReadLine();
                Program.exit(0);
            }
            try
            {
                greeter.BeginAccept(OnClientConnection, greeter);
                write("Now accepting client connections.\n");
            }
            catch (Exception e)
            {
                write("FATAL: Unable to begin client acceptence protocol.", true);
                Console.ReadLine();
                Program.exit(0);
            }
        }

        void write(string s, bool error = false, bool fatal = false)
        {
            Jaguar.IO.CoreFunctions.Write("GreetServer", s, error, fatal);
        }

        void OnClientConnection(IAsyncResult ar)
        {
            Socket new_client = ((Socket)ar.AsyncState).EndAccept(ar);
            write("Client connected from IP: " + ((IPEndPoint)new_client.RemoteEndPoint).Address + "\n");
            new Client.GuestUser(new_client);
            greeter.BeginAccept(OnClientConnection, greeter);
        }
    }
}
