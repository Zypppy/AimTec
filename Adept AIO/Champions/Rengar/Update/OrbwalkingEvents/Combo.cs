using Adept_AIO.Champions.Rengar.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Rengar.Update.OrbwalkingEvents
{
    internal class Combo
    {
        public static void OnPostAttack()
        {
            var target = Global.TargetSelector.GetTarget(SpellConfig.Q.Range);
            if (target == null || !target.IsValid) 
            {
                return;
            }

            if (SpellConfig.Q.Ready)
            {
                if (Extensions.Ferocity() == 4 && !MenuConfig.Combo["Q"].Enabled)
                {
                    return;
                }

                SpellConfig.CastQ(target);
            }
        }

        public static void OnUpdate()
        {
            var target = Global.TargetSelector.GetTarget(2000);

            if (target == null || !target.IsValid)
            {
                return;
            }

            var assassin = Global.TargetSelector.GetSelectedTarget();
            
            if (Global.Player.HasBuff("RengarR"))
            {
                if (MenuConfig.AssassinManager[target.ChampionName].Enabled || assassin != null && assassin.IsValid)
                {
                    Extensions.AssassinTarget = target;
                }
            }
            
            var distance = target.Distance(Global.Player);
            if (distance > Global.Player.AttackRange && distance < SpellConfig.Q.Range)
            {
                if (Extensions.Ferocity() == 4 && !MenuConfig.Combo["Q"].Enabled)
                {
                    return;
                }

                SpellConfig.CastQ(target);
            }

            if (SpellConfig.W.Ready && distance < SpellConfig.W.Range)
            {
                if (Extensions.ShieldPercent() >= 1 && MenuConfig.Combo["Health"].Value <= Extensions.ShieldPercent())
                {
                    SpellConfig.CastW(target);
                }
                else if (Extensions.Ferocity() == 4)
                {
                    if (MenuConfig.Combo["Mode"].Value == 1 && !Extensions.HardCC())
                    {
                        return;
                    }

                    SpellConfig.CastW(target);
                }
                else
                {
                    if (!MenuConfig.Combo["W"].Enabled)
                    {
                        return;
                    }

                    SpellConfig.CastW(target);  
                }
            }

            if (SpellConfig.E.Ready && distance < SpellConfig.E.Range)
            {
                if (!MenuConfig.Combo["E"].Enabled && Extensions.Ferocity() == 4)
                {
                    return;
                }

                SpellConfig.CastE(target);
            }
        }
    }
}
