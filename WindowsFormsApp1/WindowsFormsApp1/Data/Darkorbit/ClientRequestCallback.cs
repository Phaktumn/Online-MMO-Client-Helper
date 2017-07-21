using WindowsFormsApp1.Data.Darkorbit;

namespace WindowsFormsApp1
{
    internal class ClientRequestCallback : Command
    {
        public const ushort ID = 21314;
        public byte[] callback { get; private set; }

        public ClientRequestCallback(byte[] callback)
        {
            this.callback = callback;
            Write();
        }

        private void Write()
        {
            short totalLength = (short) (8 + callback.Length);
            //PacketWriter.Write(totalLength);
            //PacketWriter.Write(ID);
            //PacketWriter.Write(callback.Length);
            //PacketWriter.Write(callback, 0, callback.Length);
            //PacketWriter.Write((short) 2596);
        }
    }
}