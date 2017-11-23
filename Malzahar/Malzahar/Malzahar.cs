

namespace Malzahar
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
    using Invulnerable;
    using GameObjects;

    using Spell = Aimtec.SDK.Spell;
    using Aimtec.SDK.Events;

    internal class Malzahar
    {
        public static Menu Menu = new Menu("Malzahar by Zypppy", "Malzahar by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 900f);
            Q.SetSkillshot(0.5f, 100f, 1600f, false, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 650f);
            E = new Spell(SpellSlot.E, 650f);//MalzaharEmalzaharerecent
            R = new Spell(SpellSlot.R, 700f);//malzaharrsound
        }

        public Malzahar()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("c", "Combo");
            {
                Combo.Add(new MenuBool("q", "Use Q"));
                Combo.Add(new MenuList("qo", "Q Options", new[] {"Always", "Only When Enemy Has E Buff", "Only When Enemy Slowed", "Only When Hard CC'ed"}, 1));
                Combo.Add(new MenuBool("w", "Use W"));
                Combo.Add(new MenuList("wo", "W Options", new[] {"Always", "Only When Enemy Has E Buff"}, 1));
                Combo.Add(new MenuBool("e", "Use E"));
                Combo.Add(new MenuKeyBind("key", "Manual R Key:", KeyCode.T, KeybindType.Press));
                Combo.Add(new MenuList("ro", "R Options", new[] {"Always", "Only On Enemy That Has E Buff"}, 1));
            }
            Menu.Add(Combo);
            var Harass = new Menu("h", "Harass");
            {
                Harass.Add(new MenuBool("q", "Use Q"));
                Harass.Add(new MenuSlider("qm", "Use Q Mana Percent >=", 60, 0, 100));
            }
            Menu.Add(Harass);
            var Drawings = new Menu("d", "Drawings");
            {
                Drawings.Add(new MenuBool("q", "Draw Q"));
                Drawings.Add(new MenuBool("w", "Draw W"));
                Drawings.Add(new MenuBool("e", "Draw E"));
                Drawings.Add(new MenuBool("r", "Draw R"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += RenderD;
            Game.OnUpdate += GameD;
            LoadSpells();
            Console.WriteLine(("Malzahar by Zypppy - Loaded"));
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

        private void RenderD()
        {
            Vector2 gejus;
            var heropos = Render.WorldToScreen(Player.Position, out gejus);
            var xaOffset = (int)gejus.X;
            var yaOffset = (int)gejus.Y;

            if (Q.Ready && Menu["d"]["q"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Crimson);
            }
            if (W.Ready && Menu["d"]["w"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Cyan);
            }
            if (E.Ready && Menu["d"]["e"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.DarkOrchid);
            }
            if (R.Ready && Menu["d"]["r"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Snow);
            }
        }

        private void GameD()
        {
            if (Player.HasBuff("malzaharrsound"))
            {
                Orbwalker.AttackingEnabled = false;
                Orbwalker.MovingEnabled = false;
            }
            if (!Player.HasBuff("malzaharrsound"))
            {
                Orbwalker.AttackingEnabled = true;
                Orbwalker.MovingEnabled = true;
            }
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
                    Harass();
                    break;
            }
            if (Menu["c"]["key"].Enabled)
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

        private void Combo()
        {
            var t = GetBestEnemyHeroTargetInRange(Q.Range);
            if (!t.IsValidTarget())
            {
                return;
            }

            bool CQ = Menu["c"]["q"].Enabled;
            if (Q.Ready && CQ)
            {
                switch (Menu["c"]["qo"].As<MenuList>().Value)
                {
                    case 0:
                        if (t.IsValidTarget(Q.Range))
                        {
                            Q.Cast(t);
                        }
                        return;
                    case 1:
                        if (t.IsValidTarget(Q.Range) && t.HasBuff("MalzaharE"))
                        {
                            Q.Cast(t);
                        }
                        return;
                    case 2:
                        if (t.IsValidTarget(Q.Range) && t.HasBuffOfType(BuffType.Slow))
                        {
                            //Q.Cast(t);
                            Console.WriteLine("Case2");
                        }
                        return;
                    case 3:
                        if (t.IsValidTarget(Q.Range) && t.HasBuffOfType(BuffType.Charm) ||
                            t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Knockup) ||
                            t.HasBuffOfType(BuffType.Snare) || t.HasBuffOfType(BuffType.Stun) ||
                            t.HasBuffOfType(BuffType.Suppression) || t.HasBuffOfType(BuffType.Taunt))
                        {
                            //Q.Cast(t);
                            Console.WriteLine("Case 3");
                        }
                        return;
                }
            }

            bool CW = Menu["c"]["w"].Enabled;
            if (W.Ready && CW && !Player.HasBuff("malzaharrsound"))
            {
                switch (Menu["c"]["wo"].As<MenuList>().Value)
                {
                    case 0:
                        if (t.IsValidTarget(W.Range))
                        {
                            W.Cast();
                        }
                        break;
                    case 1:
                        if (t.IsValidTarget(W.Range) && t.HasBuff("MalzaharE"))
                        {
                            W.Cast();
                        }
                        break;
                }
            }

            bool CE = Menu["c"]["e"].Enabled;
            if (E.Ready && CE && !Player.HasBuff("malzaharrsound") && t.IsValidTarget(E.Range))
            {
                E.Cast(t);
            }
        }

        private void Harass()
        {
            var t = GetBestEnemyHeroTargetInRange(Q.Range);
            if (!t.IsValidSpellTarget())
            {
                return;
            }

            bool HQ = Menu["h"]["q"].Enabled;
            float MQ = Menu["h"]["qm"].As<MenuSlider>().Value;

            if (Q.Ready && HQ && Player.ManaPercent() >= MQ && t.IsValidTarget(Q.Range) &&
                !Player.HasBuff("malzaharrsound"))
            {
                Q.Cast(t);
            }
        }

        private void ManualR()
        {
            var t = GetBestEnemyHeroTargetInRange(R.Range);

            if (R.Ready && t.IsValidTarget(R.Range))
            {
                switch (Menu["c"]["ro"].As<MenuList>().Value)
                {
                    case 0:
                        R.Cast(t);
                        return;
                    case 1:
                        if (t.HasBuff("MalzaharE"))
                        {
                            R.Cast(t);
                        }
                        return;
                }
            }
        }
    }
}