using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using WindowsFormsApp1.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApp1.Tools 
{
	public class GroupName
	{
		private string name;
		public string Name
		{
		    get { return name; }
		    set { name = value.ToUpper().Trim(); }
		}
	}

	class FileIO
	{
		private List<GroupName> DataGroups = new List<GroupName>();
		private IDictionary<string, List<object>> bucket = new Dictionary<string, List<object>>();


		public string FilePath { get; private set; }


		public FileIO(string filePath) 
		{
			this.FilePath = filePath;
			if (!File.Exists(filePath))
				File.Create(filePath);
		}

		public void WriteAsync()
		{
			Console.WriteLine(@"Writing to File");
			using (var stream = new FileStream(FilePath, FileMode.Open))
			{
				using (var writer = new StreamWriter(stream))
				{
					var json = JsonConvert.SerializeObject(bucket);
					writer.Write(json);
					Console.WriteLine(json);
				}
			}
			Console.WriteLine(@"Writing Finished");
		}

		public string Read()
		{
			string rawData = string.Empty;
			FileStream stream;
			try
			{
				stream = File.OpenRead(FilePath);
			}
			catch (UnauthorizedAccessException e)
			{
				Console.WriteLine(e.Message);
				throw;
			}
			
			using (stream)
			{
				using (var reader = new StreamReader(stream))
				{
					rawData = reader.ReadToEndAsync().Result;
					reader.Close();
				}
			}

			return rawData;
		}

		public bool AddDataGroup(string groupName)
		{
			var gn = new GroupName {
				Name = groupName
			};

			Console.WriteLine($"Adding new DataGroup\n\tName: {gn.Name}");

			if (DataGroups.Find(name => name.Name == gn.Name) != null) {
				Console.WriteLine(@"Data group WAS FOUND adding it");
				return false;
			}

			Console.WriteLine(@"Data group NOT FOUND adding it");

			DataGroups.Add(gn);
			bucket.Add(gn.Name, new List<object>());
			return true;
		}

		public GroupName GetDataGroup(string groupName)
		{
			var nGN = new GroupName {Name = groupName};
			Console.WriteLine($@"Trying to find {nGN.Name} in DataGroups");
			var gN = DataGroups.Find(name => name.Name == nGN.Name);
		    if (gN == null)
                return null;

            if (string.IsNullOrEmpty(gN.Name))
		    {
				Console.WriteLine($@"{gN.Name} Does not exist");
		        return null;
		    }

			Console.WriteLine($@"Found {gN.Name}");
			return gN;
		}

		public bool AddObjectToDataGroup(object obj, GroupName groupName)
		{
			Console.WriteLine($@"adding {groupName.Name} data to bucket");
			if (!bucket.ContainsKey(groupName.Name))
			{
				Console.WriteLine($@"{groupName.Name} not found in the bucket");
				return false;
			}

			bucket[groupName.Name]?.Add(obj);
			return true;
		}

		public void Initialize()
		{
			Console.WriteLine(@"Intializing I/O");
			Dictionary<string, List<object>> tempBucketData = null;
			var jsonRaw = Read();
			if (!string.IsNullOrEmpty(jsonRaw)) 
			{
				//Gather all the info
				try
				{
					tempBucketData = JsonConvert.DeserializeObject<Dictionary<string, List<object>>>(jsonRaw);
				}
				catch (Exception e)
                {
					Debug.Assert(e != null, $"Message: {e.Message}\n\tHelp: {e.HelpLink}");
				}

				Debug.Assert(tempBucketData != null, new NullReferenceException().Message);
				foreach (var tempBucketPair in tempBucketData) {
					bucket.Add(tempBucketPair);
				}

				foreach (var pair in bucket) 
				{
					DataGroups.Add(new GroupName
					{
						Name = pair.Key
					});
				}
				Console.WriteLine(@"Finished intializing I/O");
				return;
			}
			Console.WriteLine("Finished intializing I/O\nJson file was empty");
		}

		public Account GetAccountInfo(string accountsDataGroupName, int accountIndex)
		{
		    var key = GetDataGroup(accountsDataGroupName).Name;
            return bucket.ContainsKey(key) ? (Account) bucket[key][accountIndex] : null;
		}

	    public List<Account> GetAccounts(string accountsDataGroupName)
	    {
	        if (GetDataGroup(accountsDataGroupName) == null)
	            return null;

            var key = GetDataGroup(accountsDataGroupName).Name;
	        if (!bucket.ContainsKey(key))
                return null;

            var tempList = new List<Account>(bucket[key].Count);
	        foreach (object account in bucket[key])
	        {
	            var token =  JToken.FromObject(account);
	            JToken first = token.First;
                var valuesFirst = first.Values<string>();
	            var valuesSecond = first.Next.Values<string>();

	            string nick = valuesFirst.FirstOrDefault();
	            string pass = valuesSecond.FirstOrDefault();
	            Account tempAccount
	                = new Account() { Nick = nick, Password = pass };
	            tempList.Add(tempAccount);
	        }
	        return tempList;
	    }
	}
}
