using System.Linq;
using Adept_AIO.Champions.Irelia.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Orbwalking;
using GameObjects = Adept_AIO.SDK.Extensions.GameObjects;

namespace Adept_AIO.Champions.Irelia.Update.OrbwalkingEvents
{
    internal class Clear
    {
        public static void OnPreAttack(AttackableUnit target, PreAttackEventArgs preAttackEventArgs)
        {
            if (SpellConfig.E.Ready && MenuConfig.Clear["E"].Enabled && MenuConfig.Clear["E"].Value <= Global.Player.ManaPercent())
            {
                foreach (var mob in GameObjects.JungleLarge.Where(x => x.HealthPercent() <= Global.Player.HealthPercent()))
                {
                    preAttackEventArgs.Cancel = true;
                    SpellConfig.E.CastOnUnit(mob);
                }
            }
        }

        public static void OnPostAttack(AttackableUnit target)
        {
            if (SpellConfig.W.Ready && MenuConfig.Clear["W"].Enabled)
            {
                SpellConfig.W.Cast();
                Global.Orbwalker.ResetAutoAttackTimer();
            }
        }

        public static void OnUpdate()
        {
            if (!SpellConfig.Q.Ready || 
                !MenuConfig.Clear["Q"].Enabled || 
                 MenuConfig.Clear["Q"].Value > Global.Player.ManaPercent() ||
                 MenuConfig.Clear["Check"].Enabled && Global.Player.CountEnemyHeroesInRange(2000) >= 1)
            {
                return;
            }

            var minion = GameObjects.EnemyMinions.LastOrDefault(x => x.Distance(Global.Player) <= SpellConfig.Q.Range &&
                                                                     x.Health < Global.Player.GetSpellDamage(x, SpellSlot.Q));

            if (minion == null || MenuConfig.Clear["Turret"].Enabled && minion.IsUnderEnemyTurret())
            {
                return;
            }

            SpellConfig.Q.CastOnUnit(minion);
        }
    }
}
