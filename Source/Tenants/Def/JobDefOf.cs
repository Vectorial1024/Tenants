using RimWorld;
using Verse;

namespace Tenants
{
    [DefOf]
    public static class JobDefOf
    {
        public static JobDef JobUseCommsConsoleTenants;
        public static JobDef JobUseCommsConsoleMole;
        public static JobDef JobUseCommsConsoleInviteCourier;
        public static JobDef JobCheckMailBox;

        static JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
        }
    }
}