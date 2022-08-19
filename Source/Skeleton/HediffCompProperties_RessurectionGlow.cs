using Verse;

namespace Skeleton;

public class HediffCompProperties_RessurectionGlow : HediffCompProperties
{
    public readonly string MoteColor = "Green";

    public HediffCompProperties_RessurectionGlow()
    {
        compClass = typeof(HediffComp_RessurectionGlow);
    }
}