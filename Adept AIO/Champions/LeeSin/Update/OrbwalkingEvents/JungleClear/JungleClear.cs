using System.Linq;
using Adept_AIO.Champions.LeeSin.Core.Spells;
using Adept_AIO.Champions.LeeSin.Update.Ward_Manager;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Damage.JSON;
using Aimtec.SDK.Extensions;

namespace Adept_AIO.Champions.LeeSin.Update.OrbwalkingEvents.JungleClear
{
    internal class JungleClear : IJungleClear
    {
        public bool StealEnabled { get; set; }
        public bool SmiteEnabled { get; set; }
        public bool BlueEnabled { get; set; }
        public bool Q1Enabled { get; set; }
        public bool WEnabled { get; set; }
        public bool EEnabled { get; set; }

        private readonly IWardManager _wardManager;
        private readonly ISpellConfig SpellConfig;

        public JungleClear(IWardManager wardManager, ISpellConfig spellConfig)
        {
            _wardManager = wardManager;
            SpellConfig = spellConfig;
        }

        public void OnPostAttack(Obj_AI_Minion mob)
        {
            if (mob == null || mob.Health < Global.Player.GetAutoAttackDamage(mob))
            {
                return;
            }

            if (SpellConfig.Q.Ready && SpellConfig.IsQ2() && SpellConfig.QAboutToEnd)
            {
                Global.Player.SpellBook.CastSpell(SpellSlot.Q);
            }

            if (Global.Player.Level <= 12)
            {
                if (SpellConfig.PassiveStack() > 0)
                {
                    return;
                }
                if (SpellConfig.W.Ready && WEnabled && !SpellConfig.Q.Ready)
                {
                    SpellConfig.W.CastOnUnit(Global.Player);
                }
                else if (SpellConfig.E.Ready && EEnabled && !SpellConfig.W.Ready)
                {
                    if (SpellConfig.IsFirst(SpellConfig.E))
                    {
                        SpellConfig.E.Cast(mob);
                    }
                   else if (SpellConfig.W.Ready || SpellConfig.Q.Ready)
                    {
                        return;
                    }
                    SpellConfig.E.Cast();
                }
            }
            else 
            {
                if (SpellConfig.E.Ready && EEnabled)
                {
                    SpellConfig.E.Cast(mob);
                }
                else if (SpellConfig.W.Ready && WEnabled && !SpellConfig.IsQ2())
                {
                    SpellConfig.W.CastOnUnit(Global.Player);
                }
            }
        }

        public void OnUpdate()
        {
            if (!SpellConfig.Q.Ready || !Q1Enabled)
            {
                return;
            }

            var mob = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.Distance(Global.Player) < SpellConfig.Q.Range / 2 && x.GetJungleType() != GameObjects.JungleType.Unknown && x.MaxHealth > 5);

            if (mob == null)
            {
                return;
            }

            if (!SmiteOptional.Contains(mob.UnitSkinName) && !SmiteAlways.Contains(mob.UnitSkinName))
            {
                return;
            }

            if (SpellConfig.Q.Ready && SpellConfig.IsQ2() && mob.Health < Global.Player.GetSpellDamage(mob, SpellSlot.Q, DamageStage.SecondCast))
            {
                Global.Player.SpellBook.CastSpell(SpellSlot.Q);
            }

            if (!SpellConfig.IsQ2() && Game.TickCount - SpellConfig.LastQ1CastAttempt > 500)
            {
                Global.Player.SpellBook.CastSpell(SpellSlot.Q, mob.Position);
            }
        }

        private readonly Vector3[] Positions =
        {
            new Vector3(5740, 56, 10629),
            new Vector3(5808, 54, 10319),
            new Vector3(5384, 57, 11282),
            new Vector3(9076, 53, 4446),
            new Vector3(9058, 53, 4117),
            new Vector3(9687, 56, 3490)
        };

        private double StealDamage(Obj_AI_Base mob)
        {
           return SummonerSpells.SmiteMonsters() + (SpellConfig.IsQ2() ? Global.Player.GetSpellDamage(mob, SpellSlot.Q, DamageStage.SecondCast) : 0);
        }

        private readonly string[] SmiteAlways = { "SRU_Dragon_Air", "SRU_Dragon_Fire", "SRU_Dragon_Earth", "SRU_Dragon_Water", "SRU_Dragon_Elder", "SRU_Baron", "SRU_RiftHerald" };
        private readonly string[] SmiteOptional = {"Sru_Crab", "SRU_Razorbeak", "SRU_Krug", "SRU_Murkwolf", "SRU_Gromp", "SRU_Blue", "SRU_Red"};
        private float Q2Time;

        public void StealMobs()
        {
            var smiteAbleMob = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.Distance(Global.Player) < 1300);

            if (smiteAbleMob != null && (SmiteAlways.Contains(smiteAbleMob.UnitSkinName) || SmiteOptional.Contains(smiteAbleMob.UnitSkinName)))
            {
                if (smiteAbleMob.Health < StealDamage(smiteAbleMob))
                {
                    if (SmiteOptional.Contains(smiteAbleMob.UnitSkinName) && SummonerSpells.Ammo("Smite") <= 1 || 
                        smiteAbleMob.UnitSkinName.ToLower().Contains("blue") && !BlueEnabled ||
                        smiteAbleMob.UnitSkinName.ToLower().Contains("red") && Global.Player.HealthPercent() <= 75)
                    {
                        return;
                    }

                    if (SmiteOptional.Contains(smiteAbleMob.UnitSkinName) &&
                        Global.Player.HealthPercent() >= 70)
                    {
                        return;
                    }

                    if (SpellConfig.IsQ2())
                    {
                        SpellConfig.Q.Cast();
                    }

                    if (SmiteEnabled && SummonerSpells.Smite != null && SummonerSpells.Smite.Ready)
                    {
                        SummonerSpells.Smite.CastOnUnit(smiteAbleMob);
                    }
                }
            }

            var mob = GameObjects.JungleLegendary.FirstOrDefault(x => x.Distance(Global.Player) <= 1500);
          
            if (mob == null || !SmiteEnabled)
            {
                return;
            }
          
            if (Q2Time > 0 && Game.TickCount - Q2Time <= 1500 && SummonerSpells.Smite != null && SummonerSpells.Smite.Ready && StealDamage(mob) > mob.Health)
            {
                if (SpellConfig.W.Ready && SpellConfig.IsFirst(SpellConfig.W) && Global.Player.Distance(mob) <= 500)
                {
                    SummonerSpells.Smite.CastOnUnit(mob);
                    _wardManager.WardJump(Positions.FirstOrDefault(), false);
                }
            }

            if (mob.Position.CountAllyHeroesInRange(700) <= 1 && SpellConfig.Q.Ready && SpellConfig.IsQ2() && StealDamage(mob) > mob.Health)
            {
                SpellConfig.Q.Cast();
                Q2Time = Game.TickCount;
            }
        }
    }
}
