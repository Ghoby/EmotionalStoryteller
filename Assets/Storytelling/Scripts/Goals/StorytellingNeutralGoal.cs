using ReGoap.Unity;

public class StorytellingNeutralGoal : ReGoapGoal<string, object>
{
    protected override void Awake()
    {
        base.Awake();
        goal.Set("myRequirement", 10);
        // TO DO: DEFINE GOAL
    }

}
