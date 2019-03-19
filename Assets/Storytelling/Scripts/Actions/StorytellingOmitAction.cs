using System;
using ReGoap.Core;
using ReGoap.Unity;

public class StorytellingOmitAction : ReGoapAction<string, object>
{
    protected override void Awake()
    {
        base.Awake();
        Name = "StorytellingOmitAction";
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);

        // TO DO
    }
}
