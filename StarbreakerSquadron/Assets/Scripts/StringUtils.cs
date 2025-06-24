using System.Collections.Generic;
using UnityEngine;

public static class StringUtils
{
    public const string SPAWN_TIME_FORMAT = "{0:N2}s";
    public static Dictionary<string, object> GeneralMatchMakeAlgo()
    {
        return new Dictionary<string, object>() {
            { "strategy", "ranged-absolute" },
            { "alignment", "center" },
            { "ranges", new List<int> { 1000 } }
        };
    }
}
