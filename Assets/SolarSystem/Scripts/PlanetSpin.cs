using UnityEngine;

namespace SolarSystem
{
    public sealed class PlanetSpin : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 12f;
        public float Speed { get => degreesPerSecond; set => degreesPerSecond = value; }

        private void Update()
        {
            transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.Self);
        }
    }
}