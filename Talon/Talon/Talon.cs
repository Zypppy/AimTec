﻿namespace Talon
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

    internal class Talon
    {
        public static Menu Menu = new Menu("Talon by Zypppy", "Talon by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, Q2, W, R, Ignite;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 230f);
            Q2 = new Spell(SpellSlot.Q, 550f);
            W = new Spell(SpellSlot.W, 800f);
            W.SetSkillshot(0.25f, 75, 2300, false, SkillshotType.Line);
            R = new Spell(SpellSlot.R, 550f);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner1, 600);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner2, 600);
        }

        public Talon()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("combo", "Combo");
            {
                Combo.Add(new MenuBool("useq2", "Use Standart Q"));
                Combo.Add(new MenuBool("useq", "Use Melee Q"));
                Combo.Add(new MenuBool("usew", "Use W"));
                Combo.Add(new MenuBool("user", "Use R"));
                Combo.Add(new MenuSlider("usercount", "Use R If Enemies >=", 3, 1, 5));
                Combo.Add(new MenuBool("userkill", "Use R Only If Can Kill"));
                Combo.Add(new MenuKeyBind("key", "Manual R Key:", KeyCode.T, KeybindType.Press));
                Combo.Add(new MenuBool("youmuu", "Use Youmuu GhostBlade"));
                Combo.Add(new MenuBool("tiamat", "Use Tiamat"));
                Combo.Add(new MenuBool("hydra", "Use Hydra"));
            }
            Menu.Add(Combo);
            var Harass = new Menu("harass", "Harass");
            {
                Harass.Add(new MenuBool("useq2", "Use Standart Q"));
                Harass.Add(new MenuBool("useq", "Use Melee Q"));
                Harass.Add(new MenuSlider("manaq", "Harass Q Mana", 60, 0, 100));
                Harass.Add(new MenuBool("usew", "Use W"));
                Harass.Add(new MenuSlider("manaw", "harass W Mana", 60, 0, 100));

            }
            Menu.Add(Harass);
            var LaneClear = new Menu("laneclear", "Lane Clear");
            {
                LaneClear.Add(new MenuBool("useq", "Use Standart Q"));
                LaneClear.Add(new MenuBool("useq2", "Use Melee Q"));
                LaneClear.Add(new MenuSlider("manaq", "Lane Clear Q Mana", 60, 0, 100));
                LaneClear.Add(new MenuBool("usew", "Use W"));
                LaneClear.Add(new MenuSlider("manaw", "Lane Clear W Mana", 60, 0, 100));
            }
            Menu.Add(LaneClear);
            var LastHit = new Menu("lasthit", "Last Hit");
            {
                LastHit.Add(new MenuBool("useq2", "Use Standart Q"));
                LastHit.Add(new MenuBool("useq", "Use Melee Q"));
                LastHit.Add(new MenuSlider("manaq", "Last Hit Q Mana", 60, 0, 100));
            }
            Menu.Add(LastHit);
            var JungleClear = new Menu("jungleclear", "Jungle Clear");
            {
                JungleClear.Add(new MenuBool("useq2", "Use Standart Q"));
                JungleClear.Add(new MenuBool("useq", "Use Melee Q"));
                JungleClear.Add(new MenuSlider("manaq", "Jungle Clear Q Mana", 60, 0, 100));
                JungleClear.Add(new MenuBool("usew", "Use W"));
                JungleClear.Add(new MenuSlider("manaw", "Jungle Clear W Mana", 60, 0, 100));
            }
            Menu.Add(JungleClear);
            var Killsteal = new Menu("killsteal", "Killsteal");
            {
                Killsteal.Add(new MenuBool("usew", "Use W"));
                Killsteal.Add(new MenuBool("ignite", "Use Ignite"));
            }
            Menu.Add(Killsteal);
            var Drawings = new Menu("drawings", "Drawings");
            {
                Drawings.Add(new MenuBool("drawq2", "Draw Standart Q"));
                Drawings.Add(new MenuBool("drawq", "Draw Melee Q"));
                Drawings.Add(new MenuBool("draww", "Draw W"));
                Drawings.Add(new MenuBool("drawr", "Draw R"));
                Drawings.Add(new MenuBool("drawdmg", "Draw DMG"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            //Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Talon by Zypppy - Loaded");
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

            if (Menu["drawings"]["drawq2"].Enabled && Q2.Ready)
            {
                Render.Circle(Player.Position, Q2.Range, 40, Color.Azure);
            }
            if (Menu["drawings"]["drawq"].Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Beige);
            }
            if (Menu["drawings"]["draww"].Enabled && W.Ready)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.BlueViolet);
            }
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Chocolate);
            }
            if (Menu["drawings"]["drawdmg"].Enabled)
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
                       var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.W) + Player.GetSpellDamage(unit, SpellSlot.R)
                       ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R))) / unit.MaxHealth * 100 / 100)
                       : 0));
                       Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.W) + Player.GetSpellDamage(unit, SpellSlot.R) ? Color.GreenYellow : Color.Orange);

                   });

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
                    //OnCombo();
                    break;
                case OrbwalkingMode.Mixed:
                    //OnHarass();
                    break;
                case OrbwalkingMode.Laneclear:
                    //OnLaneClear();
                    //OnJungleClear();
                    break;
                case OrbwalkingMode.Lasthit:
                    //OnLastHit();
                    break;

            }
            if (Menu["combo"]["key"].Enabled)
            {
                //ManualR();
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
            if (W.Ready && Menu["killsteal"]["usew"].Enabled)
            {
                var besttarget = GetBestKillableHero(W, DamageType.Physical, false);
                var WPredition = W.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.W) >= besttarget.Health && besttarget.IsValidTarget(W.Range))
                {
                    if (WPredition.HitChance >= HitChance.High)
                    {
                        W.Cast(WPredition.CastPosition);
                    }
                }
            }
            if (Menu["killsteal"]["ignite"].Enabled && Ignite != null)
            {
                var besttarget = GetBestKillableHero(Ignite, DamageType.True, false);
                if (besttarget != null && IgniteDamages - 100 >= besttarget.Health && besttarget.IsValidTarget(Ignite.Range))
                {
                    Ignite.CastOnUnit(besttarget);
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
    }
}