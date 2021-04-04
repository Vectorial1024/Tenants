using Verse.AI.Group;

namespace Tenants
{
    public class LordJob_TenantTheft : LordJob
    {
        public override bool GuiltyOnDowned => true;

        public override StateGraph CreateGraph()
        {
            var stateGraph = new StateGraph();
            var unused = stateGraph.AttachSubgraph(new LordJob_TenantSteal().CreateGraph()).StartingToil;
            return stateGraph;
        }
    }
}