using Aimtec;
using Aimtec.SDK.Prediction.Skillshots;
using Spell = Aimtec.SDK.Spell;

namespace Adept_AIO.Champions.Yasuo.Core
{
    internal class SpellConfig
    {
        public static Spell Q, W, E, R;

        /// <summary>
        /// Instances the spells
        /// </summary>
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 475);
            Q.SetSkillshot(0.25f, 55, 1600, false, SkillshotType.Line);
            Extension.CurrentMode = Mode.Normal;

            W = new Spell(SpellSlot.W, 400);

            E = new Spell(SpellSlot.E, 475);

            R = new Spell(SpellSlot.R, 1200);
        }

        public static void SetSkill(Mode mode)
        {
            switch (mode)
            {
                case Mode.Normal:
                    Q.SetSkillshot(0.25f, 55, int.MaxValue, false, SkillshotType.Line);
                    Q.Range = 475;
                    break;
                case Mode.Tornado:
                    Q.SetSkillshot(0.25f, 90, 1800, false, SkillshotType.Line, false, HitChance.Medium);
                    Q.Range = 1200;
                    break;
                case Mode.Dashing:
                case Mode.DashingTornado:
                    Q.SetSkillshot(0, 375, int.MaxValue, false, SkillshotType.Circle);
                    Q.Range = 220;
                    break;
             
            }
        }
    }
}
