using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using Jaguar.IO;
using Jaguar.Database;

namespace Jaguar.Client
{
    class GuestUser
    {
        Socket socket;
        byte[] buffer = new byte[1024];
        bool loggedIn = false;

        public GuestUser(Socket sck)
        {
            socket = sck;
            send("@@\u0001");
            if(CoreFunctions.IsConnected(socket))
                socket.BeginReceive(buffer, 0, 1024, SocketFlags.None, OnDataArrival, socket);
        }

        void OnDataArrival(IAsyncResult ar)
        {
            int bytesReceived = socket.EndReceive(ar);
            string packet = Encoding.UTF8.GetString(buffer);
            int packetLen = CoreFunctions.Base64Decode(packet.Substring(1, 2));
            string data = packet.Substring(3, packetLen);
            write("RX: " + data + '\n');
            string header = data.Substring(0, 2);
            string pdata = data.Substring(2);
            int hid = CoreFunctions.Base64Decode(header);
            switch (hid)
            {
                case 206: // 'CN'
                    send("DUIH\u0001");
                    break;
                case 202: // 'CJ'
                    send("DARAHIIIKHJIPAIQAdd-MM-yyyy\u0002\u0001");
                    string tpck = "@H[";
                    for (int i = 100; i < 1000; i++ )
                    {
                        tpck += (i.ToString() + ",");
                    }
                    tpck += "1000]\u0001";
                    send(tpck);
                    break;
                case 49: // '@q'
                    DateTime dt = DateTime.Now;
                    string day = dt.Day.ToString("00");
                    string month = dt.Month.ToString("00");
                    string year = dt.Year.ToString("0000");
                    send("Bc" + day + "-" + month + "-" + year + "\u0001");
                    break;
                case 4: // '@D' USER LOGIN PACKET
                    string[] logindetails = CoreFunctions.GetPacketStrings(pdata, 2);
                    if (DBManager.VerifyLogin(logindetails[0], logindetails[1]))
                    {
                        loggedIn = true;
                        VirtualUser vs = new VirtualUser(socket, DBManager.GetUserID(logindetails[0]));
                    }
                    else
                        send("@aLogin Incorrect: Invalid username/password combination.\u0001");
                    break;
                case 131: // 'BC'
                    send("BKYou must be older to play.\u0001");
                    break;
                case 42: // '@j'
                    if (DBManager.UserExists(CoreFunctions.GetPacketStrings(pdata, 1)[0]))
                        send("@dPA\u0001");
                    else
                        send("@dH\u0001");
                    break;
                case 203: // 'CK'
                    send("DZH\u0001");
                    break;
                case 197: // 'CE'
                    send("DO\u0001");
                    break;
                case 146: // 'BR'
                    send("CY1\u0001");
                    break;
                case 46: // '@n'
                    send("DO\u0001");
                    break;
                case 43: // '@k' REGISTRATION PACKET
                    RegisterUser(pdata);
                    break;
                default:
                    break;
            }
            if(CoreFunctions.IsConnected(socket) && !loggedIn)
                socket.BeginReceive(buffer, 0, 1024, SocketFlags.None, OnDataArrival, socket);
        }

        void RegisterUser(string pdata)
        {
            string username, figure, mission, password, birthdate, email;
            pdata = pdata.Substring(2);
            int unlen = CoreFunctions.Base64Decode(pdata.Substring(0, 2));
            username = pdata.Substring(2, unlen);
            pdata = pdata.Substring(4 + unlen);
            int figlen = CoreFunctions.Base64Decode(pdata.Substring(0, 2));
            figure = pdata.Substring(2, figlen);
            string gender = pdata.Substring(figlen + 6, 1);
            pdata = pdata.Substring(9 + figlen);
            int missionlen = CoreFunctions.Base64Decode(pdata.Substring(0, 2));
            mission = pdata.Substring(2, missionlen);
            pdata = pdata.Substring(4 + missionlen);
            int emaillen = CoreFunctions.Base64Decode(pdata.Substring(0, 2));
            email = pdata.Substring(2, emaillen);
            pdata = pdata.Substring(4 + emaillen);
            int birthlen = CoreFunctions.Base64Decode(pdata.Substring(0, 2));
            birthdate = pdata.Substring(2, birthlen);
            pdata = pdata.Substring(13 + birthlen);
            int passlen = CoreFunctions.Base64Decode(pdata.Substring(0, 2));
            password = pdata.Substring(2, passlen);
            if (DBManager.UserExists(username))
            {
                send("BKThe username \"" + username + "\" is already in use.");
            }
            string[] uservars = { username, figure, mission, password, birthdate, email, gender };
            if (!DBManager.CreateUser(uservars))
                send("BKUser registration encountered an error.\u0001");
        }

        void send(string s)
        {
            if (CoreFunctions.IsConnected(socket))
            {
                socket.Send(Encoding.ASCII.GetBytes(s));
                write("TX: " + s + '\n');
            }
        }

        void write(string s, bool error = false, bool fatal = false)
        {
            CoreFunctions.Write("GuestUser", s, error, fatal);
        }
    }
}
