using Aimtec;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.Harass
{
    internal interface IHarass
    {
        void OnPostAttack(AttackableUnit target);
        void OnUpdate();
    }
}
