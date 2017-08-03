using System.Collections.Generic;
using Adept_AIO.SDK.Extensions;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using GameObjects = Aimtec.SDK.Util.Cache.GameObjects;

namespace Adept_AIO.Champions.Rengar.Core
{
    internal class MenuConfig
    {
        private static Menu MainMenu;

        public static Menu Combo,
                           AssassinManager,
                           LaneClear,
                           JungleClear,
                           Killsteal,
                           Drawings;

        public static void Attach()
        {
            MainMenu = new Menu(string.Empty, "Adept AIO", true);
            MainMenu.Attach();

            Global.Orbwalker.Attach(MainMenu);

            AssassinManager = new Menu("AssassinManager", "Assassin Manager");
            foreach (var hero in GameObjects.EnemyHeroes)
            {
                AssassinManager.Add(new MenuBool(hero.ChampionName, "Assassinate: " + hero.ChampionName, false));
            }

            Combo = new Menu("Combo", "Combo")
            {
                new MenuBool("Q", "Allow Empowered Q"),
                new MenuBool("W", "Use W To Stack"),
                new MenuBool("E", "Allow Empowered E"),
                new MenuList("Mode", "Empowered W: ", new []{"Deal Damage", "Against Hard CC"}, 1),
                new MenuSlider("Health", "Force W When Below (% HP)", 10)
            };

            LaneClear = new Menu("LaneClear", "LaneClear")
            {
                new MenuBool("Check", "Don't Clear When Enemies Nearby"),
                new MenuBool("Q", "Allow Empowered Q"),
                new MenuBool("W", "Allow Empowered W"),
                new MenuSlider("Health", "Force W When Below (% HP)", 10)
            };

            JungleClear = new Menu("JungleClear", "JungleClear")
            {
                new MenuBool("Q", "Allow Empowered Q"),
                new MenuBool("W", "Allow Empowered W"),
                new MenuBool("W2", "Use W To Stack"),
                new MenuSlider("Health", "Force W When Below (% HP)", 10)
            };

            Killsteal = new Menu("Killsteal", "Killsteal")
            {
                new MenuBool("Q", "Use Q"),
                new MenuBool("W", "Use W"),
                new MenuBool("E", "Use E"),
            };

            Drawings = new Menu("Drawings", "Drawings")
            {
                new MenuSlider("Segments", "Segments", 200, 100, 300).SetToolTip("Smoothness of the circles. Less equals more FPS."),
                new MenuBool("Q", "Draw Q Range", false),
                new MenuBool("W", "Draw W Range", false),
                new MenuBool("E", "Draw E Range")
            };

            foreach (var menu in new List<Menu>
            {
                AssassinManager,
                Combo,
                LaneClear,
                JungleClear,
                Killsteal,
                Drawings,
                MenuShortcut.Credits
            })
            MainMenu.Add(menu);
        }
    }
}
