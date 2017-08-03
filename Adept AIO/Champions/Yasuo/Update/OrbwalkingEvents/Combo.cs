using System.Linq;
using Adept_AIO.Champions.Yasuo.Core;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util;

namespace Adept_AIO.Champions.Yasuo.Update.OrbwalkingEvents
{
    internal class Combo
    {
        public static void OnPostAttack()
        {
            var target = Global.TargetSelector.GetTarget(SpellConfig.R.Range);
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
                if (Extension.CurrentMode == Mode.Normal)
                {
                    Global.Player.SpellBook.CastSpell(SpellSlot.Q, target.ServerPosition);
                }
                else
                {
                    SpellConfig.Q.Cast(target);
                }
            }

            if (!SpellConfig.E.Ready)
            {
                return;
            }

            var minion = Extension.GetDashableMinion(target);
            var walkDashMinion = Extension.WalkBehindMinion(minion, target);

            if (walkDashMinion != Vector3.Zero && MenuConfig.Combo["Walk"].Enabled && Global.Orbwalker.CanMove())
            {
                Global.Orbwalker.Move(walkDashMinion);
            }
            else if (minion != null)
            {
                if (MenuConfig.Combo["Turret"].Enabled && minion.IsUnderEnemyTurret() || MenuConfig.Combo["Dash"].Value == 0 && minion.Distance(Game.CursorPos) > MenuConfig.Combo["Range"].Value)
                {
                    return;
                }
                SpellConfig.E.CastOnUnit(minion);
            }
            else if (!target.HasBuff("YasuoDashWrapper") && target.Distance(Global.Player) <= SpellConfig.E.Range && target.Distance(Global.Player) > SpellConfig.E.Range - target.BoundingRadius)
            {
                SpellConfig.E.CastOnUnit(target);
            }
        }

        public static void OnUpdate()
        {
            var target = Global.TargetSelector.GetTarget(2500);
            if (target == null)
            {
                return;
            }

            var distance = target.Distance(Global.Player);
            var minion = Extension.GetDashableMinion(target);
            var walkDashMinion = Extension.WalkBehindMinion(minion, target);
            Extension.ExtendedMinion = walkDashMinion;

            var dashDistance = Extension.DashDistance(minion, target);

            var airbourneTargets = GameObjects.EnemyHeroes.Where(x => Extension.KnockedUp(x) && x.Distance(Global.Player) <= SpellConfig.R.Range);
            var amount = airbourneTargets as Obj_AI_Hero[] ?? airbourneTargets.ToArray();

            if (SpellConfig.R.Ready && Extension.KnockedUp(target) && (amount.Length >= MenuConfig.Combo["Count"].Value || distance > 650 || distance > 350 && minion == null))
            {
                DelayAction.Queue(MenuConfig.Combo["Delay"].Enabled ? 375 + Game.Ping / 2 : 100 + Game.Ping / 2, () => SpellConfig.R.Cast());
            }

            if (SpellConfig.Q.Ready)
            {
                switch (Extension.CurrentMode)
                {
                    case Mode.Dashing:
                        if (dashDistance <= 220)
                        {
                            SpellConfig.Q.Cast(target);
                        }
                        break;
                    case Mode.DashingTornado:
                        if (minion != null)
                        {
                            if (MenuConfig.Combo["Flash"].Enabled && dashDistance > 400 && target.IsValidTarget(425) && (Dmg.Damage(target) * 1.25 > target.Health || target.CountEnemyHeroesInRange(220) >= 2))
                            {
                                DelayAction.Queue(190, () =>
                                {
                                    SpellConfig.Q.Cast();
                                    SummonerSpells.Flash.Cast(target.Position);
                                });
                            }
                        }
                        else if (dashDistance <= 220)
                        {
                            SpellConfig.Q.Cast(target);
                        }
                        break;
                    case Mode.Tornado:
                        SpellConfig.Q.Cast(target);
                        break;
                    case Mode.Normal:
                        if (distance <= SpellConfig.Q.Range)
                        {
                            SpellConfig.Q.Cast(target);
                        }
                        else if (distance > 1200)
                        {
                            var stackableMinion = GameObjects.EnemyMinions.FirstOrDefault(x => x.IsEnemy && x.Distance(Global.Player) <= SpellConfig.Q.Range);
                            if (stackableMinion == null)
                            {
                                return;
                            }

                            SpellConfig.Q.Cast(stackableMinion);
                        }
                        break;
                }
            }

            if (!SpellConfig.E.Ready)
            {
                return;
            }
            if (walkDashMinion != Vector3.Zero && MenuConfig.Combo["Walk"].Enabled && Global.Orbwalker.CanMove())
            {
                Global.Orbwalker.Move(walkDashMinion);
            }
            else if (minion != null && distance > 475)
            {
                if (MenuConfig.Combo["Turret"].Enabled && minion.IsUnderEnemyTurret() || MenuConfig.Combo["Dash"].Value == 0 && minion.Distance(Game.CursorPos) > MenuConfig.Combo["Range"].Value)
                {
                    return;
                }

                SpellConfig.E.CastOnUnit(minion);
            }
            else if (!target.HasBuff("YasuoDashWrapper") && distance <= SpellConfig.E.Range && distance > SpellConfig.E.Range - target.BoundingRadius)
            {
                SpellConfig.E.CastOnUnit(target);
            }
        }
    }
}
