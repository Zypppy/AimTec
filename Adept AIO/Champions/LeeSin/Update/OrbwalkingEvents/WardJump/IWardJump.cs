namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.WardJump
{
    internal interface IWardJump
    {
        void OnKeyPressed();
        bool Enabled { get; set; }
    }
}