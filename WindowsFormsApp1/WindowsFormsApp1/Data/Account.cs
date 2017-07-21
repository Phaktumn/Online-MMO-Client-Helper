using System.Security.Cryptography;
using WindowsFormsApp1.Tools;

namespace WindowsFormsApp1.Data
{
	class Account : IEncryptable
	{
		public string Nick;
		public string Password;
		public bool Logged = false;

        public long InitialCredits = 0;
		public long InitialUridium = 0;
		public long InitialExp = 0;
		public long InitialHonor = 0;

        public long Credits = 0;
		public long Uridium = 0;
		public long Exp = 0;
	    public short Level = 0;
	    public int ID = 0;
		public long Honor = 0;

	    public bool Premium;

	    public string Server;

	    public double GgSpinCost
	    {
	        get
	        {
	            double cost = 100;
	            if (Premium)
	                cost -= cost * 0.5;
	            return cost;
	        }
	    }

	    public string DOSID { get; set; }

	    public override string ToString()
        {
	        return $"{Nick}";
	    }

	    public string GetHash()
	    {
	        var md5 = MD5.Create();
	        return Cryptography.GetStringMD5Hash(md5, Nick);
	    }
	}
}
