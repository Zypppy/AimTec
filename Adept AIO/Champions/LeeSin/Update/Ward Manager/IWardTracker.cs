using Aimtec;

namespace Adept_AIO.Champions.LeeSin.Update.Ward_Manager
{
    public interface IWardTracker
    {
        bool IsAtWall { get; set; }

        float LastWardCreated { get; set; }

        string WardName { get; }
        Vector3 WardPosition { get; set; }

        bool IsWardReady { get; }

        string[] WardNames { get; }
    }
}