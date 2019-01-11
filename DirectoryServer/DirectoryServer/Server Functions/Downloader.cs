using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DirectoryServer
{
    class Downloader
    {
        public Downloader()
        {

        }

        public List<string> processString(string v)
        {
            List<string> returnMessages = new List<string>();

            string[] strings = v.Split(' ');
            strings[0] = strings[0].ToUpper();
            string path;

            switch (strings[0])
            {
                case "GETFILE":
                    // must specify a filename to work
                    if(strings.Length > 1)
                    {
                        path = "Txts\\" + strings[1] + ".txt";
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
                            returnMessages.Add("FILE " + strings[1]+ " " + fileContents);
                        }
                        else
                        {
                            returnMessages.Add(strings[1] + " does not exist");
                        }
                    }
                    else
                    {
                        returnMessages.Add("No file specified");
                    }
                    break;
                case "DIRECTORY":
                    path = "Txts\\";
                    DirectoryInfo directory = new DirectoryInfo(path);
                    FileInfo[] files = directory.GetFiles("*.txt");
                    for (int i = 0; i < files.Length; i++)
                    {
                        returnMessages.Add(files[i] + "\n");
                    }
                    break;

                case "QUIT":
                    returnMessages = null;
                    break;

                    // returns list of commands when it enters download mode
                case "DOWNLOAD":
                    returnMessages.Add("GETFILE FILENAME to get a certain file\nDIRECTORY to get a list of files that you can download");
                    break;

                case "COMMANDS":
                    returnMessages.Add("GETFILE FILENAME to get a certain file\nDIRECTORY to get a list of files that you can download");
                    break;
                default:
                    returnMessages.Add("Command not recognised");
                    break;

            }
            return returnMessages;
        }
    }
}
