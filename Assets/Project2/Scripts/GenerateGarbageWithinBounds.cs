using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerateGarbageWithinBounds : MonoBehaviour
{
    public GameObject[] ObjectsToSpawn;
    public GameObject[] ObjectsToSpawnFixedScale;
    public int NumberOfObjectsToSpawn;
    public Vector3 MinScale = new Vector3(0.2f, 0.2f, 0.2f);
    public Vector3 MaxScale = new Vector3(3f, 3f, 3f);

    private void Start()
    {
        int spawnCount = 0;
        while (spawnCount < NumberOfObjectsToSpawn)
        {
            spawnCount++;
            SpawnObject();
        }
    }

    private void SpawnObject()
    {
        Vector3 randomPosition = transform.TransformPoint(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * .5f);

        int type = Random.Range(0, 2);

        Transform spawnedTransform;
        
        if (type == 0)
        {
            spawnedTransform = Instantiate(ObjectsToSpawn[Random.Range(0, ObjectsToSpawn.Length)], randomPosition, Random.rotation).transform;
            float randomScale = Random.Range(MinScale.x, MaxScale.x);
            spawnedTransform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }
        else
        {
            spawnedTransform = Instantiate(ObjectsToSpawnFixedScale[Random.Range(0, ObjectsToSpawnFixedScale.Length)], randomPosition, Random.rotation).transform;
        }

        spawnedTransform.parent = transform;
    }
}
