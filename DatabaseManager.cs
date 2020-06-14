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
            var result = await ExecuteQueryAsync(new Query(id,
                $"SELECT `balance` FROM `{Configuration.DatabaseTableName}` WHERE `steamId`=@id;",
                EQueryType.Scalar, null, true, new MySqlParameter("@id", id)));

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
            CheckSetupAccount(id);

            await ExecuteQueryAsync(new Query(null,
                $"UPDATE `{Configuration.DatabaseTableName}` SET `balance`=`balance`+@increase WHERE `steamId`=@id;",
                EQueryType.NonQuery, null, false, new MySqlParameter("@id", id),
                new MySqlParameter("@increase", increaseBy.ToString(CultureInfo.InvariantCulture))));

            var result = await ExecuteQueryAsync(new Query(id,
                $"SELECT `balance` FROM `{Configuration.DatabaseTableName}` WHERE `steamId`=@id;", EQueryType.Scalar,
                null, true, new MySqlParameter("@id", id)));
            if (result != null) decimal.TryParse(result.ToString(), out output);

            Uconomy.Instance.BalanceUpdated(id, increaseBy);

            var player = PlayerTool.getPlayer(new CSteamID(id));

            if (player != null)
                player.skills.channel.send("tellExperience", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER, (uint)output);

            return output;
        }

        internal async Task<decimal> SetBalance(ulong id, decimal value)
        {
            decimal output = 0;
            CheckSetupAccount(id);

            await ExecuteQueryAsync(new Query(null,
                $"UPDATE `{Configuration.DatabaseTableName}` SET `balance`=@newBal WHERE `steamId`=@id;",
                EQueryType.NonQuery, null, false, new MySqlParameter("@id", id),
                new MySqlParameter("@newBal", value.ToString(CultureInfo.InvariantCulture))));

            var result = await ExecuteQueryAsync(new Query(id,
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
            var exists = 0;
            var result = await ExecuteQueryAsync(new Query(id))
            var result = RequestQueryExecute(false, new Query() $"SELECT FROM `{Configuration.DatabaseName}SELECT EXISTS(SELECT 1 FROM `{Configuration.DatabaseTableName}` WHERE `steamId` ='{id}' LIMIT 1);");

            if (result != null) int.TryParse(result.ToString(), out exists);

            if (exists == 0)
                ExecuteQuery(false,
                    $"insert ignore into `{Configuration.DatabaseTableName}` (balance,steamId,lastUpdated) values({Configuration.InitialBalance.ToString(CultureInfo.InvariantCulture)},'{id}',now())");
        }

        internal void CheckSchema()
        {
            var test = ExecuteQuery(true,
                $"show tables like '{Configuration.DatabaseTableName}'");

            if (test == null)
                ExecuteQuery(false,
                    $"CREATE TABLE `{Configuration.DatabaseTableName}` (`steamId` varchar(32) NOT NULL,`balance` decimal(15,2) NOT NULL DEFAULT '25.00',`lastUpdated` timestamp NOT NULL DEFAULT NOW() ON UPDATE CURRENT_TIMESTAMP,PRIMARY KEY (`steamId`)) ");
        }
    }
}