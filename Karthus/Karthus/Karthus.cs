namespace Karthus
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

    internal class Karthus
    {
        public static Menu Menu = new Menu("Karthus by Zypppy", "Karthus by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, E, E2, R;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 975f);
            Q.SetSkillshot(0.95f, 85f, float.MaxValue, false, SkillshotType.Circle);
            W = new Spell(SpellSlot.W, 1000f);
            W.SetSkillshot(0.5f, 50f, float.MaxValue, false, SkillshotType.Circle);
            E = new Spell(SpellSlot.E, 550f);
            E2 = new Spell(SpellSlot.E, 700f);
            R = new Spell(SpellSlot.R);
        }

        public Karthus()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("c", "Combo");
            {
                Combo.Add(new MenuBool("q", "Use Q"));
                Combo.Add(new MenuBool("w", "Use W"));
                Combo.Add(new MenuBool("e", "Use E / Not Working Properly ATM"));
                Combo.Add(new MenuSlider("em", "Use E Mana Percent >= ", 60, 0, 100));
            }
            Menu.Add(Combo);
            var Harass = new Menu("h", "Harass");
            {
                Harass.Add(new MenuBool("q", "Use Q"));
                Harass.Add(new MenuSlider("qm", "Use Q Mana Percent >=", 60, 0, 100));
                Harass.Add(new MenuBool("ql", "Use Q Last Hit Only Out of AA Range"));
                //Harass.Add(new MenuBool("qla", "Use Last Hit Only Out Of AA Range"));
                Harass.Add(new MenuSlider("qlm", "Use Q Last Hit Mana Percent >=", 60, 0, 100));

            }
            Menu.Add(Harass);
            var Lane = new Menu("l", "Lane Clear");
            {
                Lane.Add(new MenuBool("q", "Use Q"));
                Lane.Add(new MenuSlider("qm", "Use Q Mana Percent >=", 60, 0, 100));
                Lane.Add(new MenuBool("e", "Use E"));
                Lane.Add(new MenuSlider("ec", "E Min Minions Count", 3, 1, 10));
                Lane.Add(new MenuSlider("em", "Use E Mana Percent >=", 60, 0, 100));
            }
            Menu.Add(Lane);
            var Last = new Menu("lh", "Last Hit");
            {
                Last.Add(new MenuBool("q", "Use Q"));
                Last.Add(new MenuSlider("qm", "Use Q Mana Percent >=", 60, 0, 100));
            }
            Menu.Add(Last);
            var Jungle = new Menu("j", "Jungle Clear");
            {
                Jungle.Add(new MenuBool("q", "Use Q"));
                Jungle.Add(new MenuSlider("qm", "Use Q Mana Percent >=", 60, 0, 100));
                Jungle.Add(new MenuBool("e", "Use E"));
                Jungle.Add(new MenuSlider("em", "Use E Mana Percent >=", 60, 0, 100));
            }
            Menu.Add(Jungle);
            var Ult = new Menu("u", "R");
            {
                Ult.Add(new MenuBool("rt", "R Teamfight"));
                //Ult.Add(new MenuBool("rk", "R Killsteal"));
                //Ult.Add(new MenuSlider("rke", "Use R Killsteal Only If No Enemies", 500, 1, 1000));
                //Ult.Add(new MenuSlider("rkc", "R Killsteal Count", 2, 1, 5));
            }
            Menu.Add(Ult);
            var Drawings = new Menu("d", "Drawings");
            {
                Drawings.Add(new MenuBool("q", "Draw Q Range"));
                Drawings.Add(new MenuBool("w", "Draw W Range"));
                Drawings.Add(new MenuBool("e", "Draw E Range"));
                //Drawings.Add(new MenuBool("rr", "Draw Dont Cast R If Enemy Range"));
                Drawings.Add(new MenuBool("rd", "Draw R Damage"));
                Drawings.Add(new MenuBool("rdk", "Draw Killable Champs With R"));
            }
            Menu.Add(Drawings);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;
            LoadSpells();
            Console.WriteLine("Karthus by Zypppy - Loaded");
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
                Render.Circle(Player.Position, Q.Range, 40, Color.Indigo);
            }
            if (W.Ready && Menu["d"]["w"].Enabled)
            {
                Render.Circle(Player.Position, W.Range, 40, Color.Bisque);
            }
            if (E.Ready && Menu["d"]["e"].Enabled)
            {
                Render.Circle(Player.Position, E.Range, 40, Color.DeepPink);
            }
            //if (R.Ready && Menu["d"]["rr"].Enabled)
            //{
            //    Render.Circle(Player.Position, Menu["u"]["rke"].As<MenuSlider>().Value, 40, Color.HotPink);
            //}
            if (Menu["d"]["rd"].Enabled)
            {
                ObjectManager.Get<Obj_AI_Base>()
                  .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(50000))
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
                       var drawStartXPos = (float)(barPos.X + (unit.Health > Player.GetSpellDamage(unit, SpellSlot.R)
                       ? width * ((unit.Health - (Player.GetSpellDamage(unit, SpellSlot.R))) / unit.MaxHealth * 100 / 100)
                       : 0));
                       Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, unit.Health < Player.GetSpellDamage(unit, SpellSlot.R) ? Color.GreenYellow : Color.Orange);

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
                    LastHitH();
                    break;
                case OrbwalkingMode.Laneclear:
                    LaneClear();
                    JungleClear();
                    break;
                case OrbwalkingMode.Lasthit:
                    LastHit();
                    break;
            }
            //Killsteal();
        }

        //public static Obj_AI_Hero GetBestKillableHero(Spell spell, DamageType damageType = DamageType.True, bool ignoreShields = false)
        //{
        //    return TargetSelector.Implementation.GetOrderedTargets(spell.Range).FirstOrDefault(t => t.IsValidTarget());
        //}

        //private void Killsteal()
        //{
        //
        //    if (R.Ready && Menu["u"]["rk"].Enabled)
        //    {
        //        var besttarget = GetBestKillableHero(R, DamageType.Magical, false);
        //        var RRange = Menu["u"]["rke"].As<MenuSlider>().Value;
        //        if (!besttarget.IsZombie && besttarget != null && Player.GetSpellDamage(besttarget, SpellSlot.R) >= besttarget.Health && besttarget.IsValidTarget(50000))
        //        {
        //           
        //        }
        //    }
        //}
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
            var target = GetBestEnemyHeroTargetInRange(1500);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool CQ = Menu["c"]["q"].Enabled;
            if (Q.Ready && CQ && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }

            bool CW = Menu["c"]["w"].Enabled;
            if (W.Ready && CW && target.IsValidTarget(W.Range))
            {
                W.Cast(target);
            }

            

            bool CE = Menu["c"]["e"].Enabled;
            float ME = Menu["c"]["em"].As<MenuSlider>().Value;
            if (E.Ready && CE)
            {
                switch (Player.SpellBook.GetSpell(SpellSlot.E).ToggleState)
                {
                    case 0:
                        if (target.IsValidTarget(E.Range) && Player.ManaPercent() >= ME && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 0)
                        {
                            Console.WriteLine("Autistic Toggle State");
                        }
                        break;
                    case 1056964608:
                        if (target.IsValidTarget(E.Range + 50) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 1056964608)
                        {
                            Console.WriteLine("Autistic Toggle State 2");
                        }
                        break;
                }
            }
            bool CR = Menu["u"]["rt"].Enabled;
            if (R.Ready && Player.IsZombie && CR)
            {
                R.Cast();
            }

        }
        private void Harass()
        {
            var target = GetBestEnemyHeroTargetInRange(1500);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool HQ = Menu["h"]["q"].Enabled;
            float MQ = Menu["h"]["qm"].As<MenuSlider>().Value;
            if (Q.Ready && HQ && Player.ManaPercent() >= MQ && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
        }
        private void LastHitH()
        { 
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
            {
                bool LQ = Menu["h"]["ql"].Enabled;
                //bool QLA = Menu["h"]["qla"].Enabled;
                float LQM = Menu["h"]["qlm"].As<MenuSlider>().Value;

                if (!minion.IsValidTarget())
                {
                    return;
                }

                if (Q.Ready && LQ && Player.ManaPercent() >= LQM && !minion.IsValidAutoRange() && Player.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health)
                {
                    Q.Cast(minion);
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
        private void LaneClear()
        {

        }
        private void LastHit()
        {

        }
        public static List<Obj_AI_Minion> GetGenericJungleMinionsTargets()
        {
            return GetGenericJungleMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetGenericJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.Jungle.Where(m => !GameObjects.JungleSmall.Contains(m) && m.IsValidTarget(range)).ToList();
        }
        private void JungleClear()
        {

        }
    }
}