using System;
using Adept_AIO.Champions.Yasuo.Core;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util;

namespace Adept_AIO.Champions.Yasuo.Update.OrbwalkingEvents
{
    internal class Beyblade
    {
        public static void OnPostAttack()
        {
            var target = Global.TargetSelector.GetSelectedTarget();
            if (target == null)
            {
                return;
            }

            if (SpellConfig.R.Ready && Extension.KnockedUp(target))
            {
                SpellConfig.R.Cast();
            }
            else if (SpellConfig.Q.Ready)
            {
                SpellConfig.Q.Cast(target);
            }
        }

        public static void OnKeyPressed()
        {
            var target = Global.TargetSelector.GetSelectedTarget();
            if (target == null)
            {
                return;
            }


            var distance = target.Distance(Global.Player);
            var minion = Extension.GetDashableMinion(target);
            var dashDistance = Extension.DashDistance(minion, target);

            if (Global.Orbwalker.CanAttack() && distance <= Global.Player.AttackRange + 65)
            {
                Global.Orbwalker.Attack(target);
            }

            if (SpellConfig.R.Ready && distance > 300 && Extension.KnockedUp(target))
            {
                SpellConfig.R.Cast();
            }

            if (SpellConfig.E.Ready)
            {
                if (!target.HasBuff("YasuoDashWrapper") && distance <= SpellConfig.E.Range && !Extension.KnockedUp(target))
                {
                    SpellConfig.E.CastOnUnit(target);
                }
                else if (minion != null)
                {
                    SpellConfig.E.CastOnUnit(minion);
                }
            }

            if (SpellConfig.Q.Ready)
            {
                switch (Extension.CurrentMode)
                {
                    case Mode.DashingTornado:
                    case Mode.Dashing:
                        if (minion != null)
                        {
                            Console.WriteLine(dashDistance);
                            if (MenuConfig.Combo["Flash"].Enabled && distance > 220 && dashDistance <= 350)
                            {
                                DelayAction.Queue(145, () =>
                                {
                                    SpellConfig.Q.Cast();
                                    SummonerSpells.Flash.Cast(target.Position);
                                });
                            }
                        }
                        break;
                    case Mode.Normal:
                        if (target.IsValidTarget(SpellConfig.Q.Range))
                        {
                            SpellConfig.Q.CastOnUnit(target);
                        }
                        break;
                }
            }
        }
    }
}
