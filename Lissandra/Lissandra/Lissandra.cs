﻿namespace Lissandra
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
    using WGap;

    using Spell = Aimtec.SDK.Spell;
    using Aimtec.SDK.Events;

    internal class Lissandra
    {
        public static Menu Menu = new Menu("Lissandra by Zypppy", "Lissandra by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, E2, R;
        private MissileClient missiles;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 825f);//LissandraQ
            Q.SetSkillshot(0.5f, 75f, 2200f, false, SkillshotType.Line, false);
            W = new Spell(SpellSlot.W, 450f);//LissandraW
            E = new Spell(SpellSlot.E, 1025f);//LissandraE Toggle == 1
            E.SetSkillshot(0.5f, 125f, 850f, false, SkillshotType.Line, false);
            E2 = new Spell(SpellSlot.E, 20000f);//Toggle == 2
            R = new Spell(SpellSlot.R, 550f);//LissandraR
        }
        public Lissandra()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
                ComboMenu.Add(new MenuBool("useegap", "Use E Gapcloser"));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("rhp", "R if HP % <", 20, 0, 100));
                ComboMenu.Add(new MenuSlider("defr", "Self R If Enemy >", 3, 1, 5));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useqh", "Use Q"));
                HarassMenu.Add(new MenuSlider("qmanah", "Q Harass if Mana % >", 70, 0, 100));
            }
            Menu.Add(HarassMenu);
            var LaneClearMenu = new Menu("laneclear", "Lane Clear");
            {
                LaneClearMenu.Add(new MenuBool("useqlc", "Use Q"));
                LaneClearMenu.Add(new MenuSlider("qmanalc", "Q LaneClear if Mana % >", 70, 0, 100));
                LaneClearMenu.Add(new MenuBool("usewlc", "Use W"));
                LaneClearMenu.Add(new MenuSlider("wmanalc", "W LaneClear if Mana % >", 70, 0, 100));
                LaneClearMenu.Add(new MenuSlider("wmhitlc", "W Minion Hit >", 2, 0, 10));
            }
            Menu.Add(LaneClearMenu);
            var KillstealMenu = new Menu("ks", "Killsteal");
            {
                KillstealMenu.Add(new MenuBool("QKS", "Use Q to Killsteal"));
                KillstealMenu.Add(new MenuBool("WKS", "Use W to Killsteal"));
                KillstealMenu.Add(new MenuBool("RKS", "Use R to Killsteal"));
            }
            Menu.Add(KillstealMenu);
            var DrawingsMenu = new Menu("drawings", "Drawings");
            {
                DrawingsMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawingsMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawingsMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawingsMenu.Add(new MenuBool("drawepath", "Draw E Path"));
                DrawingsMenu.Add(new MenuBool("drawr", "Draw R Range"));
            }
            Menu.Add(DrawingsMenu);
            WGap.Gapcloser.Attach(Menu, "W Anti-GapClose");
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            Gapcloser.OnGapcloser += OnGapcloser;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDestroy += OnDestroy;

            LoadSpells();
            Console.WriteLine("Lissandra by Zypppy - Loaded");
        }
        private void OnGapcloser(Obj_AI_Hero target, WGap.GapcloserArgs Args)
        {
            if (target != null && Args.EndPosition.Distance(Player) < W.Range && W.Ready && target.IsDashing() && target.IsValidTarget(W.Range))
            {
                W.Cast(Args.EndPosition);
            }
        }

        //public void OnCreate(GameObject obj)
        //{
        //    if (obj != null && obj.IsValid)
        //    {
        //        Console.WriteLine(obj.Name);
        //    }
        //}
        public void OnCreate(GameObject obj)
        {
            var missile = obj as MissileClient;
            if (missile == null)
            {
                return;
            }

            if (missile.SpellCaster == null || !missile.SpellCaster.IsValid ||
                missile.SpellCaster.Team != ObjectManager.GetLocalPlayer().Team)
            {
                return;
            }
            var hero = missile.SpellCaster as Obj_AI_Hero;
            if (hero == null)
            {
                return;
            }
            if (missile.SpellData.Name == "LissandraEMissile")
            {
                missiles = missile;
            }

        }
        private void OnDestroy(GameObject obj)
        {
            var missile = obj as MissileClient;
            if (missile == null || !missile.IsValid)
            {
                return;
            }

            if (missile.SpellCaster == null || !missile.SpellCaster.IsValid ||
                missile.SpellCaster.Team != ObjectManager.GetLocalPlayer().Team)
            {
                return;
            }
            var hero = missile.SpellCaster as Obj_AI_Hero;
            if (hero == null)
            {
                return;
            }
            if (missile.SpellData.Name == "LissandraEMissile")
            {
                missiles = null;
            }
        }

        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;

            if (Q.Ready && Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Indigo);
            }
            if (W.Ready && Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Indigo);
            }
            if (E.Ready)
            {
                if (Menu["drawings"]["drawe"].Enabled)
                {
                    Render.Circle(Player.Position, E.Range, 40, Color.Indigo);
                }
                else if (Menu["drawings"]["drawepath"].Enabled && missiles != null)
                {
                    Render.Circle(missiles.ServerPosition, 300, 40, Color.DeepPink);
                }
            }
            if (R.Ready && Menu["drawings"]["drawr"].Enabled)
            {
              Render.Circle(Player.Position, R.Range, 40, Color.Indigo);
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
                    QCombo();
                    WCombo();
                    ECombo();
                    E2Combo();
                    RCombo();
                    break;
                case OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
                case OrbwalkingMode.Laneclear:
                    OnLaneClear();
                    break;
            }
            Killsteal();
        }
        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }
        private void Killsteal()
        {
            if (Q.Ready && Menu["ks"]["QKS"].Enabled)
            {
                var bestTarget = GetBestKillableHero(Q, DamageType.Magical, false);
                if (bestTarget != null && Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health && bestTarget.IsValidTarget(Q.Range))
                {
                    Q.Cast(bestTarget);
                }
            }

            if (W.Ready && Menu["ks"]["WKS"].Enabled)
            {
                var bestTarget = GetBestKillableHero(W, DamageType.Magical, false);
                if (bestTarget != null && Player.GetSpellDamage(bestTarget, SpellSlot.W) >= bestTarget.Health && bestTarget.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }

            if (R.Ready && Menu["ks"]["RKS"].Enabled)
            {
                var bestTarget = GetBestKillableHero(R, DamageType.Magical, false);
                if (bestTarget != null && Player.GetSpellDamage(bestTarget, SpellSlot.R) >= bestTarget.Health && bestTarget.IsValidTarget(R.Range))
                {
                    R.Cast(bestTarget);
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

        private void QCombo()
        {
            var target = GetBestEnemyHeroTargetInRange(Q.Range);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool useQ = Menu["combo"]["useq"].Enabled;
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }

        }

        private void WCombo()
        {
            var target = GetBestEnemyHeroTargetInRange(W.Range);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool useW = Menu["combo"]["usew"].Enabled;
            if (W.Ready && useW && target.IsValidTarget(W.Range))
            {
                W.Cast();
            }

        }

        private void ECombo()
        {
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool useE = Menu["combo"]["usee"].Enabled;
            if (E.Ready)
            {
                if (target.IsValidTarget(E.Range) && useE && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 1)
                {
                    E.Cast(target);
                }
            }
        }

        private void E2Combo()
        {
            var target = GetBestEnemyHeroTargetInRange(E2.Range);
            if (!target.IsValidTarget())
            {
                return;
            }
            
            bool useEGap = Menu["combo"]["useegap"].Enabled;
            if (missiles != null && E.Ready && target.IsValidTarget(300f, false, false, missiles.ServerPosition) && useEGap && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 2)
            {
                E.Cast();
            }
        }

        private void RCombo()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool useR = Menu["combo"]["user"].Enabled;
            float RHp = Menu["combo"]["rhp"].As<MenuSlider>().Value;
            float REnemies = Menu["combo"]["defr"].As<MenuSlider>().Value;
            if (R.Ready && useR && Player.HealthPercent() <= RHp && target.IsValidTarget(R.Range))
            {
                R.Cast(Player);
            }
            if (R.Ready && useR && target.IsValidTarget(R.Range) && Player.CountEnemyHeroesInRange(R.Range - 50) >= REnemies)
            {
                R.Cast(Player);
            }

        }

        
        private void OnHarass()
        {
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool useQ = Menu["harass"]["useqh"].Enabled;
            float manaQ = Menu["harass"]["qmanah"].As<MenuSlider>().Value;
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range) && Player.ManaPercent() >= manaQ)
            {
                Q.Cast(target);
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
            
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
            {
                if (!minion.IsValidTarget())
                {
                    return;
                }

                bool useQ = Menu["laneclear"]["useqlc"].Enabled;
                float manaQ = Menu["laneclear"]["qmanalc"].As<MenuSlider>().Value;
                if (Q.Ready && useQ && Player.ManaPercent() >= manaQ && minion.IsValidTarget(Q.Range))
                {
                    Q.Cast(minion);
                }

                bool useW = Menu["laneclear"]["usewlc"].Enabled;
                float manaW = Menu["laneclear"]["wmanalc"].As<MenuSlider>().Value;
                float hitW = Menu["laneclear"]["wmhitlc"].As<MenuSlider>().Value;
                if (W.Ready && useW && Player.ManaPercent() >= manaW && minion.IsValidTarget(W.Range) && GameObjects.EnemyMinions.Count(h => h.IsValidTarget(W.Range, false, false, minion.ServerPosition)) >= hitW)
                {
                    if (minion != null)
                    {
                        W.Cast();
                    }
                }
            }
        }
    }

}
