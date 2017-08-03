using Adept_AIO.Champions.Yasuo.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Yasuo.Update.Miscellaneous
{
    internal class SafetyMeasure
    {
        public static void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (!MenuConfig.Combo["Dodge"].Enabled ||
               !SpellConfig.W.Ready ||
                args == null ||
                sender == null || 
                args.Target == null ||
                sender.IsMe ||            // Yes. Something prints null. so i check everything. Leave. Me. Alone.
                sender.IsAlly || 
               !args.Sender.IsHero ||
                args.SpellData.MissileUnblockable ||
                args.SpellData.ConsideredAsAutoAttack)
            {
                return;
            }

            if (args.End.Distance(Global.Player.ServerPosition) <= 300)
            {
                SpellConfig.W.Cast(sender.ServerPosition);
            }
        }
    }
}
