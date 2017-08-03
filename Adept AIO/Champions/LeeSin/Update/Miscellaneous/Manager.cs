using Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.Combo;
using Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.Harass;
using Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.Insec;
using Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.JungleClear;
using Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.LaneClear;
using Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.LastHit;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Aimtec.SDK.Orbwalking;

namespace Adept_AIO.Champions.LeeSin.Update.Miscellaneous
{
    internal class Manager
    {
        private readonly ICombo _combo;
        private readonly IHarass _harass;
        private readonly IInsec _insec;
        private readonly IJungleClear _jungleClear;
        private readonly ILaneClear _laneClear;
        private readonly ILasthit _lasthit;

        public Manager(ICombo combo, IHarass harass, IInsec insec, IJungleClear jungleClear, ILaneClear laneClear, ILasthit lasthit)
        {
            _combo = combo;
            _harass = harass;
            _insec = insec;
            _jungleClear = jungleClear;
            _laneClear = laneClear;
            _lasthit = lasthit;
        }

        public void PostAttack(object sender, PostAttackEventArgs args)
        {
            switch (Global.Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    _combo.OnPostAttack(args.Target);
                    break;
                case OrbwalkingMode.Mixed:
                   _harass.OnPostAttack(args.Target);
                    break;
                case OrbwalkingMode.Laneclear:
                    _laneClear.OnPostAttack();
                    _jungleClear.OnPostAttack((Obj_AI_Minion)args.Target);
                    break;
            }
        }

        public void OnUpdate()
        {
            if (Global.Player.IsDead)
            {
                return;
            }

            _insec?.OnKeyPressed();

            switch (Global.Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    _combo.OnUpdate();
                    break;
                case OrbwalkingMode.Mixed:
                    _harass.OnUpdate();
                    break;
                case OrbwalkingMode.Laneclear:
                    _laneClear.OnUpdate();
                    _jungleClear.OnUpdate();
                    break;
                    case OrbwalkingMode.Lasthit:
                    _lasthit.OnUpdate();
                    break;
            }

            _jungleClear.StealMobs();
        }
    }
}
