using System.Linq;
using Adept_AIO.Champions.Kayn.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Kayn.Update.OrbwalkingEvents
{
    internal class LaneClear
    {
        public static void OnUpdate()
        {
            if (MenuConfig.LaneClear["Check"].Enabled && Global.Player.CountEnemyHeroesInRange(2000) >= 1)
            {
                return;
            }

            if (SpellConfig.W.Ready && MenuConfig.LaneClear["W"].Enabled && MenuConfig.LaneClear["W"].Value <=
                Global.Player.ManaPercent())
            {
                var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.IsValidTarget(SpellConfig.W.Range));
                if (minion == null)
                {
                    return;
                }

                SpellConfig.W.Cast(minion);
            }

            if (SpellConfig.Q.Ready && MenuConfig.LaneClear["Q"].Enabled && MenuConfig.LaneClear["Q"].Value <=
                Global.Player.ManaPercent())
            {
                var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.IsValidTarget(SpellConfig.Q.Range));
                if (minion == null)
                {
                    return;
                }
             
                SpellConfig.Q.Cast(minion);
            }
        }
    }
}
