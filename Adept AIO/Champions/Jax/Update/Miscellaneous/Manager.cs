using Adept_AIO.Champions.Jax.Core;
using Adept_AIO.Champions.Jax.Update.OrbwalkingEvents;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Orbwalking;

namespace Adept_AIO.Champions.Jax.Update.Miscellaneous
{
    internal class Manager
    {
        public static void PostAttack(object sender, PostAttackEventArgs args)
        {
            switch (Global.Orbwalker.Mode)
            {
                    case OrbwalkingMode.Combo:
                    Combo.OnPostAttack();
                    break;
                    case OrbwalkingMode.Mixed:
                    Harass.OnPostAttack();
                    break;
                    case OrbwalkingMode.Laneclear:
                    Clear.OnPostAttack();
                    break;
            }
        }

        public static void OnUpdate()
        {
            if (Global.Player.IsDead)
            {
                return;
            }

            if (Game.TickCount - SpellConfig.E.LastCastAttemptT > 2000 + Game.Ping / 2)
            {
                SpellConfig.E.LastCastAttemptT = 0;
            }
          
            switch (Global.Orbwalker.Mode)
            {
                    case OrbwalkingMode.Combo:
                    Combo.OnUpdate();
                    break;
                case OrbwalkingMode.Mixed:
                    Harass.OnUpdate();
                    break;
                    case OrbwalkingMode.Laneclear:
                    Clear.OnUpdate();
                    break;
            }
        }
    }
}
