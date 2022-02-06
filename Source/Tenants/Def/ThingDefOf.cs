using RimWorld;
using Verse;

namespace Tenants;

[DefOf]
public static class ThingDefOf
{
    public static ThingDef Tenants_MailBox;

    static ThingDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}