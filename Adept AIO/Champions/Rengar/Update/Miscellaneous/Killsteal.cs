using System.Linq;
using Adept_AIO.Champions.Rengar.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;
using Dmg = Adept_AIO.Champions.Rengar.Core.Dmg;
using MenuConfig = Adept_AIO.Champions.Rengar.Core.MenuConfig;

namespace Adept_AIO.Champions.Rengar.Update.Miscellaneous
{
    internal class Killsteal
    {
        public static void OnUpdate()
        {
            var target = GameObjects.EnemyHeroes.FirstOrDefault(x => x.Distance(Global.Player) < SpellConfig.E.Range && x.HealthPercent() <= 30);

            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (SpellConfig.Q.Ready && target.Health < Dmg.Damage(target) || (target.Health < Global.Player.GetSpellDamage(target, SpellSlot.Q) &&
                                                                              target.Distance(Global.Player) < SpellConfig.Q.Range &&
                                                                              MenuConfig.Killsteal["Q"].Enabled))
            {
                SpellConfig.Q.Cast(target);
            }
            else if (SpellConfig.W.Ready && target.Health < Dmg.Damage(target) || (target.Health < Global.Player.GetSpellDamage(target, SpellSlot.W) &&
                                                                                   target.Distance(Global.Player) < SpellConfig.W.Range &&
                                                                                   MenuConfig.Killsteal["W"].Enabled))
            {
                SpellConfig.W.Cast(target);
            }
            else if (SpellConfig.E.Ready && target.Health < Dmg.Damage(target) || (target.Health < Global.Player.GetSpellDamage(target, SpellSlot.E) &&
                                                                                   target.Distance(Global.Player) < SpellConfig.E.Range &&
                                                                                   MenuConfig.Killsteal["E"].Enabled))
            {
                SpellConfig.E.Cast(target);
            }
        }
    }
}
