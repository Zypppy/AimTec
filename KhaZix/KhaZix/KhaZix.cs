namespace KhaZix
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

    internal class KhaZix
    {
        public static Menu Menu = new Menu("KhaZix by Zypppy", "KhaZix by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, Q2, E, E2, W, R, Ignite;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 325);
            Q2 = new Spell(SpellSlot.Q, 375);
            W = new Spell(SpellSlot.W, 1000);
            E = new Spell(SpellSlot.E, 700);
            E2 = new Spell(SpellSlot.E, 1200);
            R = new Spell(SpellSlot.R, 400);
            W.SetSkillshot(0.225f, 100f, 828f, true, SkillshotType.Line);
            E.SetSkillshot(0.25f, 100f, 1000f, false, SkillshotType.Circle);
            E2.SetSkillshot(0.25f, 100f, 1000f, false, SkillshotType.Circle);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner1, 600);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner2, 600);
        }
        public KhaZix()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("combo", "Combo");
            {
                Combo.Add(new MenuBool("useq", "Use Q"));
                Combo.Add(new MenuBool("usew", "Use W"));
                Combo.Add(new MenuBool("usee", "Use E"));
                Combo.Add(new MenuBool("user", "Use R"));
                Combo.Add(new MenuSlider("usercombocount", "Use R If Enemies >=", 3, 1, 5));
                Combo.Add(new MenuKeyBind("key", "Manual R Key:", KeyCode.T, KeybindType.Press));
            }
            Menu.Add(Combo);
            var Harass = new Menu("harass", "Harass");
            {
                Harass.Add(new MenuBool("useq", "Use Q"));
                Harass.Add(new MenuSlider("manaq", "Harass Q Mana", 60, 0, 100));
                Harass.Add(new MenuBool("usew", "Use W"));
                Harass.Add(new MenuSlider("manaw", "Harass W Mana", 60, 0, 100));
            }
            Menu.Add(Harass);
            var LaneClear = new Menu("laneclear", "Lane Clear");
            {
                LaneClear.Add(new MenuBool("useq", "Use Q"));
                LaneClear.Add(new MenuSlider("manaq", "Lane Clear Mana Q", 60, 0, 100));
                LaneClear.Add(new MenuBool("usew", "Use W"));
                LaneClear.Add(new MenuSlider("manaw", "Lane Clear Mana W", 60, 0, 100));
                LaneClear.Add(new MenuBool("usee", "Use E"));
                LaneClear.Add(new MenuSlider("manae", "Lane Clear Mana E", 60, 0, 100));
                LaneClear.Add(new MenuBool("useitems", "Use Hydra/Tiamat"));
            }
            Menu.Add(LaneClear);
            var JungleClear = new Menu("jungleclear", "Jungle Clear");
            {
                JungleClear.Add(new MenuBool("useq", "Use Q"));
                JungleClear.Add(new MenuSlider("manaq", "Jungle Clear Mana Q", 60, 0, 100));
                JungleClear.Add(new MenuBool("usew", "Use W"));
                JungleClear.Add(new MenuSlider("manaw", "Jungle Clear Mana W", 60, 0, 100));
                JungleClear.Add(new MenuBool("usee", "Use E"));
                JungleClear.Add(new MenuBool("noe", "Don't Use E If Mobs In Q Range"));
                JungleClear.Add(new MenuSlider("manae", "Jungle Clear Mana E", 60, 0, 100));
                JungleClear.Add(new MenuBool("useitems", "Use Hydra/Tiamat"));
            }
            Menu.Add(JungleClear);
            var Killsteal = new Menu("killsteal", "Killsteal");
            {
                Killsteal.Add(new MenuBool("ignite", "Use Ignite"));
            }
            Menu.Add(Killsteal);
            var Drawings = new Menu("drawings", "Drawings");
            {
                Drawings.Add(new MenuBool("drawq", "Draw Q"));
                Drawings.Add(new MenuBool("draww", "Draw W"));
                Drawings.Add(new MenuBool("drawe", "Draw E"));
                Drawings.Add(new MenuBool("drawr", "Draw R"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("KhaZix by Zypppy - Loaded");
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

            if (Menu["drawings"]["drawq"].Enabled && Q.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "KhazixQLong")
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Beige);
            }
            if (Menu["drawings"]["drawq"].Enabled && Q.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "KhazixQLong")
            {
                Render.Circle(Player.Position, Q2.Range, 40, Color.Beige);
            }
            if (Menu["drawings"]["draww"].Enabled && W.Ready)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.BlueViolet);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name != "KhazixELong")
            {
                Render.Circle(Player.Position, E.Range, 40, Color.BlueViolet);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name == "KhazixELong")
            {
                Render.Circle(Player.Position, E2.Range, 40, Color.BlueViolet);
            }
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 40, Color.Chocolate);
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
                    //OnHarass();
                    break;
                case OrbwalkingMode.Laneclear:
                    //OnLaneClear();
                    //OnJungleClear();
                    break;
                case OrbwalkingMode.Lasthit:
                    //OnLastHit();
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
            bool useR = Menu["combo"]["user"].Enabled;
            float enemiesR = Menu["combo"]["usercombocount"].As<MenuSlider>().Value;
            var WPrediction = W.GetPrediction(target);
            var EPrediction = E.GetPrediction(target);
            var E2Prediction = E2.GetPrediction(target);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q.Range) && Player.SpellBook.GetSpell(SpellSlot.Q).Name != "KhazixQLong")
            {
                Q.Cast(target);
            }
            if (Q.Ready && useQ && target.IsValidTarget(Q2.Range) && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "KhazixQLong")
            {
                Q2.Cast(target);
            }
            if (W.Ready && target.IsValidTarget(W.Range) && useW)
            {
                if (WPrediction.HitChance >= HitChance.Medium)
                {
                    W.Cast(WPrediction.CastPosition);
                }
            }
            if (E.Ready && target.IsValidTarget(E.Range) && useE && Player.SpellBook.GetSpell(SpellSlot.E).Name != "KhazixELong")
            {
                if (EPrediction.HitChance >= HitChance.Medium)
                {
                    E.Cast(EPrediction.CastPosition);
                }
            }
            if (E.Ready && target.IsValidTarget(E2.Range) && useE && Player.SpellBook.GetSpell(SpellSlot.E).Name == "KhazixELong")
            {
                if (E2Prediction.HitChance >= HitChance.Medium)
                {
                    E2.Cast(E2Prediction.CastPosition);
                }
            }
        }
        private void ManualR()
        {
            var target = GetBestEnemyHeroTargetInRange(1500);
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            if (R.Ready && target.IsValidTarget(E2.Range))
            {
                R.Cast();
            }
        }
    }
}