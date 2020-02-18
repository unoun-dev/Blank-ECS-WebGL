using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;

public class Manager : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    [SerializeField] Material material;
    [SerializeField] Camera MainCamera;
    MeshInstanceRenderer render;
    EntityArchetype archetype;
    EntityManager manager;
    List <MeshInstanceRenderer> RenderList = new List <MeshInstanceRenderer>();

    void Start()
    {
        Cache();

        render = new MeshInstanceRenderer
        {
            castShadows    = ShadowCastingMode.On,
            receiveShadows = true,
            material       = new Material(material)
            {
                enableInstancing = true,
            },
            mesh    = mesh,
            subMesh = 0,
        };
        PlayerLoopManager.RegisterDomainUnload(DestroyAll, 10000);
        var world = World.Active = new World("x");
        World.Active.CreateManager(typeof(EndFrameTransformSystem));
        World.Active.CreateManager(typeof(CountUpSystem), GameObject.Find("Count").GetComponent <TMPro.TMP_Text>());
        World.Active.CreateManager <MeshInstanceRendererSystem>().ActiveCamera = GetComponent <Camera>();
        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);

        manager   = world.GetExistingManager <EntityManager>();
        archetype = manager.CreateArchetype(ComponentType.Create <Position>(), ComponentType.Create <MeshInstanceRenderer>(), ComponentType.Create <Static>());

        // for (int i = 0; i < 1000000; i++)
        for (int i = 0; i < 100000; i++)
        {
            CreateCube();
        }
    }

    void Update()
    {
        Ray        ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10.0f))
        {
            Debug.Log(hit.collider.gameObject.transform.position);
        }
    }

    private void Cache()
    {
        var colorList = new List <Color>();
        colorList.Add("#3ACF62".ToColor());
        colorList.Add("#F7F0BB".ToColor());
        colorList.Add("#F8165F".ToColor());

        foreach (var color in colorList)
        {
            render = new MeshInstanceRenderer
            {
                castShadows    = ShadowCastingMode.On,
                receiveShadows = true,
                material       = new Material(material)
                {
                    enableInstancing = true,
                    color            = color,
                },
                mesh    = mesh,
                subMesh = 0,
            };

            RenderList.Add(render);
        }
    }

    private void CreateCube()
    {
        var e = manager.CreateEntity(archetype);
        manager.SetComponentData(e, new Position
        {
            Value = new Unity.Mathematics.float3((Random.value - 0.5f) * 10, (Random.value - 0.5f) * 10, (Random.value) * 10)
        });

        int rand = UnityEngine.Random.Range(0, RenderList.Count);
        manager.SetSharedComponentData(e, RenderList[rand]);
    }

    static void DestroyAll()
    {
        World.DisposeAllWorlds();
        ScriptBehaviourUpdateOrder.UpdatePlayerLoop();
    }
}

public static class StringExtension
{
    public static Color ToColor(this string self)
    {
        var color = default(Color);
        if (!ColorUtility.TryParseHtmlString(self, out color)) {
            Debug.LogWarning("Unknown color code... " + self);
        }

        return color;
    }
}