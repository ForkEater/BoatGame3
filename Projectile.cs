using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;

    private Vector3 targetWorldPos;
    private BoatController targetBoat;

    public void Init(Vector3 targetPos, BoatController boat)
    {
        targetWorldPos = targetPos;
        targetBoat = boat;
        StartCoroutine(MoveToTarget());
    }

    IEnumerator MoveToTarget()
    {
        while (Vector3.Distance(transform.position, targetWorldPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWorldPos,
                speed * Time.deltaTime
            );

            yield return null;
        }

        // Hit!
        if (targetBoat != null)
        {
            targetBoat.takeDamage();
        }

        Destroy(gameObject);
    }
}