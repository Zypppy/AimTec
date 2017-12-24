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
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuList("qo", "Q Options", new[] { "Both Forms", "Only Human", "Only Cougar" }, 0));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuList("wo", "W Options", new[] { "Both Forms", "Only Human", "Only Cougar" }, 0));
                ComboMenu.Add(new MenuBool("usee", "Use E"));
                ComboMenu.Add(new MenuList("eo", "E Options", new[] { "Both Forms", "Only Human", "Only Cougar" }, 0));
                ComboMenu.Add(new MenuSlider("useeh", "Health To Use Human E", 30, 0, 100));
                ComboMenu.Add(new MenuSlider("useehm", "Mana To Use Human Use E", 70, 0, 100));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuList("ro", "R Options", new[] { "Always"}, 0));
                ComboMenu.Add(new MenuSlider("userr", "Use R Target in Range", 400, 0, 1400));
            }
            Menu.Add(ComboMenu);
            var HarassMenu = new Menu("harass", "Harass");
            {
                HarassMenu.Add(new MenuBool("useq", "Use Human Q to Harass"));
                HarassMenu.Add(new MenuSlider("mana", "Mana Manager", 50));
            }
            Menu.Add(HarassMenu);
            var JungleClear = new Menu("jungleclear", "Jungle Clear");
            {
                JungleClear.Add(new MenuBool("usejq", "Use Human Q in Jungle"));
                JungleClear.Add(new MenuList("qo", "Q Options", new[] { "Both Forms", "Only Human", "Only Cougar" }, 0));
                JungleClear.Add(new MenuBool("usejw", "Use Human W in Jungle"));
                JungleClear.Add(new MenuList("wo", "W Options", new[] { "Both Forms", "Only Human", "Only Cougar" }, 0));
                JungleClear.Add(new MenuBool("usejce", "Use Cougar E in Jungle"));
                JungleClear.Add(new MenuBool("usejr", "Use R in Jungle"));
                JungleClear.Add(new MenuList("ro", "R Options", new[] { "Always", "Only When Monster Has Buff" }, 0));
                JungleClear.Add(new MenuSlider("manaj", "Mana Manager For Jungle", 50));
            }
            Menu.Add(JungleClear);
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
                DrawMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawMenu.Add(new MenuList("qdo", "Q Drawing Options", new[] { "Both Forms", "Only Human", "Only Cougar" }, 0));
                DrawMenu.Add(new MenuBool("draww", "Draw W Range"));
                DrawMenu.Add(new MenuList("wdo", "W Drawing Options", new[] { "Both Forms", "Only Human", "Only Cougar" }, 0));
                DrawMenu.Add(new MenuBool("drawe", "Draw E Range"));
                DrawMenu.Add(new MenuList("edo", "E Drawing Options", new[] { "Both Forms", "Only Human", "Only Cougar" }, 0));
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

            if (Menu["drawings"]["drawq"].Enabled)
            {
                switch (Menu["drawings"]["qdo"].As<MenuList>().Value)
                {
                    case 0:
                        if (QH.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss")
                        {
                            Render.Circle(Player.Position, QH.Range, 40, Color.Indigo);
                        }
                        else if (QC.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown")
                        {
                            Render.Circle(Player.Position, QC.Range, 40, Color.Indigo);
                        }
                        break;
                    case 1:
                        if (QH.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss")
                        {
                            Render.Circle(Player.Position, QH.Range, 40, Color.Indigo);
                        }
                        break;
                    case 2:
                        if (QC.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown")
                        {
                            Render.Circle(Player.Position, QC.Range, 40, Color.Indigo);
                        }
                        break;
                }
            }
            if (Menu["drawings"]["draww"].Enabled)
            {
                switch (Menu["drawings"]["wdo"].As<MenuList>().Value)
                {
                    case 0:
                        if (WH.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack")
                        {
                            Render.Circle(Player.Position, WH.Range, 40, Color.Indigo);
                        }
                        else if (WC.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce")
                        {
                            Render.Circle(Player.Position, WC.Range, 40, Color.Indigo);
                        }
                        break;
                    case 1:
                        if (WH.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack")
                        {
                            Render.Circle(Player.Position, WH.Range, 40, Color.Indigo);
                        }
                        break;
                    case 2:
                        if (WC.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce")
                        {
                            Render.Circle(Player.Position, WC.Range, 40, Color.Indigo);
                        }
                        break;
                }
            }
            if (Menu["drawings"]["drawe"].Enabled)
            {
                switch (Menu["drawings"]["edo"].As<MenuList>().Value)
                {
                    case 0:
                        if (EH.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name == "PrimalSurge")
                        {
                            Render.Circle(Player.Position, EH.Range, 40, Color.Indigo);
                        }
                        else if (EC.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name == "Swipe")
                        {
                            Render.Circle(Player.Position, EC.Range, 40, Color.Indigo);
                        }
                        break;
                    case 1:
                        if (EH.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name == "PrimalSurge")
                        {
                            Render.Circle(Player.Position, EH.Range, 40, Color.Indigo);
                        }
                        break;
                    case 2:
                        if (EC.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name == "Swipe")
                        {
                            Render.Circle(Player.Position, EC.Range, 40, Color.Indigo);
                        }
                        break;
                }
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

            if (Player.IsDead || MenuGUI.IsChatOpen() || Player.IsRecalling())
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
            
            bool useQ = Menu["combo"]["useq"].Enabled;
            if (useQ)
            {
                var QHuman = GetBestEnemyHeroTargetInRange(QH.Range);
                var QCougar = GetBestEnemyHeroTargetInRange(QC.Range);
                switch (Menu["combo"]["qo"].As<MenuList>().Value)
                {
                    case 0:
                        if (QH.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && QHuman.IsValidTarget(QH.Range))
                        {
                            QH.Cast(QHuman);
                        }
                        else if (QC.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && QCougar.IsValidTarget(QC.Range))
                        {
                            QC.Cast();
                        }
                        break;
                    case 1:
                        if (QH.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && QHuman.IsValidTarget(QH.Range))
                        {
                            QH.Cast(QHuman);
                        }
                        break;
                    case 2:
                        if (QC.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && QCougar.IsValidTarget(QC.Range))
                        {
                            QC.Cast();
                        }
                        break;
                }
            }
            bool useW = Menu["combo"]["usew"].Enabled;
            if (useW)
            {
                var WHuman = GetBestEnemyHeroTargetInRange(WH.Range);
                var WCougar = GetBestEnemyHeroTargetInRange(WC.Range);
                switch (Menu["combo"]["wo"].As<MenuList>().Value)
                {
                    case 0:
                        if (WH.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack" && WHuman.IsValidTarget(WH.Range))
                        {
                            WH.Cast(WHuman);
                        }
                        else if (WC.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && WCougar.IsValidTarget(WC.Range))
                        {
                            WC.Cast(WCougar);
                        }
                        break;
                    case 1:
                        if (WH.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack" && WHuman.IsValidTarget(WH.Range))
                        {
                            WH.Cast(WHuman);
                        }
                        break;
                    case 2:
                        if (WC.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && WCougar.IsValidTarget(WC.Range))
                        {
                            WC.Cast(WCougar);
                        }
                        else if (WCL.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && WCougar.IsValidTarget(WCL.Range) && WCougar.HasBuff("NidaleePassiveHunted"))
                        {
                            WCL.Cast(WCougar);
                        }
                        break;
                }
            }
            
            bool useE = Menu["combo"]["usee"].Enabled;
            float hpe = Menu["combo"]["useeh"].As<MenuSlider>().Value;
            float manae = Menu["combo"]["useehm"].As<MenuSlider>().Value;
            if (useE)
            {
                var ECougar = GetBestEnemyHeroTargetInRange(EC.Range);
                switch (Menu["combo"]["eo"].As<MenuList>().Value)
                {
                    case 0:
                        if (EH.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name == "PrimalSurge" && Player.ManaPercent() >= manae && Player.HealthPercent() <= hpe)
                        {
                            EH.Cast(Player);
                        }
                        else if (EC.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name == "Swipe" && ECougar.IsValidTarget(EC.Range))
                        {
                            EC.Cast(ECougar);
                        }
                        break;
                    case 1:
                        if (EH.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name == "PrimalSurge" && Player.ManaPercent() >= manae && Player.HealthPercent() <= hpe)
                        {
                            EH.Cast(Player);
                        }
                        break;
                    case 2:
                        if (EC.Ready && Player.SpellBook.GetSpell(SpellSlot.E).Name == "Swipe" && ECougar.IsValidTarget(EC.Range))
                        {
                            EC.Cast(ECougar);
                        }
                        break;
                }
            }
            bool useR = Menu["combo"]["user"].Enabled;
            float rangeR = Menu["combo"]["userr"].As<MenuSlider>().Value;
            if (useR)
            {
                var RTarget = GetBestEnemyHeroTargetInRange(rangeR);
                switch (Menu["combo"]["ro"].As<MenuList>().Value)
                {
                    case 0:
                        if (R.Ready && !QH.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && RTarget.IsValidTarget(WC.Range))
                        {
                            R.Cast();
                        }
                        else if (R.Ready && !QC.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && RTarget.IsValidTarget(QH.Range))
                        {
                            R.Cast();
                        }
                        break;
                }
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
            foreach (var minion in GameObjects.Jungle.Where(m => m.IsValidTarget(QH.Range)).ToList())
            {
                if (!minion.IsValidTarget() || !minion.IsValidSpellTarget() || minion == null)
                {
                    return;
                }

                float manapercent = Menu["jungleclear"]["manaj"].As<MenuSlider>().Value;
                if (Player.ManaPercent() >= manapercent)
                {
                    bool useQ = Menu["jungleclear"]["usejq"].Enabled;
                    if (useQ)
                    {
                        switch (Menu["jungleclear"]["qo"].As<MenuList>().Value)
                        {
                            case 0:
                                if (QH.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && minion.IsValidTarget(QH.Range))
                                {
                                    QH.Cast(minion);
                                }
                                else if (QC.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && minion.IsValidTarget(QC.Range))
                                {
                                    QC.Cast();
                                }
                                break;
                            case 1:
                                if (QH.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && minion.IsValidTarget(QH.Range))
                                {
                                    QH.Cast(minion);
                                }
                                break;
                            case 2:
                                if (QC.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && minion.IsValidTarget(QC.Range))
                                {
                                    QC.Cast();
                                }
                                break;
                        }
                    }
                    bool useW = Menu["jungleclear"]["usejw"].Enabled;
                    if (useW)
                    {
                        switch (Menu["jungleclear"]["wo"].As<MenuList>().Value)
                        {
                            case 0:
                                if (WH.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack" && minion.IsValidTarget(WH.Range))
                                {
                                    WH.Cast(minion);
                                }
                                else if (WC.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && minion.IsValidTarget(WC.Range))
                                {
                                    WC.Cast(minion);
                                }
                                break;
                            case 1:
                                if (WH.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Bushwhack" && minion.IsValidTarget(WH.Range))
                                {
                                    WH.Cast(minion);
                                }
                                break;
                            case 2:
                                if (WC.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && minion.IsValidTarget(WC.Range))
                                {
                                    WC.Cast(minion);
                                }
                                else if (WCL.Ready && Player.SpellBook.GetSpell(SpellSlot.W).Name == "Pounce" && minion.IsValidTarget(WCL.Range) && minion.HasBuff("NidaleePassiveHunted"))
                                {
                                    WCL.Cast(minion);
                                }
                                break;
                        }
                    }
                    bool useE = Menu["jungleclear"]["usejce"].Enabled;
                    if (useE && Player.SpellBook.GetSpell(SpellSlot.E).Name == "Swipe" && minion.IsValidTarget(EC.Range))
                    {
                        EC.Cast(minion);
                    }
                    bool useR = Menu["jungleclear"]["usejr"].Enabled;
                    if (useR)
                    {
                        switch (Menu["combo"]["ro"].As<MenuList>().Value)
                        {
                            case 0:
                                if (R.Ready && !QH.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && minion.IsValidTarget(QC.Range))
                                {
                                    R.Cast();
                                }
                                else if (R.Ready && !QC.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && minion.IsValidTarget(QH.Range))
                                {
                                    R.Cast();
                                }
                                break;
                            case 1:
                                if (R.Ready && !QH.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "JavelinToss" && minion.HasBuff("NidaleePassiveHunted") && minion.IsValidTarget(WCL.Range))
                                {
                                    R.Cast();
                                }
                                else if (R.Ready && !QC.Ready && Player.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown" && !minion.HasBuff("NidaleePassiveHunted") && minion.IsValidTarget(QC.Range))
                                {
                                    R.Cast();
                                }
                                break;
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