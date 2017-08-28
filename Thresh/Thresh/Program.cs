namespace Thresh
{
    using Aimtec;
    using Aimtec.SDK.Events;


    class Program
    {
        static void Main(string[] args)
        {
            GameEvents.GameStart += GameEvents_GameStart;
        }

        private static void GameEvents_GameStart()
        {
            if (ObjectManager.GetLocalPlayer().ChampionName != "Thresh")
                return;

            var Thresh = new Thresh();
        }
    }
}