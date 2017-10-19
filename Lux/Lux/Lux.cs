namespace Lux
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

    internal class Lux
    {
        public static Menu Menu = new Menu("Lux by Zypppy", "Lux by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, E2, R;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1300f);
            Q.SetSkillshot(0.27f, 50f, 1202f, true, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 1175f);
            W.SetSkillshot(0.361f, 110f, 1077f, false, SkillshotType.Line);
            E = new Spell(SpellSlot.E, 1100f);
            E.SetSkillshot(0.25f, 350f, 1292f, false, SkillshotType.Circle);
            E2 = new Spell(SpellSlot.E, 2000f);
            R = new Spell(SpellSlot.R, 3300f);
            R.SetSkillshot(1.0f, 60f, 3426f, false, SkillshotType.Line);
        }
        public Lux()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("combo", "Combo");
            {
                Combo.Add(new MenuBool("useq", "Use Q"));
                Combo.Add(new MenuBool("usew", "Use Self W"));
                Combo.Add(new MenuSlider("usewhp", "HP % For W <=", 30, 0, 100));
                Combo.Add(new MenuBool("usee", "Use E"));
                Combo.Add(new MenuBool("user", "Use R"));
                Combo.Add(new MenuSlider("userhit", "Anemies Hit With R Target + ", 2, 0, 5));
                Combo.Add(new MenuKeyBind("key", "Manual R Key:", KeyCode.T, KeybindType.Press));
            }
            Menu.Add(Combo);
            var Harass = new Menu("harass", "Harass");
            {
                Harass.Add(new MenuBool("usee", "Use E"));
                Harass.Add(new MenuSlider("manae", "E Harass if Mana % >=", 60, 0, 100));
            }
            Menu.Add(Harass);
            var LaneClear = new Menu("laneclear", "Lane Clear");
            {
                LaneClear.Add(new MenuBool("useq", "Use Q"));
                LaneClear.Add(new MenuSlider("manaq", "Q Lane Clear if Mana % >=", 60, 0, 100));
                LaneClear.Add(new MenuBool("usee", "Use E"));
                LaneClear.Add(new MenuSlider("manae", "E Lane clear if Mana % >=", 60, 0, 100));
            }
            Menu.Add(LaneClear);
            var JungleClear = new Menu("jungle", "Jungle Clear");
            {
                JungleClear.Add(new MenuBool("useq", "Use Q"));
                JungleClear.Add(new MenuSlider("manaq", "Q Jungle Clear if Mana % >=", 60, 0, 100));
                JungleClear.Add(new MenuBool("usee", "Use E"));
                JungleClear.Add(new MenuSlider("manae", "E Jungle clear if Mana % >=", 60, 0, 100));
            }
            Menu.Add(JungleClear);
            var Killsteal = new Menu("killsteal", "Killsteal");
            {
                Killsteal.Add(new MenuBool("useq", "Use Q"));
                Killsteal.Add(new MenuBool("user", "Use R"));
            }
            Menu.Add(Killsteal);
            var Drawings = new Menu("drawings", "Drawings");
            {
                Drawings.Add(new MenuBool("drawq", "Draw Q"));
                Drawings.Add(new MenuBool("draww", "Draw W"));
                Drawings.Add(new MenuBool("drawe", "Draw E"));
                Drawings.Add(new MenuBool("drawe2", "Draw E Circle"));
                Drawings.Add(new MenuBool("drawdmg", "Draw Dmg"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Lux by Zypppy - Loaded");
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
            var LuxE = ObjectManager.Get<GameObject>().FirstOrDefault(o => o.IsValid && o.Name == "Lux_Base_E_tar_aoe_green.troy");
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
            if (E.Ready && Menu["drawings"]["drawe2"].Enabled)
            {
                if (LuxE != null)
                {
                    Render.Circle(LuxE.ServerPosition, 335, 40, Color.DeepPink);
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
                    Combo();
                    break;
                case OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case OrbwalkingMode.Laneclear:
                    LaneClear();
                    JungleClear();
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
                var QPrediction = Q.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.Q) >= besttarget.Health && besttarget.IsValidTarget(Q.Range))
                {
                    if (QPrediction.HitChance >= HitChance.High)
                    {
                        Q.Cast(QPrediction.CastPosition);
                    }
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
        private void Combo()
        {
            var target = GetBestEnemyHeroTargetInRange(5000);
            bool useQ = Menu["combo"]["useq"].Enabled;
            var QPrediction = Q.GetPrediction(target);
            bool useW = Menu["combo"]["usew"].Enabled;
            float useWhp = Menu["combo"]["usewhp"].As<MenuSlider>().Value;
            bool useE = Menu["combo"]["usee"].Enabled;
            var LuxE = ObjectManager.Get<GameObject>().FirstOrDefault(o => o.IsValid && o.Name == "Lux_Base_E_tar_aoe_green.troy");
            var EPrediction = E.GetPrediction(target);
            bool useR = Menu["combo"]["user"].Enabled;
            var RPrediction = R.GetPrediction(target);

            if (!target.IsValidTarget())
            {
                return;
            }

            if (Q.Ready && useQ && target.IsValidTarget(Q.Range))
            {
                if (QPrediction.HitChance >= HitChance.High)
                {
                    Q.Cast(QPrediction.CastPosition);
                }
            }
            if (W.Ready && useW && target.IsValidTarget(W.Range) && Player.HealthPercent() <= useWhp)
            {
                W.Cast();
            }
            if (E.Ready && useE)
            {
                if (target.IsValidTarget(E.Range) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState != 2)
                {
                    if (EPrediction.HitChance >= HitChance.High)
                    {
                        E.Cast(EPrediction.CastPosition);
                    }
                }
                else if (Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 2 && target.IsValidTarget(E2.Range) && LuxE.CountEnemyHeroesInRange(335f) >= 1)
                {
                    E2.Cast();
                }
            }
            if (R.Ready && useR && target.IsValidTarget(R.Range) && R.CastIfWillHit(target, Menu["combo"]["userhit"].As<MenuSlider>().Value))
            {
                if (RPrediction.HitChance >= HitChance.High)
                {
                    R.Cast(RPrediction.CastPosition);
                }
            }
        }
        private void ManualR()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range);
            var RPrediction = R.GetPrediction(target);

            if (R.Ready && target.IsValidTarget(R.Range))
            {
                if (RPrediction.HitChance >= HitChance.High)
                {
                    R.Cast(RPrediction.CastPosition);
                }
            }

        }
        private void Harass()
        {

        }
        private void LaneClear()
        {

        }
        private void JungleClear()
        {

        }
    }
}