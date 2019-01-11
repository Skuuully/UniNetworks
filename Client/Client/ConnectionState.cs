using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    public class ConnectionState
    {
        AutoResetEvent are_connectionInProgress;
        ManualResetEvent mre_readQueue;
        ManualResetEvent mre_writeQueue;
        Queue<String> readQueue;
        Queue<String> writeQueue;
        // Encrypted buffers
        ManualResetEvent mre_encReadQueue;
        ManualResetEvent mre_encWriteQueue;
        Queue<String> encReadQueue;
        Queue<String> encWriteQueue;
        public Socket sock;
        public bool kill = false;
        public ConsoleColor cc;


        public ConnectionState()
        {
            // initialise
            mre_encWriteQueue = new ManualResetEvent(true);
            mre_encReadQueue = new ManualResetEvent(true);
            mre_readQueue = new ManualResetEvent(true);
            mre_writeQueue = new ManualResetEvent(true);
            readQueue = new Queue<string>();
            encReadQueue = new Queue<string>();
            writeQueue = new Queue<string>();
            encWriteQueue = new Queue<string>();
            sock = null;

            Random r = new Random();
            cc = (ConsoleColor)r.Next(0, 16);
        }

        // Check if anything to read
        public bool HasRead()
        {
            if (readQueue.Count > 0)
            {
                return true;
            }
            return false;
        }

        // Check if there is something encrypted to read
        public bool HasEncRead()
        {
            if(encReadQueue.Count > 0)
            {
                return true;
            }
            return false;
        }

        // Check if something to send over network
        public bool HasWrite()
        {
            if (writeQueue.Count > 0)
            {
                return true;
            }
            return false;
        }

        // Check if something encrypted to send over network
        public bool HasEncWrite()
        {
            if(encWriteQueue.Count > 0)
            {
                return true;
            }
            return false;
        }

        // Queues up the read for the reader to process
        public int EnqueueRead(string temp)
        {
            mre_readQueue.WaitOne();
            mre_readQueue.Reset();

            readQueue.Enqueue(temp);

            mre_readQueue.Set();
            return 0;     // No actual return value, alter to indicate queue success
        }

        // Encrypted counterpart
        public int EncEnqueueRead(int key, string temp)
        {
            mre_encReadQueue.WaitOne();
            mre_encReadQueue.Reset();

            temp = Encryption.CaesarEncrypt(key, temp);
            encReadQueue.Enqueue(temp);

            mre_encReadQueue.Set();
            return 0;
        }

        //Queues up the write for the writer to process
        public int EnqueueWrite(string temp)
        {
            mre_writeQueue.WaitOne();

            mre_writeQueue.Reset();

            writeQueue.Enqueue(temp);

            mre_writeQueue.Set();
            return 0;     // No actual return value, alter to indicate queue success
        }

        //Encrypted counterpart
        public int EncEnqueueWrite(int key, string temp)
        {
            mre_encWriteQueue.WaitOne();
            mre_encWriteQueue.Reset();

            temp = Encryption.CaesarEncrypt(key, temp).ToString();
            encWriteQueue.Enqueue(temp);

            mre_encWriteQueue.Set();
            return 0;   
        }

        // Dequeus the read for the reader
        public string DequeueRead()
        {
            string temp;
            mre_readQueue.WaitOne();
            mre_readQueue.Reset();

            temp = readQueue.Dequeue();

            mre_readQueue.Set();
            return temp;
        }

        // Encrypted counterpart
        public string EncDequeueRead(int key)
        {
            string temp;
            mre_encReadQueue.WaitOne();
            mre_encReadQueue.Reset();

            temp = encReadQueue.Dequeue();
            temp = Encryption.CaesarDecrypt(key, temp);

            mre_encReadQueue.Set();
            return temp;
        }

        // dequeues the write for the writer
        public string DequeueWrite()
        {
            string temp;
            mre_writeQueue.WaitOne();
            mre_writeQueue.Reset();

            temp = writeQueue.Dequeue();

            mre_writeQueue.Set();
            return temp;
        }

        // Encrypted counterpart
        public string EncDequeueWrite(int key)
        {
            string temp;
            mre_encWriteQueue.WaitOne();
            mre_encWriteQueue.Reset();

            temp = encWriteQueue.Dequeue();
            temp = Encryption.CaesarDecrypt(key, temp);

            mre_encWriteQueue.Set();
            return temp;
        }
    }
}
