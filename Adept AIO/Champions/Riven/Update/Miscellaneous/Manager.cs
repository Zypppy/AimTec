using Adept_AIO.Champions.Riven.Core;
using Adept_AIO.Champions.Riven.Update.OrbwalkingEvents;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Orbwalking;

namespace Adept_AIO.Champions.Riven.Update.Miscellaneous
{
    internal class Manager
    {
        public static void OnUpdate()
        {
            if (Global.Player.IsDead)
            {
                return;
            }
        
            if (Animation.IAmSoTired)
            {
                if (Game.TickCount - Animation.lastReset< Animation.GetDelay())
                {
                    return;
                }
               
                Global.Orbwalker.AttackingEnabled = true;
                Animation.IAmSoTired = false;
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
                    Lane.OnUpdate();
                    Jungle.OnUpdate();
                    break;
                case OrbwalkingMode.None:
                    Extensions.AllIn = false;
                    break;
            }
       
            if (SpellConfig.Q.Ready &&
                Extensions.CurrentQCount != 1 &&
                MenuConfig.Miscellaneous["Active"].Enabled &&
               !Global.Player.HasBuff("Recall") &&
                Game.TickCount - Extensions.LastQCastAttempt >= 3580 + Game.Ping / 2 &&
                Game.TickCount - Extensions.LastQCastAttempt <= 3700 + Game.Ping / 2) 
            {
                SpellConfig.Q.Cast(Game.CursorPos);
            }
        }

        public static void PostAttack(object sender, PostAttackEventArgs args)
        {
            if (Game.TickCount - Extensions.LastQCastAttempt < 500)
            {
                return;
            }

            var target = args.Target as Obj_AI_Base;

            if (MenuConfig.BurstMode.Active)
            {
                Burst.OnPostAttack(target);
            }
            else
            {
                switch (Global.Orbwalker.Mode)
                {
                    case OrbwalkingMode.Combo:
                        Combo.OnPostAttack(target);
                        break;
                    case OrbwalkingMode.Mixed:
                        Harass.OnPostAttack();
                        break;
                    case OrbwalkingMode.Laneclear:
                        if (args.Target.IsMinion)
                        {
                            Lane.OnPostAttack();
                            Jungle.OnPostAttack(args.Target as Obj_AI_Minion);
                        }
                        else if (args.Target.IsBuilding() && SpellConfig.Q.Ready)
                        {
                            SpellConfig.Q.Cast(Global.Player.ServerPosition.Extend(Game.CursorPos, 400));
                        }
                        break;
                }
            }
        }
    }
}
