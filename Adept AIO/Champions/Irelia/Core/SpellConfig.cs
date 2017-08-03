using Aimtec;
using Aimtec.SDK.Prediction.Skillshots;
using Spell = Aimtec.SDK.Spell;

namespace Adept_AIO.Champions.Irelia.Core
{
    internal class SpellConfig
    {
        public static Spell Q, W, E, R;
        public static int RCount = 4;

        /// <summary>
        /// Instances the spells
        /// </summary>
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 650);

            W = new Spell(SpellSlot.W);

            E = new Spell(SpellSlot.E, 325);

            R = new Spell(SpellSlot.R, 1000);
            R.SetSkillshot(0.25f, 65, 1600, false, SkillshotType.Line);
        }
    }
}
