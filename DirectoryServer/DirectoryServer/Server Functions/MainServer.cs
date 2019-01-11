using System;
using System.Collections.Generic;
using System.Text;

namespace DirectoryServer
{
    class MainServer
    {
        //Public key stuff
        /*private static int g = 9;
        private static int n = 100;

        private static int a = 5;
        private int publicKey;
        private int privateKey;
        private int connectedKey;
        // g^a%n
        publicKey = (int) Math.Pow(g, a) % n;
            // Get connected key via network and calculate the private key to use for cipher
            privateKey = (int) Math.Pow(connectedKey, a) % n;

    */
        public MainServer()
        {

        }

        public List<string> processString(string v)
        {
            List<string> returnMessages = new List<string>();

            string[] strings = v.Split(' ');
            strings[0] = strings[0].ToUpper();

            switch (strings[0])
            {

                // Add key exchange stuff here
                // Public key mod6
                case "TEST":
                    returnMessages.Add("Test received");
                    break;
                case "SETFILE":
                    returnMessages.Add("Test received");
                    break;
                case "SETMODE":
                    returnMessages.Add("Test received");
                    break;
                case "GETCHUNKNUM":
                    returnMessages.Add("Test received");
                    break;
                case "GETCHUNKX":
                    returnMessages.Add("Test received");
                    break;
                case "GETCHUNKXHASH":
                    returnMessages.Add("Test received");
                    break;
                case "GETFILE":
                    returnMessages.Add("Test received");
                    break;
                case "GETHASH":
                    returnMessages.Add("Test received");
                    break;
                case "GETKEY":
                    returnMessages.Add(""/*publicKey.ToString() */);
                    break;
                default:
                    returnMessages.Add(v);
                    break;

            }
            return returnMessages;
        }
    }
}
