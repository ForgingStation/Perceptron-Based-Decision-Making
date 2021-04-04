using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct BotComponentData : IComponentData
{
    public BlobAssetReference<Collider> colliderCast;
    public float breadBias;
    public float venomBias;
    public float3 breadDirection;
    public float breadDistace;
    public float3 venomDirection;
    public float venomDistace;
    public float3 finalDirection;
    public float speed;
    public float health;
    public float healthOverTime;
    public bool pushedBack;
    public int generation;
    //Perceptron Values
    public float p_BreadWeight;
    public float p_VenomWeight;
    public float p_Survivability;
    public float p_Error;
    public float p_deltaWeightBread;
    public float p_deltaWeightVenom;
    public float p_desiredHealth;
}
