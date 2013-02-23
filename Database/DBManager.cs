using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Jaguar.IO;

namespace Jaguar.Database
{
    class DBManager
    {
        public MySqlConnection connection = null;
        public DBManager(string host, int port, string user, string pass, string database)
        {
            try
            {
                connection = new MySqlConnection(string.Format("server={0};port={1};userid={2};password={3};database={4}",
                                                           host,
                                                           port,
                                                           user,
                                                           pass,
                                                           database));
                connection.Open();
                write("Successfully connected to MySQL server at " + host + ".\n");
                write("MySQL Server Version: " + connection.ServerVersion + '\n');
            }
            catch(Exception e)
            {
                write(e.Message, true, true);
            }
        }

        static void write(string s, bool error = false, bool fatal = false)
        {
            CoreFunctions.Write("DBManager", s, error, fatal);
        }

        public static bool UserExists(string user)
        {
            bool userExists = false;
            MySqlCommand userQuery = new MySqlCommand("SELECT * FROM Users", Program.database.connection);
            MySqlDataReader reader = userQuery.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(1).Equals(user))
                    userExists = true;
            }
            reader.Close();
            return userExists;
        }

        public static bool CreateUser(string[] rd)
        {
            MySqlCommand createCommand = new MySqlCommand();
            createCommand.Connection = Program.database.connection;
            createCommand.CommandText = "INSERT INTO Users VALUES(0, @username, @password, @birthdate, @consolemission, @mission, @credits, @email, @figure, 1, @gender, 31, @creation)";
            createCommand.Parameters.AddWithValue("@username", rd[0]);
            createCommand.Parameters.AddWithValue("@password", rd[3]);
            createCommand.Parameters.AddWithValue("@birthdate", rd[4]);
            createCommand.Parameters.AddWithValue("@consolemission", Program.config.hotel_name);
            createCommand.Parameters.AddWithValue("@mission", Program.config.hotel_name);
            createCommand.Parameters.AddWithValue("@credits", Program.config.new_user_credits);
            createCommand.Parameters.AddWithValue("@email", rd[5]);
            createCommand.Parameters.AddWithValue("@figure", rd[1]);
            createCommand.Parameters.AddWithValue("@gender", rd[6]);
            DateTime now = DateTime.Now;
            string dateTime = string.Format("{0}-{1}-{2} {3}:{4}", now.Day.ToString("00"),
                   now.Month.ToString("00"), now.Year.ToString("0000"), now.Hour.ToString("00"), now.Minute.ToString("00"));
            createCommand.Parameters.AddWithValue("@creation", dateTime);
            try
            {
                createCommand.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                write("Registration Error: " + e.Message, true);
                return false;
            }
        }

        public static bool VerifyLogin(string username, string password)
        {
            MySqlCommand loginCheck = new MySqlCommand();
            loginCheck.CommandText = "SELECT * FROM Users WHERE username = @username AND password = @password";
            loginCheck.Connection = Program.database.connection;
            loginCheck.Parameters.AddWithValue("@username", username);
            loginCheck.Parameters.AddWithValue("@password", password);
            MySqlDataReader msdr = loginCheck.ExecuteReader();
            if (msdr.Read())
            {
                msdr.Close();
                return true;
            }
            else
            {
                msdr.Close();
                return false;
            }
        }

        public static int GetUserID(string username)
        {
            MySqlCommand idGetter = new MySqlCommand("SELECT ID FROM Users WHERE username = @username", Program.database.connection);
            idGetter.Parameters.AddWithValue("@username", username);
            MySqlDataReader dr = idGetter.ExecuteReader();
            int id = 0;
            if (dr.Read())
            {
                id = dr.GetInt32(0);
            }
            dr.Close();
            return id;
        }

        public static int GetUserCredits(int id)
        {
            MySqlCommand idGetter = new MySqlCommand("SELECT credits FROM Users WHERE id = @id", Program.database.connection);
            idGetter.Parameters.AddWithValue("@id", id);
            MySqlDataReader dr = idGetter.ExecuteReader();
            int credits = 0;
            if (dr.Read())
            {
                credits = dr.GetInt32(0);
            }
            dr.Close();
            return credits;
        }

        public static string GetUserMission(int id)
        {
            MySqlCommand missionGetter = new MySqlCommand("SELECT mission FROM Users WHERE ID = @id", Program.database.connection);
            missionGetter.Parameters.AddWithValue("@id", id);
            MySqlDataReader dr = missionGetter.ExecuteReader();
            string mission = "";
            if (dr.Read())
            {
                mission = dr.GetString(0);
            }
            dr.Close();
            return mission;
        }

        public static string GetUserConsoleMission(int id)
        {
            MySqlCommand missionGetter = new MySqlCommand("SELECT consolemission FROM Users WHERE ID = @id", Program.database.connection);
            missionGetter.Parameters.AddWithValue("@id", id);
            MySqlDataReader dr = missionGetter.ExecuteReader();
            string mission = "";
            if (dr.Read())
            {
                mission = dr.GetString(0);
            }
            dr.Close();
            return mission;
        }

        public static string GetUserFuseRights(int id)
        {
            int rank = 1;
            string rightsPacket = "";
            MySqlCommand getRank = new MySqlCommand("SELECT rank FROM Users WHERE ID = " + id, Program.database.connection);
            MySqlDataReader dr1 = getRank.ExecuteReader();
            if (dr1.Read())
            {
                rank = dr1.GetInt32(0);
            }
            dr1.Close();
            MySqlCommand getFuse = new MySqlCommand("SELECT fuseright FROM fuserights WHERE minrank <= " + rank, Program.database.connection);
            MySqlDataReader dr2 = getFuse.ExecuteReader();
            while (dr2.Read())
            {
                rightsPacket += dr2.GetString(0) + '\u0002';
            }
            rightsPacket = rightsPacket.Substring(0, rightsPacket.Length - 1);
            dr2.Close();
            return rightsPacket;
        }

        public static string GetUserFromID(int id)
        {
            MySqlCommand unGetter = new MySqlCommand("SELECT username FROM Users WHERE ID = @id", Program.database.connection);
            unGetter.Parameters.AddWithValue("@id", id);
            MySqlDataReader dr = unGetter.ExecuteReader();
            string un = "";
            if (dr.Read())
            {
                un = dr.GetString(0);
            }
            dr.Close();
            return un;
        }

        public static string GetUserFigure(int id)
        {
            MySqlCommand figGetter = new MySqlCommand("SELECT figure FROM Users WHERE ID = @id", Program.database.connection);
            figGetter.Parameters.AddWithValue("@id", id);
            MySqlDataReader dr = figGetter.ExecuteReader();
            string fig = "";
            if (dr.Read())
            {
                fig = dr.GetString(0);
            }
            dr.Close();
            return fig;
        }

        public static string GetUserGender(int id)
        {
            MySqlCommand figGetter = new MySqlCommand("SELECT gender FROM Users WHERE ID = @id", Program.database.connection);
            figGetter.Parameters.AddWithValue("@id", id);
            MySqlDataReader dr = figGetter.ExecuteReader();
            string fig = "";
            if (dr.Read())
            {
                fig = dr.GetString(0);
            }
            dr.Close();
            return fig;
        }

        public static string GetUserCreationDate(int id)
        {
            MySqlCommand figGetter = new MySqlCommand("SELECT creation FROM Users WHERE ID = @id", Program.database.connection);
            figGetter.Parameters.AddWithValue("@id", id);
            MySqlDataReader dr = figGetter.ExecuteReader();
            string fig = "";
            if (dr.Read())
            {
                fig = dr.GetString(0);
            }
            dr.Close();
            return fig;
        }

        public static int GetUserHCDays(int id)
        {
            MySqlCommand idGetter = new MySqlCommand("SELECT hcdays FROM Users WHERE id = @id", Program.database.connection);
            idGetter.Parameters.AddWithValue("@id", id);
            MySqlDataReader dr = idGetter.ExecuteReader();
            int hcdays = 0;
            if (dr.Read())
            {
                hcdays = dr.GetInt32(0);
            }
            dr.Close();
            return hcdays;
        }

        public static int[] GetUserHCSplit(int id)
        {
            int hcdays = GetUserHCDays(id);
            int months = hcdays / 31;
            int days = hcdays % 31;
            int[] credentials = { days, 0, months };
            return credentials;
        }

        public static void UpdateConsoleMission(int id, string motto)
        {
            MySqlCommand missChange = new MySqlCommand("UPDATE Users SET consolemission = @conmis WHERE ID = @id", Program.database.connection);
            missChange.Parameters.AddWithValue("@conmis", motto);
            missChange.Parameters.AddWithValue("@id", id);
            missChange.ExecuteNonQuery();
        }

        public static void UpdateMission(int id, string motto)
        {
            MySqlCommand missChange = new MySqlCommand("UPDATE Users SET mission = @mis WHERE ID = @id", Program.database.connection);
            missChange.Parameters.AddWithValue("@mis", motto);
            missChange.Parameters.AddWithValue("@id", id);
            missChange.ExecuteNonQuery();
        }

        public static void UpdateGender(int id, string gender)
        {
            MySqlCommand missChange = new MySqlCommand("UPDATE Users SET gender = @gender WHERE ID = @id", Program.database.connection);
            missChange.Parameters.AddWithValue("@gender", gender);
            missChange.Parameters.AddWithValue("@id", id);
            missChange.ExecuteNonQuery();
        }

        public static void UpdateFigure(int id, string figure)
        {
            MySqlCommand missChange = new MySqlCommand("UPDATE Users SET figure = @figure WHERE ID = @id", Program.database.connection);
            missChange.Parameters.AddWithValue("@figure", figure);
            missChange.Parameters.AddWithValue("@id", id);
            missChange.ExecuteNonQuery();
        }
    }
}
