namespace Nidalee
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

    internal class Nidalee
    {
        public static Menu Menu = new Menu("Nidalee by Zypppy", "Nidalee by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, Q2, W, W2, W3, E, E2, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1400);
            Q.SetSkillshot(0.25f, 40f, 1300f, true, SkillshotType.Line, false, HitChance.High);
            Q2 = new Spell(SpellSlot.Q, 355);
            W = new Spell(SpellSlot.W, 900);
            W.SetSkillshot(0.75f, 80f, float.MaxValue, false, SkillshotType.Circle, false, HitChance.Medium);
            W2 = new Spell(SpellSlot.W, 475);
            W2.SetSkillshot(0.25f, 75f, 1500f, false, SkillshotType.Line, false, HitChance.Low);
            W3 = new Spell(SpellSlot.W, 750);
            W3.SetSkillshot(0.25f, 75f, 1800f, false, SkillshotType.Line, false, HitChance.Low);
            E = new Spell(SpellSlot.E, 600);
            E2 = new Spell(SpellSlot.E, 350);
            E2.SetSkillshot(0.25f, (float)(15 * Math.PI / 180), float.MaxValue, false, SkillshotType.Cone, false, HitChance.Low);
            R = new Spell(SpellSlot.R, 500);
        }

        public Nidalee()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Human Q"));
                ComboMenu.Add(new MenuBool("usecq", "Use Cougar Q"));
                ComboMenu.Add(new MenuBool("usew", "Use Human W"));
                ComboMenu.Add(new MenuBool("usecw", "Use Cougar W"));
                //ComboMenu.Add(new MenuBool("usee", "NOT WORKING Use Human E"));
                //ComboMenu.Add(new MenuSlider("useeh", "Min HP To Use E", 30, 0, 100));
                //ComboMenu.Add(new MenuSlider("useehm", "Min Mana To Use E", 30, 0, 100));
                ComboMenu.Add(new MenuBool("usece", "Use Cougar E"));
                ComboMenu.Add(new MenuBool("user", "Use R To Switch Forms"));

            }
            Menu.Add(ComboMenu);

            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("kq", "Killsteal with Human Q"));
            }
            Menu.Add(KSMenu);

            var miscmenu = new Menu("misc", "Misc");
            {
                miscmenu.Add(new MenuBool("autoq", "Auto Human Q on CC"));
                miscmenu.Add(new MenuBool("autow", "Auto Human W on CC"));
            }
            Menu.Add(miscmenu);

            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Human Q Range"));
                DrawMenu.Add(new MenuBool("drawq2", "Draw Cougar Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw Human W Range"));
                DrawMenu.Add(new MenuBool("draww2", "Draw Cougar W Range"));
                DrawMenu.Add(new MenuBool("draww3", "Draw Cougar W Long Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw Human E Range"));
                DrawMenu.Add(new MenuBool("drawe2", "Draw Cougar E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
            }

            Menu.Add(DrawMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Nidalee by Zypppy - Loaded");
        }

        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;

            if (Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Indigo);
            }
            if (Menu["drawings"]["drawq2"].Enabled)
            {
                Render.Circle(Player.Position, Q2.Range, 40, Color.Indigo);
            }

            if (Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Fuchsia);
            }
            if (Menu["drawings"]["draww2"].Enabled)
            {
                Render.Circle(Player.Position, W2.Range, 40, Color.Fuchsia);
            }
            if (Menu["drawings"]["draww3"].Enabled)
            {
                Render.Circle(Player.Position, W3.Range, 40, Color.Fuchsia);
            }

            if (Menu["drawings"]["drawe"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.DeepPink);
            }
            if (Menu["drawings"]["drawe2"].Enabled)
            {
                Render.Circle(Player.Position, E2.Range, 40, Color.DeepPink);
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
            if (Menu["misc"]["autoq"].Enabled && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss")
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
            if (Menu["misc"]["autow"].Enabled && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack")
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
                          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
                          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
                          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(W.Range) &&
                         !Invulnerable.Check(t, DamageType.Magical)))
                {
                    W.Cast(target);
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
                Menu["killsteal"]["kq"].Enabled && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss")
            {
                var bestTarget = GetBestKillableHero(Q, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(Q.Range))
                {
                    Q.Cast(bestTarget);
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
            bool useQ2 = Menu["combo"]["usecq"].Enabled;
            bool useW = Menu["combo"]["usew"].Enabled;
            bool useW2 = Menu["combo"]["usecw"].Enabled;
            //bool useE = Menu["combo"]["usee"].Enabled;
            bool useE2 = Menu["combo"]["usece"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && target.IsValidTarget(Q.Range))
            {
                if (target != null)
                {
                    Q.Cast(target);
                }
            }
            if (Q2.Ready && useQ2 && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && target.IsValidTarget(Q2.Range))
            {
                if (target != null)
                {
                    Q2.Cast();
                }
            }
            if (W.Ready && useW && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack" && target.IsValidTarget(W.Range))
            {
                if (target != null)
                {
                    W.Cast(target);
                }
            }
            if (W2.Ready && useW2 && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && target.IsValidTarget(W2.Range))
            {
                if (target != null)
                {
                    W2.Cast(target);
                }
            }
            if (W3.Ready && useW2 && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && target.HasBuff("NidaleePassiveHunted") && Player.HasBuff("NidaleePassiveHunting") && target.IsValidTarget(W3.Range))
            {
                if (target != null)
                {
                    W3.Cast(target);
                }
            }
            if (E2.Ready && useE2 && Player.SpellBook.GetSpell(SpellSlot.E).Name == "Swipe" && target.IsValidTarget(E2.Range))
            {
                if (target != null)
                {
                    E2.Cast(target);
                }
            }
            if (R.Ready && useR && target.IsValidTarget(Q.Range))
            {
                if (target != null)
                {
                    R.Cast();
                }
            }

        }

    }
}