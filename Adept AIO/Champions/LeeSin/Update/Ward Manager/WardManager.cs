using System.Linq;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.LeeSin.Update.Ward_Manager
{
    internal class WardManager : IWardManager
    {
        private readonly IWardTracker _wardTracker;

        public float LastTimeCasted { get; private set; }

        public bool IsWardReady()
        {
            return _wardTracker.WardNames.Any(Items.CanUseItem) && Game.TickCount - _wardTracker.LastWardCreated > 1500 || _wardTracker.LastWardCreated == 0;
        }

        public WardManager(IWardTracker wardTracker)
        {
            _wardTracker = wardTracker;
        }

        public void WardJump(Vector3 position, bool maxRange)
        {
            if (Game.TickCount - _wardTracker.LastWardCreated < 1500 && _wardTracker.LastWardCreated > 0)
            {
                return;
            }

            var ward = _wardTracker.WardNames.FirstOrDefault(Items.CanUseItem);

            if (ward == null)
            {
                return;
            }

            if (maxRange)
            {
                position = Global.Player.ServerPosition.Extend(position, 600); 
            }

            Items.CastItem(ward, position);
            LastTimeCasted = Game.TickCount;
            _wardTracker.LastWardCreated = Game.TickCount;
            _wardTracker.WardPosition = position;

            if (!NavMesh.WorldToCell(position).Flags.HasFlag(NavCellFlags.Building | NavCellFlags.Wall))
            {
                _wardTracker.IsAtWall = false;
                Global.Player.SpellBook.CastSpell(SpellSlot.W, position);
            }
            else
            {
                _wardTracker.IsAtWall = true;
            }
        }
    }
}
