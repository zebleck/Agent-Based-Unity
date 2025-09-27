using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    public Light sun;      // Your main Directional Light
    public Light moon;     // Optional secondary light

    [Header("Cycle Settings")]
    public float dayLength = 120f; // seconds for a full cycle (day + night)

    private float time; // normalized [0,1] over one cycle

    void Update()
    {
        // Advance time
        time += Time.deltaTime / dayLength;
        if (time > 1f) time -= 1f;

        // Rotate sun (360° per cycle, offset so noon is at top)
        float sunAngle = time * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0);

        // Optionally rotate moon opposite sun
        if (moon != null)
        {
            moon.transform.rotation = Quaternion.Euler(sunAngle - 180f, 170f, 0);
        }

        // Adjust intensities smoothly
        float dot = Mathf.Clamp01(Vector3.Dot(sun.transform.forward, Vector3.down));
        sun.intensity = Mathf.Lerp(0f, 1f, dot);       // bright midday, off at night
        if (moon != null)
            moon.intensity = Mathf.Lerp(0.3f, 0f, dot); // opposite: bright at night
    }
}
