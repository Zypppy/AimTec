using System;
using System.Drawing;
using System.Linq;
using Adept_AIO.Champions.Jax.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;

namespace Adept_AIO.Champions.Jax.Drawings
{
    internal class DrawManager
    {
        public static void DrawDamage()
        {
            if (Global.Player.IsDead || !MenuConfig.Drawings["Dmg"].Enabled)
            {
                return;
            }

            foreach (var target in GameObjects.EnemyHeroes.Where(x => !x.IsDead && x.IsFloatingHealthBarActive && x.IsVisible))
            {
                var damage = Dmg.Damage(target);

                Global.DamageIndicator.Unit = target;
                Global.DamageIndicator.DrawDmg((float) damage, Color.FromArgb(153, 12, 177, 28));
            }
        }

        public static void RenderManager()
        {
            if (Global.Player.IsDead)
            {
                return;
            }

            if (MenuConfig.Drawings["E"].Enabled && SpellConfig.E.LastCastAttemptT > 0 && Game.TickCount - SpellConfig.E.LastCastAttemptT < 2000)
            {
                Vector2 screen;
                Render.WorldToScreen(Global.Player.Position, out screen);
                Render.Text(new Vector2(screen.X - 55, screen.Y + 40), Color.Cyan, "Time Until Q: " + (Game.TickCount - SpellConfig.E.LastCastAttemptT) + " / 2000");
            }

            if (MenuConfig.Drawings["Q"].Enabled && SpellConfig.Q.Ready)
            {
                Render.Circle(Global.Player.Position, SpellConfig.Q.Range, (uint)MenuConfig.Drawings["Segments"].Value, Color.Cyan);
            }
        }
    }
}
