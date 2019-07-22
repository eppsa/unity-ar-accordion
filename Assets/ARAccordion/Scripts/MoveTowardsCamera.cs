using UnityEngine;

public class MoveTowardsCamera : MonoBehaviour
{
    public GameObject target;

    void Update()
    {
        Vector3 newDir = Vector3.RotateTowards(transform.forward, Camera.main.transform.forward, 0.3f * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir, Camera.main.transform.up);

        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 100.0f * Time.deltaTime);
    }
}
