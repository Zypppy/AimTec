namespace Xerath
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

    internal class Xerath
    {
        public static Menu Menu = new Menu("Xerath by Zypppy", "Xerath by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1400f);
            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1400, 3.0f);
            Q.SetSkillshot(0.6f, 72f, float.MaxValue, false, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 1100f);
            W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.Circle);
            W = new Spell(SpellSlot.E, 1050f);
            E.SetSkillshot(0.25f, 70f, 1400f, true, SkillshotType.Line);
            R = new Spell(SpellSlot.R, 3520f);
            R.SetSkillshot(0.25f, 150f, 500f, false, SkillshotType.Line);
        }

        public Xerath()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("c", "Combo");
            {
                Combo.Add(new MenuBool("q", "Use Q"));
                Combo.Add(new MenuBool("w", "Use W"));
                Combo.Add(new MenuBool("e", "Use E"));
            }
            Menu.Add(Combo);
            var Ult = new Menu("u", "Ultimate");
            {

            }
            Menu.Add(Ult);
            var Harass = new Menu("h", "Harass");
            {

            }
            Menu.Add(Harass);
            var LClear = new Menu("lc", "Lane Clear");
            {

            }
            Menu.Add(LClear);
            var JClear = new Menu("jc", "Jungle Clear");
            {

            }
            Menu.Add(JClear);
            var Killsteal = new Menu("ks", "Killsteal");
            {

            }
            Menu.Add(Killsteal);
            var Drawings = new Menu("d", "Drawings");
            {

            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            LoadSpells();
            Console.WriteLine("Xerath by Zypppy - Loaded");
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
                    Combo();
                    break;
                case OrbwalkingMode.Mixed:

                    break;
                case OrbwalkingMode.Laneclear:

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

        private void Combo()
        {
            var t = GetBestEnemyHeroTargetInRange(1800);
            if (!t.IsValidTarget())
            {
                return;
            }

            bool CQ = Menu["c"]["q"].Enabled;
            if (Q.Ready && CQ && t.IsValidTarget(Q.ChargedMaxRange))
            {
                Q.Cast(t);
            }
        }
    }
}