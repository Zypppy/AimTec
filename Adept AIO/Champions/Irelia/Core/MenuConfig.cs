using System.Collections.Generic;
using Adept_AIO.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;

namespace Adept_AIO.Champions.Irelia.Core
{
    internal class MenuConfig
    {
        private static Menu MainMenu;

        public static Menu Combo,
                           Harass,
                           Clear,
                           Killsteal,
                           Drawings;

        public static void Attach()
        {
            MainMenu = new Menu(string.Empty, "Adept AIO", true);
            MainMenu.Attach();

            Global.Orbwalker.Attach(MainMenu);

            Combo = new Menu("Combo", "Combo")
            {
              new MenuBool("R", "R Minion To Gapclose Q"),
              new MenuBool("Q",  "Use Q To Gapclose"),
              new MenuBool("Killable", "Q Target If Killable"),
              new MenuBool("Force",   "Force Q To Stun (When Getting Ganked)"),
              new MenuSlider("Range", "Min. Range For Q", 450, 0, 650),
              new MenuList("Mode", "Dash Mode: ", new []{"Cursor", "Player Position"}, 0),
              new MenuBool("Turret", "Dash Turret When Killable")
            };

            Clear = new Menu("Clear", "Clear")
            {
                new MenuBool("Turret", "Check Turret"),
                new MenuBool("Check", "Avoid Farming When Nearby Enemies"),
                new MenuSliderBool("Q", "Use Q On Killable Minions (Min. Mana %)", true, 50, 0, 100),
                new MenuSliderBool("Lasthit", "Lasthit Q", true, 65, 0, 100),
                new MenuBool("W", "Use W (Jungle)"),
                new MenuSliderBool("E", "Use E On Big Mobs (Jungle) (Min. Mana %)", true, 50, 0, 100)
            };

            Harass = new Menu("Harass", "Harass")
            {
                new MenuBool("Away", "Safe Harass (Q Away)"),
                new MenuSliderBool("Q", "(Q) Min. Mana %", true, 50, 0, 100),
                new MenuBool("W", "Use W"),
                new MenuSliderBool("E", "(E) Min. Mana %", true, 50, 0, 100)
            };

            Killsteal = new Menu("Killsteal", "Killsteal")
            {
                new MenuBool("Q", "Q"),
                new MenuBool("E", "E"),
                new MenuBool("R", "R")
            };

            Drawings = new Menu("Drawings", "Drawings")
            {
                new MenuSlider("Segments", "Segments", 200, 100, 300).SetToolTip("Smoothness of the circles. Less equals more FPS."),
                new MenuBool("Engage", "Draw Q Search Range"),
                new MenuBool("Q", "Q Range"),
                new MenuBool("R", "R Range")
            };

            foreach (var menu in new List<Menu>
            {
                Combo,
                Harass,
                Clear,
                Killsteal,
                Drawings,
                MenuShortcut.Credits
            })
            MainMenu.Add(menu);
        }
    }
}
