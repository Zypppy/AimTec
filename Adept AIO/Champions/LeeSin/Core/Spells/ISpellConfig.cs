using Aimtec;
using Aimtec.SDK.Orbwalking;
using Spell = Aimtec.SDK.Spell;

namespace Adept_AIO.Champions.LeeSin.Core.Spells
{
    public interface ISpellConfig
    {
        float LastQ1CastAttempt { get; set; }
        bool QAboutToEnd { get; }
        bool IsQ2();

        bool HasQ2(Obj_AI_Base target);
        int PassiveStack();
        bool IsFirst(Spell spell);

        void QSmite(Obj_AI_Base target);
        void Load();
        void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args);

        OrbwalkerMode InsecMode { get; }
        OrbwalkerMode WardjumpMode { get; }
        OrbwalkerMode KickFlashMode { get; }

        Spell Q { get; }
        Spell W { get; }
        Spell E { get; }
        Spell R { get; }
    }
}