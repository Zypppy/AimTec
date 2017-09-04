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
            var LastHit = new Menu("lasthit", "Last Hit");
            {
                LastHit.Add(new MenuBool("useq", "Use Q"));
                LastHit.Add(new MenuSlider("manaq", "Last Hit Mana Q", 60, 0, 100));
                LastHit.Add(new MenuBool("usew", "Use W"));
                LastHit.Add(new MenuSlider("manaw", "Last Hit Mana W", 60, 0, 100));
            }
            Menu.Add(LastHit);
            var Killsteal = new Menu("killsteal", "Killsteal");
            {
                Killsteal.Add(new MenuBool("useq", "Use Q"));
                Killsteal.Add(new MenuBool("usew", "Use W"));
                Killsteal.Add(new MenuBool("usee", "Use E"));
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
                    //OnCombo();
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
                //ManualR();
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
                var besttarget = GetBestKillableHero(Q, DamageType.Physical, false);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.Q) >= besttarget.Health && besttarget.IsValidTarget(Q.Range))
                {
                    Q.Cast(besttarget);
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
            if (W.Ready && Menu["killsteal"]["usew"].Enabled)
            {
                var besttarget = GetBestKillableHero(W, DamageType.Physical, false);
                var WPrediction = W.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.W) >= besttarget.Health && besttarget.IsValidTarget(W.Range))
                {
                    if (WPrediction.HitChance >= HitChance.High)
                    {
                        W.Cast(WPrediction.CastPosition);
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
            if (E.Ready && Menu["killsteal"]["usee"].Enabled)
            {
                var besttarget = GetBestKillableHero(E, DamageType.Physical, false);
                var E2Prediction = E2.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.E) >= besttarget.Health && besttarget.IsValidTarget(E2.Range))
                {
                    if (E2Prediction.HitChance >= HitChance.High)
                    {
                        E2.Cast(E2Prediction.CastPosition);
                    }
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
    }
}