using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;

namespace SolarSystem.EditorTools
{
    [InitializeOnLoad]
    public static class SolarSystemVisualUpgrade
    {
        private const string Root = "Assets/SolarSystem/";
        private const string Textures = Root + "Textures/";
        private const string ScenePath = "Assets/Scenes/SolarSystem.unity";

        private sealed class Body
        {
            public string name, texture;
            public float radius, orbit, speed, spin, tilt;
            public Color atmosphere;
            public float atmosphereScale;
        }

        private static readonly Body[] Bodies =
        {
            new Body{name="Mercury",texture="8k_mercury.jpg",radius=.55f,orbit=8f,speed=5.6f,spin=1.1f,tilt=.03f,atmosphere=new Color(.4f,.42f,.5f),atmosphereScale=1.015f},
            new Body{name="Venus",texture="8k_venus_surface.jpg",radius=.9f,orbit=12f,speed=4.1f,spin=-.28f,tilt=177.4f,atmosphere=new Color(1f,.67f,.32f),atmosphereScale=1.055f},
            new Body{name="Earth",texture="8k_earth_daymap.jpg",radius=.96f,orbit=17f,speed=3.1f,spin=8f,tilt=23.4f,atmosphere=new Color(.12f,.48f,1f),atmosphereScale=1.045f},
            new Body{name="Mars",texture="8k_mars.jpg",radius=.7f,orbit=22f,speed=2.45f,spin=7.7f,tilt=25.2f,atmosphere=new Color(.9f,.28f,.12f),atmosphereScale=1.025f},
            new Body{name="Jupiter",texture="8k_jupiter.jpg",radius=2.45f,orbit=29f,speed=1.45f,spin=24f,tilt=3.1f,atmosphere=new Color(.8f,.56f,.36f),atmosphereScale=1.012f},
            new Body{name="Saturn",texture="8k_saturn.jpg",radius=2.05f,orbit=38f,speed=1.05f,spin=21f,tilt=26.7f,atmosphere=new Color(.9f,.72f,.48f),atmosphereScale=1.018f},
            new Body{name="Uranus",texture="2k_uranus.jpg",radius=1.52f,orbit=47f,speed=.72f,spin=-13f,tilt=97.8f,atmosphere=new Color(.3f,.82f,.9f),atmosphereScale=1.025f},
            new Body{name="Neptune",texture="2k_neptune.jpg",radius=1.5f,orbit=56f,speed=.58f,spin=14f,tilt=28.3f,atmosphere=new Color(.22f,.36f,1f),atmosphereScale=1.025f}
        };

        static SolarSystemVisualUpgrade()
        {
            EditorApplication.delayCall += TryBuild;
        }

        private static void TryBuild()
        {
            string marker = Root + "UpgradeNow.txt";
            if (!File.Exists(marker) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            File.Delete(marker);
            Build();
        }

        [MenuItem("Solar System/Build Cinematic Solar System")]
        public static void BuildFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += BuildFromMenu;
                return;
            }
            Build();
        }

        private static void Build()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder(Root + "Materials");
            EnsureFolder(Root + "Meshes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SolarSystem";

            Material sphereMaterial = new Material(Shader.Find("SolarSystem/PlanetSurface"));
            Material atmosphereMaterial = new Material(Shader.Find("SolarSystem/AtmosphereRim"));
            Material cloudMaterial = new Material(Shader.Find("SolarSystem/CloudLayer"));
            Material ringMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            Material sunMaterial = new Material(Shader.Find("SolarSystem/SunSurface"));

            GameObject sun = CreateSphere("Sun", Vector3.zero, 2.7f, 160, 96);
            Renderer sunRenderer = sun.GetComponent<Renderer>();
            Texture2D sunTexture = LoadTexture("8k_sun.jpg");
            sunMaterial.SetTexture("_MainTex", sunTexture);
            sunMaterial.SetColor("_Tint", new Color(1f,.82f,.58f,1f));
            sunMaterial.SetFloat("_Intensity", 2.4f);
            sunRenderer.sharedMaterial = sunMaterial;
            sun.AddComponent<PlanetSpin>().Speed = 1.2f;
            PlanetInfo sunInfo = sun.AddComponent<PlanetInfo>();
            sunInfo.planetName = "Sun";
            sunInfo.diameterKm = 1392700f;
            CreateAtmosphere(sun.transform, 1.10f, new Color(1f,.34f,.08f), .52f, atmosphereMaterial);
            CreateAtmosphere(sun.transform, 1.24f, new Color(1f,.56f,.18f), .22f, atmosphereMaterial);
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Point;
            sunLight.color = new Color(1f,.79f,.57f);
            sunLight.intensity = 10f;
            sunLight.range = 220f;
            sunLight.shadows = LightShadows.None;

            GameObject stars = new GameObject("Deep_Space");
            Material sky = new Material(Shader.Find("Skybox/Panoramic"));
            Texture2D milkyWay = LoadTexture("8k_stars_milky_way.jpg");
            sky.SetTexture("_MainTex", milkyWay);
            sky.SetFloat("_Exposure", 1.45f);
            sky.SetFloat("_Mapping", 1f);
            sky.SetFloat("_ImageType", 0f);
            RenderSettings.skybox = sky;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.045f,.055f,.085f);
            RenderSettings.fog = false;

            foreach (Body body in Bodies)
            {
                GameObject pivot = new GameObject(body.name + "_Orbit");
                pivot.transform.rotation = Quaternion.Euler(0f, body.orbit * 9.3f, 0f);
                PlanetOrbit orbit = pivot.AddComponent<PlanetOrbit>();
                orbit.Speed = body.speed;

                GameObject planet = CreateSphere(body.name, new Vector3(body.orbit,0,0), body.radius, 160, 96);
                planet.transform.SetParent(pivot.transform, false);
                planet.transform.localPosition = new Vector3(body.orbit,0,0);
                planet.transform.localRotation = Quaternion.Euler(body.tilt,0,0);
                planet.AddComponent<PlanetSpin>().Speed = body.spin;
                PlanetInfo info = planet.AddComponent<PlanetInfo>();
                info.planetName = body.name;
                info.diameterKm = GetDiameter(body.name);
                info.orbitalPeriodDays = GetPeriod(body.name);

                Material surface = new Material(sphereMaterial);
                Texture2D albedo = LoadTexture(body.texture);
                surface.SetTexture("_BaseMap", albedo);
                surface.SetTexture("_NormalMap", Texture2D.normalTexture);
                surface.SetFloat("_NormalStrength", .0f);
                surface.SetTexture("_SpecMap", Texture2D.whiteTexture);
                surface.SetFloat("_Smoothness", body.name == "Earth" ? .78f : .28f);
                surface.SetFloat("_Specular", body.name == "Earth" ? .62f : .2f);
                surface.SetColor("_AtmosphereColor", body.atmosphere);
                surface.SetFloat("_AtmosphereStrength", body.name == "Mercury" ? .04f : .65f);
                surface.SetFloat("_AtmospherePower", 3.2f);
                surface.SetVector("_SunPosition", sun.transform.position);
                surface.SetFloat("_Ambient", .035f);

                if (body.name == "Earth")
                {
                    surface.SetTexture("_NightMap", LoadTexture("8k_earth_nightmap.jpg"));
                    surface.SetTexture("_SpecMap", LoadTexture("8k_earth_specular_map.tif"));
                    surface.SetFloat("_NightStrength", 2.1f);
                    surface.SetTexture("_CloudMap", Texture2D.blackTexture);
                    surface.SetFloat("_CloudStrength", 0f);
                    Texture2D normal = LoadTexture("8k_earth_normal_map.tif");
                    if (normal != null) { surface.SetTexture("_NormalMap", normal); surface.SetFloat("_NormalStrength", .8f); }
                }
                planet.GetComponent<Renderer>().sharedMaterial = surface;

                if (body.name == "Venus")
                {
                    GameObject venusClouds = CreateSphere("Venus_Clouds", Vector3.zero, 1.018f, 160, 96);
                    venusClouds.transform.SetParent(planet.transform, false);
                    venusClouds.transform.localPosition = Vector3.zero;
                    Object.DestroyImmediate(venusClouds.GetComponent<SphereCollider>());
                    venusClouds.AddComponent<PlanetSpin>().Speed = -.31f;
                    Material vm = new Material(cloudMaterial);
                    vm.SetTexture("_MainTex", LoadTexture("4k_venus_atmosphere.jpg"));
                    vm.SetColor("_Tint", new Color(1f,.86f,.68f,.85f));
                    vm.SetFloat("_Opacity", .58f);
                    venusClouds.GetComponent<Renderer>().sharedMaterial = vm;
                }

                if (body.name == "Earth")
                {
                    GameObject clouds = CreateSphere("Earth_Clouds", Vector3.zero, 1.012f, 160, 96);
                    clouds.transform.SetParent(planet.transform, false);
                    clouds.transform.localPosition = Vector3.zero;
                    Object.DestroyImmediate(clouds.GetComponent<SphereCollider>());
                    clouds.AddComponent<PlanetSpin>().Speed = 8.25f;
                    Material cm = new Material(cloudMaterial);
                    cm.SetTexture("_MainTex", LoadTexture("8k_earth_clouds.jpg"));
                    cm.SetColor("_Tint", new Color(1f,1f,1f,.8f));
                    cm.SetFloat("_Opacity", .42f);
                    clouds.GetComponent<Renderer>().sharedMaterial = cm;
                }

                if (body.name != "Mercury")
                    CreateAtmosphere(planet.transform, body.atmosphereScale, body.atmosphere, body.name == "Earth" ? 1.15f : .5f, atmosphereMaterial);

                if (body.name == "Saturn")
                    CreateSaturnRings(planet.transform, body.radius, ringMaterial);

                CreateOrbitPath(body.orbit, body.name + "_Orbit_Path");
// Labels disabled: CreateLabel(planet.transform, body.name, body.radius);
            }

// Labels disabled: CreateLabel(sun.transform, "Sun", 2.7f);
            CreateCamera();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath,true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("Cinematic Solar System rebuilt.");
        }

        private static float GetDiameter(string n)
        {
            switch(n){case "Mercury":return 4879;case "Venus":return 12104;case "Earth":return 12742;case "Mars":return 6779;case "Jupiter":return 139820;case "Saturn":return 116460;case "Uranus":return 50724;default:return 49244;}
        }
        private static float GetPeriod(string n)
        {
            switch(n){case "Mercury":return 88;case "Venus":return 224.7f;case "Earth":return 365.25f;case "Mars":return 687;case "Jupiter":return 4333;case "Saturn":return 10759;case "Uranus":return 30687;default:return 60190;}
        }

        private static GameObject CreateSphere(string name, Vector3 position, float radius, int longitude, int latitude)
        {
            GameObject go = new GameObject(name);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * radius;
            MeshFilter filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = GetSharedSphereMesh(longitude, latitude);
            go.AddComponent<MeshRenderer>();
            SphereCollider collider = go.AddComponent<SphereCollider>();
            collider.radius = 1f;
            return go;
        }

        private static Mesh GetSharedSphereMesh(int longitude, int latitude)
        {
            const string meshPath = Root + "Meshes/HighDetailSphere.asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (existing != null) return existing;

            Mesh mesh = new Mesh { name = "HighDetailSphere" };
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Vector3[] vertices = new Vector3[(longitude + 1) * (latitude + 1)];
            Vector3[] normals = new Vector3[vertices.Length];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[longitude * latitude * 6];
            for (int y = 0; y <= latitude; y++)
            {
                float v = (float)y / latitude, phi = v * Mathf.PI;
                for (int x = 0; x <= longitude; x++)
                {
                    float u = (float)x / longitude, theta = u * Mathf.PI * 2f;
                    Vector3 p = new Vector3(-Mathf.Cos(theta) * Mathf.Sin(phi), Mathf.Cos(phi), Mathf.Sin(theta) * Mathf.Sin(phi));
                    int index = y * (longitude + 1) + x;
                    vertices[index] = p; normals[index] = p; uv[index] = new Vector2(u, v);
                }
            }
            int t = 0;
            for (int y = 0; y < latitude; y++)
                for (int x = 0; x < longitude; x++)
                {
                    int a = y * (longitude + 1) + x, b = a + longitude + 1;
                    triangles[t++] = a; triangles[t++] = b; triangles[t++] = a + 1;
                    triangles[t++] = a + 1; triangles[t++] = b; triangles[t++] = b + 1;
                }
            mesh.vertices = vertices; mesh.normals = normals; mesh.uv = uv; mesh.triangles = triangles;
            mesh.RecalculateTangents();
            AssetDatabase.CreateAsset(mesh, meshPath);
            return mesh;
        }

        private static void CreateAtmosphere(Transform parent,float scale,Color color,float intensity,Material template)
        {
            SphereCollider sourceCollider = parent.GetComponent<SphereCollider>();
            float bodyRadius = sourceCollider != null ? sourceCollider.radius : 1f;
            GameObject shell=CreateSphere(parent.name+"_Atmosphere",Vector3.zero,bodyRadius*scale,128,80);
            shell.transform.SetParent(parent,false);shell.transform.localPosition=Vector3.zero;
            Object.DestroyImmediate(shell.GetComponent<SphereCollider>());
            Material m=new Material(template);m.SetColor("_Color",color);m.SetFloat("_Intensity",intensity);m.SetFloat("_Power",3.2f);
            shell.GetComponent<Renderer>().sharedMaterial=m;
        }

        private static void CreateSaturnRings(Transform parent,float radius,Material template)
        {
            GameObject go=new GameObject("Saturn_Rings");go.transform.SetParent(parent,false);
            go.transform.localRotation=Quaternion.Euler(27f,0f,13f);
            Mesh mesh=new Mesh{name="Saturn_Rings_Mesh"};
            int seg=512;Vector3[] v=new Vector3[(seg+1)*2];Vector2[] uv=new Vector2[v.Length];int[] tri=new int[seg*6];
            for(int i=0;i<=seg;i++)
            {
                float a=i*Mathf.PI*2f/seg;float x=Mathf.Cos(a),z=Mathf.Sin(a);
                v[i*2]=new Vector3(x*1.18f,0,z*1.18f);v[i*2+1]=new Vector3(x*2.25f,0,z*2.25f);
                uv[i*2]=new Vector2(0,(float)i/seg);uv[i*2+1]=new Vector2(1,(float)i/seg);
                if(i<seg){int q=i*6,k=i*2;tri[q]=k;tri[q+1]=k+1;tri[q+2]=k+2;tri[q+3]=k+1;tri[q+4]=k+3;tri[q+5]=k+2;}
            }
            mesh.vertices=v;mesh.uv=uv;mesh.triangles=tri;mesh.RecalculateNormals();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            Material m=new Material(template);m.SetTexture("_BaseMap",LoadTexture("8k_saturn_ring_alpha.png"));
            m.SetColor("_BaseColor",new Color(1f,.9f,.72f,.86f));m.SetFloat("_Surface",1);m.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);m.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);m.SetInt("_ZWrite",0);m.renderQueue=3000;m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            go.AddComponent<MeshRenderer>().sharedMaterial=m;
        }

        private static void CreateOrbitPath(float radius,string name)
        {
            GameObject go=new GameObject(name);LineRenderer line=go.AddComponent<LineRenderer>();
            line.useWorldSpace=true;line.loop=true;line.positionCount=512;line.widthMultiplier=.026f;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
            Shader sh=Shader.Find("Universal Render Pipeline/Unlit");Material m=new Material(sh);m.SetColor("_BaseColor",new Color(.28f,.42f,.62f,.19f));m.SetFloat("_Surface",1);m.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);m.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);m.SetInt("_ZWrite",0);m.renderQueue=3000;m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            line.sharedMaterial=m;line.startColor=new Color(.36f,.52f,.72f,.28f);line.endColor=line.startColor;
            for(int i=0;i<512;i++){float a=i*Mathf.PI*2/512;line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}
        }

        private static void CreateLabel(Transform parent,string name,float radius)
        {
            GameObject label=new GameObject(name+"_Name");PlanetLabelBillboard billboard=label.AddComponent<PlanetLabelBillboard>();
            billboard.target=parent;billboard.verticalOffset=radius+radius*.28f;
            TextMesh text=label.AddComponent<TextMesh>();text.text=name;text.fontSize=56;text.characterSize=.12f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=new Color(.86f,.92f,1f);
        }

        private static void CreateCamera()
        {
            GameObject go=new GameObject("Main Camera");go.tag="MainCamera";
            Camera cam=go.AddComponent<Camera>();cam.clearFlags=CameraClearFlags.Skybox;cam.fieldOfView=44;cam.nearClipPlane=.03f;cam.farClipPlane=1200;cam.allowHDR=true;cam.allowMSAA=true;
            UniversalAdditionalCameraData cameraData = go.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;
            go.AddComponent<AudioListener>();go.transform.position=new Vector3(0,34,-76);go.transform.LookAt(Vector3.zero);go.AddComponent<SolarCameraController>();
            CreatePostProcessing();
        }

        private static void CreatePostProcessing()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root + "Materials/SolarCinematicProfile.asset");
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, Root + "Materials/SolarCinematicProfile.asset");
            }
            if (!profile.TryGet<Bloom>(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
            bloom.active = true; bloom.threshold.Override(1.05f); bloom.intensity.Override(.85f); bloom.scatter.Override(.72f);
            if (!profile.TryGet<Tonemapping>(out Tonemapping tone)) tone = profile.Add<Tonemapping>(true);
            tone.active = true; tone.mode.Override(TonemappingMode.ACES);
            if (!profile.TryGet<Vignette>(out Vignette vignette)) vignette = profile.Add<Vignette>(true);
            vignette.active = true; vignette.intensity.Override(.18f); vignette.smoothness.Override(.72f);
            if (!profile.TryGet<ColorAdjustments>(out ColorAdjustments color)) color = profile.Add<ColorAdjustments>(true);
            color.active = true; color.contrast.Override(12f); color.saturation.Override(7f);
            GameObject volumeObject = new GameObject("Cinematic_Post_Processing");
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true; volume.priority = 1f; volume.sharedProfile = profile;
        }

        private static Texture2D LoadTexture(string file)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(Textures+file);
        }

        private static void EnsureFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path))return;
            string parent=Path.GetDirectoryName(path).Replace("\\","/");
            if(!AssetDatabase.IsValidFolder(parent))EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
        }
    }
}