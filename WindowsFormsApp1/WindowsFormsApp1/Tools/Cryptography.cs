using System.Security.Cryptography;
using System.Text;

namespace WindowsFormsApp1.Tools {
	class Cryptography {

		#region MD5

		public static string GetStringMD5Hash(MD5 md5Hash, string input) 
		{
			byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
			StringBuilder sBuilder = new StringBuilder();

			foreach (var b in data)
			{
				sBuilder.Append(b.ToString("x2"));
			}
			
			return sBuilder.ToString();
		}
		
		#endregion

	}
}
