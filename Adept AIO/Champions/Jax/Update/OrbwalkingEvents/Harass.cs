using Adept_AIO.Champions.Jax.Core;
using Adept_AIO.Champions.Jax.Update.Miscellaneous;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;

namespace Adept_AIO.Champions.Jax.Update.OrbwalkingEvents
{
    internal class Harass
    {
        public static void OnPostAttack()
        {
            if (!SpellConfig.W.Ready || !MenuConfig.Harass["W"].Enabled)
            {
                return;
            }

            SpellConfig.W.Cast();
            Items.CastTiamat();
            Global.Orbwalker.ResetAutoAttackTimer();
        }

        public static void OnUpdate()
        {
            var target = Global.TargetSelector.GetTarget(SpellConfig.Q.Range);
            if (target == null)
            {
                return;
            }

            if (SpellConfig.E.Ready && MenuConfig.Harass["E"].Enabled)
            {
                SpellManager.CastE(target);
            }
            else if (SpellConfig.Q.Ready && MenuConfig.Harass["Q"].Enabled && Game.TickCount - SpellConfig.E.LastCastAttemptT >= 1800 || SpellConfig.E.LastCastAttemptT <= 0)
            {
                SpellConfig.Q.CastOnUnit(target);
            }
        }
    }
}
