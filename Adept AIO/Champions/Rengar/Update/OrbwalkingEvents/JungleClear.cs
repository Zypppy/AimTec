using System.Linq;
using Adept_AIO.Champions.Rengar.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;
using GameObjects = Adept_AIO.SDK.Extensions.GameObjects;

namespace Adept_AIO.Champions.Rengar.Update.OrbwalkingEvents
{
    internal class JungleClear
    {
        public static void OnPostAttack()
        {
            var mob = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(SpellConfig.Q.Range) && x.IsEnemy);
            if (mob == null)
            {
                return;
            }

            if (SpellConfig.Q.Ready && mob.Health > Global.Player.GetAutoAttackDamage(mob))
            {
                if (Extensions.Ferocity() == 4 && !MenuConfig.JungleClear["Q"].Enabled)
                {
                    return;
                }

                SpellConfig.CastQ(mob);
            }
        }

        public static void OnUpdate()
        {
            var mob = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(SpellConfig.E.Range) && x.IsEnemy);
            if (mob == null)
            {
                return;
            }

            var distance = mob.Distance(Global.Player);

            if (SpellConfig.W.Ready && distance < SpellConfig.W.Range)
            {
                if (Extensions.Ferocity() == 4 && !MenuConfig.JungleClear["W"].Enabled)
                {
                    return;
                }

                SpellConfig.CastW(mob);
            }

            if (SpellConfig.E.Ready && Extensions.Ferocity() <= 3)
            {
                SpellConfig.CastE(mob);
            }
        }
    }
}
