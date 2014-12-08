using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace MusicStream
{
    public class config
    {
        private static string path = Path.Combine(Environment.GetFolderPath(
    Environment.SpecialFolder.ApplicationData), @"DuckyStreaming.csv");
        public config()
        {
            // make sure file exits
            if (!File.Exists(path))
            {
                try
                {
                    File.CreateText(path).Close();
                }
                catch(NullReferenceException)
                {
                    System.Windows.MessageBox.Show("Can't create the config File");
                }
            }
        }

        private string[] seperators = { "::" };

        public List<ConfigEntry> GetEntrys()
        {
            StreamReader reader = File.OpenText(path);
            string line = reader.ReadLine();
            int row = 0;
            List<ConfigEntry> domains = new List<ConfigEntry>();
            while (line != null && !string.IsNullOrWhiteSpace(line))
            {
                string[] array = line.Split(seperators, 3, StringSplitOptions.None);
                if (!string.IsNullOrWhiteSpace(array[0]) && !string.IsNullOrWhiteSpace(array[1]) && !string.IsNullOrWhiteSpace(array[2]))
                {
                    domains.Add(new ConfigEntry(array[0], array[1], array[2]));//Encryption.Decrypt(array[2], key)));
                }
                line = reader.ReadLine();
                row++;
            }
            reader.Dispose();
            reader.Close();
            reader = null;
            return domains;
        }

        /// <summary>
        /// override the old config
        /// </summary>
        /// <param name="Entrys">the entrys</param>
        public void Save(List<ConfigEntry> Entrys)
        {
            File.Delete(path);
            StreamWriter writer = File.CreateText(path);
            foreach(ConfigEntry entry in Entrys)
            {
                string cpas = entry.Password;//Encryption.Encrypt(entry.Password, key);
                string line = entry.Domain + seperators[0] + entry.User + seperators[0] + cpas;
                writer.WriteLine(line);
            }
            writer.Close();
            writer.Dispose();
        }

        public Dictionary<int,string> GetDomains()
        {
            StreamReader reader = File.OpenText(path);
            string line = reader.ReadLine();
            int row = 0;
            Dictionary<int, string> domains = new Dictionary<int, string>();
            while(line != null && !string.IsNullOrWhiteSpace(line))
            {
                string[] array = line.Split(seperators, 3, StringSplitOptions.None);
                if(!string.IsNullOrWhiteSpace(array[0]))
                {
                    domains.Add(row, array[0]);
                }
                line = reader.ReadLine();
                row++;
            }
            reader.Dispose();
            reader.Close();
            reader = null;
            return domains;
        }

        public string GetUsername(int row)
        {
            if (row < 0) return null;
            StreamReader reader = File.OpenText(path);
            // skip other lines
            for (int i = 1; i < row; i++) reader.ReadLine();
            string line = reader.ReadLine();
            if (line == null || string.IsNullOrWhiteSpace(line)) return null;
            string[] array = line.Split(seperators, 3, StringSplitOptions.None);
            reader.Dispose();
            reader.Close();
            reader = null;
            return array[1];
        }

        public string GetPassword(int row)
        {
            if (row < 0) return null;
            StreamReader reader = File.OpenText(path);
            // skip other lines
            for (int i = 1; i < row; i++) reader.ReadLine();
            string line = reader.ReadLine();
            if (line == null || string.IsNullOrWhiteSpace(line)) return null;
            string[] array = line.Split(seperators, 3, StringSplitOptions.None);
            reader.Dispose();
            reader.Close();
            reader = null;
            string rawpasswd = array[2];
            return rawpasswd;//Encryption.Decrypt(rawpasswd, key);
        }

         public void NewEntry(ConfigEntry newEntry)
        {
            NewEntry(newEntry.Domain, newEntry.User, newEntry.Password);
        }

        public void NewEntry(string Domain, string User, string Password)
        {
            if(Domain != null && User != null && Password != null && !string.IsNullOrWhiteSpace(Domain) && !string.IsNullOrWhiteSpace(User) && !string.IsNullOrWhiteSpace(Password))
            {
                string rawpasswd = Password;// Encryption.Encrypt(Password, key);
                // create new entry
                string line = Domain + seperators[0] + User + seperators[0] + rawpasswd;
                StreamWriter writer = File.CreateText(path);
                writer.WriteLine(line);
                writer.Close();
                writer.Dispose();
                writer = null;
            }
        }

        public void SetDomain(int row, string Domain)
        {

        }

        public void SetUsername(int row, string Domain)
        {

        }

        public void SetPassword(int row, string Domain)
        {

        }
       

        private static string key = Environment.UserName;
    }

    public class ConfigEntry
    {
        public ConfigEntry(string Domain, string User, string Password)
        {
            this.Domain = Domain;
            this.User = User;
            this.Password = Password;
        }

        public string Domain {get;set;}
        public string User {get;set;}
        public string Password {get;set;}
    }
}
