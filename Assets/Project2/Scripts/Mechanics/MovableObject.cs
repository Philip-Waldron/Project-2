using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    Vector3 start;
    public Vector3 end;
    public float moveSpeed = 1f;
    public bool reverseAndRepeat = false;

    private bool isReversing = false;
    private bool isMoving = false;

    void Start()
    {
        start = transform.position;
    }

    void Update()
    {
        if (isMoving)
            return;

        if (!isReversing)
        {
            StartCoroutine(MoveToPosition(transform, end));
        }
        else
        {
            StartCoroutine(MoveToPosition(transform, start));
        }

        if (reverseAndRepeat)
            isReversing = !isReversing;
    }

    public IEnumerator MoveToPosition(Transform transform, Vector3 position)
    {
        isMoving = true;
        var currentPos = transform.position;
        var t = 0f;
        while (t < 1 && enabled)
        {
            t += Time.deltaTime * Mathf.Max(moveSpeed, 0.1f);
            transform.position = Vector3.Lerp(currentPos, position, t);
            yield return null;
        }

        isMoving = false;
    }
}
