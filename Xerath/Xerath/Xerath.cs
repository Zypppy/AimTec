﻿namespace Xerath
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

    internal class Xerath
    {
        public static Menu Menu = new Menu("Xerath by Zypppy", "Xerath by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1500f);
            Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1500, 1.3f);
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
                Combo.Add(new MenuList("eo", "E Options", new[] { "Always", "Only When Slowed", "Hard CC Targets" }, 1));
            }
            Menu.Add(Combo);
            EGap.Gapcloser.Attach(Menu, "E Anti - GapClose");
            var Ult = new Menu("u", "Ultimate");
            {
                Ult.Add(new MenuBool("r", "Use R Auto After Cast"));
                //Ult.Add(new MenuList("ro", "R Combo Options", new[] { "Normal", "OnTap" }, 1));
            }
            Menu.Add(Ult);
            var Harass = new Menu("h", "Harass");
            {
                Harass.Add(new MenuBool("q", "Use Q"));
                Harass.Add(new MenuSlider("qm", "Use Q Mana Percent >", 60, 0, 100));
                Harass.Add(new MenuBool("w", "Use W"));
                Harass.Add(new MenuSlider("wm", "Use W Mana Percent >=", 60, 0, 100));
            }
            Menu.Add(Harass);
            //var LClear = new Menu("lc", "Lane Clear");
            //{

            //}
            //Menu.Add(LClear);
            //var JClear = new Menu("jc", "Jungle Clear");
            //{

            //}
            //Menu.Add(JClear);
            var Killsteal = new Menu("ks", "Killsteal");
            {
                Killsteal.Add(new MenuBool("q", "Use Q"));
                Killsteal.Add(new MenuBool("w", "Use W"));
            }
            Menu.Add(Killsteal);
            var Drawings = new Menu("d", "Drawings");
            {
                Drawings.Add(new MenuBool("q", "Draw Q"));
                Drawings.Add(new MenuBool("qd", "Draw Q Damage"));
                Drawings.Add(new MenuBool("w", "Draw W"));
                Drawings.Add(new MenuBool("wd", "Draw W Damage"));
                Drawings.Add(new MenuBool("e", "Draw E"));
                Drawings.Add(new MenuBool("ed", "Draw E Damage"));
                Drawings.Add(new MenuBool("r", "Draw R On Minimap"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            Gapcloser.OnGapcloser += OnGapCloser;
            LoadSpells();
            Console.WriteLine("Xerath by Zypppy - Loaded");
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

        private void OnGapCloser(Obj_AI_Hero target, EGap.GapcloserArgs Args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (target == null || !target.IsEnemy)
            {
                return;
            }
            if (Args.EndPosition.Distance(Player) < E.Range && E.Ready && target.IsDashing() && target.IsValidTarget(E.Range))
            {
                E.Cast(target);
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
            if (Q.Ready && Menu["d"]["qd"].Enabled)
            {
                ObjectManager.Get<Obj_AI_Base>()
                 .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(1500))
                 .ToList()
                 .ForEach(
                  unit =>
                  {
                      var heroUnit = unit as Obj_AI_Hero;
                      int width = 103;
                      int height = 8;
                      int xOffset = SxOffset(heroUnit);
                      int yOffset = SyOffset(heroUnit);
                      var barPos = unit.FloatingHealthBarPosition;
                      barPos.X += xOffset;
                      barPos.Y += yOffset;

                      var drawEndXPos = barPos.X + width * (unit.HealthPercent() / 100);
                      var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.Q)
                      ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q))) / unit.MaxHealth * 100 / 100)
                      : 0));
                      Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) ? Color.GreenYellow : Color.Orange);

                  });
            }
            if (W.Ready && Menu["d"]["w"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Pink);
            }
            if (W.Ready && Menu["d"]["wd"].Enabled)
            {
                ObjectManager.Get<Obj_AI_Base>()
                 .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(1500))
                 .ToList()
                 .ForEach(
                  unit =>
                  {
                      var heroUnit = unit as Obj_AI_Hero;
                      int width = 103;
                      int height = 8;
                      int xOffset = SxOffset(heroUnit);
                      int yOffset = SyOffset(heroUnit);
                      var barPos = unit.FloatingHealthBarPosition;
                      barPos.X += xOffset;
                      barPos.Y += yOffset;

                      var drawEndXPos = barPos.X + width * (unit.HealthPercent() / 100);
                      var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.W)
                      ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.W))) / unit.MaxHealth * 100 / 100)
                      : 0));
                      Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.W) ? Color.Green : Color.Red);

                  });
            }
            if (E.Ready && Menu["d"]["e"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.DeepPink);
            }
            if (E.Ready && Menu["d"]["ed"].Enabled)
            {
                ObjectManager.Get<Obj_AI_Base>()
                 .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(1500))
                 .ToList()
                 .ForEach(
                  unit =>
                  {
                      var heroUnit = unit as Obj_AI_Hero;
                      int width = 103;
                      int height = 8;
                      int xOffset = SxOffset(heroUnit);
                      int yOffset = SyOffset(heroUnit);
                      var barPos = unit.FloatingHealthBarPosition;
                      barPos.X += xOffset;
                      barPos.Y += yOffset;

                      var drawEndXPos = barPos.X + width * (unit.HealthPercent() / 100);
                      var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.E)
                      ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.E))) / unit.MaxHealth * 100 / 100)
                      : 0));
                      Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.E) ? Color.Purple : Color.White);

                  });
            }
            if (R.Ready && Menu["d"]["r"].Enabled)
            {
                DrawCircleOnMinimap(Player.Position, R.Range, Color.White);
            }
        }
        private void Game_OnUpdate()
        {
            if (Q.IsCharging)
            {
                Orbwalker.AttackingEnabled = false;
            }
            if (!Q.IsCharging && !Player.HasBuff("XerathLocusOfPower2"))
            {
                Orbwalker.AttackingEnabled = true;
                Orbwalker.MovingEnabled = true;
            }
            if (Player.HasBuff("XerathLocusOfPower2"))
            {
                Orbwalker.AttackingEnabled = false;
                Orbwalker.MovingEnabled = false;
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
                R.Range = 1850 + 1300 * Player.SpellBook.GetSpell(SpellSlot.R).Level - 1;
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
            if (Q.Ready && Menu["ks"]["q"].Enabled && !Player.HasBuff("XerathLocusOfPower2"))
            {
                var kill = GetBestKillableHero(Q, DamageType.Magical, false);
                if (kill != null && Player.GetSpellDamage(kill, SpellSlot.Q) >= kill.Health && kill.IsValidTarget(Q.Range))
                {
                    Q.Cast(kill);
                }
            }
            if (W.Ready && Menu["ks"]["w"].Enabled && !Player.HasBuff("XerathLocusOfPower2"))
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
            bool CQ = Menu["c"]["q"].Enabled;
            if (Q.Ready && CQ && !Player.HasBuff("XerathLocusOfPower2"))
            {
                var target = GetBestEnemyHeroTargetInRange(Q.ChargedMaxRange);
                if (target.IsValidTarget(Q.ChargedMaxRange) && target != null)
                {
                    Q.Cast(target);
                }
            }

            bool CW = Menu["c"]["w"].Enabled;
            if (W.Ready && CW && !Player.HasBuff("XerathLocusOfPower2"))
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target.IsValidTarget(W.Range) && target != null)
                {
                    W.Cast(target);
                }
            }

            bool CE = Menu["c"]["e"].Enabled;

            if (E.Ready && CE && !Player.HasBuff("XerathLocusOfPower2"))
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                switch (Menu["c"]["eo"].As<MenuList>().Value)
                {
                    case 0:
                        if (target.IsValidTarget(E.Range) && target != null)
                        {
                            E.Cast(target);
                        }
                        break;
                    case 1:
                        if (target.IsValidTarget(E.Range) && target != null && target.HasBuffOfType(BuffType.Slow))
                        {
                            E.Cast(target);
                        }
                        break;
                    case 2:
                        if (target.IsValidTarget(E.Range) && target != null && target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Fear) || target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Suppression) || target.HasBuffOfType(BuffType.Taunt))
                        {
                            E.Cast(target);
                        }
                        break;
                }
            }
        }

        private void Ultimate()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range);
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
            
            bool HQ = Menu["h"]["q"].Enabled;
            float MQ = Menu["h"]["qm"].As<MenuSlider>().Value;
            if (Q.Ready && HQ && Player.ManaPercent() > MQ)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.ChargedMaxRange);
                if (target.IsValidTarget(Q.ChargedMaxRange) && target != null)
                {
                    Q.Cast(target);
                }
            }
            bool HW = Menu["h"]["w"].Enabled;
            float MW = Menu["h"]["wm"].As<MenuSlider>().Value;
            if (W.Ready && HW && Player.ManaPercent() >= MW)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target.IsValidTarget(W.Range) && target != null)
                {
                    W.Cast(target);
                }
            }
        }
    }
}