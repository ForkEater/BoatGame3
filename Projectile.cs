using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public float speed = 2f;

    private Vector3 targetWorldPos;

    public void Init(Vector3 targetPos)
    {
        targetWorldPos = targetPos;
        StartCoroutine(MoveToTarget());
    }

    IEnumerator MoveToTarget()
    {
        while (Vector3.Distance(transform.position, targetWorldPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, speed*Time.deltaTime);

            yield return null;
        }

        Destroy(gameObject);
    }
}