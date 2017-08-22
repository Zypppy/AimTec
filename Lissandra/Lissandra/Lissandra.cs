namespace Lissandra
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
        public static Spell Q, Q2, W, E, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 725);
            Q2 = new Spell(SpellSlot.Q, 850);
            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 1050);
            R = new Spell(SpellSlot.R, 700);
            Q.SetSkillshot(0.5f, 100, 1300, false, SkillshotType.Line, false, HitChance.High);
            Q2.SetSkillshot(0.5f, 150, 1300, false, SkillshotType.Line, false, HitChance.VeryHigh);
            E.SetSkillshot(0.5f, 110, 850, false, SkillshotType.Line, false, HitChance.Medium);
        }
        public Lissandra()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("rhp", "R if HP % <", 20, 0, 100));
                ComboMenu.Add(new MenuSlider("defr", "Self R If Enemy >", 3, 1, 5));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useqh", "Use Q"));
                HarassMenu.Add(new MenuSlider("qmanah", "Q Harass if Mana % >", 70, 0, 100));
                HarassMenu.Add(new MenuBool("usewh", "Use W"));
                HarassMenu.Add(new MenuSlider("wmanah", "W Harass if Mana % >", 70, 0, 100));
                HarassMenu.Add(new MenuBool("useeh", "Use E"));
                HarassMenu.Add(new MenuSlider("emanah", "E Harass if Mana % >", 70, 0, 100));
            }
            Menu.Add(HarassMenu);
            var LaneClearMenu = new Menu("laneclear", "Lane Clear");
            {
                LaneClearMenu.Add(new MenuBool("useqlc", "Use Q"));
                LaneClearMenu.Add(new MenuSlider("qmanalc", "Q LaneClear if Mana % >", 70, 0, 100));
                LaneClearMenu.Add(new MenuBool("usewlc", "Use W"));
                LaneClearMenu.Add(new MenuSlider("wmanalc", "W LaneClear if Mana % >", 70, 0, 100));
                LaneClearMenu.Add(new MenuBool("useelc", "Use E"));
                LaneClearMenu.Add(new MenuSlider("emanalc", "E LaneClear if Mana % >", 70, 0, 100));
            }
            Menu.Add(LaneClearMenu);
            var MiscMenu = new Menu("misc", "Misc");
            {
                MiscMenu.Add(new MenuBool("WGap", "Use W on GapCloser"));
                MiscMenu.Add(new MenuBool("RInt", "Use R to Interrupt"));
            }
            Menu.Add(MiscMenu);
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
                DrawingsMenu.Add(new MenuBool("drawq2", "Draw Extended Q Range"));
                DrawingsMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawingsMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawingsMenu.Add(new MenuBool("drawr", "Draw R Range"));
            }
            Menu.Add(DrawingsMenu);
            WGap.Gapcloser.Attach(Menu, "W Anti-GapClose");
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            Gapcloser.OnGapcloser += OnGapcloser;

            LoadSpells();
            Console.WriteLine("Teemo by Zypppy - Loaded");
        }
        private void OnGapcloser(Obj_AI_Hero target, WGap.GapcloserArgs Args)
        {
            if (target != null && Args.EndPosition.Distance(Player) < W.Range && W.Ready && target.IsDashing() && target.IsValidTarget(W.Range))
            {

                W.Cast();

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
            if (Q.Ready && Menu["drawings"]["drawq2"].Enabled)
            {
                Render.Circle(Player.Position, Q2.Range, 40, Color.Indigo);
            }
            if (W.Ready && Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Indigo);
            }
            if (E.Ready && Menu["drawings"]["drawe"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.Indigo);
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
                    OnCombo();
                    break;
                case OrbwalkingMode.Mixed:
                    //OnHarass();
                    break;
                case OrbwalkingMode.Laneclear:
                    //OnLaneClear();
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
        private void OnCombo()
        {
            bool useQ = Menu["combo"]["useq"].Enabled;
            bool useE = Menu["combo"]["usee"].Enabled;
            bool useW = Menu["combo"]["usew"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;
            float RHp = Menu["combo"]["rhp"].As<MenuSlider>().Value;
            float REnemies = Menu["combo"]["defr"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(Q2.Range);
            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
            {
                if (target != null)
                {
                    Q.Cast(target);
                }
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q2.Range))
            {
                if (target != null)
                {
                    Q.Cast(target);
                }
            }
            if (W.Ready && useW && target.IsValidTarget(W.Range))
            {
                if (target != null)
                {
                    W.Cast();
                }
            }
            if (E.Ready && useE && target.IsValidTarget(E.Range))
            {
                if (target != null)
                {
                    E.Cast(target);
                }
            }
            if (R.Ready && useR && Player.HealthPercent() <= RHp && target.IsValidTarget(R.Range))
            {
                if (target != null)
                {
                    R.Cast(Player);
                }
            }
            if (R.Ready && useR && target.IsValidTarget(R.Range) && Player.CountEnemyHeroesInRange(R.Range - 50) >= REnemies)
            {
                if (target != null)
                {
                    R.Cast(Player);
                }
            }
        }
    }

}
