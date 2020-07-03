using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Rocket.API;
using Rocket.Unturned.Chat;
using UnityEngine;

namespace fr34kyn01535.Uconomy.Commands
{
    public class CommandBalance : IRocketCommand
    {
        [NotNull] public string Name => "balance";

        [NotNull] public string Help => "Shows the current balance";


        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        [NotNull] public string Syntax => "<player>";

        [NotNull] public List<string> Aliases => new List<string>();

        [NotNull] public List<string> Permissions => new List<string> {"uconomy.balance"};

        public void Execute(IRocketPlayer caller, [NotNull] params string[] command)
        {
            GetAndDisplayBalance(caller, command);
        }

        public async Task GetAndDisplayBalance(IRocketPlayer caller, [NotNull] params string[] command)
        {
            var args = command.ToList();

            var target = args.GetIRocketPlayer(out var index);
            if (index > -1)
                args.RemoveAt(index);

            if (target != null)
            {
                if (!caller.HasPermission("balance.check"))
                {
                    UnturnedChat.Say(caller, Uconomy.Instance.Translate("balance_check_other_no_perms"),
                        UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));
                    return;
                }

                var otherBalance = await Uconomy.Instance.database.GetBalance(ulong.Parse(target.Id));

                UnturnedChat.Say(caller,
                    Uconomy.Instance.Translate("balance_other", target.DisplayName,
                        Uconomy.Instance.Configuration.Instance.MoneySymbol, otherBalance,
                        Uconomy.Instance.Configuration.Instance.MoneyName),
                    UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));

                return;
            }

            if (caller is ConsolePlayer)
            {
                UnturnedChat.Say(caller, Uconomy.Instance.Translate("cannot_run_console"),
                    UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));
                return;
            }

            var balance = await Uconomy.Instance.database.GetBalance(ulong.Parse(caller.Id));
            UnturnedChat.Say(caller,
                Uconomy.Instance.Translate("balance_self",
                    Uconomy.Instance.Configuration.Instance.MoneySymbol, balance,
                    Uconomy.Instance.Configuration.Instance.MoneyName),
                UnturnedChat.GetColorFromName(Uconomy.Instance.Configuration.Instance.MessageColor, Color.green));
        }
    }
}