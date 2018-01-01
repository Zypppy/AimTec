
namespace Thresh
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

    internal class Thresh
    {

        public static Menu Menu = new Menu("Zypppy Thresh", "Zypppy Thresh", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();

        public static Spell Q, Q2, W, E, R, Flash;

        public void LoadSpells()

        {
            Q = new Spell(SpellSlot.Q, 1100f); // ThreshQ  ThreshQLeap
            Q.SetSkillshot(0.5f, 60f, 1900f, true, SkillshotType.Line);
            Q2 = new Spell(SpellSlot.Q, 2000f);
            Q2.SetSkillshot(0f, 1000f, 3000f, false, SkillshotType.Circle);
            W = new Spell(SpellSlot.W, 950f);
            W.SetSkillshot(0f, 120f, 800f, false, SkillshotType.Circle);
            E = new Spell(SpellSlot.E, 450f);
            E.SetSkillshot(0.222f, 110f, 2000f, false, SkillshotType.Line);
            R = new Spell(SpellSlot.R, 400);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner1, 425);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner2, 425);
        }


        public Thresh()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuSlider("qrange", "Q Range Slider", 900, 0, 1100));
                ComboMenu.Add(new MenuBool("useq2", "Use Second Q"));
                ComboMenu.Add(new MenuSlider("dontq2", "Dont Dive Into >= Enemies", 1, 0, 5));
                ComboMenu.Add(new MenuBool("usewself", "Use W Self"));
                ComboMenu.Add(new MenuSlider("wshp", "Self W If Hp % <", 50, 0, 100));
                ComboMenu.Add(new MenuBool("usee", "Use E Push"));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("usere", "Use R If Enemy >", 3, 1, 5));

            }
            Menu.Add(ComboMenu);     
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useq", "Use Q to Harass"));
                HarassMenu.Add(new MenuSlider("manaq", "Q Mana % >", 50, 0, 100));
                HarassMenu.Add(new MenuBool("usee", "Use E to Harass"));
                HarassMenu.Add(new MenuSlider("manae", "E Mana % >", 50, 0, 100));
            }
            Menu.Add(HarassMenu);
            var miscmenu = new Menu("misc", "Misc");
            {
                miscmenu.Add(new MenuBool("autoq", "Auto Q on CC"));
            }
            Menu.Add(miscmenu);
            var DrawingsMenu = new Menu("drawings", "Drawings");
            {
                DrawingsMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawingsMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawingsMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawingsMenu.Add(new MenuBool("drawr", "Draw R Range"));
            }
            Menu.Add(DrawingsMenu);
            EGap.Gapcloser.Attach(Menu, "E Anti-GapClose");
            Menu.Attach();


            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            Gapcloser.OnGapcloser += OnGapcloser;
            LoadSpells();
            Console.WriteLine("Zypppy Thresh - Loaded");
        }

        private void OnGapcloser(Obj_AI_Hero target, EGap.GapcloserArgs Args)
        {
            if (target != null && Args.EndPosition.Distance(Player) < E.Range && E.Ready && target.IsDashing() && target.IsValidTarget(E.Range))
            {
                E.Cast(Args.EndPosition);
            }
        }
        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;

            float QRange = Menu["combo"]["qrange"].As<MenuSlider>().Value;
            if (Menu["drawings"]["drawq"].Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, QRange, 40, Color.Indigo);
            }
            if (Menu["drawings"]["draww"].Enabled && W.Ready)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Fuchsia);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.DeepPink);
            }
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Aquamarine);
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
                    break;
            }

            if (Menu["misc"]["autoq"].Enabled && Q.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "ThreshQ")
            {
                float QRange = Menu["combo"]["qrange"].As<MenuSlider>().Value;
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
                          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
                          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
                          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(QRange) &&
                         !Invulnerable.Check(t, DamageType.Magical)))
                {

                    Q.Cast(target);
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
            float QRange = Menu["combo"]["qrange"].As<MenuSlider>().Value;
            bool useQGap = Menu["combo"]["useq2"].Enabled;
            if (Q.Ready)
            {
                var targetq = GetBestEnemyHeroTargetInRange(QRange);
                var targetq2 = GetBestEnemyHeroTargetInRange(Q2.Range);
                if (targetq.IsValidTarget(QRange) && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "ThreshQ")
                {
                    Q.Cast(targetq);
                }
                if (targetq2.IsValidTarget(Q2.Range) && useQGap && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "ThreshQLeap" && targetq2.HasBuff("ThreshQ") && !R.CastIfWillHit(targetq2, Menu["combo"]["dontq2"].As<MenuSlider>().Value - 1))
                {
                    Q2.Cast();
                }
            }
            
            bool useWself = Menu["combo"]["usewself"].Enabled;
            float WSHP = Menu["combo"]["wshp"].As<MenuSlider>().Value;
            if (W.Ready && useWself && Player.HealthPercent() <= WSHP)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target.IsValidTarget(W.Range))
                {
                    W.Cast(Player);
                }
            }

            bool useE = Menu["combo"]["usee"].Enabled;
            if (E.Ready && useE)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                if (target.IsValidTarget(E.Range) && !target.HasBuff("ThreshQ"))
                {
                    E.Cast(target);
                }
            }

            bool useR = Menu["combo"]["user"].Enabled;
            float REnemies = Menu["combo"]["usere"].As<MenuSlider>().Value;
            if (R.Ready && useR && Player.CountEnemyHeroesInRange(R.Range) >= REnemies)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if (target.IsValidTarget(R.Range))
                {
                    R.Cast();
                }
            }
        }

        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            float useQMana = Menu["harass"]["manaq"].As<MenuSlider>().Value;
            if (Q.Ready && useQ && Player.ManaPercent() >= useQMana)
            {
                var targetq = GetBestEnemyHeroTargetInRange(Q.Range);
                if (targetq.IsValidTarget(Q.Range) && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "ThreshQ")
                {
                    Q.Cast(targetq);
                }
            }

            bool useE = Menu["harass"]["usee"].Enabled;
            float useEMana = Menu["harass"]["manae"].As<MenuSlider>().Value;
            if (E.Ready && useE && Player.ManaPercent() >= useEMana)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                if (target.IsValidTarget(E.Range) && !target.HasBuff("ThreshQ"))
                {
                    E.Cast(target);
                }
            }
        }
    }
}
