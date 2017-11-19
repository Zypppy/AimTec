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
        public static Spell Q, W, E, R;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 975f);
            Q.SetSkillshot(0.95f, 85f, float.MaxValue, false, SkillshotType.Circle);
            W = new Spell(SpellSlot.W, 1000f);
            W.SetSkillshot(0.5f, 50f, float.MaxValue, false, SkillshotType.Circle);
            E = new Spell(SpellSlot.E, 550f);
            R = new Spell(SpellSlot.R);
        }

        public Karthus()
        {
            Orbwalker.Attach(Menu);
            var Combo = new Menu("c", "Combo");
            {
                Combo.Add(new MenuBool("q", "Use Q"));
                Combo.Add(new MenuBool("w", "Use W"));
                Combo.Add(new MenuBool("e", "Use E"));
                Combo.Add(new MenuSlider("em", "Use E Mana Percent >= ", 60, 0, 100));
            }
            Menu.Add(Combo);
            var Harass = new Menu("h", "Harass");
            {
                Harass.Add(new MenuBool("q", "Use Q"));
                Harass.Add(new MenuSlider("qm", "Use Q Mana Percent >=", 60, 0, 100));
                Harass.Add(new MenuList("qlo", "Last Hit options", new[] { "Always", "Out Of AA Range", "Never" }, 2));
                Harass.Add(new MenuSlider("qlm", "Use Q Last Hit Mana Percent >=", 60, 0, 100));

            }
            Menu.Add(Harass);
            var Lane = new Menu("l", "Lane Clear");
            {
                Lane.Add(new MenuBool("q", "Use Q"));
                Lane.Add(new MenuSlider("qm", "Use Q Mana Percent >=", 60, 0, 100));
                Lane.Add(new MenuBool("e", "Use E"));
                Lane.Add(new MenuSlider("em", "Use E Mana Percent >=", 60, 0, 100));
            }
            Menu.Add(Lane);
            var Last = new Menu("lh", "Last Hit");
            {
                Last.Add(new MenuList("qlo", "Last Hit options", new[] { "Always", "Out Of AA Range", "Never"}, 1));
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
            }
            Menu.Add(Ult);
            var Drawings = new Menu("d", "Drawings");
            {
                Drawings.Add(new MenuBool("q", "Draw Q Range"));
                Drawings.Add(new MenuBool("w", "Draw W Range"));
                Drawings.Add(new MenuBool("e", "Draw E Range"));
                //Drawings.Add(new MenuBool("rd", "Draw R Damage"));
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
            if (Menu["d"]["rdk"].Enabled)
            {
                int killablecount = 0;

                foreach (var heroUnit in ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.IsValidTarget() && h.IsValidTarget(50000)))
                {
                    int width = 103;
                    int height = 8;
                    int xOffset = SxOffset(heroUnit);
                    int yOffset = SyOffset(heroUnit);
                    var barPos = heroUnit.FloatingHealthBarPosition;
                    barPos.X += xOffset;
                    barPos.Y += yOffset;

                    var dmg = Player.GetSpellDamage(heroUnit, SpellSlot.R);

                    var drawEndXPos = barPos.X + width * (heroUnit.HealthPercent() / 100);
                    var drawStartXPos = (float)(barPos.X + (heroUnit.Health > dmg
                                                    ? width * ((heroUnit.Health - (dmg)) / heroUnit.MaxHealth * 100 / 100)
                                                    : 0));
                    Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, height, true, heroUnit.Health < dmg ? Color.GreenYellow : Color.Orange);

                    var basepos = new Vector2(0.50f * Render.Width, 0.30f * Render.Height);

                    if (heroUnit.Health <= dmg) //Killable
                    {
                        var pos = basepos + new Vector2(0, 15 * killablecount);
                        Render.Text($"{heroUnit.ChampionName} is killable! Press R!!", pos, RenderTextFlags.Center, Color.Red);
                        killablecount += 1;
                    }
                }
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
                    case 1:
                        if (target.IsValidTarget(E.Range) && Player.ManaPercent() >= ME && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 1)
                        {
                            E.Cast();
                        }
                        break;
                    case 2:
                        if (!target.IsValidTarget(E.Range) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 2)
                        {
                            E.Cast();
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
            if (Q.Ready && Player.ManaPercent() >= Menu["h"]["qlm"].As<MenuSlider>().Value)
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                {
                    if (minion.IsValidTarget(Q.Range) && Player.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health)
                    {
                        switch (Menu["h"]["qlo"].As<MenuList>().Value)
                        {
                            case 0:
                                if (minion.IsValidTarget(Q.Range))
                                {
                                    Q.Cast(minion);
                                }
                                break;
                            case 1:
                                if (!minion.IsValidAutoRange())
                                {
                                    Q.Cast(minion);
                                }
                                break;

                        }
                    }
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
            foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
            {
                bool CQ = Menu["l"]["q"].Enabled;
                float CQM = Menu["l"]["qm"].As<MenuSlider>().Value;

                if (!minion.IsValidTarget())
                {
                    return;
                }

                if (Q.Ready && CQ && Player.ManaPercent() >= CQM && minion.IsValidTarget(Q.Range))
                {
                    Q.Cast(minion);
                }
                bool CE = Menu["l"]["e"].Enabled;
                if (E.Ready && Player.ManaPercent() >= Menu["l"]["em"].As<MenuSlider>().Value && CE)
                {
                    switch (Player.SpellBook.GetSpell(SpellSlot.E).ToggleState)
                    { 
                    case 1:
                        if (minion.IsValidTarget(E.Range) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 1)
                        {
                            E.Cast();
                        }
                        break;
                    case 2:
                        if (!minion.IsValidTarget(E.Range) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 2)
                        {
                            E.Cast();
                        }
                        break;
                    }
                }
            }
        }

        private void LastHit()
        {
            if (Q.Ready && Player.ManaPercent() >= Menu["lh"]["qm"].As<MenuSlider>().Value)
            {
                foreach (var minion in GetEnemyLaneMinionsTargetsInRange(Q.Range))
                {
                    if (minion.IsValidTarget(Q.Range) && Player.GetSpellDamage(minion, SpellSlot.Q) >= minion.Health)
                    {
                        switch (Menu["lh"]["qlo"].As<MenuList>().Value)
                        {
                            case 0:
                                if (minion.IsValidTarget(Q.Range))
                                {
                                    Q.Cast(minion);
                                }
                                break;
                            case 1:
                                if (!minion.IsValidAutoRange())
                                {
                                    Q.Cast(minion);
                                }
                                break;

                        }
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
        private void JungleClear()
        {
            foreach (var jungle in GameObjects.Jungle.Where(m => m.IsValidTarget(Q.Range)).ToList())
            {
                bool JQ = Menu["j"]["q"].Enabled;
                float JQM = Menu["j"]["qm"].As<MenuSlider>().Value;

                if (!jungle.IsValidTarget() || !jungle.IsValidSpellTarget())
                {
                    return;
                }

                if (Q.Ready && JQ && jungle.IsValidTarget(Q.Range) && Player.ManaPercent() >= JQM)
                {
                    Q.Cast(jungle);
                }
                bool JE = Menu["l"]["e"].Enabled;
                if (E.Ready && Player.ManaPercent() >= Menu["j"]["em"].As<MenuSlider>().Value && JE)
                {
                    switch (Player.SpellBook.GetSpell(SpellSlot.E).ToggleState)
                    {
                        case 1:
                            if (jungle.IsValidTarget(E.Range) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 1)
                            {
                                E.Cast();
                            }
                            break;
                        case 2:
                            if (!jungle.IsValidTarget(E.Range) && Player.SpellBook.GetSpell(SpellSlot.E).ToggleState == 2)
                            {
                                E.Cast();
                            }
                            break;
                    }
                }
            }
        }
    }
}