namespace WindowsFormsApp1.Data.Darkorbit
{
    class ClientVersionCheck : Command
    {
        public static ushort ID = 666;
        public short length { get; private set; } = 14;

        public int major { get; set; }
        public int minor { get; set; }
        public int build { get; set; }

        public ClientVersionCheck(int major, int minor, int build)
        {
            this.major = major;
            this.minor = minor;
            this.build = build;
            Write();
        }

        public void Write()
        {
            //PacketWriter.Write(length);
            //PacketWriter.Write(ID);
            //PacketWriter.Write(major);
            //PacketWriter.Write(minor);
            //PacketWriter.Write(build);
        }
    }
}
