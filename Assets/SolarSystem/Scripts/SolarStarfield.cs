using UnityEngine;

namespace SolarSystem
{
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class SolarStarfield : MonoBehaviour
    {
        [SerializeField] private int starCount = 1100;
        [SerializeField] private float innerRadius = 95f;
        [SerializeField] private float outerRadius = 180f;

        private void Awake()
        {
            ParticleSystem system = GetComponent<ParticleSystem>();
            var main = system.main;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = starCount;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            main.startLifetime = 100000f;
            main.startSpeed = 0f;
            main.startSize = .28f;
            main.startColor = Color.white;

            var emission = system.emission;
            emission.enabled = false;
            var shape = system.shape;
            shape.enabled = false;

            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.Play(true);
            Random.InitState(2026);

            for (int i = 0; i < starCount; i++)
            {
                Vector3 direction = Random.onUnitSphere;
                ParticleSystem.EmitParams particle = new ParticleSystem.EmitParams
                {
                    position = direction * Random.Range(innerRadius, outerRadius),
                    startSize = Random.Range(.16f, .34f),
                    startColor = Color.Lerp(new Color(.62f, .74f, 1f), Color.white, Random.value),
                    startLifetime = 100000f
                };
                system.Emit(particle, 1);
            }
        }
    }
}