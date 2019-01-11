using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Numerics;

namespace DirectoryServer
{
    enum loopState { MAINSERVER, UPLOAD, DOWNLOAD, FILECHECKER, AUTHENTICATE };
    class ConnManager
    {
        // Connection Tracking
        private List<Connection> connections;
        private int port;
        private Listener listener = null;

        // Server Functions variables
        private Uploader uploader;
        private Downloader downloader;
        private MainServer mainServer;
        private Authenticate authenticate;
        private loopState serverState;
        private Func<string, List<string>> stateFunction;

        Random rand = new Random();
        int b;
        public int sharedKey;
        public bool encryptionNegotiated;
        public ConnManager(int port, bool tryRandom)
        {
            connections = new List<Connection>();
            this.port = port;
            serverState = loopState.MAINSERVER;
            downloader = new Downloader();
            uploader = new Uploader();
            mainServer = new MainServer();
            authenticate = new Authenticate();
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
            conn = new Connection(sock, this, ProcessServer);



            Thread connThread = new Thread(new ThreadStart(conn.Start));
            connThread.Start();
            connections.Add(conn);
        }

        public int ProcessServer(Connection conn)
        {
            if (conn._state.HasRead())  // unencrypted buffer
            {
                string message = conn._state.DequeueRead();

                message = StripServerName(message);
                ProcessCommands(message);

                // prints out a clients guid and message to console
                Console.ForegroundColor = conn._state.cc;
                Console.WriteLine(conn.g.ToString() + " " + message);
                Console.ResetColor();

                // Change state if required
                ManageState(message);

                // Process unencrypted message to send
                List<string> temp = stateFunction(message);
                if (temp != null)
                {
                    foreach (string s in temp)
                    {
                        conn._state.EnqueueWrite(s);
                    }
                }
                else
                {
                    serverState = loopState.MAINSERVER;
                }
            }
            else if(conn._state.HasEncRead()) // encrypted buffer
            {
                string message = conn._state.EncDequeueRead(sharedKey);

                message = StripServerName(message);
                ProcessCommands(message);

                Console.ForegroundColor = conn._state.cc;
                Console.WriteLine(conn.g.ToString() + " " + message);
                Console.ResetColor();

                ManageState(message);
                
                List<string> temp = stateFunction(message);
                if (temp != null)
                {
                    foreach (string s in temp)
                    {
                        conn._state.EncEnqueueWrite(sharedKey, s);
                    }
                }
                else
                {
                    serverState = loopState.MAINSERVER;
                }
            }
            else
            {
                Thread.Sleep(1);
            }
            return 1;
        }

        private void ProcessCommands(string messageReceived)
        {
            string[] stringy = messageReceived.Split(' ');
            string[] diffieH = messageReceived.Split('@');
            Console.WriteLine(messageReceived);
            switch (stringy[0])
            {
                case "DHEBEGIN":
                    int p = int.Parse(diffieH[1].ToString());
                    int g = int.Parse(diffieH[2].ToString());
                    int A = int.Parse(diffieH[3].ToString());
                    b = rand.Next(0, 11);
                    int publicKey = (int)(Math.Pow(g, b) % p);
                    connections[0].Write("DHEPUBLIC @" + publicKey.ToString());
                    // The key to use for encryption
                    sharedKey = (int)BigInteger.ModPow(A, b, p);
                    Console.WriteLine("DHEPUBLIC, g = " + g + ", p =" + p + ", A = " + A + "\nsharedKey = " + sharedKey);
                    break;
                // key value pairs separated by @  DHEV p:5@g:10@A:20
                case "DHEPUBLIC":
                    // Retrieve values
                    p = int.Parse(diffieH[1].ToString());
                    g = int.Parse(diffieH[2].ToString());
                    A = int.Parse(diffieH[3].ToString());
                    // Create the key to use for encryption
                    sharedKey = (int)BigInteger.ModPow(A, b, p);
                    Console.WriteLine("DHEV, g = " + g + ", p =" + p + ", A = " + A + "\nsharedKey = " + sharedKey);
                    // Send list of encrpyption types that could be used
                    connections[0].Write("ENCRYPTIONTYPES @CAESAR");
                    break;
                case "ENCRYPTIONTYPES":
                    // For each string check if matches any personal encryption types, once match is found send back
                    for (int i = 0; i < diffieH.Length; i++)
                    {
                        if (diffieH[i] == "CAESAR")
                        {
                            connections[0].Write("ENCRYPTIONACCEPT @CAESAR");
                            connections[0].Write("ENCRYPTIONON");
                        }
                    }
                    break;
                case "ENCRYPTIONON":
                    Console.WriteLine("Encryption is now enabled");
                    encryptionNegotiated = true;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(messageReceived);
                    Console.ResetColor();
                    break;
            }
        }

        private void ManageState(string messageReceived)
        {
            if (serverState == loopState.MAINSERVER)
            {
                if (messageReceived.StartsWith("UPLOAD"))
                {
                    serverState = loopState.UPLOAD;
                    stateFunction = uploader.ProcessString;
                    Console.WriteLine("Server is now in: " + serverState.ToString() + " state");

                }
                else if (messageReceived.StartsWith("DOWNLOAD"))
                {
                    serverState = loopState.DOWNLOAD;
                    stateFunction = downloader.processString;
                    Console.WriteLine("Server is now in: " + serverState.ToString() + " state");

                }
                else if (messageReceived.StartsWith("AUTHENTICATE"))
                {
                    serverState = loopState.AUTHENTICATE;
                    stateFunction = authenticate.ProcessString;
                    Console.WriteLine("Server is now in: " + serverState.ToString() + " state");

                }
                else
                {
                    stateFunction = mainServer.processString;
                    Console.WriteLine("Server is now in: " + serverState.ToString() + " state");
                }
            }
        }

        // Removes info about type of server being talked to, string can then be processed normally
        private string StripServerName(string message)
        {
            if (message.StartsWith("DS "))
            {
                message = message.Remove(0, 3);

            }
            else if (message.StartsWith("FS "))
            {
                message = message.Remove(0, 3);
            }
            return message;
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
