namespace AurelionSol
{
    using Aimtec;
    using Aimtec.SDK.Damage;
    using Aimtec.SDK.Extensions;
    using Aimtec.SDK.Menu;
    using Aimtec.SDK.Menu.Components;
    using Aimtec.SDK.Orbwalking;
    using Aimtec.SDK.Prediction.Skillshots;
    using Aimtec.SDK.TargetSelector;
    using Aimtec.SDK.Util;
    using System;
    using System.Drawing;
    using System.Linq;
    using Spell = Aimtec.SDK.Spell;

    internal class AurelionSol
    {
        public static Menu Menu = new Menu("AurelionSol by Zypppy", "AurelionSol by Zypppy", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Player = ObjectManager.GetLocalPlayer();
        public static Spell Q, W, W2, E, R;
        private MissileClient missiles;

        public void LoadSpells()
        {
            Q = new Spell(SpellSlot.Q, 1500f);//AurelionSolQ AurelionSolQCancelButton
            Q.SetSkillshot(0.5f, 110f, 850f, false, SkillshotType.Line);
            W = new Spell(SpellSlot.W, 350f);//AurelionSolW
            W2 = new Spell(SpellSlot.W, 650f);//AurelionSolWToggleOff aurelionsolwactive
            E = new Spell(SpellSlot.E, 400f);//AurelionSolE
            R = new Spell(SpellSlot.R, 1419f);//AurelionSolR
            R.SetSkillshot(0.5f, 150, 4500f, false, SkillshotType.Line);
        }
        public AurelionSol()
        {
            Orbwalker.Attach(Menu);
            var ComboMenu = new Menu("combo", "Combo");
            {
                ComboMenu.Add(new MenuBool("useq", "Use Q"));
                ComboMenu.Add(new MenuBool("usew", "Use W"));
                ComboMenu.Add(new MenuBool("usewlock", "Use Outer W Movement Lock", false));
                ComboMenu.Add(new MenuBool("user", "Use R"));
                ComboMenu.Add(new MenuSlider("hitr", "R Minimum Enemeies Hit", 3, 1, 5));
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
            var MiscMenu = new Menu("misc", "Misc");
            {
                MiscMenu.Add(new MenuBool("aa", "Dusable AA Combo When W Enabled", false));
                MiscMenu.Add(new MenuBool("aa2", "Dusable AA Harass When W Enabled", false));
            }
            Menu.Add(MiscMenu);
            var KillstealMenu = new Menu("killsteal", "Killsteal");
            {
                KillstealMenu.Add(new MenuBool("RKS", "Use R to Killsteal"));
            }
            Menu.Add(KillstealMenu);
            var DrawingsMenu = new Menu("drawings", "Drawings");
            {
                DrawingsMenu.Add(new MenuBool("drawq", "Draw Q Range"));
                DrawingsMenu.Add(new MenuBool("drawq2", "Draw Circle Around Q"));
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
            WLock();
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
            if (target != null && target.IsValidTarget())
            {
                return target;
            }

            var firstTarget = ts.GetOrderedTargets(range)
                .FirstOrDefault(t => t.IsValidTarget());
            if (firstTarget != null)
            {
                return firstTarget;
            }

            return null;
        }

        public static bool AnyWallInBetween(Vector3 startPos, Vector2 endPos)
        {
            for (var i = 0; i < startPos.Distance(endPos); i++)
            {
                var point = NavMesh.WorldToCell(startPos.Extend(endPos, i));
                if (point.Flags.HasFlag(NavCellFlags.Wall | NavCellFlags.Building))
                {
                    return true;
                }
            }

            return false;
        }
        
        private void WLock()
        {
            if (Menu["combo"]["usewlock"].Enabled)
            {
                if (Player.SpellBook.GetSpell(SpellSlot.W).Name == "AurelionSolWToggleOff")
                {
                    Orbwalker.AttackingEnabled = false;
                }
                if (Player.SpellBook.GetSpell(SpellSlot.W).Name == "AurelionSolW")
                {
                    Orbwalker.AttackingEnabled = true;
                }
                var target = GetBestEnemyHeroTargetInRange(W2.Range);
                {
                    if (target.IsValidTarget(W2.Range) && target != null)
                    {
                        if (target.ServerPosition.Distance(Player.ServerPosition) < W2.Range)
                        {
                            if (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2)
                           {
                                Orbwalker.Move(target.ServerPosition.Extend(Player.ServerPosition, W2.Range));
                            }
                        }
                    }
                }
            }
            var target2 = GetBestEnemyHeroTargetInRange(W2.Range + 200);
            if (target2.IsValidTarget(W2.Range + 200) && target2 != null)
            {
                if (target2.ServerPosition.Distance(Player.ServerPosition) > W2.Range - 50)
                {
                    if (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2 && Menu["combo"]["usewlock"].Enabled)
                    {
                        Orbwalker.Move(target2.ServerPosition);
                    }
                }
            }
        }
        private void OnCombo()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range);
            if (!target.IsValidTarget())
            {
                return;
            }

            bool useQ = Menu["combo"]["useq"].Enabled;
            if (Q.Ready && useQ)
            {
                switch (Player.SpellBook.GetSpell(SpellSlot.Q).ToggleState)
                {
                    case 1:
                        if (target.IsValidTarget(Q.Range) && Player.SpellBook.GetSpell(SpellSlot.Q).ToggleState == 1)
                        {
                            Q.Cast(target);
                        }
                        break;
                    case 2:
                        if (missiles != null && target.IsValidTarget(200f, false, false, missiles.Position) &&
                            Player.SpellBook.GetSpell(SpellSlot.Q).ToggleState == 2)
                        {
                            Q.Cast();
                        }
                        break;
                }
            }

            bool useW = Menu["combo"]["usew"].Enabled;
            bool AA = Menu["misc"]["aa"].Enabled;
            if (W.Ready && useW)
            {
                if (AA)
                {
                    if (Player.SpellBook.GetSpell(SpellSlot.W).Name == "AurelionSolWToggleOff")
                    {
                        Orbwalker.AttackingEnabled = false;
                    }
                    if (Player.SpellBook.GetSpell(SpellSlot.W).Name != "AurelionSolWToggleOff")
                    {
                        Orbwalker.AttackingEnabled = true;
                    }
                }
                switch (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState)
                {
                    case 0:
                        if (target.IsValidTarget(W2.Range) && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 0)
                        {
                            W2.Cast();
                        }
                        break;
                    case 2:
                        if (target.IsValidTarget(W.Range) && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2)
                        {
                            W.Cast();
                        }
                        break;
                }
            }
            
            bool useR = Menu["combo"]["user"].Enabled;
            if (R.Ready && target.IsValidTarget(R.Range) && useR && R.CastIfWillHit(target,  Menu["combo"]["hitr"].As<MenuSlider>().Value - 1))
            {
                R.Cast(target);
            }
        }
        private void OnHarass()
        {
            var target = GetBestEnemyHeroTargetInRange(Q.Range);

            if (!target.IsValidTarget())
            {
                return;
            }

            bool useQ = Menu["harass"]["useq"].Enabled;
            float manaQ = Menu["harass"]["manaq"].As<MenuSlider>().Value;
            if (Q.Ready && useQ)
            {
                switch (Player.SpellBook.GetSpell(SpellSlot.Q).ToggleState)
                {
                    case 1:
                        if (target.IsValidTarget(Q.Range) && Player.ManaPercent() >= manaQ && Player.SpellBook.GetSpell(SpellSlot.Q).ToggleState == 1)
                        {
                            Q.Cast(target);
                        }
                        break;
                    case 2:
                        if (missiles != null && target.IsValidTarget(200f, false, false, missiles.Position) &&
                            Player.SpellBook.GetSpell(SpellSlot.Q).ToggleState == 2)
                        {
                            Q.Cast();
                        }
                        break;
                }
            }

            bool useW = Menu["harass"]["usew"].Enabled;
            bool AA = Menu["misc"]["aa2"].Enabled;
            float manaW = Menu["harass"]["manaw"].As<MenuSlider>().Value;
            if (W.Ready && useW)
            {
                if (AA)
                {
                    if (Player.SpellBook.GetSpell(SpellSlot.W).Name == "AurelionSolWToggleOff")
                    {
                        Orbwalker.AttackingEnabled = false;
                    }
                    if (Player.SpellBook.GetSpell(SpellSlot.W).Name == "AurelionSolW")
                    {
                        Orbwalker.AttackingEnabled = true;
                    }
                }
                switch (Player.SpellBook.GetSpell(SpellSlot.W).ToggleState)
                {
                    case 0:
                        if (target.IsValidTarget(W2.Range) && Player.ManaPercent() >= manaW && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 0)
                        {
                            W2.Cast();
                        }
                        break;
                    case 2:
                        if (target.IsValidTarget(W.Range) || !target.IsValidTarget(W2.Range) || Player.ManaPercent() < manaW && Player.SpellBook.GetSpell(SpellSlot.W).ToggleState == 2)
                        {
                            W.Cast();
                        }
                        break;
                }
            }
        }
        private void ManualR()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range);
            Player.IssueOrder(OrderType.MoveTo, Game.CursorPos);
            if (R.Ready && target.IsValidTarget(R.Range))
            {
                R.Cast(target);
            }
        }
    }
}