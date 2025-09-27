using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;

    private float yaw = 0f;
    private float pitch = 0f;

    void Update()
    {
        // Camera movement
        float h = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxis("Vertical");   // W/S or Up/Down
        float upDown = 0f;

        if (Input.GetKey(KeyCode.E))
            upDown += 1f;
        if (Input.GetKey(KeyCode.Q))
            upDown -= 1f;

        Vector3 move = (transform.right * h + transform.forward * v + transform.up * upDown).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;

        // Camera rotation (mouse look)
        if (Input.GetMouseButton(1)) // Right mouse button held
        {
            yaw += lookSpeed * Input.GetAxis("Mouse X");
            pitch -= lookSpeed * Input.GetAxis("Mouse Y");
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }
    }
}
