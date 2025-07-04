using System;
using System.Collections.Generic;
using UnityEngine;

public static class StringUtils
{
    public const string GUEST_NAME_FORMAT = "PLayer_{0}";
    public const string SPAWN_TIME_FORMAT = "{0:N2}s";
    public const string GAME_TIME_REMAINING_FORMAT = "{0:%m}:{0:ss}";
    public const string GAME_TIME_COUNTDOWN_LABEL = "Match Ends In:";
    public const string GAME_TIME_ENDED_LABEL = "Match Ended";

    public static DateTime epochStart = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static Dictionary<string, object> GeneralMatchMakeAlgo()
    {
        return new Dictionary<string, object>() {
            { "strategy", "ranged-absolute" },
            { "alignment", "center" },
            { "ranges", new List<int> { 1000 } }
        };
    }
}
