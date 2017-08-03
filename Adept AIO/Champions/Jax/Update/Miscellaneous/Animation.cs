using Adept_AIO.Champions.Jax.Core;
using Aimtec;
using Aimtec.SDK.Util;

namespace Adept_AIO.Champions.Jax.Update.Miscellaneous
{
    internal class Animation
    {
        public static void OnPlayAnimation(Obj_AI_Base sender, Obj_AI_BasePlayAnimationEventArgs args)
        {
            if (sender == null || !sender.IsMe)
            {
                return;
            }
        
            switch (args.Animation)
            {
                case "Spell3":
                    SpellConfig.SecondE = false;
                    break;
                case "Spell3b":
                    SpellConfig.SecondE = true;
                    DelayAction.Queue(300, ()=> SpellConfig.SecondE = false);
                    break;
            }
        }
    }
}
