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
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, Player.AttackRange * 2);
            
        }
        public Udyr()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("combo", "Combo");
            {
                Combo.Add(new MenuBool("useq", "Use Q"));
                Combo.Add(new MenuBool("usew", "Use W Always"));
                Combo.Add(new MenuBool("usewhp", "Enable W HP Check"));
                Combo.Add(new MenuSlider("hpw", "Use W When Health % <=", 30, 0, 100));
                Combo.Add(new MenuBool("usee", "Use E"));
                Combo.Add(new MenuSlider("rangee", "Use E Range", 600, 0, 1500));
                Combo.Add(new MenuBool("user", "Use R"));
            }
            Menu.Add(Combo);
            var LaneClear = new Menu("laneclear", "Lane Clear");
            {
                LaneClear.Add(new MenuBool("useq", "Use Q"));
                LaneClear.Add(new MenuSlider("manaq", "Mana Q", 60, 0, 100));
                LaneClear.Add(new MenuBool("usew", "Use W"));
                LaneClear.Add(new MenuSlider("manaw", "Mana W", 60, 0, 100));
                LaneClear.Add(new MenuBool("usewhp", "Enable W HP Check"));
                LaneClear.Add(new MenuSlider("hpw", "W Health % <=", 60, 0, 100));
                LaneClear.Add(new MenuBool("usee", "Use E"));
                LaneClear.Add(new MenuSlider("manae", "Mana E", 60, 0, 100));
                LaneClear.Add(new MenuBool("user", "Use R"));
                LaneClear.Add(new MenuSlider("manar", "Mana R", 60, 0, 100));
            }
            Menu.Add(LaneClear);
            var JungleClear = new Menu("jungleclear", "Jungle Clear");
            {
                JungleClear.Add(new MenuBool("useq", "Use Q"));
                JungleClear.Add(new MenuSlider("manaq", "Mana Q", 60, 0, 100));
                JungleClear.Add(new MenuBool("use W", "Use W"));
                JungleClear.Add(new MenuSlider("manaw", "Mana W", 60, 0, 100));
                JungleClear.Add(new MenuBool("usewhp", "Enable W HP Check"));
                JungleClear.Add(new MenuSlider("hpw", "W Health % <=", 60, 0, 100));
                JungleClear.Add(new MenuBool("usee", "Use E"));
                JungleClear.Add(new MenuSlider("manae", "Mana E", 60, 0, 100));
                JungleClear.Add(new MenuBool("user", "Use R"));
                JungleClear.Add(new MenuSlider("manar", "Mana R", 60, 0, 100));
            }
            Menu.Add(JungleClear);
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
                Render.Circle(Player.Position, Menu["combo"]["rangee"].As<MenuSlider>().Value, 40, Color.BlueViolet);
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
                    OnLaneClear();
                    OnJungleClear();
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
            bool useW = Menu["combo"]["usew"].Enabled;
            bool useWHP = Menu["combo"]["usewhp"].Enabled;
            float hpW = Menu["combo"]["hpw"].As<MenuSlider>().Value;
            bool useE = Menu["combo"]["usee"].Enabled;
            float rangeE = Menu["combo"]["rangee"].As<MenuSlider>().Value;
            bool useR = Menu["combo"]["user"].Enabled;

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range) && !R.Ready && !Player.HasBuff("UdyrPhoenixStance") && !Player.HasBuff("UdyrBearStance"))
            {
                Q.Cast();
            }
            if (W.Ready)
            {
                if (useW && target.IsValidTarget(W.Range) && !Player.HasBuff("UdyrTurtleStance"))
                {
                    W.Cast();
                }
                else if (useWHP && target.IsValidTarget(W.Range) && Player.HealthPercent() <= hpW && !Player.HasBuff("UdyrTurtleStance"))
                {
                    W.Cast();
                }
            }
            if (E.Ready && useE && target.IsValidTarget(rangeE) && !Player.HasBuff("UdyrTigerStance") && !Player.HasBuff("UdyrPhoenixStance"))
            {
                E.Cast();
            }
            if (R.Ready && useR && target.IsValidTarget(R.Range) && !Player.HasBuff("UdyrPhoenixStance"))
            {
                R.Cast();
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
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
            {
                bool useQ = Menu["laneclear"]["useq"].Enabled;
                float manaQ = Menu["laneclear"]["manaq"].As<MenuSlider>().Value;
                bool useW = Menu["laneclear"]["usew"].Enabled;
                bool useWHP = Menu["laneclear"]["usewhp"].Enabled;
                float hpW = Menu["laneclear"]["hpw"].As<MenuSlider>().Value;
                float manaW = Menu["laneclear"]["manaw"].As<MenuSlider>().Value;
                bool useE = Menu["laneclear"]["usee"].Enabled;
                float manaE = Menu["laneclear"]["manae"].As<MenuSlider>().Value;
                bool useR = Menu["laneclear"]["user"].Enabled;
                float manaR = Menu["laneclear"]["manar"].As<MenuSlider>().Value;

                if (!minion.IsValidTarget())
                {
                    return;
                }
                if (Q.Ready && useQ && minion.IsValidTarget(Q.Range) && Player.ManaPercent() >= manaQ && !R.Ready && !Player.HasBuff("UdyrPhoenixStance") && !Player.HasBuff("UdyrBearStance"))
                {
                    Q.Cast();
                }
                if (W.Ready && Player.ManaPercent() >= manaW)
                {
                    if (useW && minion.IsValidTarget(W.Range) && !Player.HasBuff("UdyrTurtleStance"))
                    {
                        W.Cast();
                    }
                    else if (useWHP && minion.IsValidTarget(W.Range) && Player.HealthPercent() <= hpW && !Player.HasBuff("UdyrTurtleStance"))
                    {
                        W.Cast();
                    }
                }
                if (E.Ready && useE && minion.IsValidTarget(E.Range) && Player.ManaPercent() >= manaE && !Player.HasBuff("UdyrTigerStance") && !Player.HasBuff("UdyrPhoenixStance"))
                {
                    E.Cast();
                }
                if (R.Ready && useR && minion.IsValidTarget(R.Range) && Player.ManaPercent() >= manaR && !Player.HasBuff("UdyrPhoenixStance"))
                {
                    R.Cast();
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
            foreach (var minion in GameObjects.Jungle.Where(m => m.IsValidTarget(E.Range)).ToList())
            {
                bool useQ = Menu["jungleclear"]["useq"].Enabled;
                float manaQ = Menu["jungleclear"]["manaq"].As<MenuSlider>().Value;
                bool useW = Menu["jungleclear"]["usew"].Enabled;
                bool useWHP = Menu["jungleclear"]["usewhp"].Enabled;
                float hpW = Menu["jungleclear"]["hpw"].As<MenuSlider>().Value;
                float manaW = Menu["jungleclear"]["manaw"].As<MenuSlider>().Value;
                bool useE = Menu["jungleclear"]["usee"].Enabled;
                float manaE = Menu["jungleclear"]["manae"].As<MenuSlider>().Value;
                bool useR = Menu["jungleclear"]["user"].Enabled;
                float manaR = Menu["jungleclear"]["manar"].As<MenuSlider>().Value;

                if (!minion.IsValidTarget())
                {
                    return;
                }
                if (Q.Ready && useQ && minion.IsValidTarget(Q.Range) && Player.ManaPercent() >= manaQ && !R.Ready && !Player.HasBuff("UdyrPhoenixStance") && !Player.HasBuff("UdyrBearStance"))
                {
                    Q.Cast();
                }
                if (W.Ready && Player.ManaPercent() >= manaW)
                {
                    if (useW && minion.IsValidTarget(W.Range) && !Player.HasBuff("UdyrTurtleStance"))
                    {
                        W.Cast();
                    }
                    else if (useWHP && minion.IsValidTarget(W.Range) && Player.HealthPercent() <= hpW && !Player.HasBuff("UdyrTurtleStance"))
                    {
                        W.Cast();
                    }
                }
                if (E.Ready && useE && minion.IsValidTarget(E.Range) && Player.ManaPercent() >= manaE && !Player.HasBuff("UdyrTigerStance") && !Player.HasBuff("UdyrPhoenixStance"))
                {
                    E.Cast();
                }
                if (R.Ready && useR && minion.IsValidTarget(R.Range) && Player.ManaPercent() >= manaR && !Player.HasBuff("UdyrPhoenixStance"))
                {
                    R.Cast();
                }
            }
        }
    }
}