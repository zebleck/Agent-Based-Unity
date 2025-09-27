using UnityEngine;

public class CarFollower : MonoBehaviour
{
    public Transform waypointsParent;
    public Transform[] waypoints;
    public float speed = 5f;
    public float reachThreshold = 0.5f;

    private int currentIndex = 0;

    private bool isForward = true;
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
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];

        // Only operate in xz plane
        Vector3 currentXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 targetXZ = new Vector3(target.position.x, 0f, target.position.z);
        Vector3 directionXZ = targetXZ - currentXZ;

        // Move towards current waypoint in xz only
        Vector3 moveXZ = directionXZ.normalized * speed * Time.deltaTime;
        Vector3 newXZ = currentXZ + moveXZ;
        transform.position = new Vector3(newXZ.x, transform.position.y, newXZ.z);

        // Rotate to face direction in xz only
        if (directionXZ.magnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionXZ);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // Check if we reached the waypoint in xz only
        if (directionXZ.magnitude < reachThreshold)
        {
            if (currentIndex == waypoints.Length - 1 && isForward)
            {
                isForward = false;
            }
            else if (currentIndex == 0 && !isForward)
            {
                isForward = true;
            }
            currentIndex += isForward ? 1 : -1;
        }

        // Bob the car up and down slightly
        Vector3 pos = new Vector3(transform.position.x, startPos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude, transform.position.z);
        transform.position = pos;
    }
}
