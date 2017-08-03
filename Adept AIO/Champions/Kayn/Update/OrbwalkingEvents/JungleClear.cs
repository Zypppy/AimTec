using System.Linq;
using Adept_AIO.Champions.Kayn.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Kayn.Update.OrbwalkingEvents
{
    internal class JungleClear
    {
        public static void OnPostAttack()
        {
            if (SpellConfig.W.Ready && MenuConfig.JungleClear["W"].Enabled && MenuConfig.JungleClear["W"].Value <=
                Global.Player.ManaPercent())
            {
                var mob = GameObjects.JungleLarge.FirstOrDefault(x => x.IsValidTarget(SpellConfig.W.Range));
                if (mob == null)
                {
                    return;
                }

                SpellConfig.W.Cast(mob);
            }
        }

        public static void OnUpdate()
        {
            if (SpellConfig.Q.Ready && MenuConfig.JungleClear["Q"].Enabled && MenuConfig.JungleClear["Q"].Value <=
                Global.Player.ManaPercent())
            {
                var mob = GameObjects.Jungle.FirstOrDefault(x => x.IsValidTarget(SpellConfig.Q.Range));
                if (mob == null)
                {
                    return;
                }
              
                SpellConfig.Q.Cast(mob);
            }
        }
    }
}
