using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Prediction.Skillshots;
using Spell = Aimtec.SDK.Spell;

namespace Adept_AIO.Champions.Rengar.Core
{
    internal class SpellConfig
    {
        public static Spell Q, W, E, R;

        /// <summary>
        /// Instances the spells
        /// </summary>
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 450);
            Q.SetSkillshot(0.5f, 150, 3000, false, SkillshotType.Line, false, HitChance.Medium);

            W = new Spell(SpellSlot.W, 425);

            E = new Spell(SpellSlot.E, 1000);
            E.SetSkillshot(0.25f, 100, 1500, true, SkillshotType.Line, false, HitChance.Medium);

            R = new Spell(SpellSlot.R);
        }

        public static void CastW(Obj_AI_Base target)
        {
            if (Global.Player.HasBuff("RengarR"))
            {
                return;
            }

            W.Cast();
        }

        public static void CastE(Obj_AI_Base target)
        {
            if (Global.Player.HasBuff("RengarR"))
            {
                return;
            }

            E.Cast(target);
        }

        public static void CastQ(Obj_AI_Base target)
        {
            if (Global.Player.HasBuff("RengarR"))
            {
                return;
            }

            Q.Cast(target);
            Global.Orbwalker.ResetAutoAttackTimer();
        }
    }
}
