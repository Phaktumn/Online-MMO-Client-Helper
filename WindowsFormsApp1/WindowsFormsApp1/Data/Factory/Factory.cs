using System.Collections.Generic;
using System.Linq;

namespace WindowsFormsApp1.Data.Factory {
	class Factory<T> where T : IEncryptable
	{
		protected Dictionary<string, T> factoryData;
		
		public Factory()
        {
			factoryData = new Dictionary<string, T>();
		}

		public virtual void AddData(T data)
		{
			factoryData.Add(data.GetHash(), data);
		}

		public T GetData(string key)
		{
			return factoryData[key];
		}

		public IDictionary<string, T> GetAllDataAsDictionary() => factoryData;
		public T[] GetAllDataValuesAsArray() => factoryData.Values.ToArray();
		public string[] GetAllDataKeysAsAray() => factoryData.Keys.ToArray();
	}
}
