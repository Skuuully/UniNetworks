using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DirectoryServer
{
    class User
    {
        public string UserName;
        // Password is only used temporarily and is never stored
        public string Password;
        public string Salt;
        public string Hash;
        public Guid SessionGuid;
        Random rand;
        public List<User> AuthenticatedUsers= new List<User>();

        public User(string userName, string salt, string hash)
        {
            UserName = userName;
            Hash = hash;
            Salt = salt;
            rand = new Random();
        }

        public void SaveUserBase()
        {
            string path = "..\\users.txt";

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
                TextWriter tw = new StreamWriter(path);
                foreach (User user in AuthenticatedUsers)
                {
                    tw.WriteLine(user.UserName + '@' + user.Salt + "@" + user.Hash + '@');
                }
                tw.Close();
            }
            else if (File.Exists(path))
            {
                TextWriter tw = new StreamWriter(path, false);
                tw.WriteLine(AuthenticatedUsers.Capacity);
                foreach (User user in AuthenticatedUsers)
                {
                    tw.WriteLine(user.UserName + '@' + user.Salt + "@" + user.Hash + '@');
                }
                tw.Close();
            }
        }

        public void LoadUserBase()
        {
            string path = "..\\users.txt";
            if (File.Exists(path))
            {
                TextReader tr = new StreamReader(path);
                string line = "";
                string fileContents = "";
                while ((line = tr.ReadLine()) != null)
                {
                    fileContents = fileContents + line;
                }
                tr.Close();
                string[] strings;
                strings = fileContents.Split('@');
                for(int i = 0; i + 3<strings.Length; i = i + 3)
                {
                    User user = new User(strings[i], strings[i + 1], strings[i+2]);
                    AuthenticatedUsers.Add(user);
                }
            }
        }

        public void AddAuthUser(User user)
        {
            AuthenticatedUsers.Add(user);
        }

        // Generates a random string of length passed in
        public string GenerateSalt(int saltLength)
        {
            char[] potentialChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
            char[] salt = new char[saltLength];
            rand = new Random();

            // for however long specified pick a random char from potential char and build up salt array
            for(int i = 0; i < salt.Length; i++)
            {
                salt[i] = potentialChar[rand.Next(potentialChar.Length)];
            }

            string finalSalt = new string(salt);
            return finalSalt;
        }

        private static byte[] GetHash(string saltedPass)
        {
            HashAlgorithm md5 = MD5.Create();
            return md5.ComputeHash(Encoding.UTF8.GetBytes(saltedPass));
        }

        public string GetHashString(string saltedPass)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in GetHash(saltedPass))
                stringBuilder.Append(b.ToString("X2"));

            return stringBuilder.ToString();
        }
    }
}
