using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Aimtec;

namespace Gangplank
{
    /// <summary>
    ///     A static (stack) class which contains a sort-of cached versions of the important game objects.
    /// </summary>
    public static class GameObjects
    {
        #region Static Fields

        /// <summary>
        ///     The ally heroes list.
        /// </summary>
        private static readonly List<Obj_AI_Hero> AllyHeroesList = new List<Obj_AI_Hero>();

        /// <summary>
        ///     The ally list.
        /// </summary>
        private static readonly List<Obj_AI_Base> AllyList = new List<Obj_AI_Base>();

        /// <summary>
        ///     The ally minions list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> AllyMinionsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The ally turrets list.
        /// </summary>
        private static readonly List<Obj_AI_Turret> AllyTurretsList = new List<Obj_AI_Turret>();

        /// <summary>
        ///     The ally wards list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> AllyWardsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The attackable unit list.
        /// </summary>
        private static readonly List<AttackableUnit> AttackableUnitsList = new List<AttackableUnit>();

        /// <summary>
        ///     The enemy heroes list.
        /// </summary>
        private static readonly List<Obj_AI_Hero> EnemyHeroesList = new List<Obj_AI_Hero>();

        /// <summary>
        ///     The enemy list.
        /// </summary>
        private static readonly List<Obj_AI_Base> EnemyList = new List<Obj_AI_Base>();

        /// <summary>
        ///     The enemy minions list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> EnemyMinionsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The enemy turrets list.
        /// </summary>
        private static readonly List<Obj_AI_Turret> EnemyTurretsList = new List<Obj_AI_Turret>();

        /// <summary>
        ///     The enemy wards list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> EnemyWardsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The game objects list.
        /// </summary>
        private static readonly List<GameObject> GameObjectsList = new List<GameObject>();

        /// <summary>
        ///     The heroes list.
        /// </summary>
        private static readonly List<Obj_AI_Hero> HeroesList = new List<Obj_AI_Hero>();

        /// <summary>
        ///     The jungle large list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> JungleLargeList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The jungle legendary list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> JungleLegendaryList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The jungle list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> JungleList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The jungle small list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> JungleSmallList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The spawn points list.
        /// </summary>
        private static readonly List<GameObject> SpawnPointsList = new List<GameObject>();

        /// <summary>
        ///     The ally spawn points list.
        /// </summary>
        private static readonly List<GameObject> AllySpawnPointsList = new List<GameObject>();

        /// <summary>
        ///     The enemy spawn points list.
        /// </summary>
        private static readonly List<GameObject> EnemySpawnPointsList = new List<GameObject>();

        /// <summary>
        ///     The large name regex list.
        /// </summary>
        private static readonly string[] LargeNameRegex =
            {
                "SRU_Murkwolf[0-9.]{1,}", "SRU_Gromp", "SRU_Blue[0-9.]{1,}",
                "SRU_Razorbeak[0-9.]{1,}", "SRU_Red[0-9.]{1,}",
                "SRU_Krug[0-9]{1,}"
            };

        /// <summary>
        ///     The legendary name regex list.
        /// </summary>
        private static readonly string[] LegendaryNameRegex = { "SRU_Dragon", "SRU_Baron", "SRU_RiftHerald" };

        /// <summary>
        ///     The minions list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> MinionsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     The small name regex list.
        /// </summary>
        private static readonly string[] SmallNameRegex = { "SRU_[a-zA-Z](.*?)Mini", "Sru_Crab" };

        /// <summary>
        ///     The turrets list.
        /// </summary>
        private static readonly List<Obj_AI_Turret> TurretsList = new List<Obj_AI_Turret>();

        /// <summary>
        ///     The wards list.
        /// </summary>
        private static readonly List<Obj_AI_Minion> WardsList = new List<Obj_AI_Minion>();

        /// <summary>
        ///     Indicates whether the <see cref="GameObjects" /> stack was initialized and saved required instances.
        /// </summary>
        private static bool _initialized;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="GameObjects" /> class.
        /// </summary>
        static GameObjects()
        {
            Initialize();
        }

        #endregion

        #region Enums

        /// <summary>
        ///     The jungle mob types.
        /// </summary>
        public enum JungleType
        {
            /// <summary>
            ///     The unknown type.
            /// </summary>
            Unknown,

            /// <summary>
            ///     The small type.
            /// </summary>
            Small,

            /// <summary>
            ///     The large type.
            /// </summary>
            Large,

            /// <summary>
            ///     The legendary type.
            /// </summary>
            Legendary
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the game objects.
        /// </summary>
        public static IEnumerable<GameObject> AllGameObjects => GameObjectsList;

        /// <summary>
        ///     Gets the ally.
        /// </summary>
        public static IEnumerable<Obj_AI_Base> Ally => AllyList;

        /// <summary>
        ///     Gets the ally heroes.
        /// </summary>
        public static IEnumerable<Obj_AI_Hero> AllyHeroes => AllyHeroesList;

        /// <summary>
        ///     Gets the ally minions.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> AllyMinions => AllyMinionsList;

        /// <summary>
        ///     Gets the ally turrets.
        /// </summary>
        public static IEnumerable<Obj_AI_Turret> AllyTurrets => AllyTurretsList;

        /// <summary>
        ///     Gets the ally wards.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> AllyWards => AllyWardsList;

        /// <summary>
        ///     Gets the attackable units.
        /// </summary>
        public static IEnumerable<AttackableUnit> AttackableUnits => AttackableUnitsList;

        /// <summary>
        ///     Gets the enemy.
        /// </summary>
        public static IEnumerable<Obj_AI_Base> Enemy => EnemyList;

        /// <summary>
        ///     Gets the enemy heroes.
        /// </summary>
        public static IEnumerable<Obj_AI_Hero> EnemyHeroes => EnemyHeroesList;

        /// <summary>
        ///     Gets the enemy minions.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> EnemyMinions => EnemyMinionsList;

        /// <summary>
        ///     Gets the enemy turrets.
        /// </summary>
        public static IEnumerable<Obj_AI_Turret> EnemyTurrets => EnemyTurretsList;

        /// <summary>
        ///     Gets the enemy wards.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> EnemyWards => EnemyWardsList;

        /// <summary>
        ///     Gets the heroes.
        /// </summary>
        public static IEnumerable<Obj_AI_Hero> Heroes => HeroesList;

        /// <summary>
        ///     Gets the jungle.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> Jungle => JungleList;

        /// <summary>
        ///     Gets the jungle large.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> JungleLarge => JungleLargeList;

        /// <summary>
        ///     Gets the jungle legendary.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> JungleLegendary => JungleLegendaryList;

        /// <summary>
        ///     Gets the jungle small.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> JungleSmall => JungleSmallList;

        /// <summary>
        ///     Gets the minions.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> Minions => MinionsList;

        /// <summary>
        ///     Gets or sets the player.
        /// </summary>
        public static Obj_AI_Hero Player { get; set; }

        /// <summary>
        ///     Gets the turrets.
        /// </summary>
        public static IEnumerable<Obj_AI_Turret> Turrets => TurretsList;

        /// <summary>
        ///     Gets the wards.
        /// </summary>
        public static IEnumerable<Obj_AI_Minion> Wards => WardsList;

        /// <summary>
        ///     Gets the spawn points.
        /// </summary>
        public static IEnumerable<GameObject> SpawnPoints => SpawnPointsList;

        /// <summary>
        ///     Gets the ally spawn points.
        /// </summary>
        public static IEnumerable<GameObject> AllySpawnPoints => AllySpawnPointsList;

        /// <summary>
        ///     Gets the enemy spawn points.
        /// </summary>
        public static IEnumerable<GameObject> EnemySpawnPoints => EnemySpawnPointsList;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Compares two <see cref="GameObject" /> and returns if they are identical.
        /// </summary>
        /// <param name="gameObject">The GameObject</param>
        /// <param name="object">The Compare GameObject</param>
        /// <returns>Whether the <see cref="GameObject" />s are identical.</returns>
        public static bool Compare(this GameObject gameObject, GameObject @object)
        {
            return gameObject != null && gameObject.IsValid && @object != null && @object.IsValid
                   && gameObject.NetworkId == @object.NetworkId;
        }

        /// <summary>
        ///     The get operation from the GameObjects stack.
        /// </summary>
        /// <typeparam name="T">
        ///     The requested <see cref="GameObject" /> type.
        /// </typeparam>
        /// <returns>
        ///     The List containing the requested type.
        /// </returns>
        public static IEnumerable<T> Get<T>()
            where T : GameObject, new()
        {
            return AllGameObjects.OfType<T>();
        }

        /// <summary>
        ///     Get the minion jungle type.
        /// </summary>
        /// <param name="minion">
        ///     The minion
        /// </param>
        /// <returns>
        ///     The <see cref="JungleType" />
        /// </returns>
        public static JungleType GetJungleType(this Obj_AI_Minion minion)
        {
            if (SmallNameRegex.Any(regex => Regex.IsMatch(minion.Name, regex)))
            {
                return JungleType.Small;
            }

            if (LargeNameRegex.Any(regex => Regex.IsMatch(minion.Name, regex)))
            {
                return JungleType.Large;
            }

            if (LegendaryNameRegex.Any(regex => Regex.IsMatch(minion.Name, regex)))
            {
                return JungleType.Legendary;
            }

            return JungleType.Unknown;
        }

        /// <summary>
        ///     Get get operation from the native GameObjects stack.
        /// </summary>
        /// <typeparam name="T">
        ///     The requested <see cref="GameObject" /> type.
        /// </typeparam>
        /// <returns>
        ///     The List containing the requested type.
        /// </returns>
        public static IEnumerable<T> GetNative<T>()
            where T : GameObject, new()
        {
            return ObjectManager.Get<T>();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The initialize method.
        /// </summary>
        internal static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            Player = Player;

            HeroesList.AddRange(ObjectManager.Get<Obj_AI_Hero>());
            MinionsList.AddRange(ObjectManager.Get<Obj_AI_Minion>().Where(o => o.Team != GameObjectTeam.Neutral && !o.Name.Contains("ward")));
            TurretsList.AddRange(ObjectManager.Get<Obj_AI_Turret>());
            JungleList.AddRange(ObjectManager.Get<Obj_AI_Minion>().Where(o => o.Team == GameObjectTeam.Neutral && o.Name != "WardCorpse" && o.Name != "Barrel" && !o.Name.Contains("SRU_Plant_")));
            WardsList.AddRange(ObjectManager.Get<Obj_AI_Minion>().Where(o => o.Name.Contains("ward")));
            SpawnPointsList.AddRange(ObjectManager.Get<GameObject>().Where(o => o.Type == GameObjectType.obj_SpawnPoint));

            GameObjectsList.AddRange(ObjectManager.Get<GameObject>());
            AttackableUnitsList.AddRange(ObjectManager.Get<AttackableUnit>());

            EnemyHeroesList.AddRange(HeroesList.Where(o => o.IsEnemy));
            EnemyMinionsList.AddRange(MinionsList.Where(o => o.IsEnemy));
            EnemyTurretsList.AddRange(TurretsList.Where(o => o.IsEnemy));
            EnemyList.AddRange(EnemyHeroesList.Cast<Obj_AI_Base>().Concat(EnemyMinionsList).Concat(EnemyTurretsList));

            AllyHeroesList.AddRange(HeroesList.Where(o => o.IsAlly));
            AllyMinionsList.AddRange(MinionsList.Where(o => o.IsAlly));
            AllyTurretsList.AddRange(TurretsList.Where(o => o.IsAlly));
            AllyList.AddRange(
                AllyHeroesList.Cast<Obj_AI_Base>().Concat(AllyMinionsList).Concat(AllyTurretsList));

            JungleSmallList.AddRange(JungleList.Where(o => o.GetJungleType() == JungleType.Small));
            JungleLargeList.AddRange(JungleList.Where(o => o.GetJungleType() == JungleType.Large));
            JungleLegendaryList.AddRange(JungleList.Where(o => o.GetJungleType() == JungleType.Legendary));

            AllyWardsList.AddRange(WardsList.Where(o => o.IsAlly));
            EnemyWardsList.AddRange(WardsList.Where(o => o.IsEnemy));

            AllySpawnPointsList.AddRange(SpawnPointsList.Where(o => o.IsAlly));
            EnemySpawnPointsList.AddRange(SpawnPointsList.Where(o => o.IsEnemy));

            GameObject.OnCreate += OnCreate;
            GameObject.OnDestroy += OnDelete;
        }

        /// <summary>
        ///     OnCreate event.
        /// </summary>
        /// <param name="sender">
        ///     The sender
        /// </param>
        private static void OnCreate(GameObject sender)
        {
            GameObjectsList.Add(sender);

            var attackableUnit = sender as AttackableUnit;
            if (attackableUnit != null)
            {
                AttackableUnitsList.Add(attackableUnit);
            }

            var hero = sender as Obj_AI_Hero;
            if (hero != null)
            {
                HeroesList.Add(hero);
                if (hero.IsEnemy)
                {
                    EnemyHeroesList.Add(hero);
                    EnemyList.Add(hero);
                }
                else
                {
                    AllyHeroesList.Add(hero);
                    AllyList.Add(hero);
                }

                return;
            }

            var minion = sender as Obj_AI_Minion;
            if (minion != null)
            {
                if (minion.Team != GameObjectTeam.Neutral)
                {
                    if (minion.Name.Contains("ward"))
                    {
                        WardsList.Add(minion);
                        if (minion.IsEnemy)
                        {
                            EnemyWardsList.Add(minion);
                        }
                        else
                        {
                            AllyWardsList.Add(minion);
                        }
                    }
                    else
                    {
                        MinionsList.Add(minion);
                        if (minion.IsEnemy)
                        {
                            EnemyMinionsList.Add(minion);
                            EnemyList.Add(minion);
                        }
                        else
                        {
                            AllyMinionsList.Add(minion);
                            AllyList.Add(minion);
                        }
                    }
                }
                else if (minion.Name != "WardCorpse" && minion.Name != "Barrel")
                {
                    JungleList.Add(minion);
                    switch (minion.GetJungleType())
                    {
                        case JungleType.Small:
                            JungleSmallList.Add(minion);
                            break;
                        case JungleType.Large:
                            JungleLargeList.Add(minion);
                            break;
                        case JungleType.Legendary:
                            JungleLegendaryList.Add(minion);
                            break;
                    }
                }

                return;
            }

            var turret = sender as Obj_AI_Turret;
            if (turret != null)
            {
                TurretsList.Add(turret);
                if (turret.IsEnemy)
                {
                    EnemyTurretsList.Add(turret);
                    EnemyList.Add(turret);
                }
                else
                {
                    AllyTurretsList.Add(turret);
                    AllyList.Add(turret);
                }
            }

            var spawnPoint = sender;
            if (spawnPoint.Type == GameObjectType.obj_SpawnPoint)
            {
                SpawnPointsList.Add(spawnPoint);
                if (spawnPoint.IsAlly)
                {
                    AllySpawnPointsList.Add(spawnPoint);
                }
                else
                {
                    EnemySpawnPointsList.Add(spawnPoint);
                }
            }
        }

        /// <summary>
        ///     OnDelete event.
        /// </summary>
        /// <param name="sender">
        ///     The sender
        /// </param>
        private static void OnDelete(GameObject sender)
        {
            foreach (var gameObject in GameObjectsList.Where(o => o.Compare(sender)).ToList())
            {
                GameObjectsList.Remove(gameObject);
            }

            var attackableUnit = sender as AttackableUnit;
            if (attackableUnit != null)
            {
                foreach (var attackableUnitObject in AttackableUnitsList.Where(a => a.Compare(attackableUnit)).ToList())
                {
                    AttackableUnitsList.Remove(attackableUnitObject);
                }
            }

            var hero = sender as Obj_AI_Hero;
            if (hero != null)
            {
                foreach (var heroObject in HeroesList.Where(h => h.Compare(hero)).ToList())
                {
                    HeroesList.Remove(heroObject);
                    if (hero.IsEnemy)
                    {
                        EnemyHeroesList.Remove(heroObject);
                        EnemyList.Remove(heroObject);
                    }
                    else
                    {
                        AllyHeroesList.Remove(heroObject);
                        AllyList.Remove(heroObject);
                    }
                }

                return;
            }

            var minion = sender as Obj_AI_Minion;
            if (minion != null)
            {
                if (minion.Team != GameObjectTeam.Neutral)
                {
                    if (minion.Name.Contains("ward"))
                    {
                        foreach (var wardObject in WardsList.Where(w => w.Compare(minion)).ToList())
                        {
                            WardsList.Remove(wardObject);
                            if (minion.IsEnemy)
                            {
                                EnemyWardsList.Remove(wardObject);
                            }
                            else
                            {
                                AllyWardsList.Remove(wardObject);
                            }
                        }
                    }
                    else
                    {
                        foreach (var minionObject in MinionsList.Where(m => m.Compare(minion)).ToList())
                        {
                            MinionsList.Remove(minionObject);
                            if (minion.IsEnemy)
                            {
                                EnemyMinionsList.Remove(minionObject);
                                EnemyList.Remove(minionObject);
                            }
                            else
                            {
                                AllyMinionsList.Remove(minionObject);
                                AllyList.Remove(minionObject);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var jungleObject in JungleList.Where(j => j.Compare(minion)).ToList())
                    {
                        JungleList.Remove(jungleObject);
                        switch (jungleObject.GetJungleType())
                        {
                            case JungleType.Small:
                                JungleSmallList.Remove(jungleObject);
                                break;
                            case JungleType.Large:
                                JungleLargeList.Remove(jungleObject);
                                break;
                            case JungleType.Legendary:
                                JungleLegendaryList.Remove(jungleObject);
                                break;
                        }
                    }
                }

                return;
            }

            var turret = sender as Obj_AI_Turret;
            if (turret != null)
            {
                foreach (var turretObject in TurretsList.Where(t => t.Compare(turret)).ToList())
                {
                    TurretsList.Remove(turretObject);
                    if (turret.IsEnemy)
                    {
                        EnemyTurretsList.Remove(turretObject);
                        EnemyList.Remove(turretObject);
                    }
                    else
                    {
                        AllyTurretsList.Remove(turretObject);
                        AllyList.Remove(turretObject);
                    }
                }
            }

            var spawnPoint = sender;
            if (spawnPoint.Type == GameObjectType.obj_SpawnPoint)
            {
                foreach (var spawnPointObject in SpawnPointsList.Where(s => s.Compare(spawnPoint)).ToList())
                {
                    SpawnPointsList.Remove(spawnPointObject);
                    if (spawnPoint.IsEnemy)
                    {
                        EnemySpawnPointsList.Remove(spawnPointObject);
                    }
                    else
                    {
                        AllySpawnPointsList.Remove(spawnPointObject);
                    }
                }
            }
        }

        #endregion
    }
}