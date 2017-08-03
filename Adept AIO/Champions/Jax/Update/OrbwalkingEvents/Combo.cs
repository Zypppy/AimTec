using System.Linq;
using Adept_AIO.Champions.Jax.Core;
using Adept_AIO.Champions.Jax.Update.Miscellaneous;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Extensions;
using GameObjects = Aimtec.SDK.Util.Cache.GameObjects;

namespace Adept_AIO.Champions.Jax.Update.OrbwalkingEvents
{
    internal class Combo
    {
        public static void OnPostAttack()
        {
            if (SpellConfig.W.Ready)
            {
                SpellConfig.W.Cast();
                Items.CastTiamat();
                Global.Orbwalker.ResetAutoAttackTimer();
            }
            var target = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidAutoRange());
            if (target == null)
            {
                return;
            }
            if (SpellConfig.R.Ready && Dmg.Damage(target) * 2 > target.Health || target.HealthPercent() <= 40)
            {
                SpellConfig.R.Cast();
            }
        }

        public static void OnUpdate()
        {
            var target = Global.TargetSelector.GetTarget(SpellConfig.Q.Range);
            if (target == null)
            {
                return;
            }

            if (SpellConfig.R.Ready && Global.Player.CountEnemyHeroesInRange(SpellConfig.Q.Range) >= MenuConfig.Combo["R"].Value && MenuConfig.Combo["R"].Enabled)
            {
                SpellConfig.R.Cast();
            }

            if (SpellConfig.E.Ready && target.Distance(Global.Player) <= MenuConfig.Combo["E"].Value && MenuConfig.Combo["E"].Enabled)
            {
                SpellManager.CastE(target);
            }

        
            if (MenuConfig.Combo["Jump"].Enabled && !(SpellConfig.E.Ready || Dmg.Damage(target) > target.Health * 0.75f) ||
                MenuConfig.Combo["Delay"].Enabled && (Game.TickCount - SpellConfig.E.LastCastAttemptT < 800 || SpellConfig.E.Ready && SpellConfig.E.LastCastAttemptT == 0))
            {
                return; 
            }

            if (target.Distance(Global.Player) > SpellConfig.Q.Range)
            {
                var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.IsValid &&
                                                                          x.Distance(target) < 300 &&
                                                                          x.Distance(target) < Global.Player.Distance(target));
                if (minion != null)
                {
                    SpellConfig.Q.CastOnUnit(minion);
                }
            }
            else
            {
                SpellConfig.Q.CastOnUnit(target);
            }
        }
    }
}
