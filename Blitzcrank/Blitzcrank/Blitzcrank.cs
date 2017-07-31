namespace Blitzcrank
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using Aimtec;
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

    internal class Blitzcrank
    {
        public static Menu Menu = new Menu("Blitzcrank by Zypppy", "Blitzcrank by Zypppy");
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, E, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            E = new Spell(SpellSlot.E, 125);
            R = new Spell(SpellSlot.R, 600);
            Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.Line, false, HitChance.High);
        }

        public Blitzcrank()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            var QSet = new Menu("qset", "Q Settings");
            {
                QSet.Add(new MenuBool("useq", "Use Q in Combo"));

            }
            var WSet = new Menu("wset", "W Settings");
            {
                WSet.Add(new MenuBool("usew", "Use W in Combo"));
                WSet.Add(new MenuList("wmode", "W Mode", new[] { "Always", "Only on Slowed/CC/Immobile" }, 1));

            }
            var ESet = new Menu("eset", "E Settings");
            {
                ESet.Add(new MenuBool("usee", "Use E in Combo"));
            }
            var RSet = new Menu("rset", "R Settings");
            {
                RSet.Add(new MenuBool("user", "Use R in Combo"));
                RSet.Add(new MenuList("rmode", "R Mode", new[] { "Always", "If Killable" }, 0));
                RSet.Add(new MenuSlider("rtick", "Include X R Ticks", 1, 1, 4));
                RSet.Add(new MenuSlider("hitr", "Use R only if Hits X", 1, 1, 5));
                RSet.Add(new MenuSlider("waster", "Don't waste R if Enemy HP lower than", 100, 0, 500));
                RSet.Add(new MenuBool("follow", "Auto R Follow", true));

                RSet.Add(new MenuBool("forcer", "Force R in TeamFights", true));
                RSet.Add(new MenuSlider("forcehit", "^- Min. Enemies", 3, 2, 5));

            }
            Menu.Add(ComboMenu);
            ComboMenu.Add(QSet);
            ComboMenu.Add(WSet);
            ComboMenu.Add(ESet);
            ComboMenu.Add(RSet);

            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
                HarassMenu.Add(new MenuBool("useq", "Use  Q to Harass"));
                HarassMenu.Add(new MenuBool("usee", "Use E to Harass"));

            }
            Menu.Add(HarassMenu);
            var FarmMenu = new Menu("farming", "Farming");
            var LaneClear = new Menu("lane", "Lane Clear");
            {
                LaneClear.Add(new MenuSlider("mana", "Mana Manager", 50));
                LaneClear.Add(new MenuBool("useq", "Use Q to Farm"));
                LaneClear.Add(new MenuBool("lastq", "^- Use for Last Hit"));
                LaneClear.Add(new MenuBool("usee", "Use E to Farm"));
                LaneClear.Add(new MenuSlider("hite", "^- if Hits", 3, 1, 6));
            }
            var JungleClear = new Menu("jungle", "Jungle Clear");
            {
                JungleClear.Add(new MenuSlider("mana", "Mana Manager", 50));
                JungleClear.Add(new MenuBool("useq", "Use Q to Farm"));
                JungleClear.Add(new MenuBool("usee", "Use E to Farm"));

            }
            Menu.Add(FarmMenu);
            FarmMenu.Add(LaneClear);
            FarmMenu.Add(JungleClear);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("ksq", "Killsteal with Q"));
                KSMenu.Add(new MenuBool("kse", "Killsteal with E"));
            }
            Menu.Add(KSMenu);
            var miscmenu = new Menu("misc", "Misc.");
            {
                miscmenu.Add(new MenuBool("autow", "Auto W on CC"));
            }
            Menu.Add(miscmenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawdamage", "Draw Damage"));
            }
            Menu.Add(DrawMenu);
            var FleeMenu = new Menu("flee", "Flee");
            {
                FleeMenu.Add(new MenuBool("useq", "Use Q to Flee"));
                FleeMenu.Add(new MenuKeyBind("key", "Flee Key:", KeyCode.G, KeybindType.Press));
            }
            Menu.Add(FleeMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            LoadSpells();
            Console.WriteLine("Blitzcrank - Loaded");
        }

        static double GetR(Obj_AI_Base target)
        {
            double meow = 0;
            if (Player.SpellBook.GetSpell(SpellSlot.R).Level == 1)
            {
                meow = 250;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.R).Level == 2)
            {
                meow = 375;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.R).Level == 3)
            {
                meow = 500;
            }

            double calc = (Player.TotalAbilityDamage - Player.BaseAbilityDamage) * 1.0;
            double full = calc + meow;
            double damage = Player.CalculateDamage(target, DamageType.Magical, full);
            return damage;
        }

        static double GetQ(Obj_AI_Base target)
        {
            double meow = 0;
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 1)
            {
                meow = 80;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 2)
            {
                meow = 135;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 3)
            {
                meow = 190;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 4)
            {
                meow = 245;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 5)
            {
                meow = 300;
            }

            double calc = (Player.TotalAbilityDamage - Player.BaseAbilityDamage) * 1.0;
            double full = calc + meow;
            double damage = Player.CalculateDamage(target, DamageType.Magical, full);
            return damage;
        }
       
        private void Render_OnPresent()
        {

            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.CornflowerBlue);
            }
            if (Menu["drawings"]["drawe"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.CornflowerBlue);
            }
            if (Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Crimson);
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
            Killsteal();
            if (Menu["misc"]["autoq"].Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
                          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
                          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
                          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(Q.Range) &&
                         !Invulnerable.Check(t, DamageType.Magical)))
                {

                    Q.Cast(target);
                }
            }
        }




        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }


        private void Killsteal()
        {
            var enemies = GetBestKillableHero(Q, DamageType.Physical, false);
            if (enemies != null)
            {
                bool useRK = Menu["killsteal"]["kr"].Enabled;
                if (useRK && (GetR(enemies) > enemies.Health || Player.GetSpellDamage(enemies, SpellSlot.R) > enemies.Health))
                {

                    if (Player.GetSpellDamage(enemies, SpellSlot.R) > GetR(enemies))
                    {
                        R.Cast();
                    }

                }
                bool useQK = Menu["killsteal"]["kq"].Enabled;
                if (useQK && (GetQ(enemies) > enemies.Health || Player.GetSpellDamage(enemies, SpellSlot.Q) > enemies.Health))
                {

                    if (Player.GetSpellDamage(enemies, SpellSlot.Q) > GetQ(enemies))
                    {
                        Q.Cast(enemies);
                    }
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

        private void OnCombo()
        {

            bool useQ = Menu["combo"]["useq"].Enabled;
            bool useE = Menu["combo"]["usee"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);

            if (!target.IsValidTarget())
            {
                return;
            }



            if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
            {

                if (target != null)
                {
                    Q.Cast(target);
                }
            }
            if (E.Ready && useE && target.IsValidTarget(E.Range))
            {

                if (target != null)
                {
                    E.Cast();
                }
            }
            

        }

    }
}

       