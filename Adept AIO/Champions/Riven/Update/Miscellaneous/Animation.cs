using System;
using System.Linq;
using Adept_AIO.Champions.Riven.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Orbwalking;

namespace Adept_AIO.Champions.Riven.Update.Miscellaneous
{
    internal class Animation
    {
        public static float lastReset;
        public static bool IAmSoTired;
        public static bool IsTargetMoving;
      
        public static void Reset()
        {
            if (Global.Orbwalker.Mode == OrbwalkingMode.None)
            {
                Global.Orbwalker.AttackingEnabled = true;
                return;
            }

            Global.Orbwalker.ResetAutoAttackTimer();
            Global.Orbwalker.AttackingEnabled = false;
            Global.Orbwalker.Move(Game.CursorPos);

            lastReset = Game.TickCount;
            IAmSoTired = true;
        }

        public static float GetDelay()
        {
            var delay  = Game.Ping / 2f - Global.Player.AttackSpeedMod * 18;
       
            var enemyObject = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(x => x.Distance(Global.Player) <= Global.Player.AttackRange + 200 && !x.IsAlly && !x.IsMe);

            if (enemyObject != null && enemyObject.IsMoving)
            {
                IsTargetMoving = true;
                delay += Extensions.CurrentQCount == 1 ? 395 : 385;
            }
            else
            {
                IsTargetMoving = false;
                delay += Extensions.CurrentQCount == 1 ? 335 : 315;
            }
           
            return delay;
        }
    }
}
