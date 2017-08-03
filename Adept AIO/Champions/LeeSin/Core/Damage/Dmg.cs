using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Damage;

namespace Adept_AIO.Champions.LeeSin.Core.Damage
{
    internal class Dmg : IDmg
    {
        private readonly ISpellConfig SpellConfig;

        public Dmg(ISpellConfig spellConfig)
        {
            SpellConfig = spellConfig;
        }

        public double Damage(Obj_AI_Base target)
        {
            if (target == null)
            {
                return 0;
            }

            var dmg = Global.Player.GetAutoAttackDamage(target);

            if (SpellConfig.E.Ready)
            {
                dmg += Global.Player.GetSpellDamage(target, SpellSlot.E) + dmg;
            }

            if (SpellConfig.Q.Ready)
            {
                dmg += Global.Player.GetSpellDamage(target, SpellSlot.Q) + dmg;
            }

            if (SpellConfig.R.Ready)
            {
                dmg += Global.Player.GetSpellDamage(target, SpellSlot.R);
            }
            return dmg;
        }
    }
}
