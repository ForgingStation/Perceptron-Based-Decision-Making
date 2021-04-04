using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class SpawnConsumable : MonoBehaviour
{
    public GameObject prefabToSpawnBread;
    public static Entity convertedEntityBread;
    public float spawnEvery_Bread;
    public GameObject prefabToSpawnVenom;
    public static Entity convertedEntityVenom;
    public float spawnEvery_Venom;

    private EntityManager em;
    private BlobAssetStore bas;
    private GameObjectConversionSettings gocs;
    private float elapsedTimeBread;
    private float elapsedTimeVenom;

    // Start is called before the first frame update
    void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        bas = new BlobAssetStore();
        gocs = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, bas);
        convertedEntityBread = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabToSpawnBread, gocs);
        convertedEntityVenom = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabToSpawnVenom, gocs);
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTimeBread += Time.deltaTime;
        elapsedTimeVenom += Time.deltaTime;
        if (elapsedTimeBread > spawnEvery_Bread)
        {
            float3 position = new float3
                (
                    UnityEngine.Random.Range(-11, 11), 0.75f, UnityEngine.Random.Range(-11, 11)
                );
            Entity e = em.Instantiate(convertedEntityBread);
            em.SetComponentData(e, new Translation { Value = position });
            em.AddComponent<ConsumableComponentData>(e);
            em.SetComponentData(e, new ConsumableComponentData { consumableType = 1 });
            if (SpawnBot.pushedBack)
            {
                float3 defaultposition = new float3(0, 0.75f, 0);
                Entity dEntity = em.Instantiate(convertedEntityBread);
                em.SetComponentData(dEntity, new Translation { Value = defaultposition });
                em.AddComponent<ConsumableComponentData>(dEntity);
                em.SetComponentData(dEntity, new ConsumableComponentData { consumableType = 2 });
                SpawnBot.pushedBack = false;
            }
            elapsedTimeBread = 0;
        }

        if (elapsedTimeVenom > spawnEvery_Venom)
        {
            float3 position = new float3
                (
                    UnityEngine.Random.Range(-11, 11), 0.75f, UnityEngine.Random.Range(-11, 11)
                );
            Entity e = em.Instantiate(convertedEntityVenom);
            em.SetComponentData(e, new Translation { Value = position });
            em.AddComponent<ConsumableComponentData>(e);
            em.SetComponentData(e, new ConsumableComponentData { consumableType = -1 });
            elapsedTimeVenom = 0;
        }
    }

    private void OnDestroy()
    {
        bas.Dispose();
    }
}
