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
        public static Spell Q, QR, W, WR, E, ER, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 350f);//HeimerdingerQ
            Q.SetSkillshot(0.5f, 400f, 1450f, false, SkillshotType.Circle);
            QR = new Spell(SpellSlot.Q, 350f);
            QR.SetSkillshot(0.5f, 400f, 1450f, false, SkillshotType.Circle);
            W = new Spell(SpellSlot.W, 1250f);//HeimerdingerW
            W.SetSkillshot(0.5f, 100f, 750f, true, SkillshotType.Line);
            WR = new Spell(SpellSlot.W, 1250f);
            WR.SetSkillshot(0.5f, 200f, 750f, true, SkillshotType.Line);
            E = new Spell(SpellSlot.E, 925f);//HeimerdingerE
            E.SetSkillshot(0.5f, 100f, 1200f, false, SkillshotType.Circle);
            ER = new Spell(SpellSlot.E, 1000f);//HeimerdingerEUlt
            ER.SetSkillshot(0.5f, 120f, 1400f, false, SkillshotType.Circle);
            R = new Spell(SpellSlot.R, 280f);//HeimerdingerR ToggleState == 1 Togglestate == 2
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
                Drawings.Add(new MenuBool("draww", "Draw W"));
                Drawings.Add(new MenuBool("drawe", "Draw E"));
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
            bool useR = Menu["combo"]["user"].Enabled;

            if (!target.IsValidTarget())
            {
                return;
            }

            bool useQ = Menu["combo"]["useq"].Enabled;
            bool useQR = Menu["combo"]["useqr"].Enabled;

            if (Q.Ready && target.IsValidTarget(650))
            {
                if (useQ)
                {
                    Q.Cast(Player.Position + 350);
                }
                else if (useQR && R.Ready && useR && Player.SpellBook.GetSpell(SpellSlot.R).ToggleState == 1 &&
                         Q.CastIfWillHit(target, Menu["combo"]["useqhit"].As<MenuSlider>().Value - 1))
                {
                    R.Cast();
                    QR.Cast(Player.Position + 350);
                }
            }

            bool useW = Menu["combo"]["usew"].Enabled;
            bool useWR = Menu["combo"]["usewr"].Enabled;
            float useWRHit = Menu["combo"]["usewhit"].As<MenuSlider>().Value;
            if (W.Ready && target.IsValidTarget(W.Range))
            {
                if (useW)
                {
                    W.Cast(target);
                }
                else if (useWR && R.Ready && useR && Player.SpellBook.GetSpell(SpellSlot.R).ToggleState == 1 && 
                    Player.CountEnemyHeroesInRange(WR.Range) >= useWRHit)
                {
                    R.Cast();
                    WR.Cast(target);
                }
            }

            bool useE = Menu["combo"]["usee"].Enabled;
            bool useER = Menu["combo"]["useer"].Enabled;
            float useE2Hit = Menu["combo"]["useehit"].As<MenuSlider>().Value;
            if (E.Ready)
            {
                if (useE && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }
                else if (useER && useR && R.Ready && Player.SpellBook.GetSpell(SpellSlot.R).ToggleState == 1 && target.IsValidTarget(ER.Range) &&
                         ER.CastIfWillHit(target, Menu["combo"]["useehit"].As<MenuSlider>().Value - 1))
                {
                    R.Cast();
                    ER.Cast(target);
                }
            }
        }
    }
}