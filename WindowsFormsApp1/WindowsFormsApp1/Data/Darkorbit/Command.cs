using System.IO;

namespace WindowsFormsApp1.Data.Darkorbit
{
    class Command
    {
        protected MemoryStream MemoryStream;
        //protected EndianBinaryWriter PacketWriter;
		//
        //public Command()
        //{
        //    MemoryStream = new MemoryStream();
        //    PacketWriter = new EndianBinaryWriter(EndianBitConverter.Big, MemoryStream);
        //}

        public byte[] ToArray()
        {
            return MemoryStream.ToArray();
        }
    }
}
