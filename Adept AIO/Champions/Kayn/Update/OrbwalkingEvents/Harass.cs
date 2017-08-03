using Adept_AIO.Champions.Kayn.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Kayn.Update.OrbwalkingEvents
{
    internal class Harass
    {
        public static void OnUpdate()
        {
            if (SpellConfig.W.Ready && MenuConfig.Harass["W"].Enabled && MenuConfig.Harass["W"].Value <= Global.Player.ManaPercent())
            {
                var target = Global.TargetSelector.GetTarget(SpellConfig.W.Range);
                if (target != null)
                {
                    SpellConfig.W.Cast(target);
                }
            }

            if (SpellConfig.Q.Ready && MenuConfig.Harass["Q"].Enabled && MenuConfig.Harass["Q"].Value <= Global.Player.ManaPercent())
            {
                var target = Global.TargetSelector.GetTarget(SpellConfig.Q.Range);
                if (target != null)
                {
                    SpellConfig.Q.Cast(target);
                }
            }
        }
    }
}
