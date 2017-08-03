using Adept_AIO.Champions.LeeSin.Core.Insec_Manager;
using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.KickFlash
{
    class KickFlash : IKickFlash
    {
        private readonly ISpellConfig SpellConfig;
        private readonly IInsec_Manager _insecManager;

        public KickFlash(ISpellConfig spellConfig, IInsec_Manager insecManager)
        {
            SpellConfig = spellConfig;
            _insecManager = insecManager;
        }

        public void OnKeyPressed()
        {
            if (!Enabled || 
                target == null || 
                !SpellConfig.R.Ready || 
                !target.IsValidTarget(SpellConfig.R.Range) || 
                SummonerSpells.Flash == null || 
                !SummonerSpells.Flash.Ready)
            {
                return;
            }

            SpellConfig.R.CastOnUnit(target);
        }

        public void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender == null || !sender.IsMe || args.SpellSlot != SpellSlot.R || !Enabled)
            {
                return;
            }

            SummonerSpells.Flash.Cast(_insecManager.InsecPosition(target));
        }

        public bool Enabled { get; set; }

        private Obj_AI_Hero target => Global.TargetSelector.GetSelectedTarget();
    }
}
