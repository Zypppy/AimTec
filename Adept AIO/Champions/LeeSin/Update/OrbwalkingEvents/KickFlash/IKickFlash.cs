using Aimtec;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.KickFlash
{
    interface IKickFlash
    {
        void OnKeyPressed();
        void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args);
        bool Enabled { get; set; }
    }
}
