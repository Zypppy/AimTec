using System.Linq;
using Adept_AIO.Champions.LeeSin.Core.Insec_Manager;
using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.Champions.LeeSin.Update.Ward_Manager;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.Insec
{
    internal class Insec : IInsec
    {
        public bool Enabled { get; set; }
        public bool QLast { get; set; }
        public bool ObjectEnabled { get; set; }

        private readonly IWardTracker _wardTracker;
        private readonly IWardManager _wardManager;
        private readonly ISpellConfig SpellConfig;
        private readonly IInsec_Manager _insecManager;
     
        public Insec(IWardTracker wardTracker, IWardManager wardManager, ISpellConfig spellConfig, IInsec_Manager insecManager)
        {
            _wardTracker = wardTracker;
            _wardManager = wardManager;
            SpellConfig = spellConfig;
            _insecManager = insecManager;
        }

        private bool WardFlash;

        private static bool _flashReady => SummonerSpells.Flash != null && SummonerSpells.Flash.Ready;

        private bool CanWardJump(Vector3 source)
        {
            var temp = 0;
            if (_wardManager.IsWardReady() && SpellConfig.W.Ready && SpellConfig.IsFirst(SpellConfig.W))
            {
                temp += 600;
            }

            if (_flashReady)
            {
                temp += 425;
            }
            return _insecPosition.Distance(source) <= temp;
        }

        private Vector3 _insecPosition => _insecManager.InsecPosition(target);

        private static Obj_AI_Hero target => Global.TargetSelector.GetSelectedTarget();

        private Obj_AI_Base ObjAiBase => ObjectManager.Get<Obj_AI_Base>().Where(x => CanWardJump(x.ServerPosition) && x.IsEnemy && !x.IsDead && x.Health > Global.Player.GetSpellDamage(x, SpellSlot.Q) && x.MaxHealth > 5 && 
                                                                                     Global.Player.Distance(x) <= SpellConfig.Q.Range)
                                                                                    .OrderBy(x => x.Distance(_insecPosition))
                                                                                    .FirstOrDefault(x => x.Distance(_insecPosition) < Global.Player.Distance(_insecPosition));

        public void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (!Enabled || sender == null || !sender.IsMe || _insecManager.InsecKickValue != 1)
            {
                return;
            }

            if (target == null || args.SpellSlot != SpellSlot.R ||
                _wardManager.IsWardReady() && SpellConfig.IsFirst(SpellConfig.W) ||
                _flashReady && Global.Player.Distance(_insecPosition) <= 175  ||
                Game.TickCount -_wardTracker.LastWardCreated < 500 && !WardFlash)
            {
                return;
            }
          
            SummonerSpells.Flash.Cast(_insecPosition);
        }
            
        public void OnKeyPressed()
        {
            if (target == null || !Enabled)
            {
                return;
            }
           
            if (SpellConfig.Q.Ready && !CanWardJump(Global.Player.ServerPosition))
            {
                Q();
            }

            if (SpellConfig.W.Ready && SpellConfig.IsFirst(SpellConfig.W) && _wardManager.IsWardReady() && !SpellConfig.IsQ2())
            {
                if (_insecPosition.Distance(Global.Player) <= 600)
                {
                    WardFlash = false;
                    _wardManager.WardJump(_insecPosition, false);
                }
                else if (_flashReady && Game.TickCount - DoNotFlash > 1500)
                {
                    if (SpellConfig.HasQ2(target))
                    {
                        return;
                    }

                    if (Game.TickCount - SpellConfig.LastQ1CastAttempt >= 1000 && _insecPosition.Distance(Global.Player) <= SpellConfig.Q.Range + 400)
                    {
                        if (ObjAiBase != null && SpellConfig.HasQ2(ObjAiBase) && _insecPosition.Distance(ObjAiBase.ServerPosition) <= 600 || 
                                                 SpellConfig.HasQ2(target)    && _insecPosition.Distance(target.ServerPosition)    <= 600)
                        {
                            return;
                        }
                            
                        WardFlash = true;
                        _wardManager.WardJump(_insecPosition, true);
                    }                  
                }
            }

            if (!SpellConfig.R.Ready)
            {
                return;
            }
           
            if (_insecManager.InsecKickValue == 0 && _flashReady && _insecPosition.Distance(Global.Player) <= 425 && 
                !(Game.TickCount - _wardTracker.LastWardCreated <= 500 && !WardFlash))
            {
                SummonerSpells.Flash?.Cast(_insecPosition);
            }
        
            if (!target.IsValidTarget(SpellConfig.R.Range) || Global.Player.Distance(_insecPosition) >= (SummonerSpells.Flash != null && SummonerSpells.Flash.Ready ? 425 : 175))
            {
                return;
            }
          
            SpellConfig.R.CastOnUnit(target);
        }

        private int DoNotFlash;

        private void Q()
        {
            if (SpellConfig.IsQ2() && Game.TickCount - SpellConfig.LastQ1CastAttempt >= 500)
            {
                SpellConfig.Q.Cast();
            }
            else if(!SpellConfig.IsQ2())
            {
                if (target.IsValidTarget(SpellConfig.Q.Range))
                {
                    if (SpellConfig.W.Ready && SpellConfig.IsFirst(SpellConfig.W) && _wardManager.IsWardReady() && QLast)
                    {
                        return;
                    }

                    SpellConfig.QSmite(target);
                    SpellConfig.Q.Cast(target);
                    DoNotFlash = Game.TickCount;
                }

                if (!ObjectEnabled || !SpellConfig.R.Ready || ObjAiBase == null)
                {
                    return;
                }

                if (ObjAiBase.Distance(_insecPosition) <= 600)
                {
                    DoNotFlash = Game.TickCount;
                }

                SpellConfig.Q.Cast(ObjAiBase.ServerPosition);
            }
        }
    }
}