using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryServer
{
    class Authenticate
    {
        public Authenticate()
        {

        }

        public User currentUser = new User("", "", "");

        public List<string> ProcessString(string v)
        {
            List<string> returnMessages = new List<string>();
            // Remove info about what type of server it is
            string[] strings = v.Split(' ');

            strings[0] = strings[0].ToUpper();
            currentUser.LoadUserBase();

            switch (strings[0])
            {
                case "COMMANDS":
                    returnMessages.Add("USERNAME user to set current username\n PASSWORD password to set current password\n" +
                        "CREATEUSER to create a user based on the current username and password\nVALIDATE to validate against all authenticated users\nQUIT to go back to main server mode");
                    break;

                // Set current user for later
                case "USERNAME":
                    currentUser.UserName = strings[1];
                    returnMessages.Add("Current username: " + currentUser.UserName);
                    Console.WriteLine("Current username set to: " + currentUser.UserName);
                    break;

                    //Set current password for later
                case "PASSWORD":
                    currentUser.Password = strings[1];
                    // Generate a salt of the user and the hash
                    currentUser.Salt = currentUser.GenerateSalt(8);
                    currentUser.Hash = currentUser.GetHashString(currentUser.Password + currentUser.Salt);
                    returnMessages.Add("Current password: " + currentUser.Password);
                    Console.WriteLine("Current password set to: " + currentUser.Password);
                    break;

                /*case "PASSWORD":
                    if(currentUser.UserName != "")
                    {
                        string password = strings[1];

                        string salt = currentUser.Salt;
                        string hash = currentUser.Hash;

                        if(hash == currentUser.GetHashString(currentUser.Password + salt))
                        {

                        }
                    } */
                    //break;

                    //Checks if the user is in the authenticated user list
                case "VALIDATE":
                    returnMessages.Add(currentUser.UserName + currentUser.Password);
                    if (currentUser.UserName != "" && currentUser.Password != "")
                    {
                        foreach (User user in currentUser.AuthenticatedUsers)
                        {
                            if (user.UserName == currentUser.UserName)
                            {
                                if (user.Hash == currentUser.Hash)
                                {
                                    Guid g = Guid.NewGuid();
                                    user.SessionGuid = g;
                                    returnMessages.Add("welcome back: " + currentUser.UserName);
                                    returnMessages.Add("SESSION " + user.SessionGuid);
                                }
                            }
                        }
                    }
                    else
                    {
                        returnMessages.Add("You are missing a username or password");
                    }
                    break;

                    // Add the current user to the list
                case "CREATEUSER":
                    if (currentUser.UserName != "" && currentUser.Password != "")
                    {
                        if(!currentUser.AuthenticatedUsers.Contains(currentUser))
                        {
                            Guid g = new Guid();
                            currentUser.SessionGuid = g;
                            currentUser.AddAuthUser(currentUser);

                            returnMessages.Add("welcome back + " + currentUser.UserName);
                            returnMessages.Add("SESSION " + currentUser.SessionGuid);

                            currentUser.SaveUserBase();
                        }
                        else
                        {
                            returnMessages.Add("That user is currently already signed up\nTry a different username or password");
                        }
                    }
                    break;

                case "TOKEN":
                    if(currentUser.AuthenticatedUsers.Contains(currentUser))
                    {
                        string token = currentUser.GenerateSalt(10);
                        returnMessages.Add(token);
                    }
                    break;

                case "VALIDATEENCRYPTION":
                    // if valid set of keys + cipher
                    // set connmanger.EncryptionNeogtioated = true;
                    // tell encrypter / connmanger what to use.
                    break;

                case "QUIT":
                    returnMessages = null;
                    break;

                    // When the server first swaps into authenticate return the commands
                case "AUTHENTICATE":
                    returnMessages.Add("USERNAME user to set current username\n PASSWORD password to set current password\n" +
                        "CREATEUSER to create a user based on the current username and password\nVALIDATE to validate against all authenticated users\nQUIT to go back to main server mode");
                    break;

                default:
                    returnMessages.Add("Not a recognised command, send COMMANDS to get a list of commands");
                    break;

            }
            return returnMessages;
        }
    }
}
