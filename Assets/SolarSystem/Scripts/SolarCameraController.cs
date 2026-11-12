using UnityEngine;
using UnityEngine.InputSystem;

namespace SolarSystem
{
    public sealed class SolarCameraController : MonoBehaviour
    {
        [SerializeField] private float orbitSensitivity = .22f;
        [SerializeField] private float zoomSensitivity = .012f;
        [SerializeField] private float minDistance = .65f;
        [SerializeField] private float maxDistance = 180f;
        [SerializeField] private float panSensitivity = .0012f;
        [SerializeField] private float smoothTime = .16f;

        private Camera cam;
        private Transform focus;
        private Vector3 focusPoint;
        private Vector3 focusVelocity;
        private float yaw;
        private float pitch = 22f;
        private float distance = 76f;
        private float targetDistance = 76f;
        private float distanceVelocity;
        private Vector2 lastPointer;
        private bool dragging;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
            focusPoint = Vector3.zero;
        }

        private void LateUpdate()
        {
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 pointer = mouse.position.ReadValue();
                Vector2 delta = mouse.delta.ReadValue();

                if (mouse.rightButton.wasPressedThisFrame) { dragging = true; lastPointer = pointer; }
                if (mouse.rightButton.wasReleasedThisFrame) dragging = false;
                if (dragging)
                {
                    yaw += delta.x * orbitSensitivity;
                    pitch = Mathf.Clamp(pitch - delta.y * orbitSensitivity, -82f, 82f);
                }

                if (mouse.middleButton.isPressed)
                {
                    float scale = distance * panSensitivity;
                    Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
                    focusPoint += rotation * new Vector3(-delta.x * scale, -delta.y * scale, 0f);
                    focus = null;
                }

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > .01f)
                    targetDistance = Mathf.Clamp(targetDistance * Mathf.Exp(-scroll * zoomSensitivity), minDistance, maxDistance);

                if (mouse.leftButton.wasPressedThisFrame)
                {
                    Ray ray = cam.ScreenPointToRay(pointer);
                    if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
                    {
                        PlanetInfo info = hit.collider.GetComponentInParent<PlanetInfo>();
                        if (info != null)
                        {
                            focus = info.transform;
                            focusPoint = focus.position;
                            SphereCollider sphere = hit.collider as SphereCollider;
                            float radius = sphere != null ? sphere.radius * focus.lossyScale.x : .7f;
                            targetDistance = Mathf.Clamp(radius * 4.6f, minDistance, 180f);
                        }
                    }
                }
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.homeKey.wasPressedThisFrame) ResetView();

            if (focus != null)
                focusPoint = Vector3.SmoothDamp(focusPoint, focus.position, ref focusVelocity, smoothTime);
            distance = Mathf.SmoothDamp(distance, targetDistance, ref distanceVelocity, smoothTime);
            Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = focusPoint + cameraRotation * new Vector3(0f, 0f, -distance);
            transform.rotation = cameraRotation;
        }

        private void ResetView()
        {
            focus = null;
            focusPoint = Vector3.zero;
            yaw = 0f;
            pitch = 22f;
            targetDistance = 76f;
        }
    }
}