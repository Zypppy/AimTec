namespace Teemo
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
    using QGap;

    using Spell = Aimtec.SDK.Spell;
    using Aimtec.SDK.Events;

    internal class Teemo
    {
        public static Menu Menu = new Menu("Teemo by Zypppy", "Teemo by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 675f);
            W = new Spell(SpellSlot.W, Player.AttackRange);
            E = new Spell(SpellSlot.E, Player.AttackRange);
            R = new Spell(SpellSlot.R, 900f);
            R.SetSkillshot(1.0f, 60f, float.MaxValue, false, SkillshotType.Circle, false, HitChance.Medium);
        }

        public Teemo()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("useqa", "Use Q AA Range"));
                ComboMenu.Add(new MenuBool("usew", "Use W In Combo"));
                ComboMenu.Add(new MenuSlider("minmanaw", "Minimum Mana To Use W", 50, 0, 100));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("minr", "Min Stacks to Use R", 0, 0, 3));
            }
            Menu.Add(ComboMenu);

            var WhitelistMenu = new Menu("qwhitelist", "Q Whitelist");
            {
              if (GameObjects.EnemyHeroes.Any())
              {
                foreach (var target in GameObjects.EnemyHeroes)
                {
                 WhitelistMenu.Add(new MenuBool(target.ChampionName.ToLower(), "Use Q: " + target.ChampionName));
                    }
              }
              else
              {
              WhitelistMenu.Add(new MenuSeperator("separaator", "No enemies found."));
              }
            }
            Menu.Add(WhitelistMenu);

            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("kq", "Killsteal with Q"));
            }
            Menu.Add(KSMenu);

            var ItemMenu = new Menu("items", "Items In Combo");
            {
                ItemMenu.Add(new MenuBool("usecutlass", "Use Bilgewater Cutlass"));
                ItemMenu.Add(new MenuBool("usegunblade", "Use Hextech Gunblade"));
                ItemMenu.Add(new MenuSlider("gunbladeslider", "Use Gunblade when enemy HP% is less than:", 70, 0, 100));
            }
            Menu.Add(ItemMenu);

            var miscmenu = new Menu("misc", "Misc");
            {
                miscmenu.Add(new MenuBool("autor", "Auto R on CC"));
            }
            Menu.Add(miscmenu);

            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
            }
            Menu.Add(DrawMenu);
            QGap.Gapcloser.Attach(Menu, "Q Anti- GapClose");
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            Gapcloser.OnGapcloser += OnGapcloser;

            LoadSpells();
            Console.WriteLine("Teemo by Zypppy - Loaded");
        }

        private void OnGapcloser(Obj_AI_Hero target, QGap.GapcloserArgs Args)
        {
            if (target != null && Args.EndPosition.Distance(Player) < Q.Range && Q.Ready && target.IsDashing() && target.IsValidTarget(Q.Range))
            {

                Q.Cast(target);

            }
        }
        

        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;

            if (Q.Ready && Menu["drawings"]["drawq"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Indigo);
            }

            if (R.Ready && Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.DeepPink);
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

            if (R.Ready && Menu["misc"]["autor"].Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
                          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
                          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
                          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(R.Range) &&
                         !Invulnerable.Check(t, DamageType.Magical)))
                {
                    R.Cast(target);
                }
            }
            if (Player.GetSpell(SpellSlot.R).Level > 0)
            {
                R.Range = 150f + 250f * Player.SpellBook.GetSpell(SpellSlot.R).Level - 1;
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
            bool useW = Menu["combo"]["usew"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;
            float rstacks = Menu["combo"]["minr"].As<MenuSlider>().Value;
            float manaw = Menu["combo"]["minmanaw"].As<MenuSlider>().Value;
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
            if (W.Ready && useW && target.IsValidTarget(Q.Range) && manaw <= Player.ManaPercent())
            {
                if (target != null)
                {
                    W.Cast();
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
