namespace Varus
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

    using Spell = Aimtec.SDK.Spell;
    using Aimtec.SDK.Events;



    internal class Varus
    {

        public static Menu Menu = new Menu("Varus By Zypppy", "Varus By Zypppy", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();

        public static Spell Q, E, R;

        public void LoadSpells()

        {
            Q = new Spell(SpellSlot.Q, 1600);
            Q.SetCharged("VarusQ", "VarusQLaunch", 900, 1625, 1.5f);
            Q.SetSkillshot(0.25f, 75f, 1500f, false, SkillshotType.Line, false);
            E = new Spell(SpellSlot.E, 925);
            E.SetSkillshot(0.265f, 120f, 1500f, false, SkillshotType.Circle, false);
            R = new Spell(SpellSlot.R, 1250);
            R.SetSkillshot(0.251f, 120f, 1962f, false, SkillshotType.Line, false);


        }
        public Varus()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("hitr", "Enemies Around Target", 2, 1, 5));
                ComboMenu.Add(new MenuKeyBind("key", "Manual R Key:", KeyCode.T, KeybindType.Press));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useq", "Use Q"));
                HarassMenu.Add(new MenuSlider("manaq", "Q Mana Slider", 60, 0, 100));
            }
            Menu.Add(HarassMenu);
            var Drawings = new Menu("drawings", "Drawings");
            {
                Drawings.Add(new MenuBool("drawq", "Draw Q Range"));
                Drawings.Add(new MenuBool("drawe", "Draw E Range"));
                Drawings.Add(new MenuBool("drawr", "Draw R Range"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Varus by Zypppy - Loaded");
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

            if (Menu["drawings"]["drawq"].Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Azure);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.Azure);
            }
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Azure);
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
            if (Menu["combo"]["key"].Enabled)
            {
                ManualR();
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
            var target = GetBestEnemyHeroTargetInRange(2000);
            bool useQ = Menu["combo"]["useq"].Enabled;
            var QPrediction = Q.GetPrediction(target);
            bool useE = Menu["combo"]["usee"].Enabled;
            var EPrediction = E.GetPrediction(target);
            bool useR = Menu["combo"]["user"].Enabled;
            var RPrediction = R.GetPrediction(target);
            float hitR = Menu["combo"]["hitr"].As<MenuSlider>().Value;


            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q.ChargedMaxRange))
            {
                if (QPrediction.HitChance >= HitChance.High)
                {
                    Q.Cast(QPrediction.CastPosition);
                }
            }
            if (E.Ready && useE && target.IsValidTarget(E.Range))
            {
                if (EPrediction.HitChance >= HitChance.High)
                {
                    E.Cast(RPrediction.CastPosition);
                }
            }
            if (R.Ready && useR && target.IsValidTarget(R.Range) && target.CountEnemyHeroesInRange(R.Width + 200) >= hitR)
            {
                if (RPrediction.HitChance >= HitChance.High)
                {
                    R.Cast(RPrediction.CastPosition);
                }
            }
        }
        private void ManualR()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range);
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            var RPrediction = R.GetPrediction(target);
            if (R.Ready && target.IsValidTarget(R.Range))
            {
                if (RPrediction.HitChance >= HitChance.High)
                {
                    R.Cast(RPrediction.CastPosition);
                }
            }
        }
        private void OnHarass()
        {
            var target = GetBestEnemyHeroTargetInRange(2000);
            bool useQ = Menu["harass"]["useq"].Enabled;
            float manaQ = Menu["harass"]["manaq"].As<MenuSlider>().Value;
            var QPrediction = Q.GetPrediction(target);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Q.Ready && useQ && target.IsValidTarget(Q.ChargedMaxRange) && Player.ManaPercent() >= manaQ)
            {
                if (QPrediction.HitChance >= HitChance.High)
                {
                    Q.Cast(QPrediction.CastPosition);
                }
            }
        }
    }
}