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
            Q = new Spell(SpellSlot.Q, 1450f);
            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1400, 1.0f);
            Q.SetSkillshot(0.6f, 72f, float.MaxValue, false, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 1100f);
            W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.Circle);
            E = new Spell(SpellSlot.E, 1050f);
            E.SetSkillshot(0.25f, 70f, 1400f, true, SkillshotType.Line);
            R = new Spell(SpellSlot.R, 6160f);
            R.SetSkillshot(0.7f, 130f, float.MaxValue, false, SkillshotType.Circle);
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
                Ult.Add(new MenuBool("r", "Use R Auto After Cast"));
                //Ult.Add(new MenuList("ro", "R Combo Options", new[] { "Normal", "OnTap" }, 1));
            }
            Menu.Add(Ult);
            var Harass = new Menu("h", "Harass");
            {
                Harass.Add(new MenuBool("q", "Use Q"));
                Harass.Add(new MenuSlider("qm", "Use Q Mana Percent >=", 60, 0, 100));
                Harass.Add(new MenuBool("w", "Use W"));
                Harass.Add(new MenuSlider("wm", "Use W Mana Percent >=", 60, 0, 100));
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
                Killsteal.Add(new MenuBool("q", "Use Q"));
                Killsteal.Add(new MenuBool("w", "Use W"));
            }
            Menu.Add(Killsteal);
            var Drawings = new Menu("d", "Drawings");
            {
                Drawings.Add(new MenuBool("q", "Draw Q"));
                Drawings.Add(new MenuBool("w", "Draw W"));
                Drawings.Add(new MenuBool("e", "Draw E"));
                Drawings.Add(new MenuBool("r", "Draw R On Minimap"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            BuffManager.OnRemoveBuff += XerathR;
            LoadSpells();
            Console.WriteLine("Xerath by Zypppy - Loaded");
        }

        private void XerathR(Obj_AI_Base sender, Buff buff)
        {
            if (sender.IsMe)
            {
                if (buff.Name == "XerathLocusOfPower2")
                {
                    Orbwalker.MovingEnabled = true;
                    Orbwalker.AttackingEnabled = true;
                }
            }
        }

        public static void DrawCircleOnMinimap(
            Vector3 center,
            float radius,
            Color color,
            int thickness = 1,
            int quality = 100)
        {
            var pointList = new List<Vector3>();
            for (var i = 0; i < quality; i++)
            {
                var angle = i * Math.PI * 2 / quality;
                pointList.Add(
                    new Vector3(
                        center.X + radius * (float)Math.Cos(angle),
                        center.Y,
                        center.Z + radius * (float)Math.Sin(angle))
                );
            }
            for (var i = 0; i < pointList.Count; i++)
            {
                var a = pointList[i];
                var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

                Render.WorldToMinimap(a, out var aonScreen);
                Render.WorldToMinimap(b, out var bonScreen);

                Render.Line(aonScreen, bonScreen, color);
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
            Vector2 gejus;
            var heropos = Render.WorldToScreen(Player.Position, out gejus);
            var xaOffset = (int)gejus.X;
            var yaOffset = (int)gejus.Y;

            if (Q.Ready && Menu["d"]["q"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Red);
            }
            if (W.Ready && Menu["d"]["w"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Pink);
            }
            if (E.Ready && Menu["d"]["e"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.DeepPink);
            }
            if (R.Ready && Menu["d"]["r"].Enabled)
            {
                DrawCircleOnMinimap(Player.Position, R.Range, Color.White);
            }
        }
        private void Game_OnUpdate()
        {
            if (Player.HasBuff("XerathLocusOfPower2"))
            {
                Orbwalker.MovingEnabled = false;
                Orbwalker.AttackingEnabled = false;
            }
            if (Player.IsDead || MenuGUI.IsChatOpen())
            {
                return;
            }
            switch (Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    Combo();
                    Ultimate();
                    break;
                case OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case OrbwalkingMode.Laneclear:
                    break;

            }
            if (Player.GetSpell(SpellSlot.R).Level > 0)
            {
                R.Range = 1850 + 1320 * Player.SpellBook.GetSpell(SpellSlot.R).Level - 1;
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
            if (Q.Ready && Menu["ks"]["q"].Enabled)
            {
                var kill = GetBestKillableHero(Q, DamageType.Magical, false);
                if (kill != null && Player.GetSpellDamage(kill, SpellSlot.Q) >= kill.Health && kill.IsValidTarget(Q.Range))
                {
                    Q.Cast(kill);
                }
            }
            if (W.Ready && Menu["ks"]["w"].Enabled)
            {
                var kill = GetBestKillableHero(W, DamageType.Magical, false);
                if (kill != null && Player.GetSpellDamage(kill, SpellSlot.W) >= kill.Health && kill.IsValidTarget(W.Range))
                {
                    W.Cast(kill);
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

        private void Combo()
        {
            var target = GetBestEnemyHeroTargetInRange(1800);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool CQ = Menu["c"]["q"].Enabled;
            if (Q.Ready && CQ && target.IsValidTarget(Q.ChargedMaxRange))
            {
                Q.Cast(target);
            }

            bool CW = Menu["c"]["w"].Enabled;
            if (W.Ready && CW && target.IsValidTarget(W.Range))
            {
                W.Cast(target);
            }

            bool CE = Menu["c"]["e"].Enabled;
            if (E.Ready && CE && target.IsValidTarget(E.Range))
            {
                E.Cast(target);
            }
        }
        private void Ultimate()
        {
            var target = GetBestEnemyHeroTargetInRange(6160);
            if (!target.IsValidTarget())
            {
                return;
            }
            bool CR = Menu["u"]["r"].Enabled;
            if (CR && target.IsValidTarget(R.Range) && Player.HasBuff("XerathLocusOfPower2"))
            {
                R.Cast(target);
            }
        }
        private void Harass()
        {
            var target = GetBestEnemyHeroTargetInRange(1800);
            if (!target.IsValidSpellTarget())
            {
                return;
            }
            bool HQ = Menu["h"]["q"].Enabled;
            float MQ = Menu["h"]["qm"].As<MenuSlider>().Value;
            if (Q.Ready && HQ && Player.ManaPercent() >= MQ && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
            bool HW = Menu["h"]["w"].Enabled;
            float MW = Menu["h"]["wm"].As<MenuSlider>().Value;
            if (W.Ready && HW && Player.ManaPercent() >= MW && target.IsValidTarget(W.Range))
            {
                W.Cast(target);
            }
        }
    }
}