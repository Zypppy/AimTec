using System.Linq;
using Adept_AIO.Champions.Yasuo.Core;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Yasuo.Update.Miscellaneous
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

            if (SpellConfig.Q.Ready && target.Health < Global.Player.GetSpellDamage(target, SpellSlot.Q) &&
                target.IsValidTarget(SpellConfig.Q.Range) &&
                MenuConfig.Killsteal["Q"].Enabled)
            {
                if (Extension.CurrentMode == Mode.Tornado && !MenuConfig.Killsteal["Q3"].Enabled)
                {
                    return;
                }

                SpellConfig.Q.Cast(target);
            }
            else if (SpellConfig.E.Ready && target.Health < Global.Player.GetSpellDamage(target, SpellSlot.E) &&
                     target.IsValidTarget(SpellConfig.E.Range) &&
                     !target.HasBuff("YasuoDashWrapper") &&
                     MenuConfig.Killsteal["E"].Enabled)
            {
                SpellConfig.E.CastOnUnit(target);
            }
            else if (MenuConfig.Killsteal["Ignite"].Enabled && SummonerSpells.Ignite != null && SummonerSpells.Ignite.Ready && target.Health < SummonerSpells.IgniteDamage(target))
            {
                SummonerSpells.Ignite.Cast(target);
            }
        }
    }
}