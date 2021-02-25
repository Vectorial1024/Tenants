using System.Reflection;
using Gastronomy;
using HarmonyLib;
using Verse;

namespace Tenants
{
    [StaticConstructorOnStartup]
    internal static class GastronomyPatch
    {
        static GastronomyPatch()
        {
            var harmony = new Harmony("Mlie.GastronomyPatch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(RestaurantController))]
        [HarmonyPatch("MayDineHere")]
        public class Prefix_RestaurantController_MayDineHere
        {
            [HarmonyPrefix]
            public static bool Prefix(ref RestaurantController __instance, ref bool __result, Pawn pawn)
            {
                if (!__instance.IsOpenedRightNow)
                {
                    return true;
                }

                if (pawn.GetTenantComponent() == null)
                {
                    return true;
                }

                if (!pawn.GetTenantComponent().IsTenant)
                {
                    return true;
                }

                if (!pawn.GetTenantComponent().Contracted)
                {
                    return true;
                }

                var shouldCountAsGuest = ModMain.instance.GetSettings<TenantsSettings>().GastronomyGuest;
                if (!__instance.allowColonists && !shouldCountAsGuest || !__instance.allowGuests && shouldCountAsGuest)
                {
                    __result = false;
                }
                else
                {
                    __result = true;
                }

                return false;
            }
        }
    }
}