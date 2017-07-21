namespace WindowsFormsApp1.Data.Darkorbit
{
    class GalaxyGate
    {
        public string Name { get; private set; }
        public ushort Id { get; private set; }
        public ushort TotalParts { get; private set; }
        
        public ushort CurrentParts { get; set; }

        public GalaxyGate(string name, ushort id, ushort totalParts, ushort currentParts)
        {
            Name = name;
            Id = id;
            TotalParts = totalParts;
            CurrentParts = currentParts;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
