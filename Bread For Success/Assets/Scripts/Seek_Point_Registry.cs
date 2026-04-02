using UnityEngine;
using System.Collections.Generic;
public static class Seek_Point_Registry
{
    // Dictionary maps each seek point to the GameObject that created it
    private static Dictionary<GameObject, GameObject> seekPointToCreator = new Dictionary<GameObject, GameObject>();
    public static Dictionary<GameObject, GameObject> Seek_Point_To_Previous_Seek_Point = new Dictionary<GameObject, GameObject>();


    /// Register a seek point with its creator

    public static void Register(GameObject seekPoint, GameObject creator)
    {
        if (seekPoint != null && creator != null)
        {
            seekPointToCreator[seekPoint] = creator;
        }
    }

    /// Get the creator of a seek point

    public static GameObject GetCreator(GameObject seekPoint)
    {
        if (seekPoint != null && seekPointToCreator.TryGetValue(seekPoint, out var creator))
        {
            return creator;
        }
        return null;
    }


    /// Remove a seek point from the registry (call this when destroying)

    public static void Unregister(GameObject seekPoint)
    {
        if (seekPoint != null)
        {
            seekPointToCreator.Remove(seekPoint);
        }
    }




    public static void Register_Previous_Seek_Point(GameObject Previous, GameObject Me)
    {
        if (Previous != null)
        {
            Seek_Point_To_Previous_Seek_Point[Me] = Previous;
        }
    }

    public static GameObject Get_Previous(GameObject seekPoint)
    {
        if (seekPoint != null && seekPointToCreator.TryGetValue(seekPoint, out var Previous))
        {
            return Previous;
        }
        return null;
    }




    public static void DebugLogAll()
    {
        Debug.Log($"[SeekPointRegistry] Total registered seek points: {seekPointToCreator.Count}");
        foreach (var kvp in seekPointToCreator)
        {
            Debug.Log($"  Seek Point: {kvp.Key.name} -> Creator: {kvp.Value.name}");
        }

        foreach (var kvp in Seek_Point_To_Previous_Seek_Point)
        {
            Debug.Log($"  Seek Point: {kvp.Key.name} -> Previous point: {kvp.Value.name}");
        }

    }


}