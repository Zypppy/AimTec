using System.Linq;
using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.LastHit
{
    internal class Lasthit : ILasthit
    {
        public bool Enabled { get; set; }

        private readonly ISpellConfig SpellConfig;

        public Lasthit(ISpellConfig spellConfig)
        {
            SpellConfig = spellConfig;
        }

        public void OnUpdate()
        {
            if (!Enabled)
            {
                return;
            }

            var minions = GameObjects.EnemyMinions.OrderBy(x => -x.Health).LastOrDefault(x => x.IsValidTarget(SpellConfig.Q.Range) && x.Health < Global.Player.GetSpellDamage(x, SpellSlot.Q));
            if (minions == null || !SpellConfig.Q.Ready || SpellConfig.IsQ2())
            {
                return;
            }           
            SpellConfig.Q.Cast(minions);
        }
    }
}
