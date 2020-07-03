using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Rocket.API;
using Rocket.Unturned.Chat;
using UnityEngine;

namespace fr34kyn01535.Uconomy.Commands
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
            var args = command.ToList();
            if (args.Count < 2)
            {
                UnturnedChat.Say(caller, Uconomy.Instance.Translate("pay_usage"),
                    UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));
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
                UnturnedChat.Say(caller, Uconomy.Instance.Translate("player_not_found"),
                    UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));
                return;
            }

            if (amount <= 0)
            {
                UnturnedChat.Say(caller, Uconomy.Instance.Translate("invalid_amount_under_zero"),
                    UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));
                return;
            }

            if (caller.Id.Equals(target.Id, StringComparison.OrdinalIgnoreCase))
            {
                UnturnedChat.Say(caller,
                    Uconomy.Instance.Translate("pay_error_pay_self"),
                    UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));
                return;
            }

            FinishCalculationsAndPay(caller, target, amount);
        }

        public async Task FinishCalculationsAndPay([NotNull] IRocketPlayer caller, [NotNull] IRocketPlayer target, decimal amount)
        {
            var currentBalance = caller is ConsolePlayer
                ? amount
                : await Uconomy.Instance.database.GetBalance(ulong.Parse(caller.Id));

            if (currentBalance - amount < 0)
                UnturnedChat.Say(caller,
                    Uconomy.Instance.Translate("pay_error_cant_afford"),
                    UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));

            await Uconomy.Instance.database.IncreaseBalance(ulong.Parse(target.Id), amount);

            if (!(caller is ConsolePlayer))
                await Uconomy.Instance.database.IncreaseBalance(ulong.Parse(caller.Id), -amount);

            UnturnedChat.Say(caller,
                Uconomy.Instance.Translate("pay_sent", target.DisplayName,
                    Uconomy.Instance.Configuration.Instance.MoneySymbol, amount,
                    Uconomy.Instance.Configuration.Instance.MoneyName),
                UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));

            UnturnedChat.Say(target,
                Uconomy.Instance.Translate("pay_received",
                    Uconomy.Instance.Configuration.Instance.MoneySymbol, amount,
                    Uconomy.Instance.Configuration.Instance.MoneyName, caller.DisplayName),
                UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));
        }
    }
}