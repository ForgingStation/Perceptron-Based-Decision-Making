using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Collections;

public class BotMovementSystem : SystemBase
{
    public BuildPhysicsWorld bpw;
    private EndSimulationEntityCommandBufferSystem es_ecb;

    protected override void OnCreate()
    {
        es_ecb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        bpw = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        PhysicsWorld pw = bpw.PhysicsWorld;
        float deltaTime = Time.DeltaTime;
        var parallelECB = es_ecb.CreateCommandBuffer().ToConcurrent();
        unsafe
        {
            Entities.ForEach((int entityInQueryIndex, Entity e, ref BotComponentData bcd, ref Translation trans, ref Rotation rot) =>
            {
                NativeList<DistanceHit> allHits = new NativeList<DistanceHit>(Allocator.Temp);
                bcd.breadDistace = 10;
                bcd.venomDistace = 10;
                ColliderDistanceInput colliderDistanceInput = new ColliderDistanceInput
                {
                    Collider = (Unity.Physics.Collider*)(bcd.colliderCast.GetUnsafePtr()),
                    Transform = new RigidTransform(rot.Value, trans.Value),
                    MaxDistance = 0.25f
                };
                if (pw.CalculateDistance(colliderDistanceInput, ref allHits))
                {
                    if (allHits.Length > 0)
                    {
                        for (int i = 0; i < allHits.Length; i++)
                        {
                            if (math.distance(trans.Value, allHits[i].Position) < 0.5f)
                            {
                                if (HasComponent<ConsumableComponentData>(allHits[i].Entity))
                                {
                                    ConsumableComponentData ccd = GetComponent<ConsumableComponentData>(allHits[i].Entity);
                                    if (ccd.consumableType == 1)
                                    {
                                        bcd.health = bcd.health + 10;
                                    }
                                    else if(ccd.consumableType == -1)
                                    {
                                        bcd.health = bcd.health - 10;
                                        bcd.speed = bcd.speed + 0.75f;
                                    }
                                    if (bcd.pushedBack)
                                    {
                                        bcd.pushedBack = false;
                                    }
                                }
                                parallelECB.DestroyEntity(entityInQueryIndex, allHits[i].Entity);
                            }
                            else
                            {
                                if (HasComponent<ConsumableComponentData>(allHits[i].Entity))
                                {
                                    ConsumableComponentData ccd = GetComponent<ConsumableComponentData>(allHits[i].Entity);
                                    if (ccd.consumableType == 1)
                                    {
                                        if (math.distance(trans.Value, allHits[i].Position) < bcd.breadDistace)
                                        {
                                            bcd.breadDistace = math.distance(trans.Value, allHits[i].Position);
                                            bcd.breadDirection = allHits[i].Position - trans.Value;
                                        }
                                    }
                                    else if(ccd.consumableType == -1)
                                    {
                                        if (math.distance(trans.Value, allHits[i].Position) < bcd.venomDistace)
                                        {
                                            bcd.venomDistace = math.distance(trans.Value, allHits[i].Position);
                                            bcd.venomDirection = allHits[i].Position - trans.Value;
                                        }
                                    }
                                }
                            }
                        }
                        if (!bcd.pushedBack)
                        {
                            bcd.finalDirection = (bcd.breadDirection * bcd.breadBias) + (bcd.venomDirection * bcd.venomBias);
                        }
                    }
                    allHits.Dispose();
                }
                else if(math.abs(math.distance(trans.Value, float3.zero)) > 14)
                {
                    bcd.finalDirection = float3.zero - trans.Value;
                    bcd.pushedBack = true;
                }
                bcd.finalDirection = math.normalize(new float3(bcd.finalDirection.x, 0, bcd.finalDirection.z));
                rot.Value = math.slerp(rot.Value, quaternion.LookRotation(bcd.finalDirection, math.up()), deltaTime * 20);
                trans.Value += (bcd.finalDirection * bcd.speed * deltaTime);

                bcd.healthOverTime += deltaTime;
                if (bcd.healthOverTime >= 1)
                {
                    bcd.health = bcd.health - 5;
                    bcd.healthOverTime = 0;
                }

                if (bcd.health < 0)
                {
                    parallelECB.AddComponent<ToClone>(entityInQueryIndex, e);
                }
            }).ScheduleParallel();

            es_ecb.AddJobHandleForProducer(Dependency);
        }
    }
}

public struct ToClone : IComponentData { }
