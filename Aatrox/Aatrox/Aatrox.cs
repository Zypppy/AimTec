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
        public static Spell Q, Q2, W, E, R, Flash, Ignite;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 650);
            Q2 = new Spell(SpellSlot.Q, 650);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1075);
            R = new Spell(SpellSlot.R, 550);
            Q.SetSkillshot(0.6f, 250, 2000, false, SkillshotType.Circle);
            Q2.SetSkillshot(0.6f, 150, 2000, false, SkillshotType.Circle);
            E.SetSkillshot(0.25f, 35, 1250, false, SkillshotType.Line);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner1, 425);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner2, 425);
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
                ComboMenu.Add(new MenuBool("useq", "Use Outer Q"));
                ComboMenu.Add(new MenuBool("useq2", "Use Inner Q"));
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
                HarassMenu.Add(new MenuBool("useq", "Use Outer Q"));
                HarassMenu.Add(new MenuBool("useq2", "Use Inner Q"));
                HarassMenu.Add(new MenuSlider("qhp", "If HP >=", 60, 0, 100));
                HarassMenu.Add(new MenuBool("usee", "Use E"));
            }
            Menu.Add(HarassMenu);
            var LaneClearMenu = new Menu("laneclear", "Lane Clear");
            {
                LaneClearMenu.Add(new MenuBool("useq", "Use Outer Q"));
                LaneClearMenu.Add(new MenuBool("useq2", "Use Inner Q"));
                LaneClearMenu.Add(new MenuBool("usew", "Use W"));
                LaneClearMenu.Add(new MenuBool("wpriority", "Priority Heal"));
                LaneClearMenu.Add(new MenuSlider("whp", "Switch To Heal If Hp <", 50, 0, 100));
                LaneClearMenu.Add(new MenuBool("usee", "Use E"));
            }
            Menu.Add(LaneClearMenu);
            var JungleClearMenu = new Menu("jungleclear", "Jungle Clear");
            {
                JungleClearMenu.Add(new MenuBool("useq", "Use Q"));
                JungleClearMenu.Add(new MenuBool("usew", "Use W"));
                JungleClearMenu.Add(new MenuBool("wpriority", "Priority Heal"));
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
                    //OnJungleClear();
                    break;

            }
            if (Menu["flee"]["key"].Enabled)
            {
                //Flee();
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
                var QPrediction = Q2.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.Q) >= besttarget.Health && besttarget.IsValidTarget(Q2.Range))
                {
                    if (QPrediction.HitChance >= HitChance.High)
                    {
                        Q2.Cast(QPrediction.CastPosition);
                    }
                }
            }
            if (E.Ready && Menu["killsteal"]["usee"].Enabled)
            {
                var besttarget = GetBestKillableHero(E, DamageType.Physical, false);
                var EPrediction = E.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.E) >= besttarget.Health && besttarget.IsValidTarget(E.Range))
                {
                    if (EPrediction.HitChance >= HitChance.High)
                    {
                        E.Cast(EPrediction.CastPosition);
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
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            bool useQ = Menu["combo"]["useq"].Enabled;
            bool useQ2 = Menu["combo"]["useq2"].Enabled;
            bool useW = Menu["combo"]["usew"].Enabled;
            float hpW = Menu["combo"]["whp"].As<MenuSlider>().Value;
            bool useE = Menu["combo"]["usee"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;
            float thpR = Menu["combo"]["rhp"].As<MenuSlider>().Value;
            float thR = Menu["combo"]["rce"].As<MenuSlider>().Value;
            var QPrediction = Q.GetPrediction(target);
            var Q2Prediction = Q2.GetPrediction(target);
            var EPrediction = E.GetPrediction(target);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && target.IsValidTarget(Q.Range) && useQ)
            {
                if (QPrediction.HitChance >= HitChance.Medium)
                {
                    Q.Cast(QPrediction.CastPosition);
                }
            }
            if (Q.Ready && target.IsValidTarget(Q2.Range) && useQ2)
            {
                if (Q2Prediction.HitChance >= HitChance.Medium)
                {
                    Q2.Cast(Q2Prediction.CastPosition);
                }
            }
            if (W.Ready && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2 && Player.HealthPercent() < hpW)
            {
                W.Cast();
            }
            if (W.Ready && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 1 && Player.HealthPercent() > hpW)
            {
                W.Cast();
            }
            if (E.Ready && target.IsValidTarget(E.Range) && useE)
            {
                if (EPrediction.HitChance >= HitChance.High)
                {
                    E.Cast(EPrediction.CastPosition);
                }
            }
            if (R.Ready && useR && target.IsValidTarget(R.Range) && Player.CountEnemyHeroesInRange(R.Range) >= thR || target.HealthPercent() < thpR)
            {
                R.Cast();
            }
        }
        private void OnHarass()
        {
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            bool useQ = Menu["harass"]["useq"].Enabled;
            bool useQ2 = Menu["harass"]["useq2"].Enabled;
            float hpQ = Menu["harass"]["qhp"].As<MenuSlider>().Value;
            bool useE = Menu["harass"]["usee"].Enabled;
            var QPrediction = Q.GetPrediction(target);
            var Q2Prediction = Q2.GetPrediction(target);
            var EPrediction = E.GetPrediction(target);
            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && target.IsValidTarget(Q.Range) && useQ && Player.HealthPercent() >= hpQ)
            {
                if (QPrediction.HitChance >= HitChance.Medium)
                {
                    Q.Cast(QPrediction.CastPosition);
                }
            }
            if (Q.Ready && target.IsValidTarget(Q2.Range) && useQ2 && Player.HealthPercent() >= hpQ)
            {
                if (Q2Prediction.HitChance >= HitChance.Medium)
                {
                    Q2.Cast(Q2Prediction.CastPosition);
                }
            }
            if (E.Ready && target.IsValidTarget(E.Range) && useE)
            {
                if (EPrediction.HitChance >= HitChance.High)
                {
                    E.Cast(EPrediction.CastPosition);
                }
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
                bool useQ = Menu["laneclear"]["useq"].Enabled;
                bool useQ2 = Menu["laneclear"]["useq2"].Enabled;
                bool useW = Menu["laneclear"]["usew"].Enabled;
                bool priorityW = Menu["laneclear"]["wpriority"].Enabled;
                float hpW = Menu["laneclear"]["whp"].As<MenuSlider>().Value;
                bool useE = Menu["laneclear"]["usee"].Enabled;
                var QPrediction = Q.GetPrediction(minion);
                var Q2Prediction = Q2.GetPrediction(minion);
                var EPrediction = E.GetPrediction(minion);
                if (!minion.IsValidTarget())
                {
                    return;
                }
                if (Q.Ready && minion.IsValidTarget(Q.Range) && useQ)
                {
                    if (QPrediction.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(QPrediction.CastPosition);
                    }
                }
                if (Q.Ready && minion.IsValidTarget(Q2.Range) && useQ2)
                {
                    if (Q2Prediction.HitChance >= HitChance.Medium)
                    {
                        Q2.Cast(Q2Prediction.CastPosition);
                    }
                }
                if (W.Ready && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2 && Player.HealthPercent() < hpW)
                {
                    W.Cast();
                }
                if (W.Ready && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 1 && Player.HealthPercent() > hpW)
                {
                    W.Cast();
                }
                if (E.Ready && minion.IsValidTarget(E.Range) && useE)
                {
                    if (EPrediction.HitChance >= HitChance.High)
                    {
                        E.Cast(EPrediction.CastPosition);
                    }
                }
                
            }
        }
    }
}
