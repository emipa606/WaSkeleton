using RimWorld;
using Verse;

namespace Skeleton;

public class HediffComp_RessurectionGlow : HediffComp
{
    private Thing light;
    private int ticks;

    private HediffCompProperties_RessurectionGlow Props => (HediffCompProperties_RessurectionGlow)props;

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (light != null)
        {
            light.Destroy();
            light = null;
        }

        if (Pawn.Map == null)
        {
            return;
        }

        if (ticks > 25)
        {
            var moteDefname = $"Mote_RessurectionGlow{Props.MoteColor}";
            var mote = DefDatabase<ThingDef>.GetNamedSilentFail(moteDefname);

            if (mote == null)
            {
                Log.ErrorOnce($"[WaSkeleton] Could not find mote named {moteDefname}, no glow will be shown",
                    moteDefname.GetHashCode());
            }

            MoteMaker.MakeStaticMote(Pawn.TrueCenter(), Pawn.Map, mote, 2);
            ticks = 0;
        }

        ticks++;
    }

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Deep.Look(ref light, "light", null);
        Scribe_Values.Look(ref ticks, "ticks");
    }
}