using System.Linq;
using Adept_AIO.Champions.Riven.Core;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Riven.Update.Miscellaneous
{
    internal class SpellManager
    {
        private static bool CanUseQ;
        private static bool CanUseW;
        private static Obj_AI_Base Unit;

        public static void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }
        
            if (args.SpellData.DisplayName.Contains("BasicAttack"))
            {
                Extensions.DidJustAuto = true;
            }

            switch (args.SpellData.Name)
            {
                case "RivenTriCleave":
                    Extensions.LastQCastAttempt = Game.TickCount;
                    Extensions.CurrentQCount++;
                    if (Extensions.CurrentQCount > 3) { Extensions.CurrentQCount = 1; }
                    CanUseQ = false;
                    Animation.Reset();
                    break;
                case "RivenMartyr":
                    CanUseW = false;
                    Global.Orbwalker.ResetAutoAttackTimer();
                    break;
                case "RivenFengShuiEngine":
                    Extensions.UltimateMode = UltimateMode.Second;
                    break;
                case "RivenIzunaBlade":
                    Extensions.UltimateMode = UltimateMode.First;
                    break;
            }
        }

        public static void OnUpdate()
        {
            if (Unit == null)
            {
                return;
            }

            if (CanUseQ && Extensions.DidJustAuto)
            {
                Global.Player.SpellBook.CastSpell(SpellSlot.Q, Unit);
                Extensions.DidJustAuto = false;
            }

            if (!CanUseW)
            {
                return;
            }

          
            Items.CastTiamat();

            Global.Orbwalker.ResetAutoAttackTimer();
            CanUseW = false;

            SpellConfig.W.Cast();
        }

        public static void CastQ(Obj_AI_Base target)
        {
            if (target.HasBuff("FioraW") || target.HasBuff("PoppyW"))
            {
                return;
            }

            Unit = target;
            CanUseQ = true;
        }

        public static void CastW(Obj_AI_Base target)
        {
            if (target.HasBuff("FioraW"))
            {
                return;
            }

            CanUseW = SpellConfig.W.Ready && InsideKiBurst(target);
            Unit = target;  
        }

        public static void CastR2(Obj_AI_Base target)
        {
            if (target.ValidActiveBuffs()
                .Where(buff => Extensions.InvulnerableList.Contains(buff.Name))
                .Any(buff => buff.Remaining > Time(target)))
            {
                return;
            }

            if (target.Distance(Global.Player) <= 300)
            {
                Items.CastTiamat();
            }

            SpellConfig.R2.Cast(target);
        }

        private static int Time(GameObject target)
        {
            return (int)(Global.Player.Distance(target) / (SpellConfig.R2.Speed * 1000 + SpellConfig.R2.Delay));
        }

        public static bool InsideKiBurst(GameObject target)
        {
            return Global.Player.HasBuff("RivenFengShuiEngine")
                 ? Global.Player.Distance(target) <= 265 + target.BoundingRadius
                 : Global.Player.Distance(target) <= 195 + target.BoundingRadius;
        }

        public static bool InsideKiBurst(Vector3 position)
        {
            return Global.Player.HasBuff("RivenFengShuiEngine")
                ? Global.Player.Distance(position) <= 265
                : Global.Player.Distance(position) <= 195;
        }
    }
}
