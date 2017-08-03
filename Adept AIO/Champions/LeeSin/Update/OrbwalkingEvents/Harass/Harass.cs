using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.Champions.LeeSin.Update.Ward_Manager;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.Harass
{
    internal class Harass : IHarass
    {
        public bool Q1Enabled { get; set; }
        public bool Q2Enabled { get; set; }
        public int Mode{ get; set; }
        public bool EEnabled { get; set; }
        public bool E2Enabled { get; set; }

        private readonly IWardManager _wardManager;
        private readonly ISpellConfig SpellConfig;

        public Harass(IWardManager wardManager, ISpellConfig spellConfig)
        {
            _wardManager = wardManager;
            SpellConfig = spellConfig;
        }

        public void OnPostAttack(AttackableUnit target)
        {
            if (target == null || !target.IsHero)
            {
                return;
            }
            if (SpellConfig.E.Ready && E2Enabled && !SpellConfig.IsFirst(SpellConfig.E))
            {
                SpellConfig.E.Cast();
            }
            else if (SpellConfig.W.Ready && Mode == 1)
            {
                SpellConfig.W.CastOnUnit(Global.Player);
            }
        }

        public void OnUpdate()
        {
            var target = Global.TargetSelector.GetTarget(SpellConfig.Q.Range);
            if (target == null)
            {
                return;
            }

            if (SpellConfig.Q.Ready && Q1Enabled)
            {
                if (SpellConfig.IsQ2() && Q2Enabled || !SpellConfig.IsQ2())
                {
                    SpellConfig.Q.Cast(target);
                }
            }

            if (SpellConfig.E.Ready)
            {
                if (SpellConfig.IsFirst(SpellConfig.E) && EEnabled && target.IsValidTarget(SpellConfig.E.Range))
                {
                    SpellConfig.E.Cast(target);
                }
            }

            if (SpellConfig.W.Ready && SpellConfig.IsFirst(SpellConfig.W) && !SpellConfig.E.Ready && !SpellConfig.Q.Ready && Mode == 0)
            {
                var pos = Global.Player.ServerPosition + (Global.Player.ServerPosition + target.ServerPosition) * 300;
                _wardManager.WardJump(pos, true);
            }
        }
    }
}
