using UnityEngine;
using Steamworks;

using Steamworks.Data;
public static class SteamworksAchievements
{
    // Make Achievements.
    public static void UnlockAchievement(string achievementId)
    {
        if (!SteamClient.IsValid) return;

        var achievement = new Achievement(achievementId);
        if (!achievement.State)
        {
            achievement.Trigger();
            SteamUserStats.StoreStats();
        }
    }

    public static void IndicateProgress(string statName, int count)
    {
        if (SteamClient.IsValid) return;
        SteamUserStats.SetStat(statName, count);
        SteamUserStats.StoreStats();
    }
}
