using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class Connection
    {

        public ConnectionState _state;
        private ConnManager _manager;
        private Reader _reader;
        private Writer _writer;

        public int sharedKey;


        public Guid g;
        private Func<Connection, int> processingFunction;


        public Connection(Socket sock, ConnManager manager, Func<Connection, int> processingFunction)
        {
            _state = new ConnectionState();
            _state.sock = sock;
            g = Guid.NewGuid();

            _manager = manager;

            _reader = new Reader(_state, _manager);
            _writer = new Writer(_state, _manager);

            if (processingFunction != null)
            {
                this.processingFunction = processingFunction;
            }
            else
            {
                this.processingFunction = this.DefaultProcessing;
            }
        }

        public void Start()
        {
            Thread readerThread = new Thread(new ThreadStart(_reader.Start));
            Thread writerThread = new Thread(new ThreadStart(_writer.Start));
            readerThread.Start();
            writerThread.Start();

            // kill loop:

            while (!_state.kill)
            {
                processingFunction(this);
            }

            readerThread.Join();
            writerThread.Join();

            _state.sock.Shutdown(SocketShutdown.Both);
            _state.sock.Close();
        }

        public bool Dead()
        {
            return _state.kill;
        }
        public void SetKill()
        {
            _state.kill = true;
        }


        public int DefaultProcessing(Connection conn)
        {
            ProcessRead();
            return 1;
        }

        // Print out incoming messages ot console
        public bool ProcessRead()
        {
            if (_state.HasRead())
            {
                string message = _state.DequeueRead();

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(message);
                Console.ResetColor();
                // process read messages
            }
            else if(_state.HasEncRead())
            {
                string message = _state.EncDequeueRead(_manager.sharedKey);

                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine(message);
                Console.ResetColor();
            }
            else
            {
                Thread.Sleep(1);
            }
            return true;
        }

        public bool ProcessWrite()
        {
            return true;
        }

        // Sends message across the network
        public void Write(string message)
        {
            if (ProcessWrite())
            {
                if(!_manager.encryptionNegotiated)
                {
                    _state.EnqueueWrite(message);
                }
                else
                {
                    _state.EncEnqueueWrite(_manager.sharedKey, message);
                }
            }
        }
    }
}
