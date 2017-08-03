using System.Linq;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec.SDK.Events;

namespace Adept_AIO
{
    using Champions.Kayn;
    using Champions.LeeSin;
    using Champions.Irelia;
    using Champions.Jax;
    using Champions.Rengar;
    using Champions.Riven;
    using Champions.Yasuo;

    internal class Bootstrap
    {
        private static void Main()
        {
            GameEvents.GameStart += GameEvents_GameStart;
        }

        private static readonly string[] Valid = { "Riven", "Irelia", "Jax", "Rengar", "Yasuo", "Kayn", "LeeSin" };

        private static void GameEvents_GameStart()
        {
            if (Valid.All(x => Global.Player.ChampionName != x))
            {
                return;
            }

            SummonerSpells.Init();
            GameObjects.Init();
            Global.Init();

            switch (Global.Player.ChampionName)
            {
                case "Irelia":
                    Irelia.Init();
                    break;
                case "Jax":
                    Jax.Init();
                    break;
                case "Kayn":
                    Kayn.Init();
                    break;
                case "LeeSin":
                    var lee = new LeeSin();
                    lee.Init();
                    break;
                case "Rengar":
                    Rengar.Init();
                    break;
                case "Riven":
                    Riven.Init();
                    break;
                case "Yasuo":
                    Yasuo.Init();
                    break;
            }
        }
    }
}