namespace Heimerdinger
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

    internal class Heimerdinger
    {
        public static Menu Menu = new Menu("Heimerdinger by Zypppy", "Heimerdinger by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, W2, E, E2, E3, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 325);
            Q.SetSkillshot(0.5f, 40f, 1100f, true, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 1100);
            W.SetSkillshot(0.5f, 40f, 3000f, true, SkillshotType.Line);
            W2 = new Spell(SpellSlot.W, 1100);
            W2.SetSkillshot(0.5f, 40f, 3000f, true, SkillshotType.Line);
            E = new Spell(SpellSlot.E, 925);
            E.SetSkillshot(0.5f, 120f, 1200f, false, SkillshotType.Circle);
            E2 = new Spell(SpellSlot.E, 1125);
            E2.SetSkillshot(0.25f + E.Delay, 120f, 1200f, false, SkillshotType.Circle);
            E3 = new Spell(SpellSlot.E, 1325);
            E3.SetSkillshot(0.3f + E2.Delay, 120f, 1200f, false, SkillshotType.Circle);
            R = new Spell(SpellSlot.R, 100);
        }
        public Heimerdinger()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("combo", "Combo");
            {
                Combo.Add(new MenuBool("useq", "Use Q"));
                Combo.Add(new MenuBool("useqr", "Use Upgraded Q"));
                Combo.Add(new MenuSlider("useqhit", "Minimum Enemies Around You For Upgraded Q", 3, 1, 5));
                Combo.Add(new MenuBool("usew", "Use W"));
                Combo.Add(new MenuBool("usewr", "Use Upgraded W"));
                Combo.Add(new MenuSlider("usewhit", "Minimum Enemies Around You For Upgraded W", 2, 1, 5));
                Combo.Add(new MenuBool("usee", "Use E"));
                Combo.Add(new MenuBool("useer", "Use Upgraded E"));
                Combo.Add(new MenuSlider("useehit", "Minimum Enemies Around You For Upgraded E", 4, 1, 5));
                Combo.Add(new MenuBool("user", "Use R"));
            }
            Menu.Add(Combo);
            var Drawings = new Menu("drawings", "Drawings");
            {
                Drawings.Add(new MenuBool("drawq", "Draw Q"));
                Drawings.Add(new MenuBool("drawq", "Draw W"));
                Drawings.Add(new MenuBool("drawq", "Draw E"));
            }
            Menu.Add(Drawings);
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Heimerdinger by Zypppy - Loaded");
        }
        private void Render_OnPresent()
        {
            Vector2 mymom;
            var heropos = Render.WorldToScreen(Player.Position, out mymom);
            var xaOffset = (int)mymom.X;
            var yaOffset = (int)mymom.Y;

            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Cornsilk);
            }
            if (Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Aquamarine);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.Beige);
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
            bool useQ2 = Menu["combo"]["useqr"].Enabled;
            float useQ2Hit = Menu["combo"]["useqhit"].As<MenuSlider>().Value;
            bool useW = Menu["combo"]["usew"].Enabled;
            var WPrediction = W.GetPrediction(target);
            bool useW2 = Menu["combo"]["usewr"].Enabled;
            float useW2Hit = Menu["combo"]["usewhit"].As<MenuSlider>().Value;
            var W2Prediction = W2.GetPrediction(target);
            bool useE = Menu["combo"]["usee"].Enabled;
            var EPrediction = E.GetPrediction(target);
            bool useE2 = Menu["combo"]["useer"].Enabled;
            float useE2Hit = Menu["combo"]["useehit"].As<MenuSlider>().Value;
            var E2Prediction = E2.GetPrediction(target);
            bool useR = Menu["combo"]["user"].Enabled;

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && target.IsValidTarget(650) && useQ)
            {
                Q.Cast(Player.Position);
            }
            if (Q.Ready && useQ2 && useR && target.IsValidTarget(650) && Player.CountEnemyHeroesInRange(Q.Range + 300) >= useQ2Hit)
            {
                R.Cast();
                Q.Cast(Player.Position);
            }
            if (W.Ready)
            {
                if (useW && target.IsValidTarget(W.Range - 20))
                {
                    if (WPrediction.HitChance >= HitChance.High)
                    {
                        W.Cast(WPrediction.CastPosition);
                    }
                }
                else if (useW2 && useR && target.IsValidTarget(W2.Range - 15) && Player.CountEnemyHeroesInRange(W2.Range) >= useW2Hit || target.HealthPercent() <= 30)
                {
                    if (W2Prediction.HitChance >= HitChance.High)
                    {
                        R.Cast();
                        W2.Cast(W2Prediction.CastPosition);
                    }
                }
            }
            if (E.Ready)
            {
                if (useE && target.IsValidTarget(E.Range))
                {
                    if (EPrediction.HitChance >= HitChance.High)
                    {
                        E.Cast(EPrediction.CastPosition);
                    }
                }
                else if (useE && useR && target.IsValidTarget(E2.Range) && Player.CountEnemyHeroesInRange(E2.Range) >= useE2Hit)
                {
                    if (E2Prediction.HitChance >= HitChance.High)
                    {
                        R.Cast();
                        E2.Cast(E2Prediction.CastPosition);
                    }
                }
            }
        }
    }
}