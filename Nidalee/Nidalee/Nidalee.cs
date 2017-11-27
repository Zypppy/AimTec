namespace Nidalee
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using Aimtec;
    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Events;
    using Aimtec.SDK.Prediction.Health;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Orbwalking;
    using Aimtec.SDK.TargetSelector;
    using Aimtec.SDK.Util.Cache;
    using Aimtec.SDK.Prediction.Skillshots;
    using Aimtec.SDK.Util;

    using Spell = Aimtec.SDK.Spell;

    internal class Nidalee
    {
        public static Menu Menu = new Menu("Nidalee by Zypppy", "Nidalee by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell QH, QC, WH, WC, WCL, EH, EC, R;
        public void LoadSpells()
        {
            QH = new Spell(SpellSlot.Q, 1500f);
            QH.SetSkillshot(0.5f, 40f, 1300f, true, SkillshotType.Line, false);
            QC = new Spell(SpellSlot.Q, 500f);
            WH = new Spell(SpellSlot.W, 900f);
            WH.SetSkillshot(0.5f, 80f, float.MaxValue, false, SkillshotType.Circle, false);
            WC = new Spell(SpellSlot.W, 475f);
            WC.SetSkillshot(0.5f, 210f, float.MaxValue, false, SkillshotType.Circle, false);
            WCL = new Spell(SpellSlot.W, 750f);
            WCL.SetSkillshot(0.5f, 100f, float.MaxValue, false, SkillshotType.Circle, false);
            EH = new Spell(SpellSlot.E, 600f);
            EC = new Spell(SpellSlot.E, 350f);
            EC.SetSkillshot(0.5f, (float)(15 * Math.PI / 180), float.MaxValue, false, SkillshotType.Cone, false);
            R = new Spell(SpellSlot.R, 500);
        }

        public Nidalee()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Human Q"));
                ComboMenu.Add(new MenuBool("usecq", "Use Cougar Q"));
                ComboMenu.Add(new MenuBool("usew", "Use Human W"));
                ComboMenu.Add(new MenuBool("usecw", "Use Cougar W"));
                ComboMenu.Add(new MenuBool("usee", "Use Human E"));
                ComboMenu.Add(new MenuSlider("useeh", "Health To Use Human E", 30, 0, 100));
                ComboMenu.Add(new MenuSlider("useehm", "Mana To Use Human Use E", 70, 0, 100));
                ComboMenu.Add(new MenuBool("usece", "Use Cougar E"));
                ComboMenu.Add(new MenuBool("user", "Use R To Switch Forms"));
                ComboMenu.Add(new MenuSlider("userr", "Use R If Target Is In Range", 400, 0, 1400));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useq", "Use Human Q to Harass"));
                HarassMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
            }
            Menu.Add(HarassMenu);
            var JunglCelear = new Menu("jungleclear", "Jungle Clear");
            {
                JunglCelear.Add(new MenuBool("usejq", "Use Human Q in Jungle"));
                JunglCelear.Add(new MenuBool("usejcq", "Use Cougar Q in Jungle"));
                JunglCelear.Add(new MenuBool("usejw", "Use Human W in Jungle"));
                JunglCelear.Add(new MenuBool("usejcw", "Use Cougar W in Jungle"));
                JunglCelear.Add(new MenuBool("usejce", "Use Cougar E in Jungle"));
                JunglCelear.Add(new MenuBool("usejr", "Use R in Jungle"));
                JunglCelear.Add(new MenuSlider("manaj", "Mana Manager For Jungle", 50));
            }
            Menu.Add(JunglCelear);
            var KSMenu = new Menu("killsteal", "Killsteal");
            {
                KSMenu.Add(new MenuBool("kq", "Killsteal with Human Q"));
            }
            Menu.Add(KSMenu);
            var miscmenu = new Menu("misc", "Misc");
            {
                miscmenu.Add(new MenuBool("autoq", "Auto Human Q on CC"));
                miscmenu.Add(new MenuBool("autow", "Auto Human W on CC"));
                miscmenu.Add(new MenuBool("autoe", "Use Auto Human E"));
                miscmenu.Add(new MenuSlider("autoeh", "Auto Use Human E When HP Below <%", 10, 0, 100));
            }
            Menu.Add(miscmenu);
            var DrawMenu = new Menu("drawings", "Drawings");
            {
                DrawMenu.Add(new MenuBool("drawq", "Draw Human Q Range"));
                DrawMenu.Add(new MenuBool("drawq2", "Draw Cougar Q Range"));
                DrawMenu.Add(new MenuBool("draww", "Draw Human W Range"));
                DrawMenu.Add(new MenuBool("draww2", "Draw Cougar W Range"));
                DrawMenu.Add(new MenuBool("draww3", "Draw Cougar W Long Range"));
                DrawMenu.Add(new MenuBool("drawe", "Draw Human E Range"));
                DrawMenu.Add(new MenuBool("drawe2", "Draw Cougar E Range"));
                DrawMenu.Add(new MenuBool("drawr", "Draw R Range"));
                DrawMenu.Add(new MenuBool("drawflee", "Draw Free Circle Around Cursor"));
            }
            Menu.Add(DrawMenu);
            var FleeMenu = new Menu("flee", "Flee");
            {
                FleeMenu.Add(new MenuBool("fleew", "Use Cougar W To Flee"));
                FleeMenu.Add(new MenuKeyBind("key", "Flee Key:", KeyCode.Z, KeybindType.Press));
            }
            Menu.Add(FleeMenu);
            Menu.Attach();

            Render.OnPresent += Render_OnPresent;
            Game.OnUpdate += Game_OnUpdate;

            LoadSpells();
            Console.WriteLine("Nidalee by Zypppy - Loaded");
        }

        private void Render_OnPresent()
        {
            Vector2 maybeworks;
            var heropos = Render.WorldToScreen(Player.Position, out maybeworks);
            var xaOffset = (int)maybeworks.X;
            var yaOffset = (int)maybeworks.Y;

            if (Menu["drawings"]["drawq"].Enabled && QH.Ready)
            {
                Render.Circle(Player.Position, QH.Range, 40, Color.Indigo);
            }
            if (Menu["drawings"]["drawq2"].Enabled && QH.Ready)
            {
                Render.Circle(Player.Position, QC.Range, 40, Color.Indigo);
            }

            if (Menu["drawings"]["draww"].Enabled && WH.Ready)
            {
                Render.Circle(Player.Position, WH.Range, 40, Color.Fuchsia);
            }
            if (Menu["drawings"]["draww2"].Enabled && WH.Ready)
            {
                Render.Circle(Player.Position, WC.Range, 40, Color.Fuchsia);
            }
            if (Menu["drawings"]["draww3"].Enabled && WH.Ready)
            {
                Render.Circle(Player.Position, WCL.Range, 40, Color.Fuchsia);
            }

            if (Menu["drawings"]["drawe"].Enabled && EH.Ready)
            {
                Render.Circle(Player.Position, EH.Range, 40, Color.DeepPink);
            }
            if (Menu["drawings"]["drawe2"].Enabled && EH.Ready)
            {
                Render.Circle(Player.Position, EC.Range, 40, Color.DeepPink);
            }
            float range = Menu["combo"]["userr"].As<MenuSlider>().Value;
            if (Menu["drawings"]["drawr"].Enabled && R.Ready)
            {
                Render.Circle(Player.Position, range, 40, Color.Aquamarine);
            }
            if (Menu["flee"]["key"].Enabled && Menu["drawings"]["drawflee"].Enabled)
            {
                Render.Circle(Game.CursorPos, 150, 50, Color.Chocolate);
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
                    Jungle();
                    break;

            }
            Killsteal();
            if (Menu["misc"]["autoq"].Enabled && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss")
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
                          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
                          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
                          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(QH.Range) &&
                         !Invulnerable.Check(t, DamageType.Magical)))
                {
                    QH.Cast(target);
                }
            }
            if (Menu["misc"]["autow"].Enabled && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack")
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(
                    t => (t.HasBuffOfType(BuffType.Charm) || t.HasBuffOfType(BuffType.Stun) ||
                          t.HasBuffOfType(BuffType.Fear) || t.HasBuffOfType(BuffType.Snare) ||
                          t.HasBuffOfType(BuffType.Taunt) || t.HasBuffOfType(BuffType.Knockback) ||
                          t.HasBuffOfType(BuffType.Suppression)) && t.IsValidTarget(WH.Range) &&
                         !Invulnerable.Check(t, DamageType.Magical)))
                {
                    WH.Cast(target);
                }
            }
            float hp = Menu["misc"]["autoeh"].As<MenuSlider>().Value;
            if (Menu["misc"]["autoe"].Enabled && EH.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name == "PrimalSurge" && Player.HealthPercent() <= hp)
            {
                EH.Cast(Player);
            }
            if (Menu["flee"]["key"].Enabled)
            {
                Flee();
            }
        }


        public static List<Obj_AI_Minion> GetAllGenericMinionsTargets()
        {
            return GetAllGenericMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetAllGenericMinionsTargetsInRange(float range)
        {
            return GetEnemyLaneMinionsTargetsInRange(range).Concat(GetGenericJungleMinionsTargetsInRange(range)).ToList();
        }

        public static List<Obj_AI_Base> GetAllGenericUnitTargets()
        {
            return GetAllGenericUnitTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Base> GetAllGenericUnitTargetsInRange(float range)
        {
            return GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(range)).Concat<Obj_AI_Base>(GetAllGenericMinionsTargetsInRange(range)).ToList();
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
            if (QH.Ready &&
                Menu["killsteal"]["kq"].Enabled && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss")
            {
                var bestTarget = GetBestKillableHero(QH, DamageType.Magical, false);
                if (bestTarget != null &&
                    Player.GetSpellDamage(bestTarget, SpellSlot.Q) >= bestTarget.Health &&
                    bestTarget.IsValidTarget(QH.Range))
                {
                    QH.Cast(bestTarget);
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
            if (!target.IsValidTarget())
            {
                return;
            }
            bool useQ = Menu["combo"]["useq"].Enabled;
            if (QH.Ready && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && target.IsValidTarget(QH.Range))
            {
                QH.Cast(target);
            }
            bool useQ2 = Menu["combo"]["usecq"].Enabled;
            if (QC.Ready && useQ2 && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && target.IsValidTarget(QC.Range))
            {
                QC.Cast();
            }
            bool useW = Menu["combo"]["usew"].Enabled;
            if (WH.Ready && useW && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack" && target.IsValidTarget(WH.Range))
            {
                WH.Cast(target);
            }
            bool useW2 = Menu["combo"]["usecw"].Enabled;
            if (WC.Ready && useW2 && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && target.IsValidTarget(WC.Range))
            {
                WC.Cast(target);
            }
            if (WCL.Ready && useW2 && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && target.HasBuff("NidaleePassiveHunted") && Player.HasBuff("NidaleePassiveHunting") && target.IsValidTarget(WCL.Range))
            {
                WCL.Cast(target);
            }
            bool useE = Menu["combo"]["usee"].Enabled;
            float hpe = Menu["combo"]["useeh"].As<MenuSlider>().Value;
            float manae = Menu["combo"]["useehm"].As<MenuSlider>().Value;
            if (EH.Ready && useE && Player.SpellBook.GetSpell(SpellSlot.E).Name == "PrimalSurge" && Player.ManaPercent() >= manae && Player.HealthPercent() <= hpe)
            {
                EH.Cast(Player);
            }
            bool useE2 = Menu["combo"]["usece"].Enabled;
            if (EC.Ready && useE2 && Player.SpellBook.GetSpell(SpellSlot.E).Name == "Swipe" && target.IsValidTarget(EC.Range))
            {
                EC.Cast(target);
            }

            bool useR = Menu["combo"]["user"].Enabled;
            float rangeR = Menu["combo"]["userr"].As<MenuSlider>().Value;
            if (R.Ready && QH.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" &&
                target.IsValidTarget(QH.Range) && useR)
            {
                R.Cast();
            }
            if (R.Ready && QC.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" &&
                target.IsValidTarget(rangeR))
            {
                R.Cast();
            }
        }
        private void OnHarass()
        {
            bool useQ = Menu["harass"]["useq"].Enabled;
            var target = GetBestEnemyHeroTargetInRange(QH.Range);
            float manapercent = Menu["harass"]["mana"].As<MenuSlider>().Value;
            if (manapercent < Player.ManaPercent())
            {
                if (!target.IsValidTarget())
                {
                    return;
                }

                if (QH.Ready && useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && target.IsValidTarget(QH.Range))
                {
                    QH.Cast(target);
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

        private void Jungle()
        {

            bool useQ = Menu["jungleclear"]["usejq"].Enabled;
            bool useQ2 = Menu["jungleclear"]["usejcq"].Enabled;
            bool useW = Menu["jungleclear"]["usejw"].Enabled;
            bool useW2 = Menu["jungleclear"]["usejcw"].Enabled;
            bool useE = Menu["jungleclear"]["usejce"].Enabled;
            bool useR = Menu["jungleclear"]["usejr"].Enabled;
            float manapercent = Menu["jungleclear"]["manaj"].As<MenuSlider>().Value;

            foreach (var minion in GameObjects.Jungle.Where(m => m.IsValidTarget(QH.Range)).ToList())
            {
                if (!minion.IsValidTarget() || !minion.IsValidSpellTarget())
                {
                    return;
                }

                if (Player.ManaPercent() >= manapercent)
                {
                    if (useQ && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && minion.IsValidTarget(QH.Range) && minion != null)
                    {
                        QH.CastOnUnit(minion);
                    }
                    if (useQ2 && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && minion.IsValidTarget(QC.Range) && minion != null)
                    {
                        QC.Cast();
                    }
                    if (useW && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack" && minion.IsValidTarget(WH.Range) && minion != null)
                    {
                        WH.CastOnUnit(minion);
                    }
                    if (useW2 && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && minion.IsValidTarget(WC.Range) && minion != null)
                    {
                        WC.CastOnUnit(minion);
                    }
                    if (useW2 && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && minion.HasBuff("NidaleePassiveHunted") && minion.IsValidTarget(WCL.Range) && minion != null)
                    {
                        WCL.CastOnUnit(minion);
                    }
                    if (useE && Player.SpellBook.GetSpell(SpellSlot.E).Name == "Swipe" && minion.IsValidTarget(EC.Range) && minion != null)
                    {
                        EC.CastOnUnit(minion);
                    }
                    if (R.Ready && useR && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && minion.IsValidTarget(QH.Range) && minion != null)
                    {
                        {
                            R.Cast();
                        }
                    }
                    if (R.Ready && useR && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && minion.IsValidTarget(QC.Range) && minion != null)
                    {
                        {
                            R.Cast();
                        }
                    }
                }
            }
        }
        private void Flee()
        {
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            bool usew = Menu["flee"]["fleew"].Enabled;
            if (usew && WC.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name != "Pounce" && R.Ready)
            {
                R.Cast();
            }
            else if (usew && WC.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce")
            {
                WC.Cast(Game.CursorPos);
            }
        }
    }
}