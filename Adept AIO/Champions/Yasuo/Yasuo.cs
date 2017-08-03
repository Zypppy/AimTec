using Adept_AIO.Champions.Yasuo.Core;
using Adept_AIO.Champions.Yasuo.Drawings;
using Adept_AIO.Champions.Yasuo.Update.Miscellaneous;
using Adept_AIO.SDK.Extensions;
using Aimtec;

namespace Adept_AIO.Champions.Yasuo
{
    internal class Yasuo
    {
        public static void Init()
        {
            MenuConfig.Attach();
            SpellConfig.Load();

            Game.OnUpdate += Manager.OnUpdate;
            Obj_AI_Base.OnPlayAnimation += Manager.OnPlayAnimation;
            Obj_AI_Base.OnProcessSpellCast += SafetyMeasure.OnProcessSpellCast;
            Global.Orbwalker.PostAttack += Manager.PostAttack;

            Render.OnRender += DrawManager.RenderManager;
            Render.OnPresent += DrawManager.DrawDamage;
            BuffManager.OnAddBuff += Manager.BuffManagerOnOnAddBuff;
            BuffManager.OnRemoveBuff += Manager.BuffManagerOnOnRemoveBuff;
        }
    }
}
