using System.Drawing;
using Adept_AIO.Champions.LeeSin.Core.Insec_Manager;
using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.SDK.Extensions;
using Aimtec;

namespace Adept_AIO.Champions.LeeSin.Drawings
{
    internal interface IDrawManager
    {
        void RenderManager();
    }

    internal class DrawManager : IDrawManager
    {
        public bool QEnabled { get; set; }
        public bool PositionEnabled { get; set; }
        public int SegmentsValue { get; set; }

        private readonly ISpellConfig SpellConfig;
        private readonly IInsec_Manager _insecManager;

        public DrawManager(ISpellConfig spellConfig, IInsec_Manager insecManager)
        {
            SpellConfig = spellConfig;
            _insecManager = insecManager;
        }
           
        public void RenderManager()
        {
            if (Global.Player.IsDead)
            {
                return;
            }

            if (QEnabled && SpellConfig.Q.Ready)
            {
                Render.Circle(Global.Player.Position, SpellConfig.Q.Range, (uint)SegmentsValue, Color.IndianRed);
            }

            var selected = Global.TargetSelector.GetSelectedTarget();

            if (PositionEnabled && selected != null && _insecManager.InsecPosition(selected) != Vector3.Zero)
            {
                Render.Circle(_insecManager.InsecPosition(selected), 65, (uint)SegmentsValue, Color.White);
            }
        }
    }
}
