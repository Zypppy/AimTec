
namespace Zypppy_Thresh
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

    internal class Thresh
    {

        public static Menu Menu = new Menu("Zypppy Thresh", "Zypppy Thresh", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();

        public static Spell Q, W, E, R;

        public void LoadSpells()

        {
            //  Q = new Spell(SpellSlot.Q, 1150);
                Q = new Spell(SpellSlot.Q, 500000);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 100);
            R = new Spell(SpellSlot.R, 450);
            Q.SetSkillshot(0.5f, 70f, 1900f, true, SkillshotType.Line, false, HitChance.High);
            E.SetSkillshot(0.125f, 100f, 2000f, false, SkillshotType.Line, false, HitChance.Medium);



        }


        public Thresh()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
            }
            Menu.Add(ComboMenu);
            //var DrawMenu = new Menu("drawings", "Drawings");
            //{
            //    DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
             //   DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
            //    DrawMenu.Add(new MenuBool("drawtoggle", "Draw Toggle"));
           // }
           // Menu.Add(DrawMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
                HarassMenu.Add(new MenuBool("useq", "Use Q to Harass"));
                HarassMenu.Add(new MenuBool("usew", "Use W to Harass"));
                HarassMenu.Add(new MenuBool("usee", "Use E to Harass"));

            }
            Menu.Add(HarassMenu);
            Menu.Attach();


            Game.OnUpdate += Game_OnUpdate;
            LoadSpells();
            Console.WriteLine("Zypppy Thresh - Loaded");
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
                    OnHarass();
                    break;
                case OrbwalkingMode.Laneclear:
                    break;

            }

            //if (Menu["misc"]["autoq"].Enabled)
            //{
              //  foreach (var target in GameObjects.EnemyHeroes.Where(
             //       t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
             //             t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
            //              t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
             //             t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(Q.Range) &&
             //            !Invulnerable.Check(t, DamageType.Magical)))
            //    {
            //
            //        Q.Cast(target);
            //    }

          //  }

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
                    //Console.WriteLine("meow");
                    Q.Cast(target);
                }
            }
            if (E.Ready && useE && target.IsValidTarget(E.Range))
            {

                if (target != null)
                {
                    E.Cast(target);
                }
            }
        }

        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            bool useE = Menu["harass"]["usee"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            float manapercent = Menu["harass"]["mana"].As<MenuSlider>().Value;
            if (manapercent < Player.ManaPercent())
            {
                if (!target.IsValidTarget())
                {
                    return;
                }


                if (E.Ready && useE && target.IsValidTarget(E.Range))
                {
                    if (target != null)
                    {
                        E.Cast(target);
                    }
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
}
