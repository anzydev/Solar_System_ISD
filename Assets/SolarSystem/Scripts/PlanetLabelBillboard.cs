using UnityEngine;

namespace SolarSystem
{
    public sealed class PlanetLabelBillboard : MonoBehaviour
    {
        public Transform target;
        public float verticalOffset = 1f;

        private void LateUpdate()
        {
            if (target == null || Camera.main == null) return;

            Transform cameraTransform = Camera.main.transform;
            transform.position = target.position + Vector3.up * verticalOffset;
            transform.rotation = Quaternion.LookRotation(-cameraTransform.forward, cameraTransform.up);
        }
    }
}