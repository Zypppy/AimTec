using System.Linq;
using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Damage.JSON;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.LeeSin.Update.Miscellaneous
{
    internal interface IKillsteal
    {
        void OnUpdate();
    }

    internal class Killsteal : IKillsteal
    {
        public bool IgniteEnabled { get; set; }
        public bool SmiteEnabled { get; set; }
        public bool QEnabled { get; set; }
        public bool EEnabled { get; set; }
        public bool REnabled { get; set; }

        private readonly ISpellConfig SpellConfig;

        public Killsteal(ISpellConfig spellConfig)
        {
            SpellConfig = spellConfig;
        }

        public void OnUpdate()
        {
            var target = GameObjects.EnemyHeroes.FirstOrDefault(x => x.Distance(Global.Player) < SpellConfig.R.Range && x.HealthPercent() <= 40);

            if (target == null || !target.IsValidTarget())
            {
                return;
            }

            if (SmiteEnabled && SummonerSpells.Smite != null && SummonerSpells.Smite.Ready && target.Health < SummonerSpells.SmiteChampions())
            {
                SummonerSpells.Smite.CastOnUnit(target);
            }
            if (SpellConfig.Q.Ready && (SpellConfig.IsQ2() ? target.Health < Global.Player.GetSpellDamage(target, SpellSlot.Q, DamageStage.SecondCast) : target.Health < Global.Player.GetSpellDamage(target, SpellSlot.Q)) &&
                target.IsValidTarget(SpellConfig.Q.Range) && QEnabled)
            {
                SpellConfig.Q.Cast(target);
            }
            else if (SpellConfig.E.Ready && target.Health < Global.Player.GetSpellDamage(target, SpellSlot.E) &&
                     target.IsValidTarget(SpellConfig.E.Range) && EEnabled)
            {
                SpellConfig.E.Cast();
            }
            else if (SpellConfig.R.Ready && target.Health < Global.Player.GetSpellDamage(target, SpellSlot.R) &&
                     target.IsValidTarget(SpellConfig.R.Range) && REnabled)
            {
                SpellConfig.R.CastOnUnit(target);
            }
            else if (IgniteEnabled && SummonerSpells.Ignite != null && SummonerSpells.Ignite.Ready && target.Health < SummonerSpells.IgniteDamage(target))
            {
                SummonerSpells.Ignite.Cast(target);
            }
        }
    }
}
