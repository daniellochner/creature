using System.Collections.Generic;
using UnityEngine;

public static class GameObjectUtility
{
    public static void SetLayerRecursively(this GameObject gameObject, int layer, List<string> ignoredLayers = null)
    {
        if (ignoredLayers == null || !ignoredLayers.Contains(LayerMask.LayerToName(gameObject.layer)))
        {
            gameObject.layer = layer;
        }

        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetLayerRecursively(layer, ignoredLayers);
        }
    }
}
