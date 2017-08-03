using System.Linq;
using Adept_AIO.Champions.Riven.Core;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.Riven.Update.Miscellaneous
{
    internal class SafetyMeasure
    {
        public static void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (!MenuConfig.Miscellaneous["Interrupt"].Enabled ||
                args == null ||
                args.Sender == null ||
                sender == null ||
                args.SpellData == null)
            {
                return;
            }
            
            if (SpellConfig.E.Ready && (TargetedSpells.Contains(args.SpellData.Name) || AntigapclosingSpells.Contains(args.SpellData.Name)) && args.Target.IsMe)
            {
                SpellConfig.E.Cast(Game.CursorPos);
            }
            else if (SpellConfig.W.Ready && SpellManager.InsideKiBurst(args.End) &&
                (AntigapclosingSpells.Contains(args.SpellData.Name) || InterrupterSpell.Contains(args.SpellData.Name)))
            {
                SpellConfig.W.Cast();
            }
        }

        private static readonly string[] AntigapclosingSpells =
        {
            "MonkeyKingSpinToWin", "KatarinaRTrigger", "HungeringStrike",
            "TwitchEParticle", "RengarPassiveBuffDashAADummy",
            "RengarPassiveBuffDash", "IreliaEquilibriumStrike",
            "BraumBasicAttackPassiveOverride", "gnarwproc",
            "hecarimrampattack", "illaoiwattack", "JaxEmpowerTwo",
            "JayceThunderingBlow", "RenektonSuperExecute",
            "vaynesilvereddebuff"
        };

        private static readonly string[] TargetedSpells =
        {
            "MonkeyKingQAttack", "FizzPiercingStrike", "IreliaEquilibriumStrike",
            "RengarQ", "GarenQAttack", "GarenRPreCast",
            "PoppyPassiveAttack", "viktorqbuff", "FioraEAttack",
            "TeemoQ"
        };

        private static readonly string[] InterrupterSpell =
        {
            "RenektonPreExecute", "TalonCutthroat", "IreliaEquilibriumStrike",
            "XenZhaoThrust3", "KatarinaRTrigger", "KatarinaE",
        };
    }
}
