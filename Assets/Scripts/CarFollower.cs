using UnityEngine;
using System.Collections;

public class CarFollower : MonoBehaviour
{
    public Transform waypointsParent;
    public Transform[] waypoints;
    public float speed = 5f;
    public float reachThreshold = 0.5f;

    private int currentIndex = 0;

    public float bobFrequency = 2f; // how fast the bobbing is
    public float bobAmplitude = 0.1f; // how high the bobbing is

    private Vector3 startPos;

    void Start()
    {
        if (waypointsParent != null)
        {
            int childCount = waypointsParent.childCount;
            waypoints = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                waypoints[i] = waypointsParent.GetChild(i);
            }
        }
        else
        {
            waypoints = new Transform[0];
            Debug.LogWarning("Waypoints parent not assigned.");
        }
        startPos = transform.position;
    }

    void Update()
    {
        // Bob the car up and down slightly
        Vector3 pos = new Vector3(transform.position.x, startPos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude, transform.position.z);
        transform.position = pos;
    }

    // Call this externally to move forward through all remaining waypoints
    public IEnumerator MoveForwardCoroutine()
    {
        if (waypoints.Length == 0 || currentIndex >= waypoints.Length - 1)
            yield break;

        // Move through all waypoints from currentIndex+1 to the end
        while (currentIndex < waypoints.Length - 1)
        {
            currentIndex++;
            Transform target = waypoints[currentIndex];

            while (true)
            {
                Vector3 currentXZ = new Vector3(transform.position.x, 0f, transform.position.z);
                Vector3 targetXZ = new Vector3(target.position.x, 0f, target.position.z);
                Vector3 directionXZ = targetXZ - currentXZ;

                if (directionXZ.magnitude < reachThreshold)
                    break;

                Vector3 moveXZ = directionXZ.normalized * speed * Time.deltaTime;
                Vector3 newXZ = currentXZ + moveXZ;
                transform.position = new Vector3(newXZ.x, transform.position.y, newXZ.z);

                // Rotate to face direction in xz only
                if (directionXZ.magnitude > 0.1f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionXZ);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }

                yield return null;
            }
        }
    }

    // Call this externally to move backward through all previous waypoints
    public IEnumerator MoveBackwardCoroutine()
    {
        if (waypoints.Length == 0 || currentIndex <= 0)
            yield break;

        // Move through all waypoints from currentIndex-1 to 0
        while (currentIndex > 0)
        {
            currentIndex--;
            Transform target = waypoints[currentIndex];

            while (true)
            {
                Vector3 currentXZ = new Vector3(transform.position.x, 0f, transform.position.z);
                Vector3 targetXZ = new Vector3(target.position.x, 0f, target.position.z);
                Vector3 directionXZ = targetXZ - currentXZ;

                if (directionXZ.magnitude < reachThreshold)
                    break;

                Vector3 moveXZ = directionXZ.normalized * speed * Time.deltaTime;
                Vector3 newXZ = currentXZ + moveXZ;
                transform.position = new Vector3(newXZ.x, transform.position.y, newXZ.z);

                // Rotate to face direction in xz only
                if (directionXZ.magnitude > 0.1f)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionXZ);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }

                yield return null;
            }
        }
    }
}
