using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GearRegistry", menuName = "GearRegistry")]
public class GearRegistry : ScriptableObject
{
    [Serializable]
    public struct GearEntry
    {
        public GearBase GearPrefab;
        public int LevelRequirement;
    }

    [SerializeField]
    private GearEntry[] _gearEntries;

    public IEnumerable<GearBase> GetGearEntries(int currentLevel)
    {
        foreach (GearEntry gearEntry in _gearEntries)
        {
            if (gearEntry.LevelRequirement <= currentLevel)
            {
                yield return gearEntry.GearPrefab;
            }
        }
    }
}