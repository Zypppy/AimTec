using Adept_AIO.Champions.Irelia.Core;
using Adept_AIO.Champions.Irelia.Update.OrbwalkingEvents;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Orbwalking;

namespace Adept_AIO.Champions.Irelia.Update.Miscellaneous
{
    internal class Manager
    {
        public static void OnPreAttack(object sender, PreAttackEventArgs preAttackEventArgs)
        {
            switch (Global.Orbwalker.Mode)
            {
                    case OrbwalkingMode.Laneclear:
                    Clear.OnPreAttack(preAttackEventArgs.Target, preAttackEventArgs);
                    break;
                    case OrbwalkingMode.Combo:
                    Combo.OnPreAttack(preAttackEventArgs.Target, preAttackEventArgs);
                    break;
            }
        }

        public static void PostAttack(object sender, PostAttackEventArgs args)
        {
            switch (Global.Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    Combo.OnPostAttack(args.Target);
                    break;
                    case OrbwalkingMode.Custom:
                    Harass.OnPostAttack(args.Target);
                    break;
                    case OrbwalkingMode.Laneclear:
                    Clear.OnPostAttack(args.Target);
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
                    case OrbwalkingMode.Custom:
                    Harass.OnUpdate();
                    break;
                    case OrbwalkingMode.Laneclear:
                    Clear.OnUpdate();
                    break;
                    case OrbwalkingMode.Freeze:
                    Lasthit.OnUpdate();
                    break;
            }
        }

        public static void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }
           
            switch (args.SpellData.Name)
            {
                case "IreliaTranscendentBlades":
                    SpellConfig.RCount--;
                   
                    if (SpellConfig.RCount <= 0)
                    {
                        SpellConfig.RCount = 4;
                    }
                    break;
            }
        }
    }
}
