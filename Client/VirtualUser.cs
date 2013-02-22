using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Jaguar.IO;
using Jaguar.Database;
using Jaguar.Client;
using Jaguar.Networking;

namespace Jaguar.Client
{
    class VirtualUser
    {
        Socket socket;
        int userID;
        byte[] buffer = new byte[1024];

        public VirtualUser(Socket connection, int id)
        {
            socket = connection;
            userID = id;
            SendWelcome();
            if (CoreFunctions.IsConnected(socket))
                socket.BeginReceive(buffer, 0, 1024, SocketFlags.None, OnDataArrival, socket);
        }

        void OnDataArrival(IAsyncResult ar)
        {
            int bytesReceived = socket.EndReceive(ar);
            string packet = Encoding.UTF8.GetString(buffer);
            int packetLen = CoreFunctions.Base64Decode(packet.Substring(1, 2));
            string data = packet.Substring(3, packetLen);
            write("REC: " + data + '\n');
            string header = data.Substring(0, 2);
            string pdata = data.Substring(2);
            int hid = CoreFunctions.Base64Decode(header);
            switch (hid)
            {

            }
            if (CoreFunctions.IsConnected(socket))
                socket.BeginReceive(buffer, 0, 1024, SocketFlags.None, OnDataArrival, socket);
        }

        void send(string s)
        {
            if (CoreFunctions.IsConnected(socket))
                socket.Send(Encoding.ASCII.GetBytes(s));
        }

        void write(string s, bool error = false, bool fatal = false)
        {
            CoreFunctions.Write("VirtualUser", s, error, fatal);
        }

        void SendWelcome()
        {
            string tpck = "@H[";
            for (int i = 100; i < 1000; i++)
            {
                tpck += (i.ToString() + ",");
            }
            tpck += "1000]\u0001";
            send(tpck);
            string fuseRights = DBManager.GetUserFuseRights(userID);
            send("@B" + fuseRights + "\u0001@DH\u0001@C\u0001");
            send("@L" + DBManager.GetUserConsoleMission(userID) + "\u0002\u0001");
            send("@r\u0001");
            send("@E\u0002" +
                 DBManager.GetUserFromID(userID) + '\u0002' +
                 DBManager.GetUserFigure(userID) + '\u0002' +
                 DBManager.GetUserGender(userID) + '\u0002' +
                 DBManager.GetUserMission(userID) + '\u0002' +
                 "Rech=s02/53,51,44\u0002Y1G\u0001");
            send("@F" + DBManager.GetUserCredits(userID) + ".0\u0001");
        }
    }
}
