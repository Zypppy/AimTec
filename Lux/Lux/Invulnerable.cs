// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable ArrangeConstructorOrDestructorBody

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace Lux
{
    /// <summary>
    ///     Invulnerable utility class
    /// </summary>
    public class Invulnerable
    {
        #region Static Fields

        /// <summary>
        ///     The invulnerable entries
        /// </summary>
        private static readonly List<InvulnerableEntry> PEntries = new List<InvulnerableEntry>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes the <see cref="Invulnerable" /> class.
        /// </summary>
        static Invulnerable()
        {
            PEntries.AddRange(
                new List<InvulnerableEntry>
                    {
                        new InvulnerableEntry("KindredrNoDeathBuff"),
                        new InvulnerableEntry("BlackShield")
                            {
                                IsShield = true,
                                DamageType = DamageType.Magical
                            },
                        new InvulnerableEntry("BansheesVeil")
                            {
                                IsShield = true,
                                DamageType = DamageType.Magical
                            },
                        new InvulnerableEntry("FioraW")
                            {
                                ChampionName = "Fiora"
                            },
                        new InvulnerableEntry("JaxCounterStrike")
                            {
                                ChampionName = "Jax",
                                IsShield = true,
                                DamageType = DamageType.Physical
                            },
                        new InvulnerableEntry("malzaharpassiveshield")
                            {
                                ChampionName = "Malzahar",
                                IsShield = true
                            },
                        new InvulnerableEntry("NocturneShroudofDarkness")
                            {
                                ChampionName = "Nocturne"
                            },
                        new InvulnerableEntry("OlafRagnarock")
                            {
                                ChampionName = "Olaf",
                                IsShield = true
                            },
                        new InvulnerableEntry("SivirE")
                            {
                                ChampionName = "Sivir"
                            },
                        new InvulnerableEntry("UndyingRage")
                            {
                                ChampionName = "Tryndamere"
                            }
                    });
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     The invulnerable entries
        /// </summary>
        public static ReadOnlyCollection<InvulnerableEntry> Entries => PEntries.AsReadOnly();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Checks if the specified target is invulnerable.
        /// </summary>
        /// <param name="hero">The target.</param>
        /// <param name="damageType">Type of the damage.</param>
        /// <param name="ignoreShields">if set to <c>true</c> [ignore shields].</param>
        /// <param name="damage">The damage.</param>
        /// <returns></returns>
        public static bool Check(
            Obj_AI_Hero hero,
            DamageType damageType = DamageType.True,
            bool ignoreShields = true,
            float damage = -1f)
        {
            if (hero.ValidActiveBuffs().Any(b => b.Type == BuffType.Invulnerability))
            {
                return true;
            }

            foreach (var entry in Entries)
            {
                if (entry.ChampionName == null || entry.ChampionName == hero.ChampionName)
                {
                    if (entry.DamageType == null || entry.DamageType == damageType)
                    {
                        if (hero.HasBuff(entry.BuffName))
                        {
                            if (!ignoreShields || !entry.IsShield)
                            {
                                if (entry.CheckFunction == null || ExecuteCheckFunction(entry, hero, damageType))
                                {
                                    if (entry.MinHealthPercent <= 0 || hero.HealthPercent() < entry.MinHealthPercent)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     Deregisters the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public static void Deregister(InvulnerableEntry entry)
        {
            if (PEntries.Any(i => i.BuffName.Equals(entry.BuffName)))
            {
                PEntries.Remove(entry);
            }
        }

        /// <summary>
        ///     Gets the item.
        /// </summary>
        /// <param name="buffName">Name of the buff.</param>
        /// <param name="stringComparison">The string comparison.</param>
        /// <returns></returns>
        public static InvulnerableEntry GetItem(
            string buffName,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return PEntries.FirstOrDefault(w => w.BuffName.Equals(buffName, stringComparison));
        }

        /// <summary>
        ///     Registers the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public static void Register(InvulnerableEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.BuffName) && !PEntries.Any(i => i.BuffName.Equals(entry.BuffName)))
            {
                PEntries.Add(entry);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Executes the check function.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="hero">The target.</param>
        /// <param name="damageType">Type of the damage.</param>
        /// <returns></returns>
        private static bool ExecuteCheckFunction(InvulnerableEntry entry, Obj_AI_Hero hero, DamageType damageType)
        {
            return entry != null && entry.CheckFunction(hero, damageType);
        }

        #endregion
    }

    /// <summary>
    ///     Entry for <see cref="Invulnerable" /> class.
    /// </summary>
    public class InvulnerableEntry
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvulnerableEntry" /> class.
        /// </summary>
        /// <param name="buffName">Name of the buff.</param>
        public InvulnerableEntry(string buffName)
        {
            BuffName = buffName;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the name of the buff.
        /// </summary>
        /// <value>
        ///     The name of the buff.
        /// </value>
        public string BuffName { get; set; }

        /// <summary>
        ///     Gets or sets the champion name.
        /// </summary>
        /// <value>
        ///     The champion name.
        /// </value>
        public string ChampionName { get; set; }

        /// <summary>
        ///     Gets or sets the check function.
        /// </summary>
        /// <value>
        ///     The check function.
        /// </value>
        public Func<Obj_AI_Base, DamageType, bool> CheckFunction { get; set; }

        /// <summary>
        ///     Gets or sets the type of the damage.
        /// </summary>
        /// <value>
        ///     The type of the damage.
        /// </value>
        public DamageType? DamageType { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this is a shield.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this is a shield; otherwise, <c>false</c>.
        /// </value>
        public bool IsShield { get; set; }

        /// <summary>
        ///     Gets or sets the minimum health percent.
        /// </summary>
        /// <value>
        ///     The minimum health percent.
        /// </value>
        public int MinHealthPercent { get; set; }

        #endregion
    }
}