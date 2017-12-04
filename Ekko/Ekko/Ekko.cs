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
            Q = new Spell(SpellSlot.Q, 950f); //EkkoQ
            Q.SetSkillshot(0.5f, 60f, 1200f, false, SkillshotType.Line, false);
            W = new Spell(SpellSlot.W, 1600f); //EkkoW
            W.SetSkillshot(3.3f, 200f, 1500f, false, SkillshotType.Circle, false);
            E = new Spell(SpellSlot.E, 600f); //EkkoE
            E.SetSkillshot(0.25f, 100f, 1650f, false, SkillshotType.Line, false, HitChance.Low);
            R = new Spell(SpellSlot.R, 1600f); //EkkoR Ekko_Base_R_TrailEnd.troy
            R.SetSkillshot(0.333f, 350f, 4800f, false, SkillshotType.Circle, false);
        }

        public Ekko()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuList("wo", "W Options", new[] { "Always", "Only When Slowed", "Hard CC Targets" }, 1));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("minrh", "Min enemies to Use R", 0, 1, 5));
            }
            Menu.Add(ComboMenu);

            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useqh", "Use Q"));
                HarassMenu.Add(new MenuSlider("hmana", "Minimum Mana To Harass", 20, 0, 100));
            }
            Menu.Add(HarassMenu);

            var LaneClearMenu = new Menu("lclear", "Lane Clear");
            {
                LaneClearMenu.Add(new MenuBool("useql", "Use Q"));
                LaneClearMenu.Add(new MenuSlider("minmq", "Minimum Minions To Hit", 1, 1, 10));
                LaneClearMenu.Add(new MenuSlider("minmanaq", "Minimum Mana To Farm", 20, 0, 100));
            }
            Menu.Add(LaneClearMenu);

            var JungleClearMenu = new Menu("jclear", "Jungle Clear");
            {
                JungleClearMenu.Add(new MenuBool("useqj", "Use Q"));
                JungleClearMenu.Add(new MenuSlider("minmq", "Minimum Monsters To Hit", 1, 1, 5));
                JungleClearMenu.Add(new MenuSlider("minmanaq", "Minimum Mana To Jungle Q", 20, 0, 100));
                JungleClearMenu.Add(new MenuBool("usewj", "Use W"));
                JungleClearMenu.Add(new MenuSlider("minmw", "Minimum Monsters To Hit", 1, 1, 5));
                JungleClearMenu.Add(new MenuSlider("minmanaw", "Minimum Mana To Jungle W", 20, 0, 100));
                JungleClearMenu.Add(new MenuBool("useej", "Use E"));
                JungleClearMenu.Add(new MenuSlider("minmanae", "Minimum Mana To Jungle E", 20, 0, 100));
            }
            Menu.Add(JungleClearMenu);

            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("kq", "Killsteal with Q"));
                KSMenu.Add(new MenuBool("kr", "Killsteal with R"));
                KSMenu.Add(new MenuSlider("minrhks", "Min enemies to Use R KS", 0, 1, 5));
            }
            Menu.Add(KSMenu);

            var MiscMenu = new Menu("misc", "Misc");
            {
                MiscMenu.Add(new MenuBool("autoq", "Auto Q on CC"));
                MiscMenu.Add(new MenuBool("autor", "Auto R Low Hp"));
                MiscMenu.Add(new MenuSlider("minhp", "Min hp to Use R", 20, 0, 100));
            }
            Menu.Add(MiscMenu);

            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawdmg", "Draw Damage"));
            }

            Menu.Add(DrawMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Ekko by Zypppy - Loaded");
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
            Vector2 gejus;
            var heropos = Render.WorldToScreen(Player.Position, out gejus);
            var EkkoR = ObjectManager.Get<GameObject>().FirstOrDefault(o => o.IsValid && o.Name == "Ekko_Base_R_TrailEnd.troy");
            var xaOffset = (int)gejus.X;
            var yaOffset = (int)gejus.Y;

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
                    Render.Circle(EkkoR.Position, 350, 40, Color.DeepPink);
                }
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
                                                            ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q)) + (Player.GetSpellDamage(unit, SpellSlot.E)) + (Player.GetSpellDamage(unit, SpellSlot.R))) / unit.MaxHealth * 100 / 100)
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
            Killsteal();

            if (Q.Ready && Menu["misc"]["autoq"].Enabled)
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
            bool autoR = Menu["misc"]["autor"].Enabled;
            float hpR = Menu["misc"]["minhp"].As<MenuSlider>().Value;
            if (R.Ready && autoR && Player.HealthPercent() <= hpR)
            {
                R.Cast();
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
                var EkkoR = ObjectManager.Get<GameObject>().FirstOrDefault(o => o.IsValid && o.Name == "Ekko_Base_R_TrailEnd.troy");
                var bestTarget = GetBestKillableHero(R, DamageType.Magical, false);
                if (bestTarget != null && EkkoR.CountEnemyHeroesInRange(350) >= Menu["killsteal"]["minrhks"].As<MenuSlider>().Value &&
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
            
            var target = GetBestEnemyHeroTargetInRange(2000);
            var EkkoR = ObjectManager.Get<GameObject>().FirstOrDefault(o => o.IsValid && o.Name == "Ekko_Base_R_TrailEnd.troy");

            if (!target.IsValidTarget())
            {
                return;
            }

            bool useQ = Menu["combo"]["useq"].Enabled;
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }

            bool useW = Menu["combo"]["usew"].Enabled;
            if (W.Ready && useW && target.IsValidTarget(W.Range))
            {
                switch (Menu["combo"]["wo"].As<MenuList>().Value)
                {
                    case 0:
                           W.Cast(target);
                        break;
                    case 1:
                        if (target.HasBuffOfType(BuffType.Slow))
                        {
                            W.Cast(target);
                        }
                        break;
                    case 2:
                        if (target.HasBuffOfType(BuffType.Charm) || target.HasBuffOfType(BuffType.Knockup) ||
                            target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Stun) ||
                            target.HasBuffOfType(BuffType.Suppression))
                        {
                            W.Cast(target);
                        }
                        break;
                }
            }

            bool useE = Menu["combo"]["usee"].Enabled;
            if (E.Ready && useE && target.IsValidTarget(E.Range))
            {
                E.Cast(target);
            }
            
            bool useR = Menu["combo"]["user"].Enabled;
            float hitR = Menu["combo"]["minrh"].As<MenuSlider>().Value;
            if (R.Ready && useR && target.IsValidTarget(R.Range) && EkkoR != null && EkkoR.CountEnemyHeroesInRange(350f) >= hitR)
            {
                R.Cast();
            }

        }

        
        private void OnHarass()
        {

            bool useQ = Menu["harass"]["useqh"].Enabled;
            float manaQ = Menu["harass"]["hmana"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);


            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range) && Player.ManaPercent() >= manaQ)
            {
                Q.Cast(target);
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
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
            {
               if (!minion.IsValidTarget())
               {
                  return;
               }
                bool useQ = Menu["lclear"]["useql"].Enabled;
                float Qhit = Menu["lclear"]["minmq"].As<MenuSlider>().Value;
                float manaQ = Menu["lclear"]["minmanaq"].As<MenuSlider>().Value;

                if (Q.Ready && useQ && Player.ManaPercent() >= manaQ && minion.IsValidTarget(Q.Range) && GameObjects.Jungle.Count(h => h.IsValidTarget(Q.Range, false, false, minion.ServerPosition)) >= Qhit)
                {
                    Q.Cast(minion);
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
                bool useQ = Menu["jclear"]["useqj"].Enabled;
                float Qhit = Menu["jclear"]["minmq"].As<MenuSlider>().Value;
                float Qmana = Menu["jclear"]["minmanaq"].As<MenuSlider>().Value;
                
                

                if (useQ && Player.ManaPercent() >= Qmana && jungle.IsValidTarget(Q.Range) && GameObjects.Jungle.Count(h => h.IsValidTarget(Q.Range, false, false, jungle.ServerPosition)) >= Qhit)
                {
                    Q.Cast(jungle);
                }

                bool useW = Menu["jclear"]["usewj"].Enabled;
                float Whit = Menu["jclear"]["minmw"].As<MenuSlider>().Value;
                float Wmana = Menu["jclear"]["minmanaw"].As<MenuSlider>().Value;
                if (useW && Player.ManaPercent() >= Wmana && jungle.IsValidTarget(W.Range) && GameObjects.Jungle.Count(h => h.IsValidTarget(W.Range, false, false, jungle.ServerPosition)) >= Whit)
                {
                    W.Cast(jungle);
                }

                bool useE = Menu["jclear"]["useej"].Enabled;
                float Emana = Menu["jclear"]["minmanae"].As<MenuSlider>().Value;
                if (useE && Player.ManaPercent() >= Emana && jungle.IsValidTarget(E.Range))
                {
                    E.Cast(jungle);
                }
            }
        }
    }
}