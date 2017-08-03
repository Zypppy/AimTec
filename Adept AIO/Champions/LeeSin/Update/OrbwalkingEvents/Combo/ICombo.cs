using Aimtec;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.Combo
{
    internal interface ICombo
    {
        void OnPostAttack(AttackableUnit target);
        void OnUpdate();
    }
}