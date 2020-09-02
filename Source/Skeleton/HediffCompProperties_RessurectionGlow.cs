using RimWorld;
using System.Linq;
using Verse;

namespace Skeleton
{
    public class HediffComp_RessurectionGlow : HediffComp
    {
        private int ticks = 0;
        private Thing light;

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
                var moteDef = (from mote in DefDatabase<ThingDef>.AllDefsListForReading where mote.defName == "Mote_RessurectionGlow" select mote).FirstOrDefault();
                MoteMaker.MakeStaticMote(Pawn.TrueCenter(), Pawn.Map, moteDef, 2);
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