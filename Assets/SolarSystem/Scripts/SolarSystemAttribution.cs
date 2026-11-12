using UnityEngine;

namespace SolarSystem
{
    public sealed class SolarSystemAttribution : MonoBehaviour
    {
        [TextArea(3, 8)]
        public string credits = "Planetary surface textures: Solar System Scope (https://www.solarsystemscope.com/textures/) — CC BY 4.0. Textures are based on NASA mission imagery and elevation data. Attribution required when redistributing the textures.";
    }
}