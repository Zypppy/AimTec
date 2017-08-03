using System.Linq;
using Adept_AIO.Champions.Irelia.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Irelia.Update.Miscellaneous
{
    internal class Killsteal
    {
        public static void OnUpdate()
        {
            var target = GameObjects.EnemyHeroes.FirstOrDefault(x => x.Distance(Global.Player) < SpellConfig.R.Range && x.HealthPercent() <= 40);

            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (SpellConfig.Q.Ready && target.Health < Dmg.Damage(target) || (target.Health <
                Global.Player.GetSpellDamage(target, SpellSlot.Q) &&
                target.Distance(Global.Player) < SpellConfig.Q.Range &&
                MenuConfig.Killsteal["Q"].Enabled))
            {
                SpellConfig.Q.CastOnUnit(target);
            }
            else if (SpellConfig.E.Ready && target.Health <
                Global.Player.GetSpellDamage(target, SpellSlot.E) &&
                target.Distance(Global.Player) < SpellConfig.E.Range &&
                MenuConfig.Killsteal["E"].Enabled)
            {
                SpellConfig.E.CastOnUnit(target);
            }
            else if (SpellConfig.R.Ready && target.Health <
                     Global.Player.GetSpellDamage(target, SpellSlot.R) &&
                     MenuConfig.Killsteal["R"].Enabled)
            {
                SpellConfig.R.CastOnUnit(target);
            }
        }
    }
}
