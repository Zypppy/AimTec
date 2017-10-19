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
        private MissileClient missiles;

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
            R.SetSkillshot(0.25f, 60f, 3426f, false, SkillshotType.Line);
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
                Drawings.Add(new MenuBool("drawqdmg", "Draw Q Dmg"));
                Drawings.Add(new MenuBool("drawe", "Draw E"));
                Drawings.Add(new MenuBool("drawedmg", "Draw E Dmg"));
                Drawings.Add(new MenuBool("drawe2", "Draw E Circle"));
                Drawings.Add(new MenuBool("drawr", "Draw R"));
                Drawings.Add(new MenuBool("drawrdmg", "Draw R Dmg"));
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
            Vector2 efkalopas;
            var heropos = Render.WorldToScreen(Player.Position, out efkalopas);
            var xaOffset = (int)efkalopas.X;
            var yaOffset = (int)efkalopas.Y;

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
                Render.Circle(Player.Position, E.Range, 40, Color.Indigo);
            }
            if (R.Ready && Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Indigo);
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
                var QPredition = Q.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.Q) >= besttarget.Health && besttarget.IsValidTarget(Q.Range))
                {
                    if (QPredition.HitChance >= HitChance.High)
                    {
                        Q.Cast(QPredition.CastPosition);
                    }
                }
            }
            if (E.Ready && Menu["killsteal"]["usee"].Enabled)
            {
                var besttarget = GetBestKillableHero(E, DamageType.Magical, false);
                var EPredition = E.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.E) >= besttarget.Health && besttarget.IsValidTarget(E.Range) && missiles.CountEnemyHeroesInRange(350) >= 1)
                {
                    if (EPredition.HitChance >= HitChance.High)
                    {
                        E.Cast(EPredition.CastPosition);
                    }
                }
            }
            if (R.Ready && Menu["killsteal"]["user"].Enabled)
            {
                var besttarget = GetBestKillableHero(R, DamageType.Magical, false);
                var RPredition = R.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.R) >= besttarget.Health && besttarget.IsValidTarget(R.Range))
                {
                    if (RPredition.HitChance >= HitChance.High)
                    {
                        R.Cast(RPredition.CastPosition);
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

        }
        private void ManualR()
        {

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