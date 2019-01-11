using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Numerics;

namespace Client
{
    class ConnManager
    {
        // Connection Tracking
        private List<Connection> connections = new List<Connection>();
        private int port;
        private Listener listener = null;

        // g and n must be the same for both the server and the client

        private int b;
        Random rand = new Random();
        public bool encryptionNegotiated;
        private string encryptionType;
        // Key calculated form DH
        public int sharedKey;

        public ConnManager(int port, bool tryRandom)
        {
            connections = new List<Connection>();
            this.port = port;

        }

        public void StartListener()
        {
            // Fire up a Listener to create new threads;
            listener = new Listener(this, port);
            Thread listenThread = new Thread(new ThreadStart(listener.Start));
            listenThread.Start();
        }

        public void Add(Socket sock)
        {

            // <TBD> Note that at current this relies on the program state to create a socket type:
            // this means all sockets that we create are going to be processServer functions
            // we likely want the ability to change the type of processing we do later on, consider how to alter this.
            Connection conn;


            conn = new Connection(sock, this, ProcessClient);


            Thread connThread = new Thread(new ThreadStart(conn.Start));
            connections.Add(conn);
            connThread.Start();
        }

        public void AddConnection(Connection connection)
        {
            connections.Add(connection);
        }

        public int ProcessClient(Connection conn)
        {
            string userInput = Program.userInput;
            string[] strings = userInput.Split(' ');
                // if something in queue dequeue and process against commands
                if (conn._state.HasRead())
                {
                    string message = conn._state.DequeueRead();
                    ProcessCommands(message, conn);
                }
                // else decrypt the queue first based upon the key then process
                else if (conn._state.HasEncRead())
                {
                    string message = conn._state.EncDequeueRead(sharedKey);
                    ProcessCommands(message, conn);
                }
                else
                {
                    Thread.Sleep(1);
                }
           
            return 1;
        }

        // Processes the commands based on a string passed in
        // Avoids duplication of switch statement for encrypted or decrypted
        private void ProcessCommands(string messageReceived, Connection connection)
        {
            string[] stringy = messageReceived.Split(' ');
            string[] diffieH = messageReceived.Split('@');

            switch (stringy[0])
            {
                // DHEBEGIN gets used by server
                case "DHEBEGIN":
                    int p = int.Parse(diffieH[1].ToString());
                    int g = int.Parse(diffieH[2].ToString());
                    int A = int.Parse(diffieH[3].ToString());
                    b = rand.Next(0, 11);
                    int publicKey = (int)(Math.Pow(g, b) % p);
                    // The key to use for encryption
                    sharedKey = (int)(Math.Pow(A, b) % p);
                    //Console.WriteLine("DHEPUBLIC, g = " + g + ", p =" + p + ", A = " + A + "\nPrivKey = " + privateKey);
                    connection.Write("DHEV @" + publicKey.ToString());
                    break;

                // key value pairs separated by @  DHEV p:5@g:10@A:20
                case "DHEPUBLIC":
                    A = int.Parse(diffieH[1].ToString());
                    // Create the key to use for encryption
                    sharedKey = (int)BigInteger.ModPow(A, Program.a, Program.p);
                    Console.WriteLine(" A = " + A + "\nShared Key = " + sharedKey);

                    // Send list of encrpyption types that could be used
                    connection.Write("ENCRYPTIONTYPES @CAESAR@ROT13");
                    break;

                // Compare types sent over and agrre upon one of them
                case "ENCRYPTIONTYPES":
                    // For each string check if matches any personal encryption types, once match is found send back
                    for (int i = 0; i < diffieH.Length; i++)
                    {
                        if (diffieH[i] == "CAESAR")
                        {
                            connection.Write("ENCRYPTIONACCEPT @CAESAR");
                            connection.Write("ENCRYPTIONON");
                            encryptionType = "CAESAR";
                        }
                        // Possible to add more encryption types here
                    }
                    break;

                case "ENCRYPTIONON":
                    Console.WriteLine("Encryption is now enabled");
                    encryptionNegotiated = true;
                    break;
                case "RENEG":
                    break;
                case "ENCRYPTIONOFF":
                    encryptionNegotiated = false;
                    break;

                    
                case "FILE": // when the server has sent a file to client
                        string path = "Txts\\" + stringy[1].ToString() + ".txt";
                        if (!File.Exists(path))
                        {
                            File.Create(path).Dispose();
                            TextWriter tw = new StreamWriter(path);
                            for (int i = 2; i < stringy.Length; i++)
                            {
                                tw.Write(stringy[i]);
                            }
                            tw.Close();
                        }
                        else if (File.Exists(path))
                        {
                            TextWriter tw = new StreamWriter(path, false);
                            for (int i = 2; i < stringy.Length; i++)
                            {
                                tw.Write(stringy[i]);
                            }
                            tw.Close();
                        }

                    Console.WriteLine("File: " + stringy[1] + ".txt has been saved");
                    break;

                case "TOKEN":
                    string token = stringy[0];
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(messageReceived);
                    Console.ResetColor();
                    break;
            }
        }

        public Connection ConnectTo(string hostname, int port, Func<Connection, int> processingFunction)
        {
            #region Connect to the specified address
            System.Net.IPHostEntry ipHostInfo = Dns.GetHostEntry(hostname);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(localEndPoint);
            #endregion

            Connection connection = new Connection(sender, this, processingFunction);
            connections.Add(connection);

            Thread clientThread = new Thread(new ThreadStart(connection.Start));
            clientThread.Start();

            return connection;
        }

    }
}
