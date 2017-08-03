using Adept_AIO.Champions.Riven.Core;
using Adept_AIO.Champions.Riven.Update.Miscellaneous;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util;

namespace Adept_AIO.Champions.Riven.Update.OrbwalkingEvents
{
    internal class Burst
    {
        public static void OnPostAttack(Obj_AI_Base target)
        {
            if (SpellConfig.R.Ready && Extensions.UltimateMode == UltimateMode.Second)
            {
                SpellConfig.R2.Cast(target);
                SpellManager.CastQ(target);
            }

            if (SpellConfig.Q.Ready)
            {
                SpellManager.CastQ(target);
            }
            else if (SpellConfig.W.Ready)
            {
                SpellManager.CastW(target);
            }
        }

        public static void OnUpdate()
        {
            var target = Global.TargetSelector.GetSelectedTarget();
            if (target == null || !MenuConfig.BurstMenu[target.ChampionName].Enabled)
            {
                return;
            }

            var distance = target.Distance(Global.Player);

            if (Global.Orbwalker.CanAttack() && distance <= Global.Player.AttackRange + 65)
            {
                Global.Orbwalker.Attack(target);
            }
            
            Extensions.AllIn = SummonerSpells.Flash != null && SummonerSpells.Flash.Ready;

            if (SpellConfig.W.Ready && SpellManager.InsideKiBurst(target))
            {
                SpellManager.CastW(target);
            }

            if (SpellConfig.R.Ready &&
                Extensions.UltimateMode == UltimateMode.First &&
                SpellConfig.E.Ready && distance < Extensions.FlashRange())
            {
                SpellConfig.E.Cast(target.ServerPosition);
                SpellConfig.R.Cast();
            }

            if (Extensions.AllIn && distance < Extensions.FlashRange() && SpellConfig.W.Ready && SpellConfig.R.Ready)
            {
                Global.Player.SpellBook.CastSpell(SpellSlot.W);
                SummonerSpells.Flash?.Cast(target.ServerPosition.Extend(Global.Player.ServerPosition, target.BoundingRadius));
            }   
            else if (SpellConfig.E.Ready)
            {
                SpellConfig.E.Cast(target);
            }
        }
    }
}
