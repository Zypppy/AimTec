namespace Malphite
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

    internal class Malphite
    {
        public static Menu Menu = new Menu("Malphite by Zypppy", "Malphite by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 625f);//SeismicShard
            W = new Spell(SpellSlot.W, 400f);//Obduracy
            E = new Spell(SpellSlot.E, 400f);//Landslide
            R = new Spell(SpellSlot.R, 1000f);//UFSlash
            R.SetSkillshot(0f, 160f, 700f, false, SkillshotType.Circle);
        }
        public Malphite()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("combo", "Combo");
            {
                Combo.Add(new MenuBool("useq", "Use Q"));
                Combo.Add(new MenuBool("usew", "Use W"));
                Combo.Add(new MenuBool("usee", "Use E"));
                Combo.Add(new MenuBool("user", "Use R Only Above Slider Value"));
                Combo.Add(new MenuSlider("hitr", "Cast R If Hit X Enemies", 2, 1, 5));
                Combo.Add(new MenuKeyBind("key", "Manual R Key:", KeyCode.T, KeybindType.Press));
            }
            Menu.Add(Combo);
            var Harass = new Menu("harass", "Harass");
            {
                Harass.Add(new MenuBool("useq", "Use Q"));
                Harass.Add(new MenuSlider("manaq", "Q Mana Slider", 60, 0, 100));
                Harass.Add(new MenuBool("usew", "Use W"));
                Harass.Add(new MenuSlider("manaw", "W Mana Slider", 60, 0, 100));
                Harass.Add(new MenuBool("usee", "Use E"));
                Harass.Add(new MenuSlider("manae", "E Mana Slider", 60, 0, 100));
            }
            Menu.Add(Harass);
            var LaneClear = new Menu("laneclear", "LaneClear");
            {
                LaneClear.Add(new MenuBool("useq", "Use Q"));
                LaneClear.Add(new MenuSlider("manaq", "Q Mana Slider", 60, 0, 100));
                LaneClear.Add(new MenuBool("usew", "Use W"));
                LaneClear.Add(new MenuSlider("manaw", "W Mana Slider", 60, 0, 100));
                LaneClear.Add(new MenuBool("usee", "Use E"));
                LaneClear.Add(new MenuSlider("manae", "E Mana Slider", 60, 0, 100));
            }
            Menu.Add(LaneClear);
            var Killsteal = new Menu("killsteal", "Killsteal");
            {
                Killsteal.Add(new MenuBool("useq", "Use Q"));
                Killsteal.Add(new MenuBool("user", "Use R"));
            }
            Menu.Add(Killsteal);
            var Drawings = new Menu("drawings", "Drawings");
            {
                Drawings.Add(new MenuBool("drawq", "Draw Q Range"));
                Drawings.Add(new MenuBool("draww", "Draw W Range"));
                Drawings.Add(new MenuBool("drawe", "Draw E Range"));
                Drawings.Add(new MenuBool("drawr", "Draw R Range"));
                Drawings.Add(new MenuBool("dmg", "Damage Indicator"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Malphite by Zypppy - Loaded");
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
                Render.Circle(Player.Position, Q.Range, 40, Color.Aqua);
            }
            if (Menu["drawings"]["draww"].Enabled && W.Ready)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Azure);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.Black);
            }
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Brown);
            }
            if (Menu["drawings"]["dmg"].Enabled)
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
                       ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.Q) + Player.GetSpellDamage(unit, SpellSlot.E) + Player.GetSpellDamage(unit, SpellSlot.R))) / unit.MaxHealth * 100 / 100)
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
                    break;
            }
            if (Menu["combo"]["key"].Enabled)
            {
                ManualR();
            }
            Killsteal();
        }
        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True, bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }
        private void Killsteal()
        {
            if (Q.Ready && Menu["killsteal"]["useq"].Enabled)
            {
                var besttarget = GetBestKillableHero(Q, DamageType.Magical, false);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.Q) >= besttarget.Health && besttarget.IsValidTarget(Q.Range))
                {
                    Q.Cast(besttarget);
                }
            }
            if (R.Ready && Menu["killsteal"]["user"].Enabled)
            {
                var besttarget = GetBestKillableHero(R, DamageType.Magical, false);
                var RPrediction = R.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.R) >= besttarget.Health && besttarget.IsValidTarget(R.Range))
                {
                    if (RPrediction.HitChance >= HitChance.High)
                    {
                        R.Cast(RPrediction.CastPosition);
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
            
            bool useQ = Menu["combo"]["useq"].Enabled;
            if (Q.Ready && useQ)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target != null)
                {
                    Q.Cast(target);
                }
            }

            bool useW = Menu["combo"]["usew"].Enabled;
            if (W.Ready && useW)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target != null)
                {
                    W.Cast();
                }
            }

            bool useE = Menu["combo"]["usee"].Enabled;
            if (E.Ready && useE)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                if (target != null)
                {
                    E.Cast();
                }
            }

            bool useR = Menu["combo"]["user"].Enabled;
            if (R.Ready && useR)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target != null && target.IsValidTarget(R.Range) && R.CastIfWillHit(target, Menu["combo"]["hitr"].As<MenuSlider>().Value - 1))
                {
                    R.Cast(target);
                }
            }
        }
        private void OnHarass()
        {
            
            bool useQ = Menu["harass"]["useq"].Enabled;
            float manaQ = Menu["harass"]["manaq"].As<MenuSlider>().Value;
            if (Q.Ready && useQ && Player.ManaPercent() >= manaQ)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target != null)
                {
                    Q.Cast(target);
                }
            }

            bool useW = Menu["harass"]["usew"].Enabled;
            float manaW = Menu["harass"]["manaw"].As<MenuSlider>().Value;
            if (W.Ready && useW && Player.ManaPercent() >= manaW)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target != null)
                {
                    W.Cast();
                }
            }

            bool useE = Menu["harass"]["usee"].Enabled;
            float manaE = Menu["harass"]["manae"].As<MenuSlider>().Value;
            if (E.Ready && useE && Player.ManaPercent() >= manaE)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                if (target != null)
                {
                    E.Cast();
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
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
            {
                bool useQ = Menu["harass"]["useq"].Enabled;
                float manaQ = Menu["harass"]["manaq"].As<MenuSlider>().Value;
                bool useW = Menu["harass"]["usew"].Enabled;
                float manaW = Menu["harass"]["manaw"].As<MenuSlider>().Value;
                bool useE = Menu["harass"]["usee"].Enabled;
                float manaE = Menu["harass"]["manae"].As<MenuSlider>().Value;

                if (!minion.IsValidTarget())
                {
                    return;
                }
                if (Q.Ready && useQ && minion.IsValidTarget(Q.Range) && Player.ManaPercent() >= manaQ)
                {
                    Q.Cast(minion);
                }
                if (W.Ready && useW && minion.IsValidTarget(W.Range) && Player.ManaPercent() >= manaW)
                {
                    W.Cast();
                }
                if (E.Ready && useE && minion.IsValidTarget(E.Range) && Player.ManaPercent() >= manaE)
                {
                    E.Cast();
                }
            }
        }
        private void ManualR()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range);
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            var RPrediction = R.GetPrediction(target);
            if (R.Ready && target.IsValidTarget(R.Range))
            {
                if (RPrediction.HitChance >= HitChance.High)
                {
                    R.Cast(RPrediction.CastPosition);
                }
            }
        }
    }
}