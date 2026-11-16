using System;
using Manager;
using Steamworks;
using Steamworks.NET;
using UnityEngine;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        public Action OnInitialize;

        public LevelSystem levelSystem = null;
        public LevelSystemAnimated levelSystemAnimated = null;

        /// <summary>
        /// Experience earned before the stored value was available. It is applied to
        /// the level as soon as the level system exists, so nothing is lost when the
        /// Steam stats arrive late.
        /// </summary>
        public int expWaitToAdd = 0;

        public Color[] colors = new Color[10];

        public static bool Initialized = false;

        /// <summary>How long to wait before asking Steam for the stats again.</summary>
        private const float RefreshRetryInterval = 0.5f;

        /// <summary>How long to wait before retrying a failed experience upload.</summary>
        private const float StoreRetryInterval = 2f;

        /// <summary>
        /// Total experience that still has to reach Steam, -1 when everything is
        /// stored. Keeping it until the upload succeeds is what stops a match's
        /// experience from silently disappearing on a crash or a force quit.
        /// </summary>
        private int _expToStore = -1;

        /// <summary>
        /// True once the stored experience was read from Steam. Until then nothing is
        /// written back: uploading a value derived from a failed read would overwrite
        /// the real progress with zero.
        /// </summary>
        private bool _statsLoaded;

        private float _nextRefreshTime;
        private float _nextStoreTime;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
            {
                return;
            }
        }

        private void Start()
        {
            if (Instance != this) return;

            //Debug.Log(LevelUtils.GetExpFromLevel(44));
        }

        private void Update()
        {
            if (Instance != this) return;

            if (!Initialized)
            {
                if (Time.realtimeSinceStartup < _nextRefreshTime) return;
                _nextRefreshTime = Time.realtimeSinceStartup + RefreshRetryInterval;
                Refresh();
                return;
            }

            if (_expToStore >= 0)
                StoreExp();
        }

        /// <summary>
        /// Reads the stored experience and builds the level system. Failed or late
        /// reads are retried from <see cref="Update"/>, so a level is never created
        /// from a read that did not return the player's real progress.
        /// </summary>
        public void Refresh()
        {
            if (Initialized) return;

            if (!SteamManager.Initialized)
            {
                // Without Steam there is no stored experience. Keep the session usable
                // but never write, so a later online session keeps its real level.
                CreateLevelSystem(0);
                return;
            }

            if (!SteamUserStats.GetStat("EXPERIENCE", out int exp))
            {
                // The user's stats have not been received yet.
                SteamUserStats.RequestCurrentStats();
                return;
            }

            _statsLoaded = true;
            CreateLevelSystem(Mathf.Max(0, exp));
        }

        private void CreateLevelSystem(int exp)
        {
            levelSystem = new LevelSystem(exp);
            levelSystemAnimated = new LevelSystemAnimated(levelSystem);

            Initialized = true;

            OnInitialize?.Invoke();

            // Experience gained while the stats were still loading still counts.
            if (expWaitToAdd != 0) StoreExp();
        }

        /// <summary>
        /// Adds experience and stores it right away, so progress survives the game
        /// being closed (or crashing) in the middle of a match.
        /// </summary>
        public void AddExp(int amount)
        {
            if (amount == 0) return;

            if (levelSystem == null)
            {
                expWaitToAdd += amount;
                return;
            }

            levelSystem.AddExperience(amount);
            _expToStore = levelSystem.GetExp();
            StoreExp();
        }

        /// <summary>
        /// Writes the current experience to Steam. When the upload fails the value is
        /// remembered and retried instead of being dropped.
        /// </summary>
        public void StoreExp()
        {
            if (levelSystem == null) return;

            if (expWaitToAdd != 0)
            {
                int pending = expWaitToAdd;
                expWaitToAdd = 0;
                levelSystem.AddExperience(pending);
                _expToStore = levelSystem.GetExp();
            }

            if (_expToStore < 0) return;
            if (!_statsLoaded || !SteamManager.Initialized) return;
            if (AchievementManager.Instance == null) return;
            if (Time.realtimeSinceStartup < _nextStoreTime) return;

            // Steam rejects uploads that are older than the server's value and replaces
            // the local stats with the server's. Adopting the stored value instead of
            // sending a lower one keeps the level from jumping back.
            if (SteamUserStats.GetStat("EXPERIENCE", out int stored) && stored > _expToStore)
            {
                _expToStore = -1;
                levelSystem.SetExp(stored);
                return;
            }

            if (AchievementManager.Instance.SetStat(AchievementManager.EStats.EXPERIENCE,
                    AchievementManager.StatsType.Int, _expToStore))
            {
                _expToStore = -1;
                return;
            }

            _nextStoreTime = Time.realtimeSinceStartup + StoreRetryInterval;
        }

        private void OnApplicationQuit()
        {
            // Quitting is the last chance to persist: ignore a pending retry delay.
            _nextStoreTime = 0;
            StoreExp();
        }

        public Color GetColor(int level)
        {
            return colors[level / 10];
        }
    }
}
