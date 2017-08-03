using System.Linq;
using Adept_AIO.Champions.Rengar.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Rengar.Update.OrbwalkingEvents
{
    internal class LaneClear
    {
        public static void OnPostAttack()
        {
            if (MenuConfig.LaneClear["Check"].Enabled && Global.Player.CountEnemyHeroesInRange(2000) > 0)
            {
                return;
            }

            var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.IsValidTarget(SpellConfig.Q.Range) && x.IsEnemy);
            if (minion == null)
            {
                return;
            }

            if (SpellConfig.Q.Ready && minion.Health < Global.Player.GetSpellDamage(minion, SpellSlot.Q))
            {
                if (Extensions.Ferocity() == 4 && !MenuConfig.LaneClear["Q"].Enabled)
                {
                    return;
                }

                SpellConfig.CastQ(minion);
            }
        }

        public static void OnUpdate()
        {
            if (MenuConfig.LaneClear["Check"].Enabled && Global.Player.CountEnemyHeroesInRange(2000) > 0)
            {
                return;
            }
            
            var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.IsValidTarget(SpellConfig.E.Range) && x.IsEnemy);
            if (minion == null)
            {
                return;
            }

            var distance = minion.Distance(Global.Player);

            if (SpellConfig.Q.Ready && distance < SpellConfig.Q.Range)
            {
                if (minion.UnitSkinName.ToLower().Contains("cannon") && minion.Health >
                    Global.Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    return;
                }

                if (Extensions.Ferocity() == 4 && !MenuConfig.LaneClear["Q"].Enabled)
                {
                    return;
                }

                SpellConfig.CastQ(minion);
            }

            if (SpellConfig.W.Ready && distance < SpellConfig.W.Range)
            {
                if (Extensions.Ferocity() == 4 && !MenuConfig.LaneClear["W"].Enabled)
                {
                    return;
                }

                SpellConfig.CastW(minion);
            }

            if (SpellConfig.E.Ready)
            {
                if (!MenuConfig.Combo["E"].Enabled && Extensions.Ferocity() == 4)
                {
                    return;
                }

                SpellConfig.CastE(minion);
            }
        }
    }
}
