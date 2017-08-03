using System.Collections.Generic;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.SDK.Extensions
{
    internal class WallExtension
    {
        private static bool HasFlag(IEnumerable<Vector3> pos)
        {
            return NavMesh.WorldToCell(pos.FirstOrDefault()).Flags.HasFlag(NavCellFlags.Wall | NavCellFlags.Building);
        }

        public static List<Vector3> GeneratePoint(Vector3 start, Vector3 end)
        {
            for (var i = 0; i < start.Distance(end); i++)
            {
                var newPoint = new List<Vector3> {start.Extend(end, i)};
                var width = GetWallWidth(Global.Player.Position, newPoint.FirstOrDefault());

                if (HasFlag(newPoint))
                {
                    return newPoint;
                }
            }
            return new List<Vector3> {Vector3.Zero};
        }

        public static float GetWallWidth(Vector3 start, Vector3 direction, int maxWallWidth = 300)
        {
            var thickness = 0f;

            for (var i = 0; i < maxWallWidth; i++)
            {
                if (HasFlag(new List<Vector3> {start.Extend(direction, i)}))
                {
                    thickness += i;
                }
                else
                {
                    return thickness;
                }
            }
            return thickness;
        }
    }
}
