using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Collections;

public class SpawnBot : MonoBehaviour
{
    public GameObject prefabBot;
    public static Entity convertedEntityBot;
    public int maxBots;
    public float spawnEvery;
    public float initialSpeed;
    public float botHealth;
    public int maxGeneration;
    public Mesh colliderMesh;
    public float perceptionRadius;
    public float desiredHealth;
    public int spawns;
    public static bool pushedBack;

    private EntityManager em;
    private BlobAssetStore bas;
    private GameObjectConversionSettings gocs;
    private float elapsedTime;
    private NativeList<Entity> spawnedBots;
    private BlobAssetReference<Unity.Physics.Collider> col;

    // Start is called before the first frame update
    void Start()
    {
        spawnedBots = new NativeList<Entity>(Allocator.Persistent);
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        bas = new BlobAssetStore();
        gocs = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, bas);
        convertedEntityBot = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabBot, gocs);
        if (colliderMesh != null)
        {
            col = CreateSphereCollider(colliderMesh);
        }
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > spawnEvery)
        {
            BotComponentData bcd = new BotComponentData();
            bcd.colliderCast = col;
            bcd.breadDirection = float3.zero;
            bcd.venomDirection = float3.zero;
            bcd.finalDirection = new float3(UnityEngine.Random.Range(0.5f, 1), 0, UnityEngine.Random.Range(0.5f, 1));
            bcd.breadDirection = 5;
            bcd.venomDistace = 2.5f;
            bcd.speed = initialSpeed;
            bcd.health = botHealth;
            bcd.breadBias = UnityEngine.Random.Range(1, 10);
            bcd.venomBias = UnityEngine.Random.Range(1, 10);
            bcd.p_BreadWeight = UnityEngine.Random.Range(0.1f, 1);
            bcd.p_VenomWeight = UnityEngine.Random.Range(0.1f, 1);
            bcd.p_desiredHealth = desiredHealth;
            bcd.generation = 1;

            for (int i = 0; i < spawnedBots.Length; i++)
            {
                if (em.GetComponentData<BotComponentData>(spawnedBots[i]).pushedBack)
                {
                    pushedBack = true;
                }
                if (em.HasComponent<ToClone>(spawnedBots[i]))
                {
                    BotComponentData oldbcd = em.GetComponentData<BotComponentData>(spawnedBots[i]);
                    bcd.speed = oldbcd.speed;
                    bcd.breadBias = oldbcd.breadBias;
                    bcd.venomBias = oldbcd.venomBias;
                    bcd.p_BreadWeight = oldbcd.p_BreadWeight;
                    bcd.p_VenomWeight = oldbcd.p_VenomWeight;
                    bcd.p_desiredHealth = oldbcd.p_desiredHealth;
                    bcd.generation = oldbcd.generation + 1;
                    if (bcd.generation > maxGeneration)
                    {
                        em.DestroyEntity(spawnedBots[i]);
                        maxBots = maxBots - 1;
                    }
                    else
                    {
                        em.DestroyEntity(spawnedBots[i]);
                    }
                    spawnedBots.RemoveAtSwapBack(i);
                    break;
                }
            }
            if (spawnedBots.Length < maxBots)
            {
                Entity e = em.Instantiate(convertedEntityBot);
                float3 position = new float3(0, 1.25f, 0);
                em.AddComponent<BotComponentData>(e);
                em.SetComponentData(e, new Translation { Value = position });
                em.SetComponentData(e, bcd);
                elapsedTime = 0;
                spawnedBots.Add(e);
                spawns++;
                elapsedTime = 0;
            }
            else
            {
                elapsedTime = 0;
            }
        }
    }

    private BlobAssetReference<Unity.Physics.Collider> CreateSphereCollider(UnityEngine.Mesh mesh)
    {
        Bounds bounds = mesh.bounds;
        CollisionFilter filter = new CollisionFilter()
        {
            BelongsTo = 1u << 13, 
            CollidesWith = 1u << 14 
        };

        return Unity.Physics.SphereCollider.Create(new SphereGeometry
        {
            Center = bounds.center,
            Radius = perceptionRadius,
        },
        filter);
    }

    private void OnDestroy()
    {
        bas.Dispose();
        spawnedBots.Dispose();
    }

    private void OnGUI()
    {
        //GUI.Box(new Rect(10, 10, 500, 40), "Cell Size : " + 20);
        GUI.Box(new Rect(10, 10, 500, 40), "Spawns : " + spawns);
        //GUI.Box(new Rect(10, 52, 500, 40), "Collisions Per Second: " + collisions / 2); //2 units colliding will be regarded as 1 collision
        GUI.skin.box.fontSize = 25;
    }

}
