using System.Linq;
using Adept_AIO.Champions.Yasuo.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;
using GameObjects = Adept_AIO.SDK.Extensions.GameObjects;

namespace Adept_AIO.Champions.Yasuo.Update.OrbwalkingEvents
{
    internal class LaneClear
    {
    
        public static void OnPostAttack()
        {
            if (MenuConfig.LaneClear["Check"].Enabled && Global.Player.CountEnemyHeroesInRange(2000) != 0)
            {
                return;
            }

            if (SpellConfig.E.Ready && MenuConfig.LaneClear["Mode"].Value == 2)
            {
                var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.Distance(Global.Player) <= SpellConfig.E.Range &&
                                                                          x.Distance(Game.CursorPos) < MenuConfig.Combo["Range"].Value &&
                                                                          !x.HasBuff("YasuoDashWrapper"));
                if (minion == null || minion.IsUnderEnemyTurret())
                {
                    return;
                }
                SpellConfig.E.CastOnUnit(minion);
            }

            if (SpellConfig.Q.Ready)
            {
                var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.Distance(Global.Player) <= SpellConfig.Q.Range);
                if (minion == null)
                {
                    return;
                }

                switch (Extension.CurrentMode)
                {
                        case Mode.Normal:
                            SpellConfig.Q.Cast(minion);
                        break;
                }
            }
        }

        public static void OnUpdate()
        {
            if (SpellConfig.Q.Ready)
            {
                switch (Extension.CurrentMode)
                {
                    case Mode.Tornado:
                    case Mode.Normal:
                        var m = GameObjects.EnemyMinions.FirstOrDefault(x => x.IsValidTarget(SpellConfig.Q.Range));
                        if (m == null)
                        {
                            return;
                        }

                        if (MenuConfig.LaneClear["Q3"].Enabled)
                        {
                            Global.Player.SpellBook.CastSpell(SpellSlot.Q, SpellConfig.Q.GetPrediction(m).CastPosition);
                        }
                        break;
                    case Mode.DashingTornado:
                    case Mode.Dashing:
                        var dashM = GameObjects.EnemyMinions.Where(x => Extension.DashDistance(x, (Obj_AI_Base) Global.Orbwalker.GetOrbwalkingTarget()) <= 220);

                        var minions = dashM as Obj_AI_Minion[] ?? dashM.ToArray();
                        if (minions.Length >= 3)
                        {
                            SpellConfig.Q.Cast(minions.FirstOrDefault());
                        }
                        break;
                }
            }

            var minion = GameObjects.EnemyMinions.FirstOrDefault(x => x.Distance(Global.Player) <= SpellConfig.E.Range &&
                                                                      x.Distance(Game.CursorPos) < MenuConfig.Combo["Range"].Value &&
                                                                     !x.HasBuff("YasuoDashWrapper"));

            if (minion == null || minion.IsUnderEnemyTurret() || MenuConfig.LaneClear["Check"].Enabled &&
                Global.Player.CountEnemyHeroesInRange(2000) != 0)
            {
                return;
            }
           
            if (SpellConfig.E.Ready || Global.Orbwalker.IsWindingUp)
            {
                switch (MenuConfig.LaneClear["Mode"].Value)
                {
                    case 1:
                        if (MenuConfig.LaneClear["EAA"].Enabled)
                        {
                            return;
                        }
                        SpellConfig.E.CastOnUnit(minion);
                        break;
                    case 2:
                        if (minion.Health < Global.Player.GetAutoAttackDamage(minion) * 1.5f)
                        {
                            return;
                        }
                        SpellConfig.E.CastOnUnit(minion);
                        break;
                }
            }
        }
    }
}
