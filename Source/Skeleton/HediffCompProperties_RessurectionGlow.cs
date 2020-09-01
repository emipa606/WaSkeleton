using RimWorld;
using Verse;

namespace Skeleton
{
    public class HediffComp_RessurectionGlow : HediffComp
    {
        private int ticks = 0;
        private Thing light;


        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (light != null) light.Destroy();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if(Pawn.Map == null)
            {
                if (light != null)
                {
                    light.Destroy();
                    light = null;
                }
                return;
            }
            if (ticks >= 5 && light?.Position != Pawn.Position)
            {
                if (light != null) light.Destroy();
                light = GenSpawn.Spawn(ThingDef.Named("RessurectionGlow"), Pawn.Position, Pawn.Map);
                ticks = 0;
            }
            ticks++;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref light, "light", null);
            Scribe_Values.Look(ref ticks, "ticks", 0);
        }

    }
}