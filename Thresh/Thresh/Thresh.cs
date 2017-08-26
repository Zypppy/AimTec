
namespace Zypppy_Thresh
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
    using EGap;

    using Spell = Aimtec.SDK.Spell;
    using Aimtec.SDK.Events;

    internal class Thresh
    {

        public static Menu Menu = new Menu("Zypppy Thresh", "Zypppy Thresh", true);

        public static Orbwalker Orbwalker = new Orbwalker();

        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();

        public static Spell Q, Q2, W, E, R, Flash;

        public void LoadSpells()

        {
            Q = new Spell(SpellSlot.Q, 1100);
            Q2 = new Spell(SpellSlot.Q, 5000);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 450);
            R = new Spell(SpellSlot.R, 400);
            Q.SetSkillshot(0.5f, 60f, 1900f, true, SkillshotType.Line, false, HitChance.VeryHigh);
            E.SetSkillshot(0.125f, 110f, 2000f, false, SkillshotType.Line, false, HitChance.Medium);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner1, 425);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerFlash")
                Flash = new Spell(SpellSlot.Summoner2, 425);
        }


        public Thresh()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("useq2", "Use Second Q"));
                ComboMenu.Add(new MenuBool("usewself", "Use W Self"));
                ComboMenu.Add(new MenuSlider("wshp", "Self W If Hp % <", 50, 0, 100));
                //ComboMenu.Add(new MenuBool("usewally", "Use W Ally"));
                //ComboMenu.Add(new MenuSlider("wahp", "Ally W If Hp % <", 50, 0, 100));
                ComboMenu.Add(new MenuBool("usee", "Use E Push"));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("usere", "Use R If Enemy >", 3, 1, 5));

            }
            Menu.Add(ComboMenu);     
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useq", "Use Q to Harass"));
                HarassMenu.Add(new MenuSlider("manaq", "Q Mana % >", 50, 0, 100));
                HarassMenu.Add(new MenuBool("usee", "Use E to Harass"));
                HarassMenu.Add(new MenuSlider("manae", "E Mana % >", 50, 0, 100));
            }
            Menu.Add(HarassMenu);
            var miscmenu = new Menu("misc", "Misc");
            {
                miscmenu.Add(new MenuBool("autoq", "Auto Q on CC"));
                miscmenu.Add(new MenuBool("flashq", "Flash Q"));
                miscmenu.Add(new MenuKeyBind("flashqkey", "Flash Q Key:", KeyCode.T, KeybindType.Press));
            }
            Menu.Add(miscmenu);
            var DrawingsMenu = new Menu("drawings", "Drawings");
            {
                DrawingsMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawingsMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawingsMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawingsMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawingsMenu.Add(new MenuBool("drawfq", "Draw Flash Q Range"));
            }
            Menu.Add(DrawingsMenu);
            EGap.Gapcloser.Attach(Menu, "E Anti-GapClose");
            Menu.Attach();


            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            Gapcloser.OnGapcloser += OnGapcloser;
            LoadSpells();
            Console.WriteLine("Zypppy Thresh - Loaded");
        }

        private void OnGapcloser(Obj_AI_Hero target, EGap.GapcloserArgs Args)
        {
            if (target != null && Args.EndPosition.Distance(Player) < E.Range && E.Ready && target.IsDashing() && target.IsValidTarget(E.Range))
            {
                E.Cast(Args.EndPosition);
            }
        }
        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;
            
            if (Menu["drawings"]["drawq"].Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Indigo);
            }
            if (Menu["drawings"]["draww"].Enabled && W.Ready)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Fuchsia);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.DeepPink);
            }
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Aquamarine);
            }
            if (Menu["drawings"]["drawfq"].Enabled && Q.Ready && Flash.Ready && Flash != null)
            {
                Render.Circle(Player.Position, Q.Range + 410, 40, Color.AntiqueWhite);
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
            if (Menu["misc"]["autoq"].Enabled && Q.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "ThreshQ")
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
            if (Menu["misc"]["flashqkey"].Enabled)
            {
                FlashQ();
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
            bool useQGap = Menu["combo"]["useq2"].Enabled;
            bool useWself = Menu["combo"]["usewself"].Enabled;
            float WSHP = Menu["combo"]["wshp"].As<MenuSlider>().Value;
            //bool useWAlly = Menu["combo"]["usewally"].Enabled;
            //float WAHP = Menu["combo"]["wahp"].As<MenuSlider>().Value;
            bool useE = Menu["combo"]["usee"].Enabled;
            bool useR = Menu["combo"]["user"].Enabled;
            float REnemies = Menu["combo"]["usere"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && target.IsValidTarget(Q.Range) && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "ThreshQ")
            {
               Q.Cast(target);
            }
            if (Q.Ready && target.IsValidTarget(Q2.Range) && useQGap && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "ThreshQLeap" && target.HasBuff("ThreshQ"))
            {
               Q2.Cast();
            }
            if (W.Ready && useWself && Player.HealthPercent() <= WSHP && target.IsValidTarget(W.Range))
            {
                W.Cast(Player);
            }
            if (E.Ready && useE && target.IsValidTarget(E.Range))
            {
                E.Cast(target);
            }
            if (R.Ready && useR && target.IsValidTarget(R.Range) && Player.CountEnemyHeroesInRange(R.Range) >= REnemies)
            {
               R.Cast();
            }
        }

        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            float useQMana = Menu["harass"]["manaq"].As<MenuSlider>().Value;
            bool useE = Menu["harass"]["usee"].Enabled;
            float useEMana = Menu["harass"]["manae"].As<MenuSlider>().Value;
            var target = GetBestEnemyHeroTargetInRange(Q.Range);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (E.Ready && useE && target.IsValidTarget(E.Range) && Player.ManaPercent() >= useEMana)
            {
                E.Cast(target);
            }
            if (Q.Ready && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "ThreshQ" && target.IsValidTarget(Q.Range) && Player.ManaPercent() >= useQMana)
            {
                Q.Cast(target);
            }
        }
        private void FlashQ()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            var target = GetBestEnemyHeroTargetInRange(Q.Range + 410);
            bool useQFlash = Menu["misc"]["flashq"].Enabled;

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && Flash.Ready && Flash != null && target.IsValidTarget() && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "ThreshQ" && useQFlash && target.Distance(Player) < Q.Range + 410)
            {
                if (Q.Cast(target.ServerPosition))
                {
                    Flash.Cast(target.ServerPosition);
                }
            }
        }
    }
}
