using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
    public static Vector3 GetRandomSpawnPoint(List<Transform> spawnPoints)
    {
        //return new Vector3(Random.Range(-15, 15), 1.5f, Random.Range(-15, 15));
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            return Vector3.zero;
        }

        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform randomSpawnPoint = spawnPoints[randomIndex];

        return randomSpawnPoint.transform.position;
    }

    public static void SetRenderLayerInChildren(Transform parentTransform, int layerNumber)
    {
        foreach (Transform childrenTransform in parentTransform.GetComponentsInChildren<Transform>(true))
        {
            if(childrenTransform.CompareTag("IgnoreLayerChange")) continue;
            
            childrenTransform.gameObject.layer = layerNumber;
        }
    }
}
