using System;
using ReGoap.Core;
using ReGoap.Unity;

public class StorytellingNextNodeAction : ReGoapAction<string, object>
{
    protected override void Awake()
    {
        base.Awake();

        // PRECONDITIONS
        // No preconditions here /////////////////////////////////////////////////////////////////////////////////////

        // EFFECTS
        //foreach (effect in Effects)
        //{
        //    effects.Set(effect.Split(':')[0], Convert.ToInt32(effect.Split(':')[1]));
        //}
        // It would be something of this kind

        // JUST FOR TESTING
        preconditions.Set("myPrecondition", true);
        effects.Set("myEffects", true);
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);
        // TO DO
        print("x");
        doneCallback(this);
    }
}
