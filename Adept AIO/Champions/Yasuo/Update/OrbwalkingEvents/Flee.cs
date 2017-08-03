using System.Linq;
using Adept_AIO.Champions.Yasuo.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Yasuo.Update.OrbwalkingEvents
{
    internal class Flee
    {
        public static void OnKeyPressed()
        {
            if (!SpellConfig.E.Ready)
            {
                return;
            }

            

            var minion = GameObjects.Minions.Where(x => x.Distance(Global.Player) <= SpellConfig.E.Range && !x.HasBuff("YasuoDashWrapper"))
                .OrderBy(x => x.Distance(Game.CursorPos)).FirstOrDefault();
            
            if (minion != null)
            {
                SpellConfig.E.CastOnUnit(minion);
            }
        }
    }
}
