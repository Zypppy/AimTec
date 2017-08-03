using Aimtec;
using Spell = Aimtec.SDK.Spell;

namespace Adept_AIO.Champions.Jax.Core
{
    internal class SpellConfig
    {
        public static Spell Q, W, E, R;

        public static bool SecondE = false;

        /// <summary>
        /// Instances the spells
        /// </summary>
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 700);

            W = new Spell(SpellSlot.W);

            E = new Spell(SpellSlot.E, 300);

            R = new Spell(SpellSlot.R);
        }
    }
}
