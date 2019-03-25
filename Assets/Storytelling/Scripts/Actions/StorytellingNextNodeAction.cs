using System;
using System.Collections.Generic;
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

        //effects.Set("score", 0);
    }

    public override void Run(IReGoapAction<string, object> previous, IReGoapAction<string, object> next, ReGoapState<string, object> settings, ReGoapState<string, object> goalState, Action<IReGoapAction<string, object>> done, Action<IReGoapAction<string, object>> fail)
    {
        base.Run(previous, next, settings, goalState, done, fail);
        // TO DO
        print("xXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx");
        //GetComponent<StorytellingAgent>().AddToScore(10);
        doneCallback(this);
    }

    public override ReGoapState<string, object> GetEffects(GoapActionStackData<string, object> stackData)
    {
        //return base.GetEffects(stackData);

        //effects.Set("scoreLimit", true); //**FUNCIONA**
        if(stackData.settings != null && stackData.settings.HasKey("score") && (int)stackData.settings.Get("score") >= 10)
        {
            effects.Set("scoreLimit", true);        
        }

        return effects;
    }

    public override List<ReGoapState<string, object>> GetSettings(GoapActionStackData<string, object> stackData)
    {
        if(stackData.currentState != null && !stackData.currentState.HasKey("score"))
        {
            settings.Set("score", 0);
        }
        var x = (int)settings.Get("score") + 10;
        settings.Set("score", x);

        print("Score --> " + (int)settings.Get("score"));

        return base.GetSettings(stackData);
    }
}
