namespace Mordekaiser
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
    using Aimtec.SDK.Events;


    internal class Mordekaiser
    {
        public static Menu Menu = new Menu("Mordekaiser by Zypppy", "Mordekaiser by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R, Ignite;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 670);
            E.SetSkillshot(0.25f, 12f * 2 * (float)Math.PI / 180, 2000f, false, SkillshotType.Cone);
            R = new Spell(SpellSlot.R, 650);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner1, 600);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner2, 600);
        }
        public Mordekaiser()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("combo", "Combo");
            {
                Combo.Add(new MenuBool("useq", "Use Q"));
                Combo.Add(new MenuBool("usew", "Use W"));
                Combo.Add(new MenuBool("usee", "Use E"));
                Combo.Add(new MenuBool("user", "Use R"));
                Combo.Add(new MenuSlider("userhp", "Use R Only If Enemy HP % <", 40, 0, 100));
            }
            Menu.Add(Combo);
            var Harass = new Menu("harass", "Harass");
            {
                Harass.Add(new MenuBool("useq", "Use Q"));
                Harass.Add(new MenuSlider("hpq", "Harass Q HP % >", 60, 0, 100));
                Harass.Add(new MenuBool("usee", "Use E"));
                Harass.Add(new MenuSlider("hpe", "harass E HP % >", 60, 0, 100));

            }
            Menu.Add(Harass);
            var LaneClear = new Menu("laneclear", "Lane Clear");
            {
                LaneClear.Add(new MenuBool("useq", "Use Q"));
                LaneClear.Add(new MenuSlider("hpq", "Laneclear Q HP % >", 60, 0, 100));
                LaneClear.Add(new MenuBool("usew", "Use W"));
                LaneClear.Add(new MenuSlider("minions", "Only If X Minions In Range", 7, 0, 10));
                LaneClear.Add(new MenuSlider("hpw", "Laneclear W HP % >", 60, 0, 100));
                LaneClear.Add(new MenuBool("usee", "Use E"));
                LaneClear.Add(new MenuSlider("hpe", "Laneclear E HP % >", 60, 0, 100));
            }
            Menu.Add(LaneClear);
            var JungleClear = new Menu("jungleclear", "Jungle Clear");
            {
                JungleClear.Add(new MenuBool("useq", "Use Q"));
                JungleClear.Add(new MenuSlider("hpq", "JungleClear Q HP % >", 60, 0, 100));
                JungleClear.Add(new MenuBool("usew", "Use W"));
                JungleClear.Add(new MenuSlider("hpw", "JungleClear W HP % >", 60, 0, 100));
                JungleClear.Add(new MenuBool("usee", "Use E"));
                JungleClear.Add(new MenuSlider("hpe", "JungleClear E HP % >", 60, 0, 100));
            }
            Menu.Add(JungleClear);
            var Killsteal = new Menu("killsteal", "Killsteal");
            {
                Killsteal.Add(new MenuBool("usee", "Use E"));
                Killsteal.Add(new MenuBool("user", "Use R"));
                Killsteal.Add(new MenuBool("ignite", "Use Ignite"));
            }
            Menu.Add(Killsteal);
            var Drawings = new Menu("drawings", "Drawings");
            {
                Drawings.Add(new MenuBool("drawq", "Draw Q"));
                Drawings.Add(new MenuBool("draww", "Draw W"));
                Drawings.Add(new MenuBool("drawe", "Draw E"));
                Drawings.Add(new MenuBool("drawr", "Draw R"));
                Drawings.Add(new MenuBool("drawdmg", "Draw DMG"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Mordekaiser by Zypppy - Loaded");
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
            Vector2 efka;
            var heropos = Render.WorldToScreen(Player.Position, out efka);
            var xaOffset = (int)efka.X;
            var yaOffset = (int)efka.Y;

            if (Menu["drawings"]["drawq"].Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, Player.AttackRange, 40, Color.Crimson);
            }
            if (Menu["drawings"]["draww"].Enabled && W.Ready && Player.GetSpell(SpellSlot.W).ToggleState != 2)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Aqua);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.Red);
            }
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Blue);
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
                       var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.W) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R)
                       ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.W) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R))) / unit.MaxHealth * 100 / 100)
                       : 0));
                       Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.W) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R) ? Color.GreenYellow : Color.Orange);

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
        }
        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True, bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }
        private void Killsteal()
        {
            if (E.Ready && Menu["killsteal"]["usee"].Enabled)
            {
                var besttarget = GetBestKillableHero(E, DamageType.Magical, false);
                var EPrediction = E.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.E) >= besttarget.Health && besttarget.IsValidTarget(E.Range))
                {
                    if (EPrediction.HitChance >= HitChance.High)
                    {
                        E.Cast(EPrediction.CastPosition);
                    }
                }
            }
            if (R.Ready && Menu["killsteal"]["user"].Enabled)
            {
                var besttarget = GetBestKillableHero(R, DamageType.Magical, false);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.R) > besttarget.Health && besttarget.IsValidTarget(R.Range))
                {
                    R.Cast(besttarget);
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
            var target = GetBestEnemyHeroTargetInRange(1500);
            bool useQ = Menu["combo"]["useq"].Enabled;
            bool useW = Menu["combo"]["usew"].Enabled;
            bool useE = Menu["combo"]["usee"].Enabled;
            var EPrediction = E.GetPrediction(target);
            bool useR = Menu["combo"]["user"].Enabled;
            float hpR = Menu["combo"]["userhp"].As<MenuSlider>().Value;

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && target.IsValidTarget(Player.AttackRange + target.BoundingRadius) && useQ)
            {
                Q.Cast();
            }
            if (W.Ready && Player.GetSpell(SpellSlot.W).ToggleState != 2 && target.IsValidTarget(W.Range) && Player.CountEnemyHeroesInRange(250) >= 1)
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
            if (R.Ready && target.IsValidTarget(R.Range) && target.HealthPercent() < hpR)
            {
                R.Cast(target);
            }
        }
        private void OnHarass()
        {
            var target = GetBestEnemyHeroTargetInRange(1500);
            bool useQ = Menu["harass"]["useq"].Enabled;
            float useQHP = Menu["harass"]["hpq"].As<MenuSlider>().Value;
            bool useE = Menu["harass"]["usee"].Enabled;
            float useEHP = Menu["harass"]["hpe"].As<MenuSlider>().Value;
            var EPrediction = E.GetPrediction(target);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && Player.HealthPercent() >= useQHP && useQ && target.IsValidTarget(Player.AttackRange + target.BoundingRadius))
            {
                Q.Cast();
            }
            if (E.Ready && Player.HealthPercent() >= useEHP && target.IsValidTarget(E.Range) && useE)
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
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(1500))
            {
                bool useQ = Menu["laneclear"]["useq"].Enabled;
                float useQHP = Menu["laneclear"]["hpq"].As<MenuSlider>().Value;
                bool useW = Menu["laneclear"]["usew"].Enabled;
                float useWHP = Menu["laneclear"]["hpw"].As<MenuSlider>().Value;
                float minionsW = Menu["laneclear"]["minions"].As<MenuSlider>().Value;
                bool useE = Menu["laneclear"]["usee"].Enabled;
                float useEHP = Menu["laneclear"]["hpe"].As<MenuSlider>().Value;
                var EPrediction = E.GetPrediction(minion);

                if (!minion.IsValidTarget())
                {
                    return;
                }
                if (Q.Ready && Player.HealthPercent() >= useQHP && useQ && minion.IsValidTarget(Player.AttackRange + minion.BoundingRadius))
                {
                    Q.Cast();
                }
                if (W.Ready && Player.GetSpell(SpellSlot.W).ToggleState != 2 && Player.HealthPercent() >= useWHP && minion.IsValidTarget(W.Range) && GameObjects.EnemyMinions.Count(h => h.IsValidTarget(250, false, false, minion.ServerPosition)) >= minionsW)
                {
                    W.Cast();
                }
                if (E.Ready && Player.HealthPercent() >= useEHP && minion.IsValidTarget(E.Range))
                {
                    if (EPrediction.HitChance >= HitChance.High)
                    {
                        E.Cast(EPrediction.CastPosition);
                    }
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
            foreach (var minion in GameObjects.Jungle.Where(m => m.IsValidTarget(1500)).ToList())
            {
                bool useQ = Menu["hungleclear"]["useq"].Enabled;
                float useQHP = Menu["hungleclear"]["hpq"].As<MenuSlider>().Value;
                bool useW = Menu["hungleclear"]["usew"].Enabled;
                float useWHP = Menu["hungleclear"]["hpw"].As<MenuSlider>().Value;
                bool useE = Menu["hungleclear"]["usee"].Enabled;
                float useEHP = Menu["hungleclear"]["hpe"].As<MenuSlider>().Value;
                var EPrediction = E.GetPrediction(minion);

                if (!minion.IsValidTarget())
                {
                    return;
                }
                if (Q.Ready && Player.HealthPercent() >= useQHP && useQ && minion.IsValidTarget(Player.AttackRange + minion.BoundingRadius))
                {
                    Q.Cast();
                }
                if (W.Ready && Player.GetSpell(SpellSlot.W).ToggleState != 2 && Player.HealthPercent() >= useWHP && minion.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
                if (E.Ready && Player.HealthPercent() >= useEHP && minion.IsValidTarget(E.Range))
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