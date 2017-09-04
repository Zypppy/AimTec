using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhaZix
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
            if (ObjectManager.GetLocalPlayer().ChampionName != "Khazix")
                return;

            var KhaZix = new KhaZix();
        }
    }
}