using System.Linq;
using Adept_AIO.Champions.Yasuo.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Extensions;
using GameObjects = Adept_AIO.SDK.Extensions.GameObjects;

namespace Adept_AIO.Champions.Yasuo.Update.OrbwalkingEvents
{
    internal class JungleClear
    {
        public static void OnPostAttack()
        {
            if (SpellConfig.E.Ready && MenuConfig.JungleClear["E"].Enabled)
            {
                var minion = GameObjects.Jungle.FirstOrDefault(x => x.IsValid && x.Distance(Global.Player) <= SpellConfig.E.Range && !x.HasBuff("YasuoDashWrapper"));

                if (minion == null)
                {
                    return;
                }

                SpellConfig.E.CastOnUnit(minion);
            }

            if (SpellConfig.Q.Ready)
            {
                var minion = GameObjects.Jungle.FirstOrDefault(x => x.Distance(Global.Player) < SpellConfig.Q.Range && x.Health > 5);
                if (minion == null)
                {
                    return;
                }

                if (Extension.CurrentMode == Mode.Tornado && !MenuConfig.JungleClear["Q3"].Enabled ||
                    Extension.CurrentMode == Mode.Normal && !MenuConfig.JungleClear["Q"].Enabled)
                {
                    return;
                }
            
                Global.Player.SpellBook.CastSpell(SpellSlot.Q, minion.ServerPosition);
            }
        }
    }
}
