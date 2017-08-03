using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.Champions.LeeSin.Update.Ward_Manager;
using Aimtec;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.WardJump
{
    internal class WardJump : IWardJump
    {
        public bool Enabled { get; set; }

        private readonly IWardTracker _wardTracker;

        private readonly IWardManager _wardManager;

        private readonly ISpellConfig SpellConfig;

        public WardJump(IWardTracker wardTracker, IWardManager wardManager, ISpellConfig spellConfig)
        {
            _wardTracker = wardTracker;
            _wardManager = wardManager;
            SpellConfig = spellConfig;
        }

        public void OnKeyPressed()
        {
            if (!Enabled)
            {
                return;
            }

            if (SpellConfig.W.Ready && SpellConfig.IsFirst(SpellConfig.W) && _wardTracker.IsWardReady)
            {
                _wardManager.WardJump(Game.CursorPos, true);
            }
        }
    }
}
