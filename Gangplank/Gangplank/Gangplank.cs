namespace Gangplank
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

    internal class Gangplank
    {
        public static Menu Menu = new Menu("Gangplank by Zypppy", "Gangplank by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, R;
        private const float ExplosionRange = 400;
        private const float LinkRange = 650;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000f);
            E.SetSkillshot(0.5f, 40, float.MaxValue, false, SkillshotType.Circle);
            R = new Spell(SpellSlot.R);
            R.SetSkillshot(0.9f, 100, float.MaxValue, false, SkillshotType.Circle);
        }

        public Gangplank()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("c", "Combo");
            {
                Combo.Add(new MenuBool("q", "Use Q"));
                Combo.Add(new MenuBool("e", "Use E"));
                Combo.Add(new MenuBool("r", "Use R"));
                Combo.Add(new MenuSlider("rmin", "Minimum Enemies To Cast R", 3, 1, 5));
            }
            Menu.Add(Combo);
            var Harass = new Menu("h", "Harass");
            {
                Harass.Add(new MenuBool("q", "Use Q"));
                Harass.Add(new MenuSlider("qm", "Use Q Mana Percent", 60, 0, 100));
            }
            Menu.Add(Harass);
            var Misc = new Menu("m", "Misc");
            {
                Misc.Add(new MenuSlider("stacks", "Number of stacks to keep", 1, 0, 4));
                Misc.Add(new MenuBool("autobe", "Auto Explode Barrel"));
                Misc.Add(new MenuSlider("autob", "Auto Explode Barrel When can hit", 2, 0, 5));
                Misc.Add(new MenuBool("autow", "Auto W low HP"));
                Misc.Add(new MenuSlider("autowhp", "Auto W Health % <=", 10, 0, 100));
                Misc.Add(new MenuSlider("autowmana", "Auto W Mana % >=", 60, 0, 100));
            }
            Menu.Add(Misc);
            var Cleanse = new Menu("cl", "Cleanse W");
            {
                Cleanse.Add(new MenuBool("qss", "Cleanse Enabled"));
                Cleanse.Add(new MenuSlider("qsshp", "When HP % <=", 20, 0, 100));
                Cleanse.Add(new MenuBool("blind", "Use On Blind"));
                Cleanse.Add(new MenuBool("charm", "Use On Charm"));
                Cleanse.Add(new MenuBool("fear", "Use On Fear"));
                Cleanse.Add(new MenuBool("knockup", "Use On Knockup"));
                Cleanse.Add(new MenuBool("polymorph", "Use On Polymorph"));
                Cleanse.Add(new MenuBool("snare", "Use On Snare"));
                Cleanse.Add(new MenuBool("stun", "Use On Stun"));
                Cleanse.Add(new MenuBool("taunt", "Use On Taunt"));
                Cleanse.Add(new MenuBool("exhaust", "Use On Exhaust"));
                Cleanse.Add(new MenuBool("supression", "Use On Supression"));
            }
            Menu.Add(Cleanse);
            var Killsteal = new Menu("ks", "Killsteal");
            {
                Killsteal.Add(new MenuBool("q", "Use Q"));
            }
            Menu.Add(Killsteal);
            var Drawings = new Menu("d", "Drawings");
            {
                Drawings.Add(new MenuBool("q", "Draw Q"));
                Drawings.Add(new MenuBool("e", "Draw E"));
                Drawings.Add(new MenuBool("ee", "E Explosion Range"));
            }
            Menu.Add(Drawings);
            Menu.Attach();
            LoadSpells();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += KegSpawn;
            GameObject.OnDestroy += KegKilled;
            Console.WriteLine("Gangplank by Zypppy - Loaded");
        }
        public List<GameObject> Keg = new List<GameObject>();

        

        private void KegKilled(GameObject sender)
        {
            if (sender.Name == "Barrel")
            {
                Keg.Remove(sender);
            }
        }
        private void KegSpawn(GameObject sender)
        {
            if (sender.Name == "Barrel")
            {
                Keg.Add(sender);
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
            Vector2 gejus;
            var heropos = Render.WorldToScreen(Player.Position, out gejus);
            var xaOffset = (int)gejus.X;
            var yaOffset = (int)gejus.Y;

            if (Q.Ready && Menu["d"]["q"].Enabled)
            {
                Render.Circle(Player.Position, Q.Range, 40, Color.Red);
            }

            if (E.Ready && Menu["d"]["e"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.White);
            }

            if (E.Ready && Menu["d"]["ee"].Enabled)
            {
                if (Keg.Count > 0)
                {
                    foreach (var Barrel in Keg)
                    {
                        if (Barrel != null)
                        {
                            if (Barrel.CountEnemyHeroesInRange(350) != 0)
                            {
                                Render.Circle(Barrel.ServerPosition, 350, 40, Color.Green);
                            }
                            if (Barrel.CountEnemyHeroesInRange(350) == 0)
                            {
                                Render.Circle(Barrel.ServerPosition, 350, 40, Color.Red);
                            }
                        }
                    }
                }
            }
        }

        private void Game_OnUpdate()
        {
            switch (Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    Combo();
                    break;
                case OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case OrbwalkingMode.Laneclear:
                    break;
            }
            Killsteal();
            AutoExplode();
            AutoWHeal();
            AutoWQSS();

        }

        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }
        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsValidTarget(range)).ToList();
        }

        public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True,
            bool ignoreShields = false)
        {
            return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        }

        private void Killsteal()
        {
            if (Q.Ready && Menu["ks"]["q"].Enabled)
            {
                var target = GetBestKillableHero(Q, DamageType.Physical, false);
                if (target != null && Player.GetSpellDamage(target, SpellSlot.Q) >= target.Health && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
            }
        }

        private void AutoExplode()
        {
            if (Menu["m"]["autobe"].Enabled)
            {
                if (Keg.Count > 0)
                {
                    foreach (var Barrel in Keg)
                    {
                        if (Barrel != null)
                        {
                            var target = GetBestEnemyHeroTargetInRange(E.Range);
                            float EHit = Menu["m"]["autob"].As<MenuSlider>().Value;
                            if (target != null && Barrel != null && Q.Ready && target.Distance(Barrel) < 400 && Player.Distance(Barrel) <= Q.Range)
                            {
                                Q.Cast(Barrel.Position);
                            }
                        }
                    }
                }
            }
        }

        private void AutoWHeal()
        {

        }

        private void AutoWQSS()
        {

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

        private void Harass()
        {

        }
    }
}