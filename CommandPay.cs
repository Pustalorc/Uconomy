using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Commands;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace fr34kyn01535.Uconomy
{
    public class CommandPay : IRocketCommand
    {
        [NotNull] public string Help => "Pays a specific player money from your account";

        [NotNull] public string Name => "pay";

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        [NotNull] public string Syntax => "<player> <amount>";

        [NotNull] public List<string> Aliases => new List<string>();

        [NotNull] public List<string> Permissions => new List<string> {"uconomy.pay"};

        public void Execute(IRocketPlayer caller, [NotNull] params string[] command)
        {
            if (command.Length != 2)
            {
                UnturnedChat.Say(caller, Uconomy.Instance.Translations.Instance.Translate("command_pay_invalid"),
                    UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                return;
            }

            var otherPlayer = command.GetCSteamIDParameter(0)?.ToString();
            var otherPlayerOnline = UnturnedPlayer.FromName(command[0]);
            if (otherPlayerOnline != null) otherPlayer = otherPlayerOnline.Id;

            if (otherPlayer != null)
            {
                if (caller.Id == otherPlayer)
                {
                    UnturnedChat.Say(caller,
                        Uconomy.Instance.Translations.Instance.Translate("command_pay_error_pay_self"),
                        UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                    return;
                }

                if (!decimal.TryParse(command[1], out var amount) || amount <= 0)
                {
                    UnturnedChat.Say(caller,
                        Uconomy.Instance.Translations.Instance.Translate("command_pay_error_invalid_amount"),
                        UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                    return;
                }

                if (caller is ConsolePlayer)
                {
                    Uconomy.Instance.Database.IncreaseBalance(otherPlayer, amount);
                    if (otherPlayerOnline != null)
                    {
                        UnturnedChat.Say(caller,
                            Uconomy.Instance.Translations.Instance.Translate("command_pay_private",
                                otherPlayerOnline.CharacterName, amount,
                                Uconomy.Instance.Configuration.Instance.MoneyName),
                            UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                        UnturnedChat.Say(otherPlayerOnline,
                            Uconomy.Instance.Translations.Instance.Translate("command_pay_console", amount,
                                Uconomy.Instance.Configuration.Instance.MoneyName),
                            UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                    }
                    else
                        UnturnedChat.Say(caller,
                            Uconomy.Instance.Translations.Instance.Translate("command_pay_private", otherPlayer,
                                amount, Uconomy.Instance.Configuration.Instance.MoneyName),
                            UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                }
                else
                {
                    var myBalance = Uconomy.Instance.Database.GetBalance(caller.Id);
                    if (myBalance - amount <= 0)
                    {
                        UnturnedChat.Say(caller,
                            Uconomy.Instance.Translations.Instance.Translate("command_pay_error_cant_afford"),
                            UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                    }
                    else
                    {
                        Uconomy.Instance.Database.IncreaseBalance(caller.Id, -amount);
                        if (otherPlayerOnline != null)
                            UnturnedChat.Say(caller,
                                Uconomy.Instance.Translations.Instance.Translate("command_pay_private",
                                    otherPlayerOnline.CharacterName, amount,
                                    Uconomy.Instance.Configuration.Instance.MoneyName),
                                UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                        else
                            UnturnedChat.Say(caller,
                                Uconomy.Instance.Translations.Instance.Translate("command_pay_private", otherPlayer,
                                    amount, Uconomy.Instance.Configuration.Instance.MoneyName),
                                UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));

                        Uconomy.Instance.Database.IncreaseBalance(otherPlayer, amount);
                        if (otherPlayerOnline != null)
                            UnturnedChat.Say(otherPlayerOnline.CSteamID,
                                Uconomy.Instance.Translations.Instance.Translate("command_pay_other_private", amount,
                                    Uconomy.Instance.Configuration.Instance.MoneyName, caller.DisplayName),
                                UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));

                        Uconomy.Instance.HasBeenPayed((UnturnedPlayer) caller, otherPlayer, amount);
                    }
                }
            }
            else
            {
                UnturnedChat.Say(caller,
                    Uconomy.Instance.Translations.Instance.Translate("command_pay_error_player_not_found"));
            }
        }
    }
}