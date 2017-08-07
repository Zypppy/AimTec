namespace Ekko
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

    internal class Ekko
    {
        public static Menu Menu = new Menu("Ekko by Zypppy", "Ekko by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 600f);
            Q.SetSkillshot(0.66f, 60f, 1200f, false, SkillshotType.Line, false, HitChance.High);
            W = new Spell(SpellSlot.W, 1600f);
            W.SetSkillshot(3.3f, 375f, 1650f, false, SkillshotType.Circle, false, HitChance.Medium);
            E = new Spell(SpellSlot.E, 450);
            E.SetSkillshot(0.5f, 60f, 500f, false, SkillshotType.Line, false, HitChance.Medium);
            R = new Spell(SpellSlot.R, 1600f);
            R.SetSkillshot(0.333f, 350f, 1650f, false, SkillshotType.Circle, false, HitChance.High);
        }

        public Ekko()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("minrh", "Min enemies to Use R", 0, 0, 5));
            }
            Menu.Add(ComboMenu);

            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("kq", "Killsteal with Q"));
                KSMenu.Add(new MenuBool("kr", "Killsteal with R"));
                KSMenu.Add(new MenuSlider("minrhks", "Min enemies to Use R KS", 0, 0, 5));
            }
            Menu.Add(KSMenu);

            var MiscMenu = new Menu("misc", "Misc");
            {
                MiscMenu.Add(new MenuBool("autoq", "Auto Q on CC"));
            }
            Menu.Add(MiscMenu);

            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
            }

            Menu.Add(DrawMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Ekko by Zypppy - Loaded");
        }

        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var EkkoR = ObjectManager.Get<GameObject>().FirstOrDefault(o => o.IsValid && o.IsAlly && o.Name == "Ekko_Base_R_TrailEnd.troy");
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;

            if (Q.Ready && Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Indigo);
            }

            if (W.Ready && Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Indigo);
            }

            if (E.Ready && Menu["drawings"]["drawe"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.DeepPink);
            }
            if (R.Ready && Menu["drawings"]["drawr"].Enabled)
            {
                if (EkkoR != null)
                {
                    Render.Circle(EkkoR.Position, R.Range, 40, Color.DeepPink);
                }
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

            if (R.Ready && Menu["misc"]["autoq"].Enabled)
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
                var bestTarget = GetBestKillableHero(Q, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(Q.Range))
                {
                    R.Cast(bestTarget);
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
            bool useQ2 = Menu["combo"]["useqa"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;
            float rstacks = Menu["combo"]["minr"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range) && Menu["qwhitelist"][target.ChampionName.ToLower()].As<MenuBool>().Enabled)
            {
                if (target != null)
                {
                    Q.Cast(target);
                }
            }
            if (Q.Ready && useQ2 && target.IsValidTarget(Player.AttackRange) && Menu["qwhitelist"][target.ChampionName.ToLower()].As<MenuBool>().Enabled)
            {
                if (target != null)
                {
                    Q.Cast(target);
                }
            }
            if (R.Ready && useR && Player.GetSpell(SpellSlot.R).Ammo >= rstacks && target.IsValidTarget(R.Range))
            {
                if (target != null)
                {
                    R.Cast(target);
                }
            }
            var ItemCutlass = Player.SpellBook.Spells.Where(o => o != null && o.SpellData != null).FirstOrDefault(o => o.SpellData.Name == "BilgewaterCutlass");
            if (ItemCutlass != null)
            {
                Spell Cutlass = new Spell(ItemCutlass.Slot, 550);
                if (Menu["items"]["usecutlass"].Enabled && Cutlass.Ready)
                {
                    var Enemies = GameObjects.EnemyHeroes.Where(t => t.IsValidTarget(Cutlass.Range, true) && !t.IsInvulnerable);
                    foreach (var enemy in Enemies.Where(e =>
                            // TODO: CHANGE LOGICS
                            e.Health <= Player.Health && Player.CountEnemyHeroesInRange(1000) <= 1 ||
                            e.IsFacing(Player) && e.Health >= Player.Health &&
                            Player.CountEnemyHeroesInRange(1000) <= 1 ||
                            e.TotalAttackDamage >= 100 &&
                            Player.CountEnemyHeroesInRange(1000) <= 2 ||
                            e.IsFacing(Player) && e.Health >= Player.Health &&
                            Player.CountEnemyHeroesInRange(1000) >= 3 ||
                            e.TotalAttackDamage >= Player.TotalAttackDamage &&
                            Player.CountEnemyHeroesInRange(1000) <= 3))
                    {

                        Cutlass.Cast(enemy);
                    }
                }
            }
            var ItemGunblade = Player.SpellBook.Spells.Where(o => o != null && o.SpellData != null).FirstOrDefault(o => o.SpellData.Name == "HextechGunblade");
            if (ItemGunblade != null)
            {
                Spell Gunblade = new Spell(ItemGunblade.Slot, 700);
                if (Menu["items"]["usegunblade"].Enabled && Gunblade.Ready)
                {
                    var Enemies = GameObjects.EnemyHeroes.Where(t => t.IsValidTarget(Gunblade.Range, true) && !t.IsInvulnerable);

                    foreach (var enemy in Enemies.Where(
                        e => e.Health <= e.MaxHealth / 100 * (Menu["items"]["gunbladeslider"].Value)))
                    {
                        Gunblade.Cast(enemy);
                    }
                }
            }

        }

    }
}