﻿namespace Varus
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

    internal class Varus
    {

        public static Menu Menu = new Menu("Varus By Zypppy", "Varus By Zypppy", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();

        public static Spell Q, E, R;

        public void LoadSpells()

        {
            Q = new Spell(SpellSlot.Q, 850);
            Q.SetCharged("VarusQ", "VarusQLaunch", 300, 1525, 4.0f);
            Q.SetSkillshot(0.600f, 75f, 1800f, false, SkillshotType.Line, false, HitChance.High);
            E = new Spell(SpellSlot.E, 850);
            E.SetSkillshot(0.650f, 150f, 1800f, false, SkillshotType.Circle, false, HitChance.High);
            R = new Spell(SpellSlot.R, 850);
            R.SetSkillshot(0.650f, 150f, 1800f, false, SkillshotType.Line, false, HitChance.High);


        }


        public Varus()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
            }


            Menu.Add(ComboMenu);

            Menu.Attach();

            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Varus by Zypppy - Loaded");
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



        }




        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
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
            var target = GetBestEnemyHeroTargetInRange(Q.ChargedMaxRange);

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

        }

    }
}