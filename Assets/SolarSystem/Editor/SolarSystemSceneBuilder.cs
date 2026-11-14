using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using SolarSystem;

[InitializeOnLoad]
public static class SolarSystemSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SolarSystem.unity";
    private const string TextureRoot = "Assets/SolarSystem/Textures/";
    private static bool rebuildPending;

    private sealed class Body
    {
        public string name, texture, description;
        public float radius, orbit, speed, spin, period, diameter;
        public Color fallback;
    }

    private static readonly Body[] Bodies =
    {
        new Body { name="Mercury", texture="2k_mercury.jpg", radius=.38f, orbit=7f, speed=8.0f, spin=2f, period=88f, diameter=4879, fallback=new Color(.58f,.54f,.49f), description="The smallest planet and the closest planet to the Sun." },
        new Body { name="Venus", texture="2k_venus_surface.jpg", radius=.66f, orbit=10f, speed=5.8f, spin=-.45f, period=224.7f, diameter=12104, fallback=new Color(.82f,.65f,.38f), description="A hot world with a dense atmosphere and bright cloud cover." },
        new Body { name="Earth", texture="2k_earth_daymap.jpg", radius=.72f, orbit=14f, speed=4.4f, spin=22f, period=365.25f, diameter=12742, fallback=new Color(.16f,.38f,.8f), description="Our home planet, with liquid-water oceans and a life-supporting atmosphere." },
        new Body { name="Mars", texture="2k_mars.jpg", radius=.52f, orbit=18f, speed=3.5f, spin=20f, period=687f, diameter=6779, fallback=new Color(.72f,.28f,.16f), description="The red planet, with ancient river valleys and the tallest volcano in the Solar System." },
        new Body { name="Jupiter", texture="2k_jupiter.jpg", radius=1.75f, orbit=24f, speed=1.9f, spin=34f, period=4333f, diameter=139820, fallback=new Color(.75f,.57f,.4f), description="The largest planet, a gas giant famous for its Great Red Spot." },
        new Body { name="Saturn", texture="2k_saturn.jpg", radius=1.48f, orbit=31f, speed=1.35f, spin=29f, period=10759f, diameter=116460, fallback=new Color(.78f,.69f,.49f), description="A gas giant surrounded by a spectacular system of icy rings." },
        new Body { name="Uranus", texture="2k_uranus.jpg", radius=1.12f, orbit=38f, speed=.95f, spin=-17f, period=30687f, diameter=50724, fallback=new Color(.42f,.78f,.82f), description="An ice giant that rotates on its side." },
        new Body { name="Neptune", texture="2k_neptune.jpg", radius=1.08f, orbit=45f, speed=.75f, spin=18f, period=60190f, diameter=49244, fallback=new Color(.18f,.3f,.78f), description="A distant ice giant with extremely fast winds." }
    };

    static SolarSystemSceneBuilder()
    {
        EditorApplication.delayCall += BuildIfNeeded;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && rebuildPending)
        {
            rebuildPending = false;
            EditorApplication.delayCall += PerformRebuild;
        }
    }

    private static void BuildIfNeeded()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        const string rebuildMarker = "Assets/SolarSystem/RebuildNow.txt";
        if (System.IO.File.Exists(rebuildMarker))
        {
            System.IO.File.Delete(rebuildMarker);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                rebuildPending = true;
                EditorApplication.isPlaying = false;
                return;
            }
            PerformRebuild();
            return;
        }
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null) return;

        AssetDatabase.Refresh();
        foreach (Body body in Bodies)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(TextureRoot + body.texture) == null)
            {
                Debug.LogWarning("Solar System: waiting for texture import: " + body.texture);
                EditorApplication.delayCall += BuildIfNeeded;
                return;
            }
        }

        BuildScene();
    }

    [MenuItem("Solar System/Rebuild Solar System Scene")]
    public static void RebuildScene()
    {
        if (!EditorUtility.DisplayDialog("Rebuild Solar System", "Recreate the generated SolarSystem scene? The existing generated scene will be replaced.", "Rebuild", "Cancel"))
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            rebuildPending = true;
            EditorApplication.isPlaying = false;
            return;
        }

        PerformRebuild();
    }

    private static void PerformRebuild()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            rebuildPending = true;
            EditorApplication.isPlaying = false;
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            AssetDatabase.DeleteAsset(ScenePath);
        BuildScene();
    }

    private static void BuildScene()
    {
        EnsureFolder("Assets/SolarSystem/Materials");
        EnsureFolder("Assets/SolarSystem/Meshes");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "SolarSystem";

        Material sunMaterial = MakeMaterial("Sun_Emissive", "2k_sun.jpg", Color.white, true);
        GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sun.name = "Sun";
        sun.transform.localScale = Vector3.one * 3.4f;
        sun.GetComponent<Renderer>().sharedMaterial = sunMaterial;
        sun.AddComponent<PlanetSpin>().Speed = 2.5f;
// Labels disabled: CreateLabel(sun.transform, "Sun", 3.4f);
        PlanetInfo sunInfo = sun.AddComponent<PlanetInfo>();
        sunInfo.planetName = "Sun";
        sunInfo.description = "The star at the centre of our Solar System. Its gravity holds the planets in orbit.";
        sunInfo.diameterKm = 1392700f;
        Light sunLight = sun.AddComponent<Light>();
        sunLight.type = LightType.Point;
        sunLight.color = new Color(1f, .82f, .58f);
        sunLight.intensity = 5f;
        sunLight.range = 115f;
        sunLight.shadows = LightShadows.None;

        foreach (Body body in Bodies)
        {
            Material material = MakeMaterial(body.name + "_Surface", body.texture, body.fallback, false);
            GameObject pivot = new GameObject(body.name + "_Orbit");
            pivot.transform.position = Vector3.zero;
            pivot.transform.rotation = Quaternion.Euler(0f, body.orbit * 8f, 0f);
            PlanetOrbit orbit = pivot.AddComponent<PlanetOrbit>();
            orbit.Speed = body.speed;

            GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = body.name;
            planet.transform.SetParent(pivot.transform, false);
            planet.transform.localPosition = new Vector3(body.orbit, 0f, 0f);
            planet.transform.localScale = Vector3.one * body.radius;
            planet.GetComponent<Renderer>().sharedMaterial = material;
            planet.AddComponent<PlanetSpin>().Speed = body.spin;
            PlanetInfo info = planet.AddComponent<PlanetInfo>();
            info.planetName = body.name;
            info.description = body.description;
            info.orbitalPeriodDays = body.period;
            info.diameterKm = body.diameter;

            CreateOrbitPath(body.orbit, body.name + "_OrbitPath");
// Labels disabled: CreateLabel(planet.transform, body.name, body.radius);

            if (body.name == "Saturn")
                CreateSaturnRings(planet.transform, body.radius);
        }

        CreateStarfield();
        CreateCamera();
        GameObject credits = new GameObject("Credits");
        credits.AddComponent<SolarSystemAttribution>();

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.16f, .18f, .25f);
        RenderSettings.skybox = null;
        RenderSettings.fog = false;

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("Solar System scene generated successfully. Open SolarSystem.unity and press Play.");
    }

    private static Material MakeMaterial(string name, string textureName, Color fallback, bool emissive)
    {
        string path = "Assets/SolarSystem/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader targetShader = Shader.Find(emissive ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit");
        if (targetShader == null) targetShader = Shader.Find("Standard");
        if (material == null)
        {
            material = new Material(targetShader);
            AssetDatabase.CreateAsset(material, path);
        }
        else if (material.shader != targetShader)
        {
            material.shader = targetShader;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureRoot + textureName);
        material.SetColor("_BaseColor", fallback);
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
        if (emissive && material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", Color.white);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateOrbitPath(float radius, string objectName)
    {
        GameObject path = new GameObject(objectName);
        LineRenderer line = path.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = 160;
        line.widthMultiplier = .04f;
        line.numCapVertices = 2;
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        Material mat = new Material(shader);
        mat.color = new Color(.35f, .56f, .78f, .38f);
        mat.SetFloat("_Surface", 1f);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.sharedMaterial = mat;
        line.startColor = mat.color;
        line.endColor = mat.color;
        for (int i = 0; i < 160; i++)
        {
            float angle = i * Mathf.PI * 2f / 160f;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }
    }

    private static void CreateLabel(Transform parent, string label, float radius)
    {
        GameObject labelObject = new GameObject(label + "_Label");
        PlanetLabelBillboard billboard = labelObject.AddComponent<PlanetLabelBillboard>();
        billboard.target = parent;
        billboard.verticalOffset = radius + .45f;
        TextMesh text = labelObject.AddComponent<TextMesh>();
        text.text = label;
        text.fontSize = 64;
        text.characterSize = .25f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = new Color(.82f, .9f, 1f);
    }

    private static void CreateSaturnRings(Transform planet, float radius)
    {
        const int segments = 160;
        const float inner = 1.18f;
        const float outer = 2.05f;
        Mesh mesh = new Mesh { name = "Saturn_Ring_Mesh" };
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[segments * 6];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle), z = Mathf.Sin(angle);
            vertices[i * 2] = new Vector3(x * inner, 0f, z * inner);
            vertices[i * 2 + 1] = new Vector3(x * outer, 0f, z * outer);
            uv[i * 2] = new Vector2(0f, (float)i / segments);
            uv[i * 2 + 1] = new Vector2(1f, (float)i / segments);
            if (i < segments)
            {
                int t = i * 6, v = i * 2;
                triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                triangles[t + 3] = v + 1; triangles[t + 4] = v + 3; triangles[t + 5] = v + 2;
            }
        }
        mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GameObject ring = new GameObject("Saturn_Rings");
        ring.transform.SetParent(planet, false);
        ring.transform.localScale = Vector3.one * radius;
        ring.transform.localRotation = Quaternion.Euler(25f, 0f, 12f);
        MeshFilter filter = ring.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        MeshRenderer renderer = ring.AddComponent<MeshRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        Material ringMat = new Material(shader);
        ringMat.color = new Color(.92f, .82f, .65f, .82f);
        Texture2D ringTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureRoot + "2k_saturn_ring_alpha.png");
        if (ringMat.HasProperty("_BaseMap")) ringMat.SetTexture("_BaseMap", ringTexture);
        if (ringMat.HasProperty("_MainTex")) ringMat.SetTexture("_MainTex", ringTexture);
        ringMat.SetFloat("_Surface", 1f);
        ringMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        ringMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        ringMat.SetInt("_ZWrite", 0);
        ringMat.renderQueue = 3000;
        ringMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        renderer.sharedMaterial = ringMat;
        string meshPath = "Assets/SolarSystem/Meshes/Saturn_Ring_Mesh.asset";
        if (AssetDatabase.LoadAssetAtPath<Mesh>(meshPath) == null)
            AssetDatabase.CreateAsset(mesh, meshPath);
        else
            Object.DestroyImmediate(mesh);
        filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
    }

    private static void CreateStarfield()
    {
        GameObject stars = new GameObject("Distant_Stars");
        ParticleSystem ps = stars.AddComponent<ParticleSystem>();
        stars.AddComponent<SolarStarfield>();
        var main = ps.main;
        main.loop = false; main.playOnAwake = false; main.maxParticles = 1400;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = 100000f; main.startSize = new ParticleSystem.MinMaxCurve(.08f, .22f);
        main.startColor = new Color(.72f, .82f, 1f, 1f);
        var emission = ps.emission; emission.enabled = false;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1400];
        Random.InitState(2026);
        for (int i = 0; i < particles.Length; i++)
        {
            Vector3 direction = Random.onUnitSphere;
            particles[i].position = direction * Random.Range(90f, 170f);
            particles[i].startSize = Random.Range(.06f, .2f);
            particles[i].startColor = Color.Lerp(new Color(.48f,.62f,.9f), Color.white, Random.value);
            particles[i].remainingLifetime = 100000f;
        }
        ps.SetParticles(particles, particles.Length);
        ParticleSystemRenderer renderer = stars.GetComponent<ParticleSystemRenderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        renderer.sharedMaterial = new Material(shader);
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.008f, .012f, .035f, 1f);
        camera.fieldOfView = 52f;
        camera.nearClipPlane = .1f;
        camera.farClipPlane = 500f;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(0f, 28f, -66f);
        cameraObject.transform.LookAt(Vector3.zero);
        cameraObject.AddComponent<SolarCameraController>();
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }
}