﻿using System;
using System.Threading;
using JetBrains.Annotations;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;

namespace fr34kyn01535.Uconomy
{
    public sealed class Uconomy : RocketPlugin<UconomyConfiguration>
    {
        public static Uconomy Instance;

        public DatabaseManager database;

        public delegate void PlayerBalanceUpdate(UnturnedPlayer player, decimal newBalance);

        public event PlayerBalanceUpdate OnBalanceUpdate;

        public delegate void PlayerBalanceCheck(UnturnedPlayer player, decimal balance);

        public event PlayerBalanceCheck OnBalanceCheck;

        public delegate void PlayerPay(UnturnedPlayer sender, ulong receiver, decimal amt);

        public event PlayerPay OnPlayerPay;

        [NotNull]
        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                {"setbalance_usage", "Incorrect usage, correct usage: /setbalance <player> <amount>"},
                {"player_not_found", "Failed to find a player!"},
                {"invalid_amount_under_zero", "Invalid amount provided. Cannot be under 0."},
                {"setbalance_private", "Set {0}'s balance to {1} {2} {3}."},
                {"setbalance_other", "Your balance was set to {0} {1} {2} by {3}."},
                {"pay_usage", "Incorrect usage, correct usage: /pay <player> <amount>"},
                {"pay_error_pay_self", "You cant pay yourself."},
                {"pay_error_cant_afford", "Your balance does not allow for this payment."},
                {"pay_sent", "You paid {0} a total of {0} {1} {2}."},
                {"pay_received", "You received a payment of {0} {1} {2} from {3}."},
                {"balance_check_other_no_perms", "Insufficient Permissions!"},
                {"balance_other", "{0}'s current balance is: {1} {2} {3}."},
                {"balance_self", "Your current balance is: {0} {1} {2}."},
                {"cannot_run_console", "Cannot run this command with the arguments provided from console."},
                {"lowered_balance", "Lowered the balance of {0} by {1} {2} {3}."},
                {"balance_lowered", "Your balance was lowered by {0} {1} {2}. This action was done by {3}."}
            };

        protected override void Load()
        {
            Instance = this;
            database = new DatabaseManager(Configuration.Instance);

            U.Events.OnPlayerConnected += Connected;

            if (Configuration.Instance.SyncBalanceToExp)
                UnturnedPlayerEvents.OnPlayerUpdateExperience += ExperienceChanged;
        }

        protected override void Unload()
        {
            if (Configuration.Instance.SyncBalanceToExp)
                UnturnedPlayerEvents.OnPlayerUpdateExperience -= ExperienceChanged;

            U.Events.OnPlayerConnected -= Connected;

            database = null;
            Instance = null;
        }

        private void ExperienceChanged([NotNull] UnturnedPlayer player, uint experience)
        {
            database.SetBalance(player.CSteamID.m_SteamID, experience);
        }

        private void Connected([NotNull] UnturnedPlayer player)
        {
            ThreadPool.QueueUserWorkItem(async o =>
            {
                await database.CheckSetupAccount(player.CSteamID.m_SteamID);

                if (!Configuration.Instance.UseCache) return;

                var balance = (uint) await Instance.database.GetBalance(player.CSteamID.m_SteamID);
                Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>
                    player.Player.skills.channel.send("tellExperience", ESteamCall.ALL,
                        ESteamPacket.UPDATE_RELIABLE_BUFFER, balance));
            });
        }

        internal void HasBeenPayed(UnturnedPlayer sender, ulong receiver, decimal amt)
        {
            OnPlayerPay?.Invoke(sender, receiver, amt);
        }

        internal void BalanceUpdated(ulong steamId, decimal amt)
        {
            if (OnBalanceUpdate == null) return;

            var player = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(steamId)));
            OnBalanceUpdate(player, amt);
        }

        internal void OnBalanceChecked(ulong steamId, decimal balance)
        {
            if (OnBalanceCheck == null) return;

            var player = UnturnedPlayer.FromCSteamID(new CSteamID(steamId));
            OnBalanceCheck(player, balance);
        }
    }
}