namespace AurelionSol
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

    internal class AurelionSol
    {
        public static Menu Menu = new Menu("AurelionSol by Zypppy", "AurelionSol by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, W2, E, R;
        private MissileClient missiles;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1000f);
            W = new Spell(SpellSlot.W, 350f);
            W2 = new Spell(SpellSlot.W, 650f);
            E = new Spell(SpellSlot.E, 400f);
            R = new Spell(SpellSlot.R, 1419f);
            Q.SetSkillshot(0.25f, 110, 701, false, SkillshotType.Line);
            R.SetSkillshot(0.25f, 250, 4536, false, SkillshotType.Line);
        }
        public AurelionSol()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuBool("user", "Use R Only Above Slider Value"));
                ComboMenu.Add(new MenuSlider("hitr", "R Minimum Enemeies Hit", 3, 0, 5));
                ComboMenu.Add(new MenuKeyBind("key", "Manual R Key:", KeyCode.T, KeybindType.Press));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useq", "Use Q"));
                HarassMenu.Add(new MenuSlider("manaq", "Minimum Mana To Use Q", 70, 0, 100));
                HarassMenu.Add(new MenuBool("usew", "Use W"));
                HarassMenu.Add(new MenuSlider("manaw", "Minimum Mana To Use W", 70, 0, 100));
            }
            Menu.Add(HarassMenu);
            var KillstealMenu = new Menu("killsteal", "Killsteal");
            {
                KillstealMenu.Add(new MenuBool("RKS", "Use R to Killsteal"));
            }
            Menu.Add(KillstealMenu);
            var DrawingsMenu = new Menu("drawings", "Drawings");
            {
                DrawingsMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawingsMenu.Add(new MenuBool("drawq2", "Draw Q Path Range"));
                DrawingsMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawingsMenu.Add(new MenuBool("draww2", "Draw Active W Range"));
                DrawingsMenu.Add(new MenuBool("drawr", "Draw R Range"));
            }
            Menu.Add(DrawingsMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += OnCreate;
            GameObject.OnDestroy += OnDestroy;

            LoadSpells();
            Console.WriteLine("AurelionSol by Zypppy - Loaded");
        }
        public void OnCreate(GameObject obj)
        {
            var missile = obj as MissileClient;
            if (missile == null)
            {
                return;
            }

            if (missile.SpellCaster == null || !missile.SpellCaster.IsValid ||
                missile.SpellCaster.Team != ObjectManager.GetLocalPlayer().Team)
            {
                return;
            }
            var hero = missile.SpellCaster as Obj_AI_Hero;
            if (hero == null)
            {
                return;
            }
            if (missile.SpellData.Name == "AurelionSolQMissile")
            {
                missiles = missile;
            }

        }
        private void OnDestroy(GameObject obj)
        {
            var missile = obj as MissileClient;
            if (missile == null || !missile.IsValid)
            {
                return;
            }

            if (missile.SpellCaster == null || !missile.SpellCaster.IsValid ||
                missile.SpellCaster.Team != ObjectManager.GetLocalPlayer().Team)
            {
                return;
            }
            var hero = missile.SpellCaster as Obj_AI_Hero;
            if (hero == null)
            {
                return;
            }
            if (missile.SpellData.Name == "AurelionSolQMissile")
            {
                missiles = null;
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
            if (Q.Ready && Menu["drawings"]["drawq2"].Enabled)
            {
                if (missiles != null)
                {
                    Render.Circle(missiles.ServerPosition, 250, 40, Color.DeepPink);
                }
            }
            if (W.Ready && Menu["drawings"]["draww2"].Enabled)
            {
                Render.Circle(Player.Position, W2.Range, 40, Color.DeepSkyBlue);
            }
            if (W.Ready && Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Cornsilk);
            }
            if (R.Ready && Menu["drawings"]["drawr"].Enabled)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Crimson);
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
                    break;
            }
            if (Menu["combo"]["key"].Enabled)
            {
                ManualR();
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
            if (R.Ready && Menu["killsteal"]["RKS"].Enabled)
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
            var target = GetBestEnemyHeroTargetInRange(R.Range);
            bool useQ = Menu["combo"]["useq"].Enabled;
            var QPrediction = Q.GetPrediction(target);
            bool useW = Menu["combo"]["usew"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;
            float hitR = Menu["combo"]["hitr"].As<MenuSlider>().Value;
            var RPrediction = R.GetPrediction(target);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready)
            {
                if (target.IsValidTarget(Q.Range) && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).ToggleState == 1)
                {
                    if (QPrediction.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(QPrediction.CastPosition);
                    }
                }
                else if (missiles != null && target.IsValidTarget(250f, false, false, missiles.ServerPosition) && Player.SpellBook.GetSpell(SpellSlot.Q).ToggleState == 2)
                {
                    Q.Cast();
                }
            }
            if (W.Ready)
            {
                if (target.IsValidTarget(W2.Range) && useW && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState != 2)
                {
                    W.Cast();
                }
                else if (target.IsValidTarget(W.Range) && useW && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2)
                {
                    W.Cast();
                }
                else if (!target.IsValidTarget(W2.Range) && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2)
                {
                    W.Cast();
                }
            }
            if (R.Ready && target.IsValidTarget(R.Range) && useR && R.CastIfWillHit(target, Menu["combo"]["hitr"].As<MenuSlider>().Value))
            {
                if (RPrediction.HitChance >= HitChance.High)
                {
                    R.Cast(RPrediction.CastPosition);
                }
            }
        }
        private void OnHarass()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range);
            bool useQ = Menu["harass"]["useq"].Enabled;
            float manaQ = Menu["harass"]["manaq"].As<MenuSlider>().Value;
            var QPrediction = Q.GetPrediction(target);
            bool useW = Menu["harass"]["usew"].Enabled;
            float manaW = Menu["harass"]["manaw"].As<MenuSlider>().Value;

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready)
            {
                if (target.IsValidTarget(Q.Range) && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).ToggleState == 1 && Player.ManaPercent() >= manaQ)
                {
                    if (QPrediction.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(QPrediction.CastPosition);
                    }
                }
                else if (missiles != null && target.IsValidTarget(250f, false, false, missiles.ServerPosition) && Player.SpellBook.GetSpell(SpellSlot.Q).ToggleState == 2)
                {
                    Q.Cast();
                }
            }
            if (W.Ready)
            {
                if (target.IsValidTarget(W2.Range) && useW && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState != 2 && Player.ManaPercent() >= manaW)
                {
                    W.Cast();
                }
                else if (target.IsValidTarget(W.Range) && useW && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2)
                {
                    W.Cast();
                }
                else if (!target.IsValidTarget(W2.Range) && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2)
                {
                    W.Cast();
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