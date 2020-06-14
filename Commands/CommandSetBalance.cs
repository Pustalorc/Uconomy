using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Rocket.API;
using Rocket.Unturned.Chat;
using UnityEngine;

namespace fr34kyn01535.Uconomy.Commands
{
    public class CommandSetBalance : IRocketCommand
    {
        [NotNull] public string Help => "Sets the balance of a specific player.";

        [NotNull] public string Name => "setbalance";

        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        [NotNull] public string Syntax => "<player> <amount>";

        [NotNull] public List<string> Aliases => new List<string>();

        [NotNull] public List<string> Permissions => new List<string> {"uconomy.setbalance"};

        public void Execute(IRocketPlayer caller, [NotNull] params string[] command)
        {
            var args = command.ToList();

            if (args.Count < 2)
            {
                UnturnedChat.Say(caller, Uconomy.Instance.Translations.Instance.Translate("setbalance_usage"),
                    UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                return;
            }

            var target = args.GetIRocketPlayer(out var index);
            if (index > -1)
                args.RemoveAt(index);

            var amount = args.GetDecimal(out index);
            if (index > -1)
                args.RemoveAt(index);

            if (target == null)
            {
                UnturnedChat.Say(caller, Uconomy.Instance.Translations.Instance.Translate("player_not_found"),
                    UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                return;
            }

            if (amount <= 0)
            {
                UnturnedChat.Say(caller, Uconomy.Instance.Translations.Instance.Translate("invalid_amount_under_zero"),
                    UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
                return;
            }

            Uconomy.Instance.Database.SetBalance(ulong.Parse(target.Id), amount);

            UnturnedChat.Say(caller,
                Uconomy.Instance.Translations.Instance.Translate("setbalance_private", target.DisplayName,
                    Uconomy.Instance.Configuration.Instance.MoneySymbol, amount,
                    Uconomy.Instance.Configuration.Instance.MoneyName),
                UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));

            UnturnedChat.Say(target,
                Uconomy.Instance.Translations.Instance.Translate("setbalance_other",
                    Uconomy.Instance.Configuration.Instance.MoneySymbol, amount,
                    Uconomy.Instance.Configuration.Instance.MoneyName, caller.DisplayName),
                UnturnedChat.GetColorFromName(Uconomy.MessageColor, Color.green));
        }
    }
}