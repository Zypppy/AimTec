﻿namespace Yasuo
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using Aimtec;
    using Aimtec.SDK.Prediction.Health;
    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Orbwalking;
    using Aimtec.SDK.TargetSelector;
    using Aimtec.SDK.Util.Cache;
    using Aimtec.SDK.Prediction.Skillshots;
    using Aimtec.SDK.Util;
    using Aimtec.SDK.Events;

    using Spell = Aimtec.SDK.Spell;

    internal class Yasuo
    {
        public static Menu Menu = new Menu("Yasuo by Zypppy", "Yasuo by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, Q2, W, E, R, Ignite;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 500f);
            Q.SetSkillshot(0.4f, 50f, float.MaxValue, false, SkillshotType.Line);
            Q2 = new Spell(SpellSlot.Q, 1100);
            Q2.SetSkillshot(0.333f, 90f, 1203f, false, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 400);
            W.SetSkillshot(0.25f, 100f, 850f, false, SkillshotType.Line);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 1200);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner1, 600);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner2, 600);
        }

        public Yasuo()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("combo", "Combo");
            {
                Combo.Add(new MenuBool("useq", "Use Q"));
                Combo.Add(new MenuBool("usee", "Use E"));
                Combo.Add(new MenuBool("useem", "Use E On Minions"));
                Combo.Add(new MenuBool("useefgap", "Use E GapClose On Minions"));
                Combo.Add(new MenuSlider("useegap", "Use E-GapCloser When Enemy In Range >=", 475, 375, 1300));
                Combo.Add(new MenuBool("usersmart", "Use Smart R"));
                Combo.Add(new MenuSlider("userhp", "When Enemy Hp <= %", 70, 0, 100));
                Combo.Add(new MenuSlider("userhit", "Or When Kocked Up Enemy >=", 2, 0, 5));
                Combo.Add(new MenuBool("userauto", "Auto R"));
                Combo.Add(new MenuSlider("userautohit", "When Kocked Up enemy >=", 3, 0, 5));
                Combo.Add(new MenuSlider("useraround", "Or When Enemy In Range <=", 2, 0, 5));
                Combo.Add(new MenuSlider("usermyhp", "Or When My Hp % >=", 50, 0, 100));
            }
            Menu.Add(Combo);
            var Harass = new Menu("harass", "Harass");
            {
                Harass.Add(new MenuBool("useq", "Use Q"));
                Harass.Add(new MenuBool("useq2", "Use Tornado Q"));
            }
            Menu.Add(Harass);
            var LaneClear = new Menu("laneclear", "Lane Clear");
            {
                LaneClear.Add(new MenuBool("useq", "Use Q To Lane Clear"));
                LaneClear.Add(new MenuBool("useq2", "Use Tornado Q To Lane Clear"));
                LaneClear.Add(new MenuBool("usee", "Use E To Lane Clear"));
            }
            Menu.Add(LaneClear);
            var LastHit = new Menu("lasthit", "Last Hit");
            {
                LastHit.Add(new MenuBool("useq", "Use Q To Last Hit"));
                LastHit.Add(new MenuBool("useq2", "Use Tornade Q To Last Hit"));
                LastHit.Add(new MenuBool("usee", "Use E To Last Hit"));
            }
            Menu.Add(LastHit);
            var JungleClear = new Menu("jungleclear", "Jungle Clear");
            {
                JungleClear.Add(new MenuBool("useq", "Use Q To Jungle Clear"));
                JungleClear.Add(new MenuBool("useq2", "Use Tornado Q To Jungle Clear"));
                JungleClear.Add(new MenuBool("usee", "Use E To Jungle Clear"));
            }
            Menu.Add(JungleClear);
            var Killsteal = new Menu("killsteal", "Killsteal");
            {
                Killsteal.Add(new MenuBool("useq", "Use Q To Killsteal"));
                Killsteal.Add(new MenuBool("useq2", "Use Tornado Q To Killsteal"));
                Killsteal.Add(new MenuBool("usee", "Use E To Killsteal"));
                Killsteal.Add(new MenuBool("user", "Use R To Killsteal"));
                Killsteal.Add(new MenuBool("ignite", "Use Ignite To Killsteal"));
            }
            Menu.Add(Killsteal);
            var Drawings = new Menu("drawings", "Drawings");
            {
                Drawings.Add(new MenuBool("drawq", "Draw Q"));
                Drawings.Add(new MenuBool("drawq2", "Draw Tornado Q"));
                Drawings.Add(new MenuBool("drawe", "Draw E"));
                Drawings.Add(new MenuBool("drawegap", "Draw E GapClosing Range"));
                Drawings.Add(new MenuBool("drawr", "Draw R"));
                Drawings.Add(new MenuBool("drawdmg", "Draw DMG"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Yasuo by Zypppy - Loaded");
        }
        private static int IgniteDamages
        {
            get
            {
                int[] Hello = new int[] { 70, 90, 110, 130, 150, 170, 190, 210, 230, 250, 270, 290, 310, 330, 350, 370, 390, 410 };

                return Hello[Player.Level - 1];
            }
        }
        public static readonly List<string> SpecialChampions = new List<string> { "Annie", "Jhin" };
        public static int SxOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 1 : 10;
        }
        public static int SyOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 3 : 20;
        }
        private void Render_OnPresent()
        {
            Vector2 mymom;
            var heropos = Render.WorldToScreen(Player.Position, out mymom);
            var xaOffset = (int)mymom.X;
            var yaOffset = (int)mymom.Y;

            if (Menu["drawings"]["drawq"].Enabled && Q.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "YasuoQ3W")
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Azure);
            }
            if (Menu["drawings"]["drawq2"].Enabled && Q2.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "YasuoQ3W")
            {
                Render.Circle(Player.Position, Q2.Range, 40, Color.Beige);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.BlueViolet);
            }
            if (Menu["drawings"]["drawegap"].Enabled)
            {
                Render.Circle(Player.Position, Menu["combo"]["useegap"].As<MenuSlider>().Value, 40, Color.BlueViolet);
            }
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Chocolate);
            }
            if (Menu["drawings"]["drawdmg"].Enabled)
            {
                ObjectManager.Get<Obj_AI_Base>()
                  .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(1500))
                  .ToList()
                  .ForEach(
                   unit =>
                   {
                       var heroUnit = unit as Obj_AI_Hero;
                       int width = 103;
                       int height = 8;
                       int xOffset = SxOffset(heroUnit);
                       int yOffset = SyOffset(heroUnit);
                       var barPos = unit.FloatingHealthBarPosition;
                       barPos.X += xOffset;
                       barPos.Y += yOffset;

                       var drawEndXPos = barPos.X + width * (unit.HealthPercent() / 100);
                       var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R)
                       ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R))) / unit.MaxHealth * 100 / 100)
                       : 0));
                       Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R) ? Color.GreenYellow : Color.Orange);

                   });

            }
        }
        private void Game_OnUpdate()
        {
            if (Player.IsDead || MenuGUI.IsChatOpen())
            {
                return;
            }
            switch (Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    OnCombo();
                    break;
                case OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
                case OrbwalkingMode.Laneclear:
                    OnLaneClear();
                    OnJungleClear();
                    break;
                case OrbwalkingMode.Lasthit:
                    OnLastHit();
                    break;
            }
            Killsteal();
        }
        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True, bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }
        private void Killsteal()
        {
            if (Q.Ready && Menu["killsteal"]["useq"].Enabled && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "YasuoQ3W")
            {
                var besttarget = GetBestKillableHero(Q, DamageType.Physical, false);
                var QPrediction = Q.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.Q) >= besttarget.Health && !Player.IsDashing() && besttarget.IsValidTarget(Q.Range))
                {
                    if (QPrediction.HitChance >= HitChance.High)
                    {
                        Q.Cast(QPrediction.CastPosition);
                    }
                }
            }
            if (Q.Ready && Menu["killsteal"]["useq2"].Enabled && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "YasuoQ3W")
            {
                var besttarget = GetBestKillableHero(Q, DamageType.Physical, false);
                var Q2Prediction = Q2.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.Q) >= besttarget.Health && !Player.IsDashing() && besttarget.IsValidTarget(Q2.Range))
                {
                    if (Q2Prediction.HitChance >= HitChance.High)
                    {
                        Q2.Cast(Q2Prediction.CastPosition);
                    }
                }
            }
            if (E.Ready && Menu["killsteal"]["usee"].Enabled)
            {
                var besttarget = GetBestKillableHero(E, DamageType.Mixed, false);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.E) >= besttarget.Health && besttarget.IsValidTarget(E.Range))
                {
                    E.Cast(besttarget);
                }
            }
            if (R.Ready && Menu["killsteal"]["user"].Enabled)
            {
                var besttarget = GetBestKillableHero(R, DamageType.Physical, false);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.R) >= besttarget.Health && besttarget.IsValidTarget(R.Range) && (besttarget.HasBuffOfType(BuffType.Knockup) || besttarget.HasBuffOfType(BuffType.Knockback)))
                {
                    R.Cast();
                }
            }
            if (Menu["killsteal"]["ignite"].Enabled && Ignite != null)
            {
                var besttarget = GetBestKillableHero(Ignite, DamageType.True, false);
                if (besttarget != null && IgniteDamages - 100 >= besttarget.Health && besttarget.IsValidTarget(Ignite.Range))
                {
                    Ignite.CastOnUnit(besttarget);
                }
            }
        }
        public static Obj_AI_Hero GetBestEnemyHeroTarget()
        {
            return GetBestEnemyHeroTargetInRange(float.MaxValue);
        }
        public static Obj_AI_Hero GetBestEnemyHeroTargetInRange(float range)
        {
            var ts = TargetSelector.Implementation;
            var target = ts.GetTarget(range);
            if (target != null && target.IsValidTarget() && !Invulnerable.Check(target))
            {
                return target;
            }
            var firstTarget = ts.GetOrderedTargets(range)
                .FirstOrDefault(t => t.IsValidTarget() && !Invulnerable.Check(t));
            if (firstTarget != null)
            {
                return firstTarget;
            }
            return null;
        }
        private void OnCombo()
        {
            var target = GetBestEnemyHeroTargetInRange(1500);
            bool useQ = Menu["combo"]["useq"].Enabled;
            bool useQ2 = Menu["combo"]["useq"].Enabled;
            bool useE = Menu["combo"]["usee"].Enabled;
            bool useEM = Menu["combo"]["useem"].Enabled;
            bool useEGap = Menu["combo"]["useefgap"].Enabled;
            float distanceE = Menu["combo"]["useegap"].As<MenuSlider>().Value;
            bool useSmartR = Menu["combo"]["usersmart"].Enabled;
            float tRHP = Menu["combo"]["userhp"].As<MenuSlider>().Value;
            float hitR = Menu["combo"]["userhit"].As<MenuSlider>().Value;
            bool useR = Menu["combo"]["userauto"].Enabled;
            float hittR = Menu["combo"]["userautohit"].As<MenuSlider>().Value;
            float aroundTR = Menu["combo"]["useraround"].As<MenuSlider>().Value;
            float selfhpR = Menu["combo"]["usermyhp"].As<MenuSlider>().Value;
            var QPrediction = Q.GetPrediction(target);
            var Q2Prediction = Q2.GetPrediction(target);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Q.Ready && target.IsValidTarget(Q.Range) && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "YasuoQ3W" && !Player.IsDashing())
            {
                if (QPrediction.HitChance >= HitChance.High)
                {
                    Q.Cast(QPrediction.CastPosition);
                }
            }
            if (Q.Ready && target.IsValidTarget(375) && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "YasuoQ3W" && Player.IsDashing())
            {
                Q.Cast();
            }
            if (Q.Ready && target.IsValidTarget(Q2.Range) && useQ2 && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "YasuoQ3W" && !Player.IsDashing())
            {
                if (Q2Prediction.HitChance >= HitChance.Medium)
                {
                    Q2.Cast(Q2Prediction.CastPosition);
                }
            }
            if (Q.Ready && target.IsValidTarget(375) && useQ2 && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "YasuoQ3W" && Player.IsDashing())
            {
                Q2.Cast();
            }
            if (E.Ready && target.IsValidTarget(E.Range))
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
                {
                    if (useE && !target.HasBuff("YasuoDashWrapper"))
                    {
                        E.Cast(target);
                    }
                    else if (useE && target.HasBuff("YasuoDashWrapper") && minion.Distance(target) <= Q.Range && useEM)
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }
            //if (useEGap && E.Ready && target.IsValidTarget(distanceE))
            //{
            //    if (target.Distance(Player) > E.Range)
            //    {
            //foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
            // {
            //  if (minion.IsValidTarget(E.Range) && minion != null)
            //{
            //if (minion.Distance(Player) <= E.Range && minion.Distance(target) <= distanceE && target.Distance(Player) > E.Range)
            //{
            //E.CastOnUnit(minion);
            //}
            //}
            //}
            //}
            //}
            if (E.Ready && useEGap && Player.Distance(target) > Player.GetFullAttackRange(target))
            {
                var validMinions = GetEnemyLaneMinionsTargetsInRange(E.Range).Where(m => !m.HasBuff("YasuoDashWrapper"));
                foreach (var minion in validMinions)
                {
                    var distanceToMinionAfterDash = E.Range - Player.Distance(minion);
                    if (Player.ServerPosition.Extend(minion.ServerPosition, distanceToMinionAfterDash).Distance(target) < Player.Distance(target))
                    {
                        E.CastOnUnit(minion);
                    }
                }
            }
            if (R.Ready && target.IsValidTarget(R.Range))
            {
                if (useSmartR)
                {
                    var KockedUp = GameObjects.EnemyHeroes
                    .Where(x => x.IsValidTarget(R.Range))
                    .Where(x => x.HasBuffOfType(BuffType.Knockback) || x.HasBuffOfType(BuffType.Knockup));

                    var enemies = KockedUp as IList<Obj_AI_Hero> ?? KockedUp.ToList();
                    if (enemies.Count >= hittR)
                    {
                        R.CastOnUnit(enemies.FirstOrDefault());
                    }
                    else if (target.HealthPercent() <= tRHP)
                    {
                        R.Cast();
                    }
                }
                else if (useR)
                {
                    var KockedUp = GameObjects.EnemyHeroes
                    .Where(x => x.IsValidTarget(R.Range))
                    .Where(x => x.HasBuffOfType(BuffType.Knockback) || x.HasBuffOfType(BuffType.Knockup));

                    var enemies = KockedUp as IList<Obj_AI_Hero> ?? KockedUp.ToList();
                    if (enemies.Count >= hittR)
                    {
                        R.CastOnUnit(enemies.FirstOrDefault());
                    }
                    else if (target.CountEnemyHeroesInRange(700) <= aroundTR && (target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Knockback)))
                    {
                        R.Cast();
                    }
                    else if (Player.HealthPercent() >= selfhpR && (target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Knockback)))
                    {
                        R.Cast();
                    }
                }
            }
        }
        private void OnHarass()
        {
            var target = GetBestEnemyHeroTargetInRange(1500);
            bool useQ = Menu["harass"]["useq"].Enabled;
            bool useQ2 = Menu["harass"]["useq2"].Enabled;
            var QPrediction = Q.GetPrediction(target);
            var Q2Prediction = Q2.GetPrediction(target);
            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && target.IsValidTarget(Q.Range) && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "YasuoQ3W" && !Player.IsDashing())
            {
                if (QPrediction.HitChance >= HitChance.High)
                {
                    Q.Cast(QPrediction.CastPosition);
                }
            }
            if (Q.Ready && target.IsValidTarget(375) && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "YasuoQ3W" && Player.IsDashing())
            {
                Q.Cast();
            }
            if (Q.Ready && target.IsValidTarget(Q2.Range) && useQ2 && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "YasuoQ3W" && !Player.IsDashing())
            {
                if (Q2Prediction.HitChance >= HitChance.Medium)
                {
                    Q2.Cast(Q2Prediction.CastPosition);
                }
            }
            if (Q.Ready && target.IsValidTarget(375) && useQ2 && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "YasuoQ3W" && Player.IsDashing())
            {
                Q2.Cast();
            }
        }
        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsValidTarget(range)).ToList();
        }
        private void OnLaneClear()
        {
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q2.Range))
            {
                bool useQ = Menu["laneclear"]["useq"].Enabled;
                bool useQ2 = Menu["laneclear"]["useq2"].Enabled;
                bool useE = Menu["laneclear"]["usee"].Enabled;
                var QPrediction = Q.GetPrediction(minion);
                var Q2Prediction = Q2.GetPrediction(minion);
                if (!minion.IsValidTarget())
                {
                    return;
                }
                if (Q.Ready)
                {
                    if (useQ && minion.IsValidTarget(Q.Range) && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "YasuoQ3W")
                    {
                        if (QPrediction.HitChance >= HitChance.High)
                        {
                            Q.Cast(QPrediction.CastPosition);
                        }
                    }
                    else if (useQ2 && minion.IsValidTarget(Q2.Range) && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "YasuoQ3W" && !Player.IsDashing())
                    {
                        if (Q2Prediction.HitChance >= HitChance.Medium)
                        {
                            Q2.Cast(Q2Prediction.CastPosition);
                        }
                    }
                    else if (useQ2 && minion.IsValidTarget(375) && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "YasuoQ3W" && Player.IsDashing())
                    {
                        Q2.Cast();
                    }
                    else if (Menu["lasthit"]["useq"].Enabled && Player.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "YasuoQ3W")
                    {
                        if (QPrediction.HitChance >= HitChance.High)
                        {
                            Q.Cast(QPrediction.CastPosition);
                        }
                    }
                }
                if (E.Ready && useE && minion.IsValidTarget(E.Range))
                {
                    E.CastOnUnit(minion);
                }
            }
        }
        private void OnLastHit()
        {
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q2.Range))
            {
                bool useQ = Menu["lasthit"]["useq"].Enabled;
                bool useQ2 = Menu["lasthit"]["useq2"].Enabled;
                bool useE = Menu["lasthit"]["usee"].Enabled;
                var QPrediction = Q.GetPrediction(minion);
                var Q2Prediction = Q2.GetPrediction(minion);
                if (!minion.IsValidTarget())
                {
                    return;
                }
                if (Q.Ready)
                {
                    if (useQ && minion.IsValidTarget(Q.Range) && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "YasuoQ3W" && Player.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health)
                    {
                        if (QPrediction.HitChance >= HitChance.High)
                        {
                            Q.Cast(QPrediction.CastPosition);
                        }
                    }
                    else if (useQ2 && minion.IsValidTarget(Q2.Range) && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "YasuoQ3W" && !Player.IsDashing() && Player.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health)
                    {
                        if (Q2Prediction.HitChance >= HitChance.Medium)
                        {
                            Q2.Cast(QPrediction.CastPosition);
                        }
                    }
                }
                if (E.Ready && useE && minion.IsValidTarget(E.Range) && Player.GetSpellDamage(minion, SpellSlot.E) >= minion.Health)
                {
                    E.CastOnUnit(minion);
                }
            }
        }
        public static List<Obj_AI_Minion> GetGenericJungleMinionsTargets()
        {
            return GetGenericJungleMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetGenericJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.Jungle.Where(m => !GameObjects.JungleSmall.Contains(m) && m.IsValidTarget(range)).ToList();
        }
        private void OnJungleClear()
        {
            foreach (var minion in GameObjects.Jungle.Where(m => m.IsValidTarget(Q2.Range)).ToList())
            {
                bool useQ = Menu["jungleclear"]["useq"].Enabled;
                bool useQ2 = Menu["jungleclear"]["useq2"].Enabled;
                bool useE = Menu["jungleclear"]["usee"].Enabled;
                var QPrediction = Q.GetPrediction(minion);
                var Q2Prediction = Q2.GetPrediction(minion);
                if (!minion.IsValidTarget() || !minion.IsValidSpellTarget())
                {
                    return;
                }
                if (Q.Ready)
                {
                    if (useQ && minion.IsValidTarget(Q.Range) && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "YasuoQ3W")
                    {
                        if (QPrediction.HitChance >= HitChance.High)
                        {
                            Q.Cast(QPrediction.CastPosition);
                        }
                    }
                    else if (useQ2 && minion.IsValidTarget(Q2.Range) && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "YasuoQ3W" && !Player.IsDashing())
                    {
                        if (Q2Prediction.HitChance >= HitChance.Medium)
                        {
                            Q.Cast(QPrediction.CastPosition);
                        }
                    }
                }
                if (E.Ready && useE && minion.IsValidTarget(E.Range))
                {
                    E.CastOnUnit(minion);
                }
            }
        }
    }
}