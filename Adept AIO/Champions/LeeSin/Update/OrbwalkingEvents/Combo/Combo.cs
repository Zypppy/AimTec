using System;
using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.Champions.LeeSin.Update.Ward_Manager;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Damage.JSON;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.Combo
{
    internal class Combo : ICombo
    {
        public bool TurretCheckEnabled { get; set; }
        public bool Q1Enabled { get; set; }
        public bool Q2Enabled { get; set; }
        public bool WEnabled { get; set; }
        public bool WardEnabled { get; set; }
        public bool EEnabled { get; set; }

        private readonly IWardManager _wardManager;
        private readonly ISpellConfig SpellConfig;
      
        public Combo(IWardManager wardManager, ISpellConfig spellConfig)
        {
            _wardManager = wardManager;
            SpellConfig = spellConfig;
        }

        public void OnPostAttack(AttackableUnit target)
        {
            if (target == null)
            {
                return;
            }

            if (SpellConfig.W.Ready && WEnabled)
            {
                SpellConfig.W.Cast(Global.Player);
            }
            else if (SpellConfig.E.Ready && EEnabled)
            {
                if (!SpellConfig.IsFirst(SpellConfig.E))
                {
                    SpellConfig.E.Cast();
                }
            }
        }

        public void OnUpdate()
        {
            var target = Global.TargetSelector.GetTarget(1600);
            if (target == null)
            {
                return;
            }
            Console.WriteLine(WardEnabled);
            var distance = target.Distance(Global.Player);

            if (SpellConfig.R.Ready && SpellConfig.Q.Ready && Q1Enabled && distance <= 550 && target.Health <= Global.Player.GetSpellDamage(target, SpellSlot.R) + 
                                                                                                               Global.Player.GetSpellDamage(target, SpellSlot.Q) + 
                                                                                                               Global.Player.GetSpellDamage(target, SpellSlot.Q, DamageStage.SecondCast))
            {
                SpellConfig.R.CastOnUnit(target);
                SpellConfig.Q.Cast(target); 
            }

            if (SpellConfig.Q.Ready && Q1Enabled)
            {
                if (distance > 1300)
                {
                    return;
                }

                if (SpellConfig.IsQ2())
                {
                    if (TurretCheckEnabled && target.IsUnderEnemyTurret() || !Q2Enabled)
                    {
                        return;
                    }
                 
                    SpellConfig.Q.Cast();
                }
                else
                {
                    SpellConfig.QSmite(target);
                    SpellConfig.Q.Cast(target);
                }
            }
            else if (SpellConfig.W.Ready && SpellConfig.IsFirst(SpellConfig.W) && _wardManager.IsWardReady() && WEnabled && WardEnabled && distance > (SpellConfig.Q.Ready ? 1000 : 600))
            {
                if (Game.TickCount - SpellConfig.Q.LastCastAttemptT <= 1000 && SpellConfig.Q.LastCastAttemptT > 0)
                {
                    return;
                }

                _wardManager.WardJump(target.Position, true);
            }

            if (SpellConfig.E.Ready && EEnabled && SpellConfig.IsFirst(SpellConfig.E) && distance <= 350)
            {
               SpellConfig.E.Cast(target);
            }
        }
    }
}
