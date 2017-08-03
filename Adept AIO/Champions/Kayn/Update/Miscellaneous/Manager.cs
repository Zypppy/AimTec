using Adept_AIO.Champions.Kayn.Update.OrbwalkingEvents;
using Adept_AIO.SDK.Extensions;
using Aimtec.SDK.Orbwalking;

namespace Adept_AIO.Champions.Kayn.Update.Miscellaneous
{
    internal class Manager
    {
        public static void PostAttack(object sender, PostAttackEventArgs args)
        {
            switch (Global.Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    Combo.OnPostAttack(args.Target);
                    break;
                case OrbwalkingMode.Laneclear:
                    JungleClear.OnPostAttack();
                    break;
            }
        }

        public static void OnUpdate()
        {
            if (Global.Player.IsDead)
            {
                return;
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
                    LaneClear.OnUpdate();
                    JungleClear.OnUpdate();
                    break;
            }
        }
    }
}
