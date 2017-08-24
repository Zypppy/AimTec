namespace Garen
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

    internal class Garen
    {
        public static Menu Menu = new Menu("Nidalee by Zypppy", "Nidalee by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 200);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 320);
            R = new Spell(SpellSlot.R, 400);
        }

        public Garen()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuSlider("qrange", "Q Range", 200, 175, 500));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuSlider("wrange", "W Range", 200, 175, 500));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
                ComboMenu.Add(new MenuBool("user", "Use R To Execute"));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useq", "Use Q to Harass"));
            }
            Menu.Add(HarassMenu);
            var LaneClearMenu = new Menu("laneclear", "Lane Clear");
            {
                LaneClearMenu.Add(new MenuBool("useqlc", "Lane Q to Clear"));
                LaneClearMenu.Add(new MenuBool("useqlh", "Use Q Last Hit"));
                LaneClearMenu.Add(new MenuBool("useelc", "Use E to Clear"));
            }
            Menu.Add(LaneClearMenu);
            var JunglClearMenu = new Menu("jungleclear", "Jungle Clear");
            {
                JunglClearMenu.Add(new MenuBool("usejq", "Use Q in Jungle"));
                JunglClearMenu.Add(new MenuBool("useje", "Use E in Jungle"));
            }
            Menu.Add(JunglClearMenu);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("kr", "Killsteal with R"));
            }
            Menu.Add(KSMenu);
            var miscmenu = new Menu("misc", "Misc");
            {
                miscmenu.Add(new MenuBool("autoq", "Auto Q on Slows"));
            }
            Menu.Add(miscmenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawqdmg", "Draw Q DMG"));
                DrawMenu.Add(new MenuBool("drawrdmg", "Draw R DMG"));
            }
            Menu.Add(DrawMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Garen by Zypppy - Loaded");
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
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;

            float rangeq = Menu["combo"]["qrange"].As<MenuSlider>().Value;
            if (Menu["drawings"]["drawq"].Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, rangeq, 40, Color.Indigo);
            }
            float rangew = Menu["combo"]["wrange"].As<MenuSlider>().Value;
            if (Menu["drawings"]["draww"].Enabled && W.Ready)
            {
                Render.Circle(Player.Position, rangew, 40, Color.Fuchsia);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.DeepPink);
            }
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Aquamarine);
            }
            if (Menu["drawings"]["drawqdmg"].Enabled && Q.Ready)
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
                            var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R)
                                                            ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R))) / unit.MaxHealth * 100 / 100)
                                                            : 0));

                            Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R) ? Color.GreenYellow : Color.Orange);

                        });
            }
            if (Menu["drawings"]["drawrdmg"].Enabled && R.Ready)
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
                            var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.R)
                                                            ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.R))) / unit.MaxHealth * 100 / 100)
                                                            : 0));

                            Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.R) ? Color.GreenYellow : Color.Orange);

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
                    OnCombo();
                    break;
                case OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
                case OrbwalkingMode.Laneclear:
                    OnLaneClear();
                    OnJungleClear();
                    break;

            }
            Killsteal();
            if (Menu["misc"]["autoq"].Enabled)
            {
                if (Player.HasBuffOfType(BuffType.Slow))
                {
                    Q.Cast();
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
            if (R.Ready && Menu["killsteal"]["kr"].Enabled)
            {
                var besttarget = GetBestKillableHero(R, DamageType.Magical, false);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.R) >= besttarget.Health && besttarget.IsValidTarget(R.Range))
                {
                    R.Cast(besttarget);
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
            float rangeQ = Menu["combo"]["qrange"].As<MenuSlider>().Value;
            bool useW = Menu["combo"]["usew"].Enabled;
            float rangeW = Menu["combo"]["wrange"].As<MenuSlider>().Value;
            bool useE = Menu["combo"]["usee"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(1000);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && useQ && target.IsValidTarget(rangeQ))
            {
                Q.Cast();
            }
            if (W.Ready && useW && target.IsValidTarget(rangeW))
            {
                W.Cast();
            }
            if (E.Ready && useE && target.IsValidTarget(E.Range) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 1)
            {
                E.Cast();
            }
            if (R.Ready && useR && target.IsValidTarget(R.Range) && target.Health <= Player.GetSpellDamage(target, SpellSlot.R))
            {
                R.Cast(target);
            }
        }
        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            float rangeQ = Menu["combo"]["qrange"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(rangeQ);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && useQ && target.IsValidTarget(rangeQ))
            {
                Q.Cast();
            }
        }
        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsValidTarget(range)).ToList();
        }
        private void OnLaneClear()
        {
            bool useQ = Menu["laneclear"]["useqlc"].Enabled;
            bool useQL = Menu["laneclear"]["useqlh"].Enabled;
            bool useE = Menu["laneclear"]["useelc"].Enabled;
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
            {
                if (Q.Ready && useQ && minion != null && minion.IsValidTarget(200))
                {
                    Q.Cast();
                }
                if (Q.Ready && useQL && minion != null && minion.IsValidTarget(200) && minion.Health <= Player.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast();
                }
                if (E.Ready && useE && minion != null && minion.IsValidTarget(E.Range) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 1)
                {
                    E.Cast();
                }
                else if (E.Ready && minion.IsValidTarget(E.Range) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 2)
                {
                    return;
                }
            }
        }
        public static List<Obj_AI_Minion> GetGenericJungleMinionsTargets()
        {
            return GetGenericJungleMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetGenericJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.Jungle.Where(m => !GameObjects.JungleSmall.Contains(m) && m.IsValidTarget(range)).ToList();
        }
        private void OnJungleClear()
        {
            bool useQ = Menu["jungleclear"]["usejq"].Enabled;
            bool useE = Menu["jungleclear"]["useje"].Enabled;
            foreach (var minion in GameObjects.Jungle.Where(m => m.IsValidTarget(E.Range)).ToList())
            {
                if (!minion.IsValidTarget() || !minion.IsValidSpellTarget())
                {
                    return;
                }
                if (Q.Ready && useQ && minion.IsValidTarget(250))
                {
                    Q.Cast();
                }
                if (E.Ready && useE && minion != null && minion.IsValidTarget(E.Range) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 1)
                {
                    E.Cast();
                }
            }
        }
    }
}
