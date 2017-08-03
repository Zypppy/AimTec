using System.Linq;
using Adept_AIO.Champions.Riven.Core;
using Adept_AIO.Champions.Riven.Update.Miscellaneous;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Riven.Update.OrbwalkingEvents
{
    internal class Lane
    {
        public static void OnPostAttack()
        {
            var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.Distance(Global.Player) < Extensions.EngageRange() &&
                                                                      x.Health > Global.Player.GetAutoAttackDamage(x));

            if (minion == null || MenuConfig.Lane["Check"].Enabled && Global.Player.CountEnemyHeroesInRange(2000) >= 1)
            {
                return;
            }

            if (SpellConfig.Q.Ready && MenuConfig.Lane["Q"].Enabled)
            {
                SpellManager.CastQ(minion);
            }

            if (SpellConfig.W.Ready && MenuConfig.Lane["W"].Enabled &&
                minion.Health < Global.Player.GetSpellDamage(minion, SpellSlot.W) &&
                (minion.UnitSkinName == "SRU_ChaosMinionSiege" || minion.UnitSkinName == "SRU_OrderMinionSiege"))
            {
                SpellManager.CastW(minion);
            }
        }


        public static void OnUpdate()
        {
            var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.Distance(Global.Player) < Extensions.EngageRange());

            if (minion == null || MenuConfig.Lane["Check"].Enabled &&
                Global.Player.CountEnemyHeroesInRange(1500) >= 1)
            {
                return;
            }

            if (SpellConfig.E.Ready && MenuConfig.Lane["E"].Enabled)
            {
                SpellConfig.E.CastOnUnit(minion);
            }
        }
    }
}
