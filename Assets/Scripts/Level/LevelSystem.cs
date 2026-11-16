using System;
using UnityEngine;

namespace Level
{
    [Serializable]
    public class LevelSystem
    {
        public event EventHandler OnLevelChanged;
        public event EventHandler OnExpChanged;

        private int experience, level, expToNext;

        public LevelSystem(int exp)
        {
            experience = exp;
            SetLevel();
        }

        /// <summary>
        /// Adds experience to the in-memory level. Persisting it is the level
        /// manager's job: it retries the Steam upload until it is accepted and
        /// makes sure a stored value can never be replaced by a lower one.
        /// </summary>
        public void AddExperience(int amount)
        {
            int lastLevel = level;
            experience += amount;
            SetLevel();
            if (lastLevel < level)
            {
                OnLevelChanged?.Invoke(this, EventArgs.Empty);
            }

            OnExpChanged?.Invoke(this, EventArgs.Empty);
        }
        public int GetMinExp()
        {
            float total = 0;
            for (int i = 1; i < level + 1; i++)
            {
                total += Mathf.Floor(i + 300 * Mathf.Pow(2, i / 7f));
            }
            return level == 0 ? 0 : (int)total;
        }

        void SetLevel()
        {
            float total = 0;
            for (int i = 1; i < 99; i++)
            {
                total += Mathf.Floor(i + 300 * Mathf.Pow(2, i / 7f));
                if (total > experience)
                {
                    level = i - 1;
                    expToNext = (int)total;
                    return;
                }
            }

            level = 99;
            expToNext = int.MaxValue;
        }

        public void SetExp(int ex)
        {
            int lastLevel = level;
            experience = ex;
            SetLevel();
            if (lastLevel < level)
            {
                OnLevelChanged?.Invoke(this, EventArgs.Empty);
            }

            OnExpChanged?.Invoke(this, EventArgs.Empty);
        }

        public int GetLevel()
        {
            return level;
        }

        public int GetExpToNext()
        {
            return expToNext;
        }

        public int GetExp()
        {
            return experience;
        }
    }

    public static class LevelUtils
    {
        public static int GetExpFromLevel(int level)
        {
            float total = 0;
            for (int i = 1; i < 99; i++)
            {
                total += Mathf.Floor(i + 300 * Mathf.Pow(2, i / 7f));
                if (i >= level)
                {
                    return (int)total;
                }
            }

            return 0;
        }
    }
}
