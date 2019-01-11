using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Encryption
    {
        // Add to every time a new encryption type is supported
        public enum EncryptionTypes { CAESAR, ROT13};
        // Methods to encrypt and decerypt with caesar cipher
        // Returns a string based on original string shifted by a key
        // must only alter letters
        public static string CaesarEncrypt(int key, string toEncrypt)
        {
            if (key > 25 || key < 0)
            {
                key = key % 25;
            }
            char[] originalString = toEncrypt.ToCharArray();
            char[] encryptedString = originalString;
            for(int i = 0; i < originalString.Length; i++)
            {
                // add key to all values when decrypted does not matter what symbol it has become should return to same
                encryptedString[i] = (char)(originalString[i] + key);
            }
            // Old method which would keep some symbols the same

            /*for (int i = 0; i < originalString.Length; i++)
            {
                if(originalString[i] != 64 && originalString[i] != 32)
                {
                    if(originalString[i] >= 'A' && originalString[i] <= 'Z')
                    {
                        encryptedString[i] = (char)(originalString[i] + key);
                    }
                    else if(originalString[i] >= 'a' && originalString[i] <= 'z')
                    {
                        encryptedString[i] = (char)(originalString[i] + key);
                    }
                }
                else
                {
                    // if a ' ' or '@' stay same
                    encryptedString[i] = originalString[i];
                }
            } */
            string complete = new string(encryptedString);
            return complete;
        }

        public static string CaesarDecrypt(int key, string toDecrypt)
        {
            return CaesarEncrypt(-key, toDecrypt);
        }


        // ROT-13 can be treated as a caesar cipher with a set key of 13
        // COULD potentially encrypt DH exchange with ROT13 for some form of security?
        public static string ROT13Encrypt(string toEncrypt)
        {
            return CaesarEncrypt(13, toEncrypt);
        }

        public static string ROT13Decrypt(string toDecrypt)
        {
            return CaesarDecrypt(13, toDecrypt);
        }
    }
}
