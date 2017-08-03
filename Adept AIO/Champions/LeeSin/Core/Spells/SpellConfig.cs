using System.Linq;
using Adept_AIO.SDK.Extensions;
using Adept_AIO.SDK.Usables;
using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Prediction.Skillshots;
using Spell = Aimtec.SDK.Spell;

namespace Adept_AIO.Champions.LeeSin.Core.Spells
{
    internal class SpellConfig : ISpellConfig 
    {
        public float LastQ1CastAttempt { get; set; }

        public bool QAboutToEnd => Game.TickCount - LastQ1CastAttempt >= 1900 + Game.Ping / 2f;

        public bool IsQ2()
        {
            return !IsFirst(Q) && Q.Ready;
        }

        public bool IsFirst(Spell spell)
        {
            return Global.Player
                .SpellBook.GetSpell(spell.Slot)
                .SpellData.Name.ToLower()
                .Contains("one");
        }

        public bool HasQ2(Obj_AI_Base target)
        {
            return target.HasBuff("BlindMonkSonicWave");
        }

        /// <summary>
        /// [BETA]
        /// </summary>
        /// <param name="target"></param>
        public void QSmite(Obj_AI_Base target)
        {
          
            var list = Q.GetPrediction(target).CollisionObjects;
            var first = list.FirstOrDefault();

            if (SummonerSpells.Smite == null || !SummonerSpells.Smite.Ready || SummonerSpells.Ammo("Smite") < 2 ||
                list.Count < 1 || list[0] == target || first == null ||
                first.Health > SummonerSpells.SmiteMonsters() ||
                first.ServerPosition.Distance(Global.Player) > SummonerSpells.Smite.Range)
            {
                return;
            }

            SummonerSpells.Smite.CastOnUnit(first);
            Global.Player.SpellBook.CastSpell(SpellSlot.Q, target.ServerPosition);
        }

        public Spell W { get; private set; }
        public Spell Q { get; private set; }
        public Spell E { get; private set; }
        public Spell R { get; private set; }

        public OrbwalkerMode InsecMode { get; set; }
        public OrbwalkerMode WardjumpMode { get; set; }
        public OrbwalkerMode KickFlashMode { get; set; }

        private const string PassiveName = "blindmonkpassive_cosmetic";

        public int PassiveStack()
        {
           return Global.Player.HasBuff(PassiveName) ? Global.Player.GetBuffCount(PassiveName) : 0;
        }

        public void Load()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            Q.SetSkillshot(0.25f, 65, 1800, true, SkillshotType.Line);

            W = new Spell(SpellSlot.W, 700);
         
            E = new Spell(SpellSlot.E, 350);

            R = new Spell(SpellSlot.R, 375);
        }

        public void OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender == null || !sender.IsMe)
            {
                return;
            }

            if (args.SpellSlot == SpellSlot.Q && args.SpellData.Name.ToLower().Contains("one"))
            {
                LastQ1CastAttempt = Game.TickCount;
            }
        }
    }
}
