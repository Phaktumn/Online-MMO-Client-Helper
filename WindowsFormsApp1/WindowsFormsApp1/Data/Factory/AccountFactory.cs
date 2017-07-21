using System.Linq;

namespace WindowsFormsApp1.Data.Factory {
	class AccountFactory : Factory<Account>
	{
		public Account LoggedAccount;

		public AccountFactory() {

		}

		public override void AddData(Account data)
		{
		    var hash = data.GetHash();
		    if (base.factoryData.ContainsKey(hash))
		    {
		        factoryData[hash].DOSID = data.DOSID;
                return;
		    }

			base.factoryData.Add(hash, data);
		}

		public Account GetAccountFromMD5HashName(string md5HashName)
		{
		    if (!factoryData.ContainsKey(md5HashName))
		        return null;
			return factoryData[md5HashName];
		}

		public string GetFirst()
		{
			return factoryData.FirstOrDefault().Key;
		}
	}
}
