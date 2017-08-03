using System.Linq;
using Adept_AIO.Champions.Irelia.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;
using GameObjects = Aimtec.SDK.Util.Cache.GameObjects;

namespace Adept_AIO.Champions.Irelia.Update.OrbwalkingEvents
{
    internal class Harass
    {
        public static void OnPostAttack(AttackableUnit target)
        {
            if (target == null)
            {
                return;
            }

            if (SpellConfig.W.Ready && MenuConfig.Harass["W"].Enabled)
            {
                SpellConfig.W.Cast();
                Global.Orbwalker.ResetAutoAttackTimer();
            }
            else if (MenuConfig.Harass["Safe"].Enabled && SpellConfig.Q.Ready)
            {
                var minion = GameObjects.EnemyMinions.Where(x => x.Distance(target) > 300).OrderBy(x => -x.Distance(target)).LastOrDefault();
                if (minion != null)
                {
                    SpellConfig.Q.CastOnUnit(minion);
                }
            }
        }

        public static void OnUpdate()
        {
            if (SpellConfig.Q.Ready && MenuConfig.Harass["Q"].Enabled && MenuConfig.Harass["Q"].Value <= Global.Player.ManaPercent())
            {
                var target = Global.TargetSelector.GetTarget(SpellConfig.Q.Range);

                if (target == null)
                {
                    return;
                }

                if (MenuConfig.Harass["Safe"].Enabled)
                {
                    if (target.Distance(Global.Player) < Global.Player.AttackRange)
                    {
                        return;
                    }

                    var minion = GameObjects.EnemyMinions.Where(x => x.Health < Global.Player.GetSpellDamage(x, SpellSlot.Q)).OrderBy(x => x.Distance(target)).LastOrDefault();
                    if (minion == null)
                    {
                        return;
                    }

                    SpellConfig.Q.CastOnUnit(minion);
                }
                else
                {
                    SpellConfig.Q.CastOnUnit(target);
                }
            }

            if (SpellConfig.E.Ready && MenuConfig.Harass["E"].Enabled && MenuConfig.Harass["E"].Value <= Global.Player.ManaPercent())
            {
                var target = Global.TargetSelector.GetTarget(SpellConfig.E.Range);

                if (target == null)
                {
                    return;
                }

                SpellConfig.E.CastOnUnit(target);
            }
        }
    }
}
