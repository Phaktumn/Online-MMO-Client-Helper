using System;
using System.Net.Sockets;
using System.Threading;
using WindowsFormsApp1.Data.Darkorbit;

namespace WindowsFormsApp1
{
    abstract class Client
    {
        public bool Running { get; set; } = true;

        public Thread thread { get; private set; }
        public TcpClient TcpClient { get; private set; }
        public NetworkStream Stream { get; private set; }

        public string Ip { get; set; }
        public int Port { get; set; }

        public event EventHandler<EventArgs> OnConnected;
        public event EventHandler<EventArgs> OnDisconnected;

        public Client()
        {
            TcpClient = new TcpClient();
        }

        public void Connect(string ip, int port)
        {
            Ip = ip;
            Port = port;

            thread = new Thread(Run);

            try
            {
                TcpClient.Connect(ip, port);
            }
            catch (SocketException e)
            {
                Thread.Sleep(5000);
                OnDisconnected?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (!TcpClient.Connected)
                return;

            Stream = TcpClient.GetStream();
            OnConnected?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Stop()
        {
            Running = false;
            thread?.Abort();
            TcpClient?.Close();
            Stream?.Close();
        }

        public void Disconnect()
        {
            TcpClient?.Client?.Disconnect(false);
            TcpClient?.Close();
            Stream?.Close();
            thread = new Thread(new ThreadStart(Run));
            TcpClient = new TcpClient();
        }
        
        public void Send(Command command)
        {
            Send(command.ToArray());
        }

        public void Send(byte[] buffer)
        {
            if(!Running)
                return;

            if (!IsConnected())
            {
                OnDisconnected?.Invoke(this, EventArgs.Empty);
                return;
            }

            Stream.Write(buffer, 0, buffer.Length);
            Stream.Flush();
        }

        public void Run()
        {
            while (true)
            {
                if (!IsConnected())
                {
                    OnDisconnected?.Invoke(this, EventArgs.Empty);
                    return;
                }

                //Parse(new EndianBinaryReader(EndianBitConverter.Big, Stream));
            }
        }
        
        protected bool IsConnected()
        {
            if (TcpClient?.Client == null || !Stream.CanWrite || !Stream.CanRead)
                return false;
            try
            {
                return !(TcpClient.Client.Poll(1, SelectMode.SelectRead) 
                    && TcpClient.Client.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        //public abstract void Parse(EndianBinaryReader reader);
    }
}
