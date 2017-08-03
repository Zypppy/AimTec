using Adept_AIO.Champions.Kayn.Core;
using Adept_AIO.Champions.Kayn.Drawings;
using Adept_AIO.Champions.Kayn.Update.Miscellaneous;
using Adept_AIO.SDK.Extensions;
using Aimtec;

namespace Adept_AIO.Champions.Kayn
{
    internal class Kayn
    {
        public static void Init()
        {
            MenuConfig.Attach();
            SpellConfig.Load();

            Game.OnUpdate += Killsteal.OnUpdate;
            Game.OnUpdate += Manager.OnUpdate;
            Global.Orbwalker.PostAttack += Manager.PostAttack;

            Render.OnRender += DrawManager.RenderManager;
        }
    }
}
