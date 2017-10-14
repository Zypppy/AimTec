namespace Tryndamere
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
    using Aimtec.SDK.Events;

    using Spell = Aimtec.SDK.Spell;

    internal class Tryndamere
    {
        public static Menu Menu = new Menu("Tryndamere by Zypppy", "Tryndamere by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R, Ignite;
        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 830);
            E = new Spell(SpellSlot.E, 660);
            E.SetSkillshot(0.325f, 125f, 700f, false, SkillshotType.Line);
            R = new Spell(SpellSlot.R);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner1, 600);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerDot")
                Ignite = new Spell(SpellSlot.Summoner2, 600);
        }
        public Tryndamere()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("combo", "Combo");
            {
                Combo.Add(new MenuBool("useq", "Use Q"));
                Combo.Add(new MenuSlider("useqhp", "If HP % <", 30, 0, 100));
                Combo.Add(new MenuBool("usewf", "Use W When Not Facing"));
                Combo.Add(new MenuBool("usew", "Use W Both Facing"));
                Combo.Add(new MenuBool("usee", "Use E"));
                Combo.Add(new MenuBool("user", "Use R"));
                Combo.Add(new MenuSlider("userhp", " If HP % <", 10, 0, 100));
                Combo.Add(new MenuBool("items", "Use Tiamat and Hydra"));
            }
            Menu.Add(Combo);
            var Harass = new Menu("harass", "Harass");
            {
                Harass.Add(new MenuBool("usee", "Use E"));
                Harass.Add(new MenuSlider("useehp", "If HP % >=", 20, 0, 100));
            }
            Menu.Add(Harass);
            var LaneClear = new Menu("laneclear", "Lane Clear");
            {
                LaneClear.Add(new MenuBool("usee", "Use E"));
            }
            Menu.Add(LaneClear);
            var JungleClear = new Menu("jungleclear", "Jungle Clear");
            {
                JungleClear.Add(new MenuBool("usee", "Use E"));
            }
            Menu.Add(JungleClear);
            var Killsteal = new Menu("killsteal", "Killsteal");
            {
                Killsteal.Add(new MenuBool("usee", "Use E"));
                Killsteal.Add(new MenuBool("ignite", "Use Ignite"));
            }
            Menu.Add(Killsteal);
            var Flee = new Menu("flee", "Flee");
            {
                Flee.Add(new MenuBool("usee", "Use E"));
                Flee.Add(new MenuKeyBind("key", "Flee Key:", KeyCode.Z, KeybindType.Press));
            }
            Menu.Add(Flee);
            var Drawings = new Menu("drawings", "Drawings");
            {
                Drawings.Add(new MenuBool("draww", "Draw W Range"));
                Drawings.Add(new MenuBool("drawe", "Draw E Range"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.PostAttack += OnPostAttack;

            LoadSpells();
            Console.WriteLine("Tryndamere by Zypppy - Loaded");
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

            if (Menu["drawings"]["draww"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Aquamarine);
            }
            if (Menu["drawings"]["drawe"].Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.Beige);
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
            if (Menu["flee"]["key"].Enabled)
            {
                FleeE();
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
                var besttarget = GetBestKillableHero(E, DamageType.Physical, false);
                var EPredition = E.GetPrediction(besttarget);
                if (besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.E) >= besttarget.Health && besttarget.IsValidTarget(E.Range))
                {
                    if (EPredition.HitChance >= HitChance.High)
                    {
                        E.Cast(EPredition.CastPosition);
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
        private void OnCombo()
        {
            var target = GetBestEnemyHeroTargetInRange(1500);
            bool useQ = Menu["combo"]["useq"].Enabled;
            float useQHP = Menu["combo"]["useqhp"].As<MenuSlider>().Value;
            bool useWF = Menu["combo"]["usewf"].Enabled;
            bool useW = Menu["combo"]["usew"].Enabled;
            bool useE = Menu["combo"]["usee"].Enabled;
            var EPredition = E.GetPrediction(target);
            bool useR = Menu["combo"]["user"].Enabled;
            float useRHP = Menu["combo"]["userhp"].As<MenuSlider>().Value;

            if (!target.IsValidTarget())
            {
                return;
            }
            if (Q.Ready && target.IsValidTarget(400) && useQ && Player.HealthPercent() < useQHP)
            {
                Q.Cast();
            }
            if (W.Ready)
            {
                if (!target.IsFacing(Player) && useWF && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
                else if (target.IsFacing(Player) && useW && target.IsValidTarget(W.Range) && Player.HealthPercent() < target.HealthPercent())
                {
                    W.Cast();
                }
            }
            if (E.Ready && target.IsValidTarget(E.Range) && useE)
            {
                if (EPredition.HitChance >= HitChance.High)
                {
                    E.Cast(EPredition.CastPosition);
                }
            }
            if (R.Ready && !Player.IsRecalling() && Player.HealthPercent() <= useRHP && target.IsValidTarget(500))
            {
                R.Cast();
            }
        }
        public void OnPostAttack(object sender, PostAttackEventArgs args)
        {
            var heroTarget = args.Target as Obj_AI_Hero;
            var ItemTiamatHydra = Player.SpellBook.Spells.Where(o => o != null && o.SpellData != null).FirstOrDefault(o => o.SpellData.Name == "ItemTiamatCleave" || o.SpellData.Name == "ItemTitanicHydraCleave");
            if (heroTarget == null)
            {
                return;
            }
            if (ItemTiamatHydra != null)
            {
                Spell Tiamat = new Spell(ItemTiamatHydra.Slot, 400);
                {
                    if (Menu["combo"]["items"].Enabled && Tiamat.Ready && heroTarget.IsValidTarget(Tiamat.Range))
                    {
                        Tiamat.Cast();
                    }
                }
            }
        }
        private void OnHarass()
        {
            var target = GetBestEnemyHeroTargetInRange(1500);
            bool useE = Menu["harass"]["usee"].Enabled;
            float useEHP = Menu["harass"]["useehp"].As<MenuSlider>().Value;
            var EPredition = E.GetPrediction(target);

            if (!target.IsValidTarget())
            {
                return;
            }
            if (E.Ready && useE && target.IsValidTarget(E.Range) && Player.HealthPercent() >= useEHP)
            {
                if (EPredition.HitChance >= HitChance.High)
                {
                    E.Cast(EPredition.CastPosition);
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
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(W.Range))
            {
                bool useE = Menu["laneclear"]["usee"].Enabled;
                var EPrediciotn = E.GetPrediction(minion);
                if (!minion.IsValidTarget())
                {
                    return;
                }
                if (E.Ready && minion.IsValidTarget(E.Range) && useE)
                {
                    if (EPrediciotn.HitChance >= HitChance.Medium)
                    {
                        E.Cast(EPrediciotn.CastPosition);
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
            foreach (var minion in GameObjects.Jungle.Where(m => m.IsValidTarget(W.Range)).ToList())
            {
                bool useE = Menu["laneclear"]["usee"].Enabled;
                var EPrediciotn = E.GetPrediction(minion);

                if (!minion.IsValidTarget() || !minion.IsValidSpellTarget())
                {
                    return;
                }
                if (E.Ready && minion.IsValidTarget(E.Range) && useE)
                {
                    if (EPrediciotn.HitChance >= HitChance.Medium)
                    {
                        E.Cast(EPrediciotn.CastPosition);
                    }
                }
            }
        }
        private void FleeE()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            bool useE = Menu["flee"]["fleee"].Enabled;
            if (useE && E.Ready && E.Ready)
            {
                E.Cast(Game.CursorPos);
            }
        }
    }
}