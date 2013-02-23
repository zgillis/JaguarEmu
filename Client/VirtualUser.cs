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
        string username, figure, sex, mission, consolemission, figure2;

        public static List<VirtualUser> OnlineUsers = new List<VirtualUser>();

        public VirtualUser(Socket connection, int id)
        {
            socket = connection;
            userID = id;
            username = DBManager.GetUserFromID(id);
            figure = DBManager.GetUserFigure(id);
            sex = DBManager.GetUserGender(id);
            mission = DBManager.GetUserMission(id);
            consolemission = DBManager.GetUserConsoleMission(id);
            figure2 = "ch=s02/53,51,44";
            SendWelcome();
            if (CoreFunctions.IsConnected(socket))
                socket.BeginReceive(buffer, 0, 1024, SocketFlags.None, OnDataArrival, socket);
            OnlineUsers.Add(this);
        }

        void OnDataArrival(IAsyncResult ar)
        {
            int bytesReceived = socket.EndReceive(ar);
            string data = Encoding.UTF8.GetString(buffer);
            List<string> packets = new List<string>();
            while (data.ToCharArray()[0] != '\u0000')
            {
                int packetLen = CoreFunctions.Base64Decode(data.Substring(1, 2));
                string packet = data.Substring(1, 2+packetLen);
                packets.Add(packet);
                data = data.Substring(packetLen + 3);
            }
            foreach(string x in packets)
            {
                ProcessPacket(x);
            }
            buffer = new byte[1024];
            if (CoreFunctions.IsConnected(socket))
                socket.BeginReceive(buffer, 0, 1024, SocketFlags.None, OnDataArrival, socket);
            else
            {
                try { socket.Close(); } catch { }
                OnlineUsers.Remove(this);
            }
        }

        void ProcessPacket(string packet)
        {
            int packetLen = CoreFunctions.Base64Decode(packet.Substring(1, 2));
            string data = packet.Substring(2);
            write("RX: " + data + '\n');
            string header = data.Substring(0, 2);
            string pdata = data.Substring(2);
            int hid = CoreFunctions.Base64Decode(header);
            switch (hid)
            {
                case 7: // '@G' - Check if user is HC or not
                    int[] hcinfo = DBManager.GetUserHCSplit(userID);
                    send("@Gclub_habbo\u0002" + CoreFunctions.VL64Encode(hcinfo[0]) +
                         CoreFunctions.VL64Encode(hcinfo[1]) + 
                         CoreFunctions.VL64Encode(hcinfo[2]) +
                         "I\u0001");
                    break;
                case 8: // '@H' - Get credit balance
                    send("@F" + DBManager.GetUserCredits(userID) + ".0\u0001");
                    break;
                case 12: // '@L' - Get messenger mission and messages
                    send("@L" + consolemission + "\u0002\u0001");
                    break;
                case 157: // 'B]' - Get current badge
                    // NOT IMPLEMENTED
                    break;
                case 36: // '@d' - Update console mission
                    int mottoLen = CoreFunctions.Base64Decode(pdata.Substring(0,2));
                    string motto = pdata.Substring(2, mottoLen);
                    DBManager.UpdateConsoleMission(userID, motto);
                    break;
                case 41: // '@i' - Search for a user on in-game console
                    string[] searchArgs = CoreFunctions.GetPacketStrings(pdata, 2);
                    int uid = DBManager.GetUserID(searchArgs[0]);
                    if (uid == 0)
                        send("B@MESSENGER\u0002H\u0001");
                    else
                        send("B@MESSENGER\u0002" + CoreFunctions.VL64Encode(searchArgs[0].Length) + searchArgs[0] + "\u0002" + 
                             CoreFunctions.VL64Encode(DBManager.GetUserConsoleMission(uid).Length) + DBManager.GetUserConsoleMission(uid) + 
                             "\u0002H\u0002" + DBManager.GetUserCreationDate(uid) + DBManager.GetUserFigure(uid) + "\u0002\u0001");
                    break;
                case 44: // '@l' - Update look/mission/options
                    bool chg_motto=false, chg_figure=false, chg_gender=false;
                    string new_motto="", new_figure="", new_gender="";
                    try
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            string cipher = pdata.Substring(0, 2);
                            if (cipher == "@D")
                            {
                                int figlen = CoreFunctions.Base64Decode(pdata.Substring(2, 2));
                                new_figure = pdata.Substring(4, figlen);
                                chg_figure = true;
                                pdata = pdata.Substring(4 + figlen);
                            }
                            else if (cipher == "@E")
                            {
                                new_gender = pdata.Substring(4, 1);
                                chg_gender = true;
                                pdata = pdata.Substring(5);
                            }
                            else if (cipher == "@F")
                            {
                                int mottolen = CoreFunctions.Base64Decode(pdata.Substring(2, 2));
                                new_motto = pdata.Substring(4, mottolen);
                                chg_motto = true;
                                pdata = pdata.Substring(4 + mottolen);
                            }
                        }
                    }
                    catch { }
                    if(chg_motto)
                        DBManager.UpdateMission(userID, new_motto);
                    if(chg_gender)
                        DBManager.UpdateGender(userID, new_gender);
                    if (chg_figure)
                        DBManager.UpdateFigure(userID, new_figure);
                    send("@E\u0002" + username + "\u0002" + new_figure + "\u0002" + new_gender + "\u0002" +
                         new_motto + "\u0002H" + figure2 + "\u0002HI\u0001");
                    break;
                default:
                    break;
            }
        }

        public void send(string s)
        {
            if (CoreFunctions.IsConnected(socket))
            {
                socket.Send(Encoding.ASCII.GetBytes(s));
                write("TX: " + s + '\n');
            }
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
            send("@E\u0002" + username + "\u0002" + figure + "\u0002" + sex + "\u0002" + mission + "\u0002Re" + figure2 + "\u0002Y1G\u0001");
        }
    }
}
