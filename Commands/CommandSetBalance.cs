using System.Collections.Generic;
using JetBrains.Annotations;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using UnityEngine;

namespace fr34kyn01535.Uconomy
{
    public class CommandSetBalance : IRocketCommand
    {
        [NotNull] public string Help => "Sets the balance of a specific player.";

        [NotNull] public string Name => "setbalance";

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        [NotNull] public string Syntax => "<player> <amount>";

        [NotNull] public List<string> Aliases => new List<string>();

        [NotNull] public List<string> Permissions => new List<string> { "uconomy.setbalance" };

        public void Execute(IRocketPlayer caller, [NotNull] params string[] command)
        {
            if (command.Length != 2)
            {
                UnturnedChat.Say(caller, Uconomy.Instance.Translations.Instance.Translate("command_setbalance_invalid"),
                    UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                return;
            }

            var otherPlayer = command.GetCSteamIDParameter(0)?.ToString();
            var otherPlayerOnline = UnturnedPlayer.FromName(command[0]);
            if (otherPlayerOnline != null) otherPlayer = otherPlayerOnline.Id;

            if (otherPlayer != null)
            {
                if (!decimal.TryParse(command[1], out var amount) || amount <= 0)
                {
                    amount = 0;
                }

                if (caller is ConsolePlayer)
                {
                    Uconomy.Instance.Database.SetBalance(otherPlayer, amount);
                    if (otherPlayerOnline != null)
                    {
                        UnturnedChat.Say(caller,
                            Uconomy.Instance.Translations.Instance.Translate("command_setbalance_private",
                                otherPlayerOnline.CharacterName, amount,
                                Uconomy.Instance.Configuration.Instance.MoneyName),
                            UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                        UnturnedChat.Say(otherPlayerOnline,
                            Uconomy.Instance.Translations.Instance.Translate("command_setbalance_console", amount,
                                Uconomy.Instance.Configuration.Instance.MoneyName),
                            UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                    }
                    else
                        UnturnedChat.Say(caller,
                            Uconomy.Instance.Translations.Instance.Translate("command_setbalance_private", otherPlayer,
                                amount, Uconomy.Instance.Configuration.Instance.MoneyName),
                            UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                }
                else
                {
                    Uconomy.Instance.Database.SetBalance(otherPlayer, amount);
                    if (otherPlayerOnline != null)
                    {
                        UnturnedChat.Say(otherPlayerOnline.CSteamID,
                            Uconomy.Instance.Translations.Instance.Translate("command_setbalance_other_private", amount,
                                Uconomy.Instance.Configuration.Instance.MoneyName, caller.DisplayName),
                            UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                        UnturnedChat.Say(caller,
                                Uconomy.Instance.Translations.Instance.Translate("command_setbalance_private",
                                    otherPlayerOnline.CharacterName, amount,
                                    Uconomy.Instance.Configuration.Instance.MoneyName),
                                UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                    }
                    else
                        UnturnedChat.Say(caller,
                            Uconomy.Instance.Translations.Instance.Translate("command_setbalance_private", otherPlayer,
                                amount, Uconomy.Instance.Configuration.Instance.MoneyName),
                            UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));

                    Uconomy.Instance.BalanceUpdated(otherPlayer, amount);
                }
            }
            else
            {
                UnturnedChat.Say(caller,
                    Uconomy.Instance.Translations.Instance.Translate("command_setbalance_error_player_not_found"));
            }
        }
    }
}