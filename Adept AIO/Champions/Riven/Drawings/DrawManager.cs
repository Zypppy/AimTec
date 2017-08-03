using System;
using System.Drawing;
using System.Linq;
using Adept_AIO.Champions.Riven.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Orbwalking;

namespace Adept_AIO.Champions.Riven.Drawings
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
                Global.DamageIndicator.DrawDmg((float)damage, Color.FromArgb(153, 12, 177, 28));
            }
        }

        public static void RenderBasics()
        {
            if (Global.Player.IsDead)
            {
                return;
            }

            if (MenuConfig.Drawings["Harass"].Enabled && Global.Orbwalker.Mode == OrbwalkingMode.Mixed)
            {
                Vector2 screenPos;
                Render.WorldToScreen(Global.Player.Position, out screenPos);
                Render.Text(new Vector2(screenPos.X - 65, screenPos.Y + 30), Color.Aqua, "PATTERN: " + Extensions.Current);
            }

            if (MenuConfig.Drawings["Engage"].Enabled)
            {
                if (Extensions.AllIn)
                {
                    Render.Circle(Global.Player.Position, Extensions.FlashRange(),
                        (uint)MenuConfig.Drawings["Segments"].Value, Color.Yellow);
                }
                else
                {
                    Render.Circle(Global.Player.Position, Extensions.EngageRange(),
                        (uint)MenuConfig.Drawings["Segments"].Value, Color.White);
                }
            }

            if (MenuConfig.Drawings["R2"].Enabled && SpellConfig.R2.Ready && Extensions.UltimateMode == UltimateMode.Second)
            {
                Render.Circle(Global.Player.Position, SpellConfig.R2.Range, (uint)MenuConfig.Drawings["Segments"].Value, Color.OrangeRed);
            }
        }
    }
}
