using System.Linq;
using Adept_AIO.SDK.Extensions;
using Aimtec;
using Spell = Aimtec.SDK.Spell;

namespace Adept_AIO.SDK.Usables
{
    internal class SummonerSpells
    {
        public static Spell Flash, Ignite, Smite, Exhaust;

        //Todo: Improve this bullshit.
        public static void Init()
        {
            var spellbookName1 = Global.Player.SpellBook.GetSpell(SpellSlot.Summoner1).Name.ToLower();
            var spellbookName2 = Global.Player.SpellBook.GetSpell(SpellSlot.Summoner2).Name.ToLower();

            switch (spellbookName1)
            {
                case "summonerflash":
                    Flash = new Spell(SpellSlot.Summoner1, 425);
                    break;
                case "summonerdot":
                    Ignite = new Spell(SpellSlot.Summoner1, 600);
                    break;
                case "summonerexhaust":
                    Exhaust = new Spell(SpellSlot.Summoner1, 650);
                    break;
                case "summonersmite":
                    Smite = new Spell(SpellSlot.Summoner1, 700);
                    break;
            }

            switch (spellbookName2)
            {
                case "summonerflash":
                    Flash = new Spell(SpellSlot.Summoner2, 425);
                    break;
                case "summonerdot":
                    Ignite = new Spell(SpellSlot.Summoner2, 600);
                    break;
                case "summonerexhaust":
                    Exhaust = new Spell(SpellSlot.Summoner2, 650);
                    break;
                case "summonersmite":
                    Smite = new Spell(SpellSlot.Summoner2, 700);
                    break;
            }
        }

        public static int IgniteDamage(Obj_AI_Base target)
        {
            return (int)(50 + 20 * Global.Player.Level - target.HPRegenRate / 5 * 3);
        }

        public static int SmiteMonsters()
        {
            var level = Global.Player.Level;

            var index = level / 5;

            int[] damage = { 370 + 20 * level, 330 + 30 * level, 240 + 40 * level, 100 + 50 * level };

            return damage[index];
        }

        public static int SmiteChampions()
        {
            int[] Dmg = { 28, 36, 44, 52, 60, 68, 76, 84, 92, 100, 108, 116, 124, 132, 140, 148, 156, 166 };

            return Dmg[Global.Player.Level - 1];
        }

        public static int Ammo(string summonerName)
        {
            var spell = Global.Player.SpellBook.Spells.FirstOrDefault(x => x.Name.Contains(summonerName));
            return spell?.Ammo ?? 0;
        }
    }
}
