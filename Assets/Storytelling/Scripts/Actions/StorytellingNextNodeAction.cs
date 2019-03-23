﻿using System;
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
        //preconditions.Set("myPrecondition", true);

        //effects.Set("myRequirement", true);
        //effects.Set("myRequirement", 10);

        //effects.Set("scoreLimit", true); **FUNCIONA**
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);
        // TO DO
        print("xXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx");
        GetComponent<StorytellingAgent>().AddToScore(10);
        print("-----------------------> " + GetComponent<StorytellingAgent>().GetScore());
        doneCallback(this);
    }

    public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
    {
        //return base.GetEffects(stackData);

        //effects.Set("scoreLimit", true); **FUNCIONA**

        if (GetComponent<StorytellingAgent>().GetScore() == 10)
        {
            effects.Set("scoreLimit", true);
        }
        else
        {
            effects.Clear();
        }
            

        return effects;
    }
}
