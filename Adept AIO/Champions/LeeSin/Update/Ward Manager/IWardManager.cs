using Aimtec;

namespace Adept_AIO.Champions.LeeSin.Update.Ward_Manager
{
    internal interface IWardManager
    {
        void WardJump(Vector3 position, bool maxRange);

        bool IsWardReady();
    }
}