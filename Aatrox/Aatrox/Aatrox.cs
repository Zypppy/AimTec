namespace Aatrox
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
    using Aimtec.SDK.Events;

    internal class Aatrox
    {
        public static Menu Menu = new Menu("Aatrox by Zypppy", "Aatrox by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, Q2, W, E, R, Ignite;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650f); //AatroxQ
            Q.SetSkillshot(0.6f, 270f, 2000f, false, SkillshotType.Circle);
            Q2 = new Spell(SpellSlot.Q, 650f); //AatroxQ
            Q2.SetSkillshot(0.6f, 150f, 2000f, false, SkillshotType.Circle);
            W = new Spell(SpellSlot.W); //AatroxW AatroxW2
            E = new Spell(SpellSlot.E, 1075f); //AatroxE
            E.SetSkillshot(0.5f, 40f, 1200f, false, SkillshotType.Line);
            R = new Spell(SpellSlot.R, 550f); // AatroxR
           
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner1, 600);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner2, 600);
        }

        public Aatrox()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuList("qco", "Q Combo Options", new[] { "Outer Q", "Inner Q" }, 1));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuSlider("whp", "Switch To Heal if Hp <", 60, 0, 100));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("rhp", "Target HP <", 60, 0, 100));
                ComboMenu.Add(new MenuSlider("rce", "Or Enemy >=", 2, 1, 5));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("usee", "Use E"));
            }
            Menu.Add(HarassMenu);
            var LaneClearMenu = new Menu("laneclear", "Lane Clear");
            {
                LaneClearMenu.Add(new MenuBool("useq", "Use Outer Q"));
                LaneClearMenu.Add(new MenuList("qlo", "Q Lane Clear Options", new[] { "Outer Q", "Inner Q" }, 1));
                LaneClearMenu.Add(new MenuBool("usew", "Use W"));
                LaneClearMenu.Add(new MenuSlider("whp", "Switch To Heal If Hp <", 50, 0, 100));
                LaneClearMenu.Add(new MenuBool("usee", "Use E"));
            }
            Menu.Add(LaneClearMenu);
            var JungleClearMenu = new Menu("jungleclear", "Jungle Clear");
            {
                JungleClearMenu.Add(new MenuBool("useq", "Use Q"));
                JungleClearMenu.Add(new MenuList("qjo", "Q Jungle Clear Options", new[] { "Outer Q", "Inner Q" }, 1));
                JungleClearMenu.Add(new MenuBool("usew", "Use W"));
                JungleClearMenu.Add(new MenuSlider("whp", "Switch To Heal If Hp <", 50, 0, 100));
                JungleClearMenu.Add(new MenuBool("usee", "Use E"));
            }
            Menu.Add(JungleClearMenu);
            var FleeMenu = new Menu("flee", "Flee");
            {
                FleeMenu.Add(new MenuBool("useq", "Use Q"));
                FleeMenu.Add(new MenuBool("usee", "Use E To Slow enemy"));
                FleeMenu.Add(new MenuKeyBind("key", "Flee Key:", KeyCode.Z, KeybindType.Press));
            }
            Menu.Add(FleeMenu);
            var KillstealMenu = new Menu("killsteal", "Killsteal");
            {
                KillstealMenu.Add(new MenuBool("useq", "Use Q"));
                KillstealMenu.Add(new MenuBool("usee", "Use E"));
                KillstealMenu.Add(new MenuBool("user", "Use R"));
                KillstealMenu.Add(new MenuBool("ignite", "Use Ignite"));
            }
            Menu.Add(KillstealMenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawdmg", "Draw DMG"));
            }
            Menu.Add(DrawMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Aatrox by Zypppy - Loaded");
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

            if (Menu["drawings"]["drawq"].Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Azure);
            }
            if (Menu["drawings"]["draww"].Enabled && W.Ready)
            {
                Render.Circle(Player.Position, Player.AttackRange, 40, Color.Beige);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.BlueViolet);
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
                       var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R)
                       ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R))) / unit.MaxHealth * 100 / 100)
                       : 0));
                       Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R) ? Color.GreenYellow : Color.Orange);

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
            if (Menu["flee"]["key"].Enabled)
            {
                Flee();
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
            if (R.Ready && Menu["killsteal"]["user"].Enabled)
            {
                var besttarget = GetBestKillableHero(R, DamageType.Magical, false);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.R) >= besttarget.Health && besttarget.IsValidTarget(R.Range))
                {
                    R.Cast(besttarget);
                }
            }
            if (Q.Ready && Menu["killsteal"]["useq"].Enabled)
            {
                var besttarget = GetBestKillableHero(Q, DamageType.Physical, false);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.Q) >= besttarget.Health && besttarget.IsValidTarget(Q2.Range))
                {
                    Q2.Cast(besttarget);
                }
            }
            if (E.Ready && Menu["killsteal"]["usee"].Enabled)
            {
                var besttarget = GetBestKillableHero(E, DamageType.Physical, false);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.E) >= besttarget.Health && besttarget.IsValidTarget(E.Range))
                {
                    E.Cast(besttarget);
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

        private void OnCombo()
        {
            var target = GetBestEnemyHeroTargetInRange(E.Range);


            if (!target.IsValidTarget())
            {
                return;
            }

            bool useQ = Menu["combo"]["useq"].Enabled;
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
            {
                switch (Menu["combo"]["qco"].As<MenuList>().Value)
                {
                    case 0:
                        Q.Cast(target);
                        break;
                    case 1:
                        Q2.Cast(target);
                        break;
                }

            }

            bool useW = Menu["combo"]["usew"].Enabled;
            float hpW = Menu["combo"]["whp"].As<MenuSlider>().Value;
            if (W.Ready && useW && target.IsValidTarget(500))
            {
                switch (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState)
                {
                    case 1:
                        if (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 1 && Player.HealthPercent() > hpW)
                        {
                            W.Cast();
                        }
                        break;
                    case 2:
                        if (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2 && Player.HealthPercent() < hpW)
                        {
                            W.Cast();
                        }
                        break;
                }
            }

            bool useE = Menu["combo"]["usee"].Enabled;
            if (E.Ready && target.IsValidTarget(E.Range) && useE)
            {
                E.Cast(target);
            }

            bool useR = Menu["combo"]["user"].Enabled;
            float thpR = Menu["combo"]["rhp"].As<MenuSlider>().Value;
            float thR = Menu["combo"]["rce"].As<MenuSlider>().Value;
            if (R.Ready && useR && target.IsValidTarget(R.Range) && Player.CountEnemyHeroesInRange(R.Range) >= thR || target.HealthPercent() < thpR)
            {
                R.Cast();
            }
        }

        private void OnHarass()
        {
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool useE = Menu["harass"]["usee"].Enabled;
            if (E.Ready && target.IsValidTarget(E.Range) && useE)
            {
                E.Cast(target);
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
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(E.Range))
            {
                if (!minion.IsValidTarget())
                {
                    return;
                }

                bool useQ = Menu["laneclear"]["useq"].Enabled;
                if (Q.Ready && useQ && minion.IsValidTarget(Q.Range))
                {
                    switch (Menu["laneclear"]["qlo"].As<MenuList>().Value)
                    {
                        case 0:
                            Q.Cast(minion);
                            break;
                        case 1:
                            Q2.Cast(minion);
                            break;
                    }

                }

                bool useW = Menu["laneclear"]["usew"].Enabled;
                float hpW = Menu["laneclear"]["whp"].As<MenuSlider>().Value;
                if (W.Ready && useW && minion.IsValidTarget(500))
                {
                    switch (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState)
                    {
                        case 1:
                            if (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 1 && Player.HealthPercent() > hpW)
                            {
                                W.Cast();
                            }
                            break;
                        case 2:
                            if (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2 && Player.HealthPercent() < hpW)
                            {
                                W.Cast();
                            }
                            break;
                    }
                }

                bool useE = Menu["laneclear"]["usee"].Enabled;
                if (E.Ready && minion.IsValidTarget(E.Range) && useE)
                {
                    E.Cast(minion);
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
            foreach (var jungle in GameObjects.Jungle.Where(m => m.IsValidTarget(E.Range)).ToList())
            {
                if (!jungle.IsValidTarget() || !jungle.IsValidSpellTarget())
                {
                    return;
                }

                bool useQ = Menu["jungleclear"]["useq"].Enabled;
                if (Q.Ready && useQ && jungle.IsValidTarget(Q.Range))
                {
                    switch (Menu["jungleclear"]["qjo"].As<MenuList>().Value)
                    {
                        case 0:
                            Q.Cast(jungle);
                            break;
                        case 1:
                            Q2.Cast(jungle);
                            break;
                    }

                }

                bool useW = Menu["jungleclear"]["usew"].Enabled;
                float hpW = Menu["jungleclear"]["whp"].As<MenuSlider>().Value;
                if (W.Ready && useW && jungle.IsValidTarget(500))
                {
                    switch (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState)
                    {
                        case 1:
                            if (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 1 && Player.HealthPercent() > hpW)
                            {
                                W.Cast();
                            }
                            break;
                        case 2:
                            if (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2 && Player.HealthPercent() < hpW)
                            {
                                W.Cast();
                            }
                            break;
                    }
                }

                bool useE = Menu["jungleclear"]["usee"].Enabled;
                if (E.Ready && jungle.IsValidTarget(E.Range) && useE)
                {
                    E.Cast(jungle);
                }
            }
        }

        private void Flee()
        {
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);

            bool useQ = Menu["flee"]["useq"].Enabled;
            if (useQ && Q.Ready)
            {
                Q.Cast(Game.CursorPos);
            }

            bool useE = Menu["flee"]["usee"].Enabled;
            if (useE && E.Ready && target.IsValidTarget(E.Range))
            {
                E.Cast(target);
            }
        }
    }
}
