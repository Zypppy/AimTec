namespace Blitzcrank
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

    internal class Blitzcrank
    {
        public static Menu Menu = new Menu("Blitzcrank by Zypppy", "Blitzcrank by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 950);
            W = new Spell(SpellSlot.W, 125);
            E = new Spell(SpellSlot.E, 125);
            R = new Spell(SpellSlot.R, 600);
            Q.SetSkillshot(0.25f, 70f, 1700f, true, SkillshotType.Line, false, HitChance.High);
        }

        public Blitzcrank()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
            }
            Menu.Add(ComboMenu);

            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("kq", "Killsteal with Q"));
                KSMenu.Add(new MenuBool("kr", "Killsteal with R"));
            }
            Menu.Add(KSMenu);

            var SupportItemsMenu = new Menu("supportitems", "Support Items");
            {
                SupportItemsMenu.Add(new MenuBool("usefotm", "Use Face of the Mountain"));
                SupportItemsMenu.Add(new MenuSlider("fotmslider", "and HP% is less than:", 40, 0, 100));
                SupportItemsMenu.Add(new MenuBool("usesolari", "Use Solari"));
                SupportItemsMenu.Add(new MenuSlider("solarislider", "Use Solari when allies in range >=", 3, 1, 5));
                SupportItemsMenu.Add(new MenuSlider("solarislider2", "and HP% is less than:", 40, 0, 100));

            }
            Menu.Add(SupportItemsMenu);
            
            var miscmenu = new Menu("misc", "Misc");
            {
                miscmenu.Add(new MenuBool("autoq", "Auto Q on CC"));
            }
            Menu.Add(miscmenu);

            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
            }

            Menu.Add(DrawMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Blitzcrank by Zypppy - Loaded");
        }

        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;

            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, 950, 40, Color.CornflowerBlue);
            }

            if (Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.CornflowerBlue);
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
            if (Q.Ready &&
                Menu["killsteal"]["kq"].Enabled)
            {
                var bestTarget = GetBestKillableHero(Q, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(Q.Range))
                {
                    Q.Cast(bestTarget);
                }
            }
            if (R.Ready &&
                Menu["killsteal"]["kr"].Enabled)
            {
                var bestTarget = GetBestKillableHero(R, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.R) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(R.Range))
                {
                    R.Cast();
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
            bool useFOTM = Menu["supportitems"]["usefotm"].Enabled;
            bool useSOLARI = Menu["supportitems"]["usesolari"].Enabled;
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
            if (E.Ready && useE && target.IsValidTarget(300))
            {

                if (target != null)
                {
                    E.Cast();
                }
            }
            var ItemSolari = Player.SpellBook.Spells.Where(o => o != null && o.SpellData != null).FirstOrDefault(o => o.SpellData.Name == "IronStylus");
            if (ItemSolari != null)
            {
                Spell Solari = new Spell(ItemSolari.Slot, 600);
                if (useSOLARI && Solari.Ready)
                {
                    var Allies = GameObjects.AllyHeroes.Where(t => t.IsValidTarget(Solari.Range, true));
                    foreach (var ally in Allies.Where(
                        a => Player.CountAllyHeroesInRange(Solari.Range) >=
                             Menu["supportitems"]["solarislider"].As<MenuSlider>().Value &&
                             a.Health <= a.MaxHealth / 100 *
                             Menu["supportitems"]["solarislider2"].As<MenuSlider>().Value))
                    {
                        Solari.Cast();
                    }
                    if (HealthPrediction.Implementation.GetPrediction(Player, 250 + Game.Ping) <= Player.MaxHealth * 0)
                    {
                        Solari.Cast();
                    }
                }
            }

            var ItemFaceOfTheMountain = Player.SpellBook.Spells.Where(o => o != null && o.SpellData != null).FirstOrDefault(o => o.SpellData.Name == "HealthBomb");
            if (ItemFaceOfTheMountain != null)
            {
                Spell FOTM = new Spell(ItemFaceOfTheMountain.Slot, 700);
                if (useFOTM && FOTM.Ready)
                {
                    var Allies = GameObjects.AllyHeroes.Where(t => t.IsValidTarget(FOTM.Range, true) && !t.IsMe);
                    foreach (var ally in Allies.Where(
                        a => Player.CountAllyHeroesInRange(FOTM.Range) >= 0 &&
                             a.Health <= a.MaxHealth / 100 *
                             Menu["supportitems"]["fotmslider"].As<MenuSlider>().Value))
                    {
                        FOTM.Cast(ally);
                    }
                    if (HealthPrediction.Implementation.GetPrediction(Player, 250 + Game.Ping) <= Player.MaxHealth * 0)
                    {
                        FOTM.Cast(Player);
                    }
                }
            }


        }

    }
}