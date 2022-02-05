using Verse.AI.Group;

namespace Tenants;

public class LordJob_TenantSteal : LordJob
{
    public override bool GuiltyOnDowned => true;

    public override StateGraph CreateGraph()
    {
        var stateGraph = new StateGraph();
        var lordToil_TenantStealCover = new LordToil_TenantStealCover
        {
            useAvoidGrid = true
        };
        stateGraph.AddToil(lordToil_TenantStealCover);
        var lordToil_TenantStealCover2 = new LordToil_TenantStealCover
        {
            cover = false,
            useAvoidGrid = true
        };
        stateGraph.AddToil(lordToil_TenantStealCover2);
        var transition = new Transition(lordToil_TenantStealCover, lordToil_TenantStealCover2);
        transition.AddTrigger(new Trigger_TicksPassedAndNoRecentHarm(1200));
        stateGraph.AddTransition(transition);
        return stateGraph;
    }
}