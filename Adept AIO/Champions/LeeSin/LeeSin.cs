using System.Collections.Generic;
using Adept_AIO.Champions.LeeSin.Core.Insec_Manager;
using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.KickFlash;
using Adept_AIO.Champions.LeeSin.Update.Ward_Manager;
using Aimtec;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Util;

namespace Adept_AIO.Champions.LeeSin
{
    using Drawings;
    using Update.Miscellaneous;
    using Update.OrbwalkingEvents.Combo;
    using Update.OrbwalkingEvents.Harass;
    using Update.OrbwalkingEvents.Insec;
    using Update.OrbwalkingEvents.JungleClear;
    using Update.OrbwalkingEvents.LaneClear;
    using Update.OrbwalkingEvents.LastHit;
    using Update.OrbwalkingEvents.WardJump;
    using SDK.Extensions;

    internal class LeeSin
    {
        public void Init()
        {
            var spellConfig = new SpellConfig();
            spellConfig.Load();

            var insecManager = new Insec_Manager(spellConfig);

            var wardtracker = new WardTracker(spellConfig);
            var wardmanager = new WardManager(wardtracker);

            var wardjump = new WardJump(wardtracker, wardmanager, spellConfig);
            var insec = new Insec(wardtracker, wardmanager, spellConfig, insecManager);
            var kickFlash = new KickFlash(spellConfig, insecManager);

            var combo = new Combo(wardmanager, spellConfig);
           
            var harass = new Harass(wardmanager, spellConfig);
            var jungle = new JungleClear(wardmanager, spellConfig);
            var lane = new LaneClear(spellConfig);
            var lasthit = new Lasthit(spellConfig);
            var killsteal = new Killsteal(spellConfig);
            var drawManager = new DrawManager(spellConfig, insecManager);

            var mainmenu = new Menu("main", "Adept AIO", true);
            mainmenu.Attach();

            var insecMode = new OrbwalkerMode("Insec", KeyCode.T, null, insec.OnKeyPressed);
            var wardjumpMode = new OrbwalkerMode("Wardjump", KeyCode.G, null, wardjump.OnKeyPressed);
            var kickFlashMode = new OrbwalkerMode("Kick Flash", KeyCode.A, null, kickFlash.OnKeyPressed);

            insecMode.MenuItem.OnValueChanged += (sender, args) => insec.Enabled = args.GetNewValue<MenuKeyBind>().Value;
            wardjumpMode.MenuItem.OnValueChanged += (sender, args) => wardjump.Enabled = args.GetNewValue<MenuKeyBind>().Value;
            kickFlashMode.MenuItem.OnValueChanged += (sender, args) => kickFlash.Enabled = args.GetNewValue<MenuKeyBind>().Value;

            Global.Orbwalker.AddMode(insecMode);
            Global.Orbwalker.AddMode(wardjumpMode);
            Global.Orbwalker.AddMode(kickFlashMode);
            Global.Orbwalker.Attach(mainmenu);

            var insecMenu = new Menu("Insec", "Insec");
            var insecObject = new MenuBool("Object", "Use Q On Minions").SetToolTip("Uses Q to gapclose to every minion");
            var insecQLast = new MenuBool("Last", "Use Q After Insec").SetToolTip("Only possible if no minions near target");
            var insecPosition = new MenuList("Position", "Insec Position", new[] {"Ally Turret", "Ally Hero"}, 0);
            var insecKick = new MenuList("Kick", "Kick Type: ", new[] {"Flash R", "R Flash"}, 1);

            insecObject.OnValueChanged += (sender, args) => insec.ObjectEnabled = args.GetNewValue<MenuBool>().Value;
            insecQLast.OnValueChanged += (sender, args) => insec.QLast = args.GetNewValue<MenuBool>().Value;
            insecPosition.OnValueChanged += (sender, args) => insecManager.InsecPositionValue = args.GetNewValue<MenuList>().Value;
            insecKick.OnValueChanged += (sender, args) => insecManager.InsecKickValue = args.GetNewValue<MenuList>().Value;

            insec.ObjectEnabled = insecObject.Enabled;
            insec.QLast = insecQLast.Enabled;

            insecManager.InsecPositionValue = insecPosition.Value; 
            insecManager.InsecKickValue = insecKick.Value;

            insecMenu.Add(insecObject);
            insecMenu.Add(insecQLast);
            insecMenu.Add(insecPosition);
            insecMenu.Add(insecKick);
            mainmenu.Add(insecMenu);

            var comboMenu = new Menu("Combo", "Combo");
            var comboTurret = new MenuBool("Turret", "Don't Q2 Into Turret");
            var comboQ    = new MenuBool("Q", "Use Q");
            var comboQ2   = new MenuBool("Q2", "Use Q2");
            var comboW    = new MenuBool("W", "Use W");
            var comboWard = new MenuBool("Ward", "Use Wards");
            var comboE    = new MenuBool("E", "Use E");

            foreach (var b in new List<MenuBool>
            {
                comboTurret,
                comboQ,
                comboQ2,
                comboW,
                comboWard,
                comboE
            }) comboMenu.Add(b);
            mainmenu.Add(comboMenu);

            combo.TurretCheckEnabled = comboMenu["Turret"].Enabled;
            combo.Q1Enabled = comboMenu["Q"].Enabled;
            combo.Q2Enabled = comboMenu["Q2"].Enabled;
            combo.WEnabled = comboMenu["W"].Enabled;
            combo.WardEnabled = comboMenu["Ward"].Enabled;
            combo.EEnabled = comboMenu["E"].Enabled;

            comboTurret.OnValueChanged += (sender, args) => combo.TurretCheckEnabled = args.GetNewValue<MenuBool>().Value;
            comboQ.OnValueChanged += (sender, args) => combo.Q1Enabled = args.GetNewValue<MenuBool>().Value;
            comboQ2.OnValueChanged += (sender, args) => combo.Q2Enabled = args.GetNewValue<MenuBool>().Value;
            comboW.OnValueChanged += (sender, args) => combo.WEnabled = args.GetNewValue<MenuBool>().Value;
            comboWard.OnValueChanged += (sender, args) => combo.WardEnabled = args.GetNewValue<MenuBool>().Value;
            comboE.OnValueChanged += (sender, args) => combo.EEnabled = args.GetNewValue<MenuBool>().Value;

            var harassMenu = new Menu("Harass", "Harass");
            var harassQ = new MenuBool("Q", "Use Q");
            var harassQ2 = new MenuBool("Q2", "Use Q2");
            var harassMode = new MenuList("Mode", "W Mode: ", new[] {"Away", "W Self"}, 0);
            var harassE = new MenuBool("E", "Use E");
            var harassE2 = new MenuBool("E2", "Use E2");

            harassMenu.Add(harassQ);
            harassMenu.Add(harassQ2);
            harassMenu.Add(harassMode);
            harassMenu.Add(harassE);
            harassMenu.Add(harassE2);
            mainmenu.Add(harassMenu);

            harass.Q1Enabled = harassMenu["Q"].Enabled;
            harass.Q2Enabled = harassMenu["Q2"].Enabled;
            harass.Mode = harassMenu["Mode"].Value;
            harass.EEnabled = harassMenu["E"].Enabled;
            harass.E2Enabled = harassMenu["E2"].Enabled;

            harassQ.OnValueChanged += (sender, args) => harass.Q1Enabled = args.GetNewValue<MenuBool>().Value;
            harassQ2.OnValueChanged += (sender, args) => harass.Q2Enabled = args.GetNewValue<MenuBool>().Value;
            harassMode.OnValueChanged += (sender, args) => harass.Mode = args.GetNewValue<MenuList>().Value;
            harassE.OnValueChanged += (sender, args) => harass.EEnabled = args.GetNewValue<MenuBool>().Value;
            harassE2.OnValueChanged += (sender, args) => harass.E2Enabled = args.GetNewValue<MenuBool>().Value;

            var jungleMenu = new Menu("Jungle", "Jungle");
            var jungleSteal = new MenuBool("Steal", "Steal Legendary");
            var jungleSmite = new MenuBool("Smite", "Smite Big Mobs");
            var jungleBlue = new MenuBool("Blue", "Smite Blue Buff");
            var jungleQ = new MenuBool("Q", "Q");
            var jungleW = new MenuBool("W", "W");
            var jungleE = new MenuBool("E", "E");

            foreach (var b in new List<MenuBool>
            {
                jungleSteal,
                jungleSmite,
                jungleBlue,
                jungleQ,
                jungleW,
                jungleE
            }) jungleMenu.Add(b);
            mainmenu.Add(jungleMenu);

            jungle.StealEnabled = jungleMenu["Steal"].Enabled;
            jungle.SmiteEnabled = jungleMenu["Smite"].Enabled;
            jungle.BlueEnabled = jungleMenu["Blue"].Enabled;
            jungle.Q1Enabled = jungleMenu["Q"].Enabled;
            jungle.WEnabled = jungleMenu["W"].Enabled;
            jungle.EEnabled = jungleMenu["E"].Enabled;

            jungleSteal.OnValueChanged += (sender, args) => jungle.StealEnabled = args.GetNewValue<MenuBool>().Value;
            jungleSmite.OnValueChanged += (sender, args) => jungle.Q1Enabled = args.GetNewValue<MenuBool>().Value;
            jungleBlue.OnValueChanged += (sender, args) => jungle.BlueEnabled = args.GetNewValue<MenuBool>().Value;
            jungleQ.OnValueChanged += (sender, args) => jungle.Q1Enabled = args.GetNewValue<MenuBool>().Value;
            jungleW.OnValueChanged += (sender, args) => jungle.WEnabled = args.GetNewValue<MenuBool>().Value;
            jungleE.OnValueChanged += (sender, args) => jungle.EEnabled = args.GetNewValue<MenuBool>().Value;

            var laneMenu = new Menu("Lane", "Lane");
            var laneCheck = new MenuBool("Check", "Don't Clear When Enemies Nearby");
            var laneQ = new MenuBool("Q", "Q");
            var laneW = new MenuBool("W", "W");
            var laneE = new MenuBool("E", "E");

            foreach (var b in new List<MenuBool>
            {
                laneCheck,
                laneQ,
                laneW,
                laneE
            }) laneMenu.Add(b);
            mainmenu.Add(laneMenu);

            lane.CheckEnabled = laneMenu["Check"].Enabled;
            lane.Q1Enabled = laneMenu["Q"].Enabled;
            lane.WEnabled = laneMenu["W"].Enabled;
            lane.EEnabled = laneMenu["E"].Enabled;

            laneCheck.OnValueChanged += (sender, args) => lane.CheckEnabled = args.GetNewValue<MenuBool>().Value;
            laneQ.OnValueChanged += (sender, args) => lane.Q1Enabled = args.GetNewValue<MenuBool>().Value;
            laneW.OnValueChanged += (sender, args) => lane.WEnabled = args.GetNewValue<MenuBool>().Value;
            laneE.OnValueChanged += (sender, args) => lane.EEnabled = args.GetNewValue<MenuBool>().Value;

            var lasthitMenu = new Menu("Lasthit", "Lasthit");
            var lasthitEnabled = new MenuBool("Enabled", "Enabled");

            lasthitMenu.Add(lasthitEnabled);
            mainmenu.Add(lasthitMenu);
            lasthit.Enabled = lasthitMenu["Enabled"].Enabled;
            lasthitEnabled.OnValueChanged += (sender, args) => lasthit.Enabled = args.GetNewValue<MenuBool>().Value;

            var ksMenu = new Menu("Killsteal", "Killsteal");
            var ksIgnite = new MenuBool("Ignite", "Ignite");
            var ksSmite = new MenuBool("Smite", "Smite");
            var ksQ = new MenuBool("Q", "Q");
            var ksE = new MenuBool("E", "E");
            var ksR = new MenuBool("R", "R");

            foreach (var b in new List<MenuBool>
            {
                ksIgnite, ksSmite, ksQ, ksE, ksR
            })
            ksMenu.Add(b);
            mainmenu.Add(ksMenu);

            killsteal.IgniteEnabled = ksMenu["Ignite"].Enabled;
            killsteal.SmiteEnabled = ksMenu["Smite"].Enabled;
            killsteal.QEnabled = ksMenu["Q"].Enabled;
            killsteal.EEnabled = ksMenu["E"].Enabled;
            killsteal.REnabled = ksMenu["R"].Enabled;

            ksIgnite.OnValueChanged += (sender, args) => killsteal.IgniteEnabled = args.GetNewValue<MenuBool>().Value;
            ksSmite.OnValueChanged += (sender, args) => killsteal.SmiteEnabled = args.GetNewValue<MenuBool>().Value;
            ksQ.OnValueChanged += (sender, args) => killsteal.QEnabled = args.GetNewValue<MenuBool>().Value;
            ksE.OnValueChanged += (sender, args) => killsteal.EEnabled = args.GetNewValue<MenuBool>().Value;
            ksR.OnValueChanged += (sender, args) => killsteal.REnabled = args.GetNewValue<MenuBool>().Value;

            var drawMenu = new Menu("Draw", "Drawings");
            var drawSegments = new MenuSlider("Segments", "Segments", 200, 100, 300).SetToolTip("Smoothness of the circles. Less equals more FPS.");
            var drawPosition = new MenuBool("Position", "Insec Position");
            var drawQ = new MenuBool("Q", "Q Range");

            drawMenu.Add(drawSegments);
            drawMenu.Add(drawPosition);
            drawMenu.Add(drawQ);
            mainmenu.Add(drawMenu);

            drawManager.QEnabled = drawMenu["Q"].Enabled;
            drawManager.SegmentsValue = drawMenu["Segments"].Value;
            drawManager.PositionEnabled = drawMenu["Position"].Enabled;

            drawSegments.OnValueChanged += (sender, args) => drawManager.SegmentsValue = args.GetNewValue<MenuSlider>().Value;
            drawPosition.OnValueChanged += (sender, args) => drawManager.PositionEnabled = args.GetNewValue<MenuBool>().Value;
            drawQ.OnValueChanged += (sender, args) => drawManager.QEnabled = args.GetNewValue<MenuBool>().Value;

            var manager = new Manager(combo, harass, insec, jungle, lane, lasthit);
          
            Game.OnUpdate += manager.OnUpdate;
            Game.OnUpdate += killsteal.OnUpdate;

            Global.Orbwalker.PostAttack += manager.PostAttack;

            Render.OnRender += drawManager.RenderManager;

            Obj_AI_Base.OnProcessSpellCast += insec.OnProcessSpellCast;
            Obj_AI_Base.OnProcessSpellCast += kickFlash.OnProcessSpellCast;
            Obj_AI_Base.OnProcessSpellCast += spellConfig.OnProcessSpellCast;

            GameObject.OnCreate += wardtracker.OnCreate;
        }
    }
}