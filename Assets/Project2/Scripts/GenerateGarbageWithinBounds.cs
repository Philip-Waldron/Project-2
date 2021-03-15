﻿using UnityEngine;
using Random = UnityEngine.Random;

public class GenerateGarbageWithinBounds : MonoBehaviour
{
    public GameObject[] ObjectsToSpawn;
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
        Transform spawnedTransform = Instantiate(ObjectsToSpawn[Random.Range(0, ObjectsToSpawn.Length - 1)], randomPosition, Random.rotation).transform;
        spawnedTransform.localScale = new Vector3(Random.Range(MinScale.x, MaxScale.x), Random.Range(MinScale.y, MaxScale.y), Random.Range(MinScale.z, MaxScale.z));
        spawnedTransform.parent = transform;
    }
}
