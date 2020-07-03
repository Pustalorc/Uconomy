using Pustalorc.Libraries.MySqlConnectorWrapper.Configuration;
using Rocket.API;

namespace fr34kyn01535.Uconomy
{
    public class UconomyConfiguration : IConnectorConfiguration, IRocketPluginConfiguration
    {
        public string DatabaseAddress { get; set; }
        public string DatabaseUsername { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseTableName { get; set; }
        public ushort DatabasePort { get; set; }
        public bool UseCache { get; set; }
        public ulong CacheRefreshIntervalMilliseconds { get; set; }
        public byte CacheSize { get; set; }

        public bool SyncBalanceToExp;
        public decimal InitialBalance;
        public string MoneySymbol;
        public string MoneyName;
        public string MessageColor;

        public void LoadDefaults()
        {
            DatabaseAddress = "127.0.0.1";
            DatabaseUsername = "unturned";
            DatabasePassword = "password";
            DatabaseName = "unturned";
            DatabaseTableName = "uconomy";
            DatabasePort = 3306;
            UseCache = true;
            CacheRefreshIntervalMilliseconds = 30000;
            CacheSize = 24;
            SyncBalanceToExp = false;
            InitialBalance = 30;
            MoneySymbol = "$";
            MoneyName = "Credits";
            MessageColor = "blue";
        }
    }
}