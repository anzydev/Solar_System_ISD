using UnityEngine;

namespace SolarSystem
{
    public sealed class PlanetInfo : MonoBehaviour
    {
        [TextArea(2, 5)] public string description;
        public string planetName;
        public float orbitalPeriodDays;
        public float diameterKm;

        public string Summary =>
            $"{planetName}\n{description}\nDiameter: {diameterKm:N0} km  |  Orbital period: {orbitalPeriodDays:N1} days";
    }
}