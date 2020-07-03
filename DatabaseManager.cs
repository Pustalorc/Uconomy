using System.Globalization;
using System.Threading.Tasks;
using I18N.West;
using JetBrains.Annotations;
using MySql.Data.MySqlClient;
using Pustalorc.Libraries.MySqlConnectorWrapper;
using Pustalorc.Libraries.MySqlConnectorWrapper.Queries;
using SDG.Unturned;
using Steamworks;

namespace fr34kyn01535.Uconomy
{
    public class DatabaseManager : ConnectorWrapper<UconomyConfiguration>
    {
        public DatabaseManager([NotNull] UconomyConfiguration configuration) : base(configuration)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new CP1250();

            CheckSchema();
        }

        /// <summary>
        /// Retrieves the current balance of a specific account.
        /// </summary>
        /// <param name="id">The Steam 64 ID of the account to retrieve the balance from.</param>
        /// <returns>The balance of the account.</returns>
        public async Task<decimal> GetBalance(ulong id)
        {
            decimal output = 0;

            var result = await ExecuteQueryAsync(new Query(
                $"SELECT `balance` FROM `{Configuration.DatabaseTableName}` WHERE `steamId`=@id;", EQueryType.Scalar,
                null, true, new MySqlParameter("@id", id)));

            if (result != null) decimal.TryParse(result.ToString(), out output);
            Uconomy.Instance.OnBalanceChecked(id, output);

            return output;
        }

        /// <summary>
        /// Increases the account balance of the specific ID with IncreaseBy.
        /// </summary>
        /// <param name="id">Steam 64 ID of the account.</param>
        /// <param name="increaseBy">The amount that the account should be changed with (can be negative).</param>
        /// <returns>The new balance of the account.</returns>
        public async Task<decimal> IncreaseBalance(ulong id, decimal increaseBy)
        {
            decimal output = 0;

            await CheckSetupAccount(id);

            await ExecuteQueryAsync(new Query(
                $"UPDATE `{Configuration.DatabaseTableName}` SET `balance`=`balance`+@increase WHERE `steamId`=@id;",
                EQueryType.NonQuery,
                o => ExecuteTransaction(new Query(
                    $"SELECT `balance` FROM `{Configuration.DatabaseTableName}` WHERE `steamId`=@id;",
                    EQueryType.Scalar, null, true, new MySqlParameter("@id", id))), false,
                new MySqlParameter("@id", id),
                new MySqlParameter("@increase", increaseBy.ToString(CultureInfo.InvariantCulture))));

            var result = await ExecuteQueryAsync(new Query(
                $"SELECT `balance` FROM `{Configuration.DatabaseTableName}` WHERE `steamId`=@id;", EQueryType.Scalar,
                null, true, new MySqlParameter("@id", id)));
            if (result != null) decimal.TryParse(result.ToString(), out output);

            Uconomy.Instance.BalanceUpdated(id, increaseBy);

            if (!Uconomy.Instance.Configuration.Instance.UseCache) return output;

            var player = PlayerTool.getPlayer(new CSteamID(id));

            if (player != null)
                player.skills.channel.send("tellExperience", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER,
                    (uint) output);

            return output;
        }

        internal async Task<decimal> SetBalance(ulong id, decimal value)
        {
            decimal output = 0;

            await CheckSetupAccount(id);

            await ExecuteQueryAsync(new Query(
                $"UPDATE `{Configuration.DatabaseTableName}` SET `balance`=@newBal WHERE `steamId`=@id;",
                EQueryType.NonQuery,
                o => ExecuteTransaction(new Query(
                    $"SELECT `balance` FROM `{Configuration.DatabaseTableName}` WHERE `steamId`=@id;",
                    EQueryType.Scalar, null, true, new MySqlParameter("@id", id))), false,
                new MySqlParameter("@id", id),
                new MySqlParameter("@newBal", value.ToString(CultureInfo.InvariantCulture))));

            var result = await ExecuteQueryAsync(new Query(
                $"SELECT `balance` FROM `{Configuration.DatabaseTableName}` WHERE `steamId`=@id;", EQueryType.Scalar,
                null, true, new MySqlParameter("@id", id)));
            if (result != null) decimal.TryParse(result.ToString(), out output);

            Uconomy.Instance.BalanceUpdated(id, value);

            return output;
        }

        /// <summary>
        /// Ensures that the account exists in the database and creates it if it isn't.
        /// </summary>
        /// <param name="id">Steam 64 ID of the account to ensure its existence.</param>
        /// <returns>A bool, where true means the account has been setup, false means it already exists.</returns>
        public async Task<bool> CheckSetupAccount(ulong id)
        {
            var result = await ExecuteQueryAsync(new Query(
                $"SELECT `balance` FROM `{Configuration.DatabaseTableName}` WHERE `steamId`=@id;", EQueryType.Scalar,
                null, true, new MySqlParameter("@id", id)));

            if (result != null && decimal.TryParse(result.ToString(), out _)) return false;

            await ExecuteQueryAsync(new Query(
                $"INSERT IGNORE INTO `{Configuration.DatabaseTableName}` (`balance`,`steamId`) values(@initialBalance,@id);",
                EQueryType.NonQuery, null, false,
                new MySqlParameter("@initialBalance",
                    Configuration.InitialBalance.ToString(CultureInfo.InvariantCulture)),
                new MySqlParameter("@id", id)));

            return true;
        }

        private void CheckSchema()
        {
            var test = ExecuteQuery(new Query($"SHOW TABLES LIKE '{Configuration.DatabaseTableName}';",
                EQueryType.Scalar));

            if (test.Output == null)
                ExecuteQuery(new Query(
                    $"CREATE TABLE `{Configuration.DatabaseTableName}` (`steamId` BIGINT UNSIGNED NOT NULL, `balance` decimal(15,2) NOT NULL, `lastUpdated` timestamp NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP, PRIMARY KEY (`steamId`));",
                    EQueryType.NonQuery));
        }
    }
}