using PlayFab;
using UnityEngine;

public static class PlayFabRuntimeConfig
{
    private const string TitleId = "90C56";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyTitleId()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = TitleId;
        }
    }
}
