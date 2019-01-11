using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Numerics;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start the client
            Program prog = new Program();
            prog.StartClientMode();
        }
        private ConnManager manager;
        List<Connection> connections = new List<Connection>();
        bool fsConnected = false;
        bool dsConnected = false;
        Random rand = new Random();
        public static int g = 15;
        public static int p = 137;
        public static int a = 0;
        public Connection DSConnection;
        public Connection FSConnection;
        public static string userInput;

        public Program()
        {
            // Constructor to set port number when called
            this.manager = new ConnManager(11000, false);
        }
        
        private void StartClientMode()
        {
            Console.WriteLine("Starting the client");
            userInput = "";


            // A continuous loop looking for <EOF> to end the connection, doesn't search within a string
            while (userInput != "<EOF>")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Enter a string: ");
                Console.ResetColor();
                userInput = Console.ReadLine();
                string[] userIn = userInput.Split(' ');

                if (userInput.StartsWith("DS"))
                {
                    if (userIn[1] == "CONNECT" && !dsConnected)
                    {
                        // do all that crap above, add the connection to a list with a name
                        DSConnection = Connect(11000);
                        connections.Add(DSConnection);
                        manager.AddConnection(DSConnection);
                        dsConnected = true;

                        // SEND DHEBEGIN
                        // a = random between 1 and 10
                        a = (int)rand.Next(1, 101);
                        int personalKey = (int)BigInteger.ModPow(g, a, p);
                        Console.WriteLine("a = " + a  + " Personal Key: " + personalKey);
                        DSConnection.Write("DHEBEGIN @" + p + "@" + g + "@" + personalKey);
                    }
                    if(dsConnected)
                    {
                        if(userIn[1] == "LOGIN" && userIn.Length == 4)
                        {
                            DSConnection.Write("DS AUTHENTICATE");
                            DSConnection.Write("DS USERNAME" + userIn[2]);
                            DSConnection.Write("DS PASSWORD" + userIn[3]);
                            DSConnection.Write("DS CREATEUSER");
                            DSConnection.Write("DS QUIT");
                        }
                        else if (userIn[1] == "SPAM")
                        {
                            DSConnection.Write("DS 1");
                            DSConnection.Write("DS 2");
                            DSConnection.Write("DS 3");
                            DSConnection.Write("DS 4");
                            DSConnection.Write("DS 5");
                            DSConnection.Write("DS " + userInput + userInput);
                            DSConnection.Write("DS " + userInput + userInput);
                            DSConnection.Write("DS " + userInput + userInput);
                            DSConnection.Write("DS " + userInput + userInput);
                        }
                        else if (userIn[1] == "TEST")
                        {
                            DSConnection.Write("DS AbcdefghijklmnopqrstuvwxyzABcdefghijklmnopqrstuvwxyzABCdefghijklmnopqrstuvwxyzABCDefghijklmnopqrstuvwxyz" +
                                "AbcdefghijklmnopqrstuvwxyzABcdefghijklmnopqrstuvwxyzABCdefghijklmnopqrstuvwxyzABCDefghijklmnopqrstuvwxyz" +
                                "AbcdefghijklmnopqrstuvwxyzABcdefghijklmnopqrstuvwxyzABCdefghijklmnopqrstuvwxyzABCDefghijklmnopqrstuvwxyz");
                        }
                        else
                        {
                            DSConnection.Write("DS " + userInput);
                        }
                    }
                }
                else if (userInput.StartsWith("FS") )
                {
                    if (userIn[1] == "CONNECT" && !fsConnected)
                    {
                        // do all that crap above, add the connection to a list with a name
                        FSConnection = Connect(12000);
                        connections.Add(FSConnection);
                        fsConnected = true;

                        a = (int)rand.Next(1, 101);
                        int personalKey = (int)BigInteger.ModPow(g, a, p);
                        Console.WriteLine("a = " + a + " Personal Key: " + personalKey);
                        FSConnection.Write("DHEBEGIN @" + p + "@" + g + "@" + personalKey);
                    }
                    if(fsConnected)
                    {
                        if (userIn[1] == "LOGIN" && userIn.Length == 4)
                        {
                            FSConnection.Write("FS AUTHENTICATE");
                            FSConnection.Write("FS USERNAME " + userIn[2]);
                            FSConnection.Write("FS PASSWORD " + userIn[3]);
                            FSConnection.Write("FS CREATEUSER");
                            FSConnection.Write("FS QUIT");
                        }
                        else if (userIn[1] == "SPAM")
                        {
                            FSConnection.Write("FS 1");
                            FSConnection.Write("FS 2");
                            FSConnection.Write("FS 3");
                            FSConnection.Write("FS 4");
                            FSConnection.Write("FS 5");
                            FSConnection.Write("FS " + userInput + userInput);
                            FSConnection.Write("FS " + userInput + userInput);
                            FSConnection.Write("FS " + userInput + userInput);
                            FSConnection.Write("FS " + userInput + userInput);
                        }
                        else if (userIn[1] == "TEST")
                        {
                            FSConnection.Write("FS AbcdefghijklmnopqrstuvwxyzABcdefghijklmnopqrstuvwxyzABCdefghijklmnopqrstuvwxyzABCDefghijklmnopqrstuvwxyz" +
                                "AbcdefghijklmnopqrstuvwxyzABcdefghijklmnopqrstuvwxyzABCdefghijklmnopqrstuvwxyzABCDefghijklmnopqrstuvwxyz" +
                                "AbcdefghijklmnopqrstuvwxyzABcdefghijklmnopqrstuvwxyzABCdefghijklmnopqrstuvwxyzABCDefghijklmnopqrstuvwxyz");
                        }
                        else
                        {
                            FSConnection.Write("FS " + userInput);
                        }
                    }
                }
                
            }
        }

        Connection Connect(int _port)
        {
            // Set default parameters for the connection
            int port = _port;
            IPAddress address = null;
            // Connect to the server on localhost:port
            /*do
            {
                Console.Write("Please enter an IP address to connect to: ");
                userInput = Console.ReadLine();
            } while (!IPAddress.TryParse(userInput, out address));
            */

            #region Connection to server
            System.Net.IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            address = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(address, port);

            Socket sender = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(localEndPoint);
            #endregion

            // Connection should handle all of the reading, writing, and processing of a message thread
            // no connection manager for this program as we are generating the connection here
            Connection connection = new Connection(sender, manager, manager.ProcessClient);

            // When we create threads there are multiple ways to do so
            // you have already seen asynchrnous methods, this is an alternative
            Thread clientThread = new Thread(new ThreadStart(connection.Start));
            clientThread.Start();
            
            return connection;
        }
    }
}
