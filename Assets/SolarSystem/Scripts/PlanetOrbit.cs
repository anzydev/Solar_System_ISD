using UnityEngine;

namespace SolarSystem
{
    public sealed class PlanetOrbit : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 8f;
        [SerializeField] private bool reverse;
        public float Speed { get => degreesPerSecond; set => degreesPerSecond = value; }

        private void Update()
        {
            float direction = reverse ? -1f : 1f;
            transform.Rotate(Vector3.up, degreesPerSecond * direction * Time.deltaTime, Space.World);
        }
    }
}