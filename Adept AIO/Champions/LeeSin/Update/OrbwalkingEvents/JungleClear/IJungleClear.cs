using Aimtec;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.JungleClear
{
    internal interface IJungleClear
    {
        void OnPostAttack(Obj_AI_Minion mob);

        void OnUpdate();

        void StealMobs();
    }
}