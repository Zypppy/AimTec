namespace Morgana
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
    using EGap;

    using Spell = Aimtec.SDK.Spell;
    using Aimtec.SDK.Events;

    internal class Morgana
    {
        public static Menu Menu = new Menu("Morgana by Zypppy", "Morgana by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1300f);
            Q.SetSkillshot(0.5f, 70f, 1200f, true, SkillshotType.Line, false);
            W = new Spell(SpellSlot.W, 900f);
            W.SetSkillshot(0.5f, 280f, float.MaxValue, false, SkillshotType.Circle, false);
            E = new Spell(SpellSlot.E, 800f);
            R = new Spell(SpellSlot.R, 625f);
        }

        public Morgana()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuList("wo", "W Options", new [] {"Always", "Hard CC Targets" }, 1));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("hitr", "Min enemies to hit with R", 1, 1, 5));

            }
            Menu.Add(ComboMenu);

            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("kq", "Killsteal with Q"));
            }
            Menu.Add(KSMenu);

            var miscmenu = new Menu("misc", "Misc");
            {
                miscmenu.Add(new MenuBool("autoq", "Auto Q on CC"));
                miscmenu.Add(new MenuBool("autow", "Auto W on CC"));
            }
            Menu.Add(miscmenu);

            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
            }

            Menu.Add(DrawMenu);
            EGap.Gapcloser.Attach(Menu, "E Anti-GapCloser");

            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            Gapcloser.OnGapcloser += OnGapcloser;

            LoadSpells();
            Console.WriteLine("Morgana by Zypppy - Loaded");
        }

        private void OnGapcloser(Obj_AI_Hero target, EGap.GapcloserArgs Args)
        {
            if (target != null && Args.EndPosition.Distance(Player) <= 800 && E.Ready && target.IsDashing() && target.IsValidTarget(E.Range))
            {

                E.Cast();

            }
        }

        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;

            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Indigo);
            }

            if (Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Fuchsia);
            }

            if (Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.DeepPink);
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
                    break;
                case OrbwalkingMode.Laneclear:
                    break;

            }
            Killsteal();
            if (Menu["misc"]["autoq"].Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
                          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
                          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
                          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(Q.Range) &&
                         !Invulnerable.Check(t, DamageType.Magical)))
                {
                    Q.Cast(target);
                }
            }
            if (Menu["misc"]["autow"].Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
                          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
                          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
                          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(W.Range) &&
                         !Invulnerable.Check(t, DamageType.Magical)))
                {
                    W.Cast(target);
                }
            }
        }

        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }

        private void Killsteal()
        {
            if (Q.Ready &&
                Menu["killsteal"]["kq"].Enabled)
            {
                var bestTarget = GetBestKillableHero(Q, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(Q.Range))
                {
                    Q.Cast(bestTarget);
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
            bool useW = Menu["combo"]["usew"].Enabled;
            if (W.Ready && useW)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                switch (Menu["combo"]["wo"].As<MenuList>().Value)
                {
                    case 0:
                        if (target.IsValidTarget(W.Range) && target != null)
                        {
                            W.Cast(target);
                        }
                        break;
                    case 1:
                        if (target.IsValidTarget(W.Range) && target != null && target.HasBuffOfType(BuffType.Charm) ||
                            target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Snare) ||
                            target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Suppression) ||
                            target.HasBuffOfType(BuffType.Taunt))
                        {
                            W.Cast(target);
                        }
                        break;
                }
            }

            bool useQ = Menu["combo"]["useq"].Enabled;
            if (Q.Ready && useQ)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target.IsValidTarget(Q.Range) && target != null)
                {
                    Q.Cast(target);
                }
            }

            bool useR = Menu["combo"]["user"].Enabled;
            float hitR = Menu["combo"]["hitr"].As<MenuSlider>().Value;
            if (useR && Player.CountEnemyHeroesInRange(R.Range - 150) >= hitR && R.Ready)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if (target.IsValidTarget(R.Range - 50) && target != null)
                {
                    R.Cast();
                }
            }
        }
        
    }
}