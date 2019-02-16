using UnityEngine;

public class CameraController : MonoBehaviour {

    public Vector2 horizontal;
    public Vector2 vertical;
    public float cameraMoveSpeed = 20;
    public float middleMouseMoveFactor = 0.05f;
    public float cameraHeight = 20;

    private Vector3 lastMousePos;

    void Update()
    {
        KeysMove();

        if (Input.GetMouseButtonDown(2)) //Middle Mouse
            lastMousePos = Input.mousePosition;

        if (Input.GetMouseButton(2))
            MiddleMouseMove();
    }

    void KeysMove()
    {
        Vector3 pos = transform.position;

        pos.x += Input.GetAxis("Horizontal") * cameraMoveSpeed * Time.deltaTime;
        pos.z += Input.GetAxis("Vertical") * cameraMoveSpeed * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, horizontal.x, horizontal.y);
        pos.z = Mathf.Clamp(pos.z, vertical.x, vertical.y);

        transform.position = pos;
    }

    void MiddleMouseMove()
    {
        if (lastMousePos != Input.mousePosition)
        {
            Vector3 diff = (lastMousePos - Input.mousePosition) * middleMouseMoveFactor;
            Vector3 pos = transform.position;

            pos.x += diff.x;
            pos.z += diff.y;

            pos.x = Mathf.Clamp(pos.x, horizontal.x, horizontal.y);
            pos.z = Mathf.Clamp(pos.z, vertical.x, vertical.y);

            transform.position = pos;
            lastMousePos = Input.mousePosition;
        }
    }
}
