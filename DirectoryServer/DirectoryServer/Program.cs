using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DirectoryServer
{
    // Starts the server with a set port number
    class Program
    {
        private ConnManager manager;
        public Program()
        {
            // Construcotr sets a port number to use
            this.manager = new ConnManager(11000, false);
        }

        private void startServerMode()
        {
                // debugging if you need to find out the IP address the server is connected to
                System.Net.IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                Console.WriteLine(ipAddress);

                // start listener
                manager.StartListener();
        }

        static void Main(string[] args)
        {
            // Default intro message as demanded by Patrick Merritt
            Console.WriteLine("Starting server mode");

            // Program will start up as either a pure client, pure server, or client server configuration.
            Program prog = new Program();
            prog.startServerMode();

        }
    }
}
