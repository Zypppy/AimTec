using System;
using System.Linq;
using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.LeeSin.Core.Insec_Manager
{
    class Insec_Manager : IInsec_Manager
    {
        public int InsecKickValue { get; set; }
        public int InsecPositionValue { get; set; }

        private readonly ISpellConfig SpellConfig;

        public Insec_Manager(ISpellConfig spellConfig)
        {
            SpellConfig = spellConfig;
        }

        public float DistanceBehindTarget(Obj_AI_Base target)
        {
            return Math.Min((Global.Player.BoundingRadius + target.BoundingRadius + 50) * 1.25f, SpellConfig.R.Range);
        }

        public Vector3 InsecPosition(Obj_AI_Base target)
        {
            return target.ServerPosition + (target.ServerPosition - GetTargetEndPosition()).Normalized() *
                   DistanceBehindTarget(target);
        }

        public Vector3 GetTargetEndPosition()
        {
            var ally = GameObjects.AllyHeroes.FirstOrDefault(x => x.Distance(Global.Player) <= 2000);
            var turret = GameObjects.AllyTurrets.OrderBy(x => x.Distance(Global.Player)).FirstOrDefault();

            switch (InsecPositionValue)
            {
                case 0:

                    if (turret != null)
                    {
                        return turret.ServerPosition;
                    }
                    else if (ally != null)
                    {
                        return ally.ServerPosition;
                    }
                    break;
                case 1:
                    if (ally != null)
                    {
                        return ally.ServerPosition;
                    }
                    else if (turret != null)
                    {
                        return turret.ServerPosition;
                    }
                    break;
            }
            return Vector3.Zero;
        }
    }
}
