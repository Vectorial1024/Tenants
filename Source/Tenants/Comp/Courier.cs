using System.Collections.Generic;
using Verse;

namespace Tenants;

public class Courier : ThingComp
{
    public bool isCourier = false;
    public List<ThingDef> items = new List<ThingDef>();
}