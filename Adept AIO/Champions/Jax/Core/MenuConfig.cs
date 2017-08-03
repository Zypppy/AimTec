using System.Collections.Generic;
using Adept_AIO.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;

namespace Adept_AIO.Champions.Jax.Core
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
                new MenuSliderBool("E", "Start E If Distance <", true, 700, 350, 800),
                new MenuBool("Jump", "Only Q If E Is Up OR Killable"),
                new MenuBool("Delay", "Delay Jump When E Is Active"),
                new MenuSliderBool("R", "Use R If 'x' Enemies Nearby", true, 2, 1, 5),
                new MenuBool("Check", "Check For Enemy Turrets")
            };

            Harass = new Menu("Harass", "Harass")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W"),
                new MenuBool("E", "E", false)
            };

            Clear = new Menu("Clear", "Clear")
            {
                new MenuBool("Check", "Don't Clear If Nearby Enemies"),
                new MenuBool("Q", "(Q) Jump to Big Mobs"),
                new MenuBool("W", "(W) After Auto"),
                new MenuBool("E", "(E) After Auto")
            };

            Killsteal = new Menu("Killsteal", "Killsteal")
            {
                new MenuBool("Q", "(Q)"),
            };

            Drawings = new Menu("Drawings", "Drawings")
            {
                new MenuSlider("Segments", "Segments", 200, 100, 300).SetToolTip("Smoothness of the circles. Less equals more FPS."),
                new MenuBool("Q", "Draw Q Range"),
                new MenuBool("E", "Draw E-Q Time"),
                new MenuBool("Dmg", "Draw Damage"),
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
