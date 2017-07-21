using System.Security.Cryptography;
using WindowsFormsApp1.Tools;
using static WindowsFormsApp1.Data.GameSettings;

namespace WindowsFormsApp1.Data
{
    internal class Item : IEncryptable
    {
        public string Name;
        public string Id;
        public int Cost;
        public CurrencyTypes Currency;

        public ItemTypes Type;
        public ItemSubTypes SubType;
        public Category ItemCategory;

        protected Item(string name, int cost, CurrencyTypes currency, Category category)
        {
            Name = name;
            Id = Cryptography.GetStringMD5Hash(MD5.Create(), Name);
            Cost = cost;
            Currency = currency;
            ItemCategory = category;
        }

        public string GetHttpBuyContentString()
        {
            return $"{Type}_{SubType}_{Name}";
        }

        public string GetHash()
        {
            return Cryptography.GetStringMD5Hash(MD5.Create(), Name);
        }
    }

    class Ammo : Item
    {
        public Ammo(string name, int cost, CurrencyTypes currency, ItemSubTypes subType, Category category) 
            : base(name, cost, currency, category)
        {
            Type = ItemTypes.ammunition;
            SubType = subType;
        }
    }
}
