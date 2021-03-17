using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerateGarbageWithinBounds : MonoBehaviour
{
    [Serializable]
    public struct PieceOfGarbage
    {
        public GameObject GarbageObject;
        public Vector3 MinScale;
        public Vector3 MaxScale;
        [Tooltip("Uniform scale will use the largest value from MaxScale and the smallest value from MinScale")]
        public bool UniformScale;
    }

    public PieceOfGarbage[] CommonGarbage;
    public PieceOfGarbage[] UncommonGarbage;
    public PieceOfGarbage[] RareGarbage;
    public PieceOfGarbage[] EpicGarbage;
    public PieceOfGarbage[] LegendaryGarbage;
    public int AmountOfGarbageToSpawn;

    [Range(0f, 100f)]
    public float ChanceToSpin = 2f;
    public Vector3 MinSpinTorque;
    public Vector3 MaxSpinTorque;

    private void Start()
    {
        int spawnCount = 0;
        while (spawnCount < AmountOfGarbageToSpawn)
        {
            spawnCount++;
            SpawnGarbage();
        }
    }

    private void SpawnGarbage()
    {
        PieceOfGarbage pieceOfGarbage = RollForGarbage();

        Vector3 randomPosition = transform.TransformPoint(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * .5f);
        Transform spawnedTransform = Instantiate(pieceOfGarbage.GarbageObject, randomPosition, Random.rotation).transform;

        SetGarbageScale(pieceOfGarbage, spawnedTransform);

        RollForSpin(spawnedTransform);
    }

    private void SetGarbageScale(PieceOfGarbage pieceOfGarbage, Transform garbageTransform)
    {
        if (pieceOfGarbage.UniformScale)
        {
            float maxScale = Mathf.Max(pieceOfGarbage.MaxScale.x, pieceOfGarbage.MaxScale.y, pieceOfGarbage.MaxScale.z);
            float minScale = Mathf.Min(pieceOfGarbage.MinScale.x, pieceOfGarbage.MinScale.y, pieceOfGarbage.MinScale.z);
            float randomUniformScale = Random.Range(minScale, maxScale);
            garbageTransform.localScale = new Vector3(randomUniformScale, randomUniformScale, randomUniformScale);
        }
        else
        {
            garbageTransform.localScale = new Vector3(Random.Range(pieceOfGarbage.MinScale.x, pieceOfGarbage.MaxScale.x),
                Random.Range(pieceOfGarbage.MinScale.y, pieceOfGarbage.MaxScale.y),
                Random.Range(pieceOfGarbage.MinScale.z, pieceOfGarbage.MaxScale.z));
        }
    }

    private PieceOfGarbage RollForGarbage()
    {
        int roll = Random.Range(0, 100);

        if (roll < 1)
        {
            return LegendaryGarbage[Random.Range(0, LegendaryGarbage.Length)];
        }
        if (roll < 6)
        {
            return EpicGarbage[Random.Range(0, EpicGarbage.Length)];
        }
        if (roll < 20)
        {
            return RareGarbage[Random.Range(0, RareGarbage.Length)];
        }
        if (roll < 50)
        {
            return UncommonGarbage[Random.Range(0, UncommonGarbage.Length)];
        }

        // if (roll < 100)
        return CommonGarbage[Random.Range(0, CommonGarbage.Length)];
    }

    private void RollForSpin(Transform garbageToSpin)
    {
        float roll = Random.Range(0f, 100f);

        if (roll < ChanceToSpin)
        {
            var torque = new Vector3(Random.Range(MinSpinTorque.x, MaxSpinTorque.x), Random.Range(MinSpinTorque.y, MaxSpinTorque.y), Random.Range(MinSpinTorque.z, MaxSpinTorque.z));
            garbageToSpin.GetComponent<Rigidbody>().AddTorque(torque, ForceMode.Impulse);
        }
    }
}
