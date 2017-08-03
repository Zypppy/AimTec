using System.Drawing;
using Adept_AIO.Champions.Kayn.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;

namespace Adept_AIO.Champions.Kayn.Drawings
{
    internal class DrawManager
    {
        public static void RenderManager()
        {
            if (Global.Player.IsDead)
            {
                return;
            }

            if (MenuConfig.Drawings["W"].Enabled && SpellConfig.W.Ready)
            {
                Render.Circle(Global.Player.Position, SpellConfig.W.Range, (uint)MenuConfig.Drawings["Segments"].Value, Color.IndianRed);
            }

            if (MenuConfig.Drawings["R"].Enabled && SpellConfig.R.Ready)
            {
                Render.Circle(Global.Player.Position, SpellConfig.R.Range, (uint)MenuConfig.Drawings["Segments"].Value, Color.IndianRed);
            }
        }
    }
}
