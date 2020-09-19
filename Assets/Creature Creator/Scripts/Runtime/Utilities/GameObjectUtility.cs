using UnityEngine;

public static class GameObjectUtility
{
    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;

        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }
}
