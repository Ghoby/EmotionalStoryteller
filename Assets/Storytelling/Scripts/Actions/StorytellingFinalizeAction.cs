using System;
using System.Collections.Generic;
using ReGoap.Core;
using ReGoap.Unity;

public class StorytellingFinalizeAction : ReGoapAction<string, object>
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
        preconditions.Set("scoreLimit", true);
        effects.Set("myRequirement", true);

        //effects.Set("myRequirement", true);
        //effects.Set("myRequirement", 10);
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);
        // TO DO
        print("yYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYy");
        doneCallback(this);
    }

    public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
    {
        print("AAAAAAAAAAAAAAAAAAAAAAAAA");
        return base.GetSettings(stackData);
    }
}
