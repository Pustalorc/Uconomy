using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Rocket.API;
using Rocket.Unturned.Player;

namespace fr34kyn01535.Uconomy
{
    public static class Utils
    {
        public static IRocketPlayer GetIRocketPlayer([NotNull] this IEnumerable<string> args, out int index)
        {
            IRocketPlayer output = null;
            index = args.ToList().FindIndex(k =>
            {
                output = UnturnedPlayer.FromName(k);
                if (output == null && ulong.TryParse(k, out var id) && id > 76561197960265728)
                    output = new RocketPlayer(id.ToString());

                return output != null;
            });
            return output;
        }

        public static decimal GetDecimal([NotNull] this IEnumerable<string> args, out int index)
        {
            var output = decimal.Zero;
            index = args.ToList().FindIndex(k => decimal.TryParse(k, out output));
            return output;
        }
    }
}