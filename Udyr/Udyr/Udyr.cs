namespace Udyr
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

    internal class Udyr
    {
        public static Menu Menu = new Menu("Udyr by Zypppy", "Udyr by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, Player.AttackRange * 2);
            W = new Spell(SpellSlot.W, Player.AttackRange * 2);
            E = new Spell(SpellSlot.E, Player.AttackRange * 2);
            R = new Spell(SpellSlot.R, Player.AttackRange * 2);
        }

        public Udyr()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("combo", "Combo");
            {
                Combo.Add(new MenuBool("useq", "Use Q"));
                Combo.Add(new MenuBool("usew", "Use W"));
                Combo.Add(new MenuSlider("whp", "Use W Health <=", 20, 0, 100));
                Combo.Add(new MenuBool("usee", "Use E"));
                Combo.Add(new MenuBool("user", "Use R"));
                Combo.Add(new MenuList("cs", "Combo Settings", new[] { "TigerCombo", "PhoenixCombo" }, 1));
            }
            Menu.Add(Combo);
            var Misc = new Menu("misc", "Misc");
            {
                Misc.Add(new MenuBool("fleee", "Use E To Flee"));
                Misc.Add(new MenuKeyBind("key", "Flee Key:", KeyCode.Z, KeybindType.Press));
            }
            Menu.Add(Misc);
            var Drawings = new Menu("drawings", "Drawings");
            {
                Drawings.Add(new MenuBool("drawq", "Draw Q"));
                Drawings.Add(new MenuBool("draww", "Draw W"));
                Drawings.Add(new MenuBool("drawe", "Draw E"));
                Drawings.Add(new MenuBool("drawr", "Draw R"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Udyr by Zypppy - Loaded");
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

            if (Menu["drawings"]["drawq"].Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Beige);
            }
            if (Menu["drawings"]["draww"].Enabled && W.Ready)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.AliceBlue);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.BlueViolet);
            }
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Chocolate);
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
            if (Menu["misc"]["key"].Enabled)
            {
                Flee();
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
            bool useW = Menu["combo"]["usew"].Enabled;
            float WHP = Menu["combo"]["whp"].As<MenuSlider>().Value;
            bool useE = Menu["combo"]["usee"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;

            var target = GetBestEnemyHeroTargetInRange(Player.AttackRange + 800);
            if (target.IsValidTarget())
            {
                switch (Menu["combo"]["cs"].As<MenuList>().Value)
                {
                    case 0:
                        if (useE && E.Ready)
                        {
                            if (Player.Distance(target) > E.Range)
                            {
                                E.Cast();
                            }
                            else if (Player.Distance(target) <= E.Range && !target.HasBuff("udyrbearstuncheck"))
                            {
                                E.Cast();
                            }
                        }
                        if (useR && R.Ready && !Player.HasBuff("UdyrPhoenixStance") && Player.Distance(target) <= R.Range && target.HasBuff("udyrbearstuncheck"))
                        {
                            R.Cast();
                        }
                        if (useQ && Q.Ready && Player.Distance(target) <= Q.Range && target.HasBuff("udyrbearstuncheck"))
                        {
                            Q.Cast();
                        }
                        break;
                    case 1:
                        if (useE && E.Ready)
                        {
                            if (Player.Distance(target) > E.Range)
                            {
                                E.Cast();
                            }
                            else if (Player.Distance(target) <= E.Range && !target.HasBuff("udyrbearstuncheck"))
                            {
                                E.Cast();
                            }
                        }
                        if (useQ && Q.Ready && Player.Distance(target) <= Q.Range && target.HasBuff("udyrbearstuncheck"))
                        {
                            Q.Cast();
                        }
                        if (useR && R.Ready && !Player.HasBuff("UdyrPhoenixStance") && Player.Distance(target) <= R.Range && target.HasBuff("udyrbearstuncheck"))
                        {
                            R.Cast();
                        }
                        break;
                }
            }
            if (useW && Player.Distance(target) < 1000)
            {
                if (Player.HealthPercent() <= WHP && !Player.HasBuff("udyrmonkeyagilitybuff"))
                {
                    W.Cast();
                }
            }
        }
        
        private void Flee()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            bool usee = Menu["misc"]["fleee"].Enabled;
            if (usee && E.Ready)
            {
                E.Cast();
            }
        }
    }
}