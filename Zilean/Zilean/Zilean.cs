﻿namespace Zilean
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

    internal class LeeSin
    {

        public static Menu Menu = new Menu("Lee Sin By Zypppy", "Lee Sin By Zypppy", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();

        public static Spell Q, Q2, W, W2, E, E2, R;

        public void LoadSpells()

        {
            Q = new Spell(SpellSlot.Q, 1000);
            W = new Spell(SpellSlot.W, 700);
            Q.SetSkillshot(0.25f, 60f, 1800f, true, SkillshotType.Circular, false, HitChance.High);



        }


        public LeeSin()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("useqq", "Use Second Q"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuBool("useww", "Use Second W"));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
                ComboMenu.Add(new MenuBool("useed", "Use Second E"));
            }


            Menu.Add(ComboMenu);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("kq", "Killsteal with Q "));
                KSMenu.Add(new MenuBool("kr", "Killsteal with R "));
            }
            Menu.Add(KSMenu);
            Menu.Attach();

            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Lee Sin by Zypppy - Loaded");
        }



        static double GetR(Obj_AI_Base target)
        {
            double meow = 0;
            if (Player.SpellBook.GetSpell(SpellSlot.R).Level == 1)
            {
                meow = 150;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.R).Level == 2)
            {
                meow = 300;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.R).Level == 3)
            {
                meow = 450;
            }

            double calc = (Player.TotalAttackDamage - Player.BaseAttackDamage) * 2.0;
            double full = calc + meow;
            double damage = Player.CalculateDamage(target, DamageType.Physical, full);
            return damage;
        }

        static double GetQ(Obj_AI_Base target)
        {
            double meow = 0;
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 1)
            {
                meow = 50;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 2)
            {
                meow = 80;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 3)
            {
                meow = 110;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 4)
            {
                meow = 140;
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Q).Level == 5)
            {
                meow = 170;
            }

            double calc = (Player.TotalAttackDamage - Player.BaseAttackDamage) * 0.9;
            double full = calc + meow;
            double damage = Player.CalculateDamage(target, DamageType.Physical, full);
            return damage;
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
                        R.Cast(enemies);
                    }

                }
                bool useQK = Menu["killsteal"]["kq"].Enabled;
                if (useQK && (GetQ(enemies) > enemies.Health || Player.GetSpellDamage(enemies, SpellSlot.Q) > enemies.Health && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "BlindMonkQOne"))
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
            bool useQ2 = Menu["combo"]["useqq"].Enabled;
            bool useW = Menu["combo"]["usew"].Enabled;
            bool useW2 = Menu["combo"]["useww"].Enabled;
            bool useE = Menu["combo"]["usee"].Enabled;
            bool useE2 = Menu["combo"]["useed"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);

            if (!target.IsValidTarget())
            {
                return;
            }



            if (Q.Ready && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "BlindMonkQOne" && target.IsValidTarget(Q.Range))
            {

                if (target != null)
                {
                    Q.Cast(target);
                }
            }
            if (Q2.Ready && useQ2 && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "BlindMonkQTwo" && target.IsValidTarget(Q2.Range) && target.HasBuff("BlindMonkQOne"))
            {

                if (target != null)
                {
                    DelayAction.Queue(3000, () =>
                    {
                        Q2.Cast();
                    });
                }
            }
            if (W.Ready && useW && Player.SpellBook.GetSpell(SpellSlot.W).Name == "BlindMonkWOne" && target.IsValidTarget(W.Range))
            {

                if (target != null)
                {
                    W.CastOnUnit(Player);
                }
            }
            if (W2.Ready && useW2 && Player.SpellBook.GetSpell(SpellSlot.W).Name == "BlindMonkWTwo" && target.IsValidTarget(W2.Range))
            {

                if (target != null)
                {
                    {
                        W2.Cast();
                    }
                }
            }
            if (E.Ready && useE && Player.SpellBook.GetSpell(SpellSlot.E).Name == "BlindMonkEOne" && target.IsValidTarget(E.Range))
            {

                if (target != null)
                {
                    E.Cast();
                }
            }
            if (E2.Ready && useE2 && Player.SpellBook.GetSpell(SpellSlot.E).Name == "BlindMonkETwo" && target.IsValidTarget(E2.Range) && target.HasBuff("BlindMonkEOne"))
            {

                if (target != null)

                {
                    DelayAction.Queue(2500, () =>
                    {
                        E2.Cast();
                    });
                }
            }

        }

    }
}