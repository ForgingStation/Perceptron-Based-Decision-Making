using Unity.Entities;

public class BotPerceptronSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref BotComponentData bcd) =>
        {
            bcd.p_Survivability = (bcd.speed * bcd.p_BreadWeight) + (bcd.health * bcd.p_VenomWeight);
            bcd.p_Error = bcd.p_desiredHealth - bcd.p_Survivability;
            bcd.p_deltaWeightBread = bcd.p_Error * bcd.speed * 0.00001f;
            bcd.p_deltaWeightVenom = bcd.p_Error * bcd.health * 0.00001f;
            bcd.p_BreadWeight += bcd.p_deltaWeightBread;
            bcd.p_VenomWeight += bcd.p_deltaWeightVenom;

            if (bcd.health > bcd.p_Survivability)
            {
                bcd.venomBias = 1;
                bcd.breadBias = 0.0001f;
            }
            else
            {
                bcd.venomBias = 0.0001f;
                bcd.breadBias = 1;
            }
        }).ScheduleParallel();
    }
}
