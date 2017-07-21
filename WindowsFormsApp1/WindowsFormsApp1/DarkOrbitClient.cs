using System;
using System.IO;
using WindowsFormsApp1.Data.Darkorbit;
using WindowsFormsApp1.Data.Fade;

namespace WindowsFormsApp1
{
    class DarkOrbitClient : Client
    {
        private FadeProxyClient fadeProxy;

        public DarkOrbitClient(FadeProxyClient proxy)
        {
            fadeProxy = proxy;

            fadeProxy.StageOneLoaded += (sender, args) =>
            {
                byte[] secret = proxy.GenerateKey();
                SendEncoded(new ClientRequestCallback(secret));
            };

            fadeProxy.StageOneFailed += (sender, args) =>
            {
                Console.WriteLine(@"StageOne failed");
            };
        }

        public override void Stop()
        {
            base.Stop();
            fadeProxy.Disconnect();
        }

        public void SendEncoded(Command command)
        {
            if (!Running)
                return;

            Send(fadeProxy.Encrypt(command.ToArray()));
        }

        //public override void Parse(EndianBinaryReader reader)
        //{
        //    ushort length;
        //    ushort id;
        //    byte[] content;
		//
        //    if(!IsConnected() || TcpClient.Available == 0) 
        //        return;
        //    var lengthBuffer = reader.ReadBytes(2);
        //    if(BitConverter.IsLittleEndian)
        //        Array.Reverse(lengthBuffer);
		//
        //    length = BitConverter.ToUInt16(lengthBuffer, 0);
        //    if(!IsConnected())
        //        return;
		//
        //    content = fadeProxy.Decrypt(reader.ReadBytes(length));
		//
        //    EndianBinaryReader cachedReader = new EndianBinaryReader(EndianBitConverter.Big, new MemoryStream(content));
		//
        //    id = cachedReader.ReadUInt16();
		//
        //    Console.WriteLine($"ID: {id} Content: {cachedReader.ReadString()}");
        //}
    }
}
