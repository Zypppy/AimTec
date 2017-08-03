using System.Linq;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Riven.Core
{
    internal class Dmg
    {
        public static double Damage(Obj_AI_Base target)
        {
            if (target == null)
            {
                return 0;
            }

            var dmg = 0d;

            if (SummonerSpells.Ignite != null && SummonerSpells.Ignite.Ready)
            {
                dmg += SummonerSpells.IgniteDamage(target);
            }

            if (Global.Orbwalker.CanAttack())
            {
                dmg += Global.Player.GetAutoAttackDamage(target);
            }

            if (SpellConfig.W.Ready)
            {
                dmg += Global.Player.GetSpellDamage(target, SpellSlot.W);
            }

            if (SpellConfig.Q.Ready)
            {
                var count = 4 - Extensions.CurrentQCount;
                dmg += (Global.Player.GetSpellDamage(target, SpellSlot.Q) + dmg) * count;
            }

            if (SpellConfig.R.Ready)
            {
                dmg += Global.Player.GetSpellDamage(target, SpellSlot.R);
            }
            return dmg;
        }
    }
}
