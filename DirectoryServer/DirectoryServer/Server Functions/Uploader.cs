using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DirectoryServer
{
    class Uploader
    {
        // handels uploading of a file
        public Uploader()
        {

        }

        public List<string> ProcessString(string v)
        {
            List<string> returnMessages = new List<string>();

            string[] strings = v.Split('@');
            strings[0] = strings[0].ToUpper();
            Guid filename;
            string path;

            switch (strings[0])
            {
                //writes over the current file if there is one
                case "FORCEWRITE":
                    filename = Guid.NewGuid();
                    if (strings.Length > 1)
                    {
                        path = "Txts\\" + strings[1].ToString() + ".txt";
                        if (!File.Exists(path))
                        {
                            File.Create(path).Dispose();
                            TextWriter tw = new StreamWriter(path);
                            for (int i = 2; i < strings.Length; i++)
                            {
                                tw.Write(strings[i]);
                            }
                            tw.Close();
                        }
                        else if (File.Exists(path))
                        {
                            TextWriter tw = new StreamWriter(path, false);
                            for (int i = 2; i < strings.Length; i++)
                            {
                                tw.Write(strings[i]);
                            }
                            tw.Close();
                        }
                        v = v.Substring(5);
                        returnMessages.Add(strings[1].ToString() + " written");
                    }
                    break;
                //writes the file
                case "WRITE":
                    filename = Guid.NewGuid();
                    if (strings.Length > 1)
                    {
                        path = "Txts\\" + strings[1].ToString() + ".txt";
                        if (!File.Exists(path))
                        {
                            File.Create(path).Dispose();
                            TextWriter tw = new StreamWriter(path);
                            for (int i = 2; i < strings.Length; i++)
                            {
                                tw.Write(strings[i]);
                            }
                            tw.Close();

                        }
                        else if (File.Exists(path))
                        {
                            TextWriter tw = new StreamWriter(path, true);
                            for (int i = 2; i < strings.Length; i++)
                            {
                                tw.Write(strings[i]);
                            }
                            tw.Close();
                        }
                        v = v.Substring(5);
                        returnMessages.Add(strings[1].ToString() + " written");
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
                    returnMessages =  null;
                    break;

                    //Returns a list of commands when it enters upload mode
                case "UPLOAD":
                    returnMessages.Add("WRITE FILENAME to write a file\nFORCEWRITE FILENAME to write over afile if it exists\nDIRECTORY for a list of files\nQUIT to be able to swap between modes");
                    break;

                case "COMMANDS":
                    returnMessages.Add("WRITE FILENAME to write a file\nFORCEWRITE FILENAME to write over afile if it exists\nDIRECTORY for a list of files\nQUIT to be able to swap between modes");
                    break;
                default:
                    returnMessages.Add("Command not recognised");
                    break; 

            }
            return returnMessages;
        }
    }
}
