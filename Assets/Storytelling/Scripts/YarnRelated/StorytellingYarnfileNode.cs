using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Unity.FSM;
using ReGoap.Utilities;

// Using the state machine interface
public class StorytellingYarnfileNode : SmState
{
    int Index;
    List<string> PreConditions;
    List<string> Effects;
    string Body;

    public StorytellingYarnfileNode(int index, string body)
    {
        base.Awake();
        Index = index;
        Body = body;
       
        PreConditions = FetchPreConditions();
        Effects = FetchEffects();
        foreach(var p in PreConditions)
        {
            //print(p);
        }
    }

    List<string> FetchPreConditions()
    {
        var tagSection = Body.Split('\n')[1].Replace("tags: ", "");
        var preCondSection = tagSection.Split('|')[1].Trim();
        var preCondInterior = preCondSection.Split('\n')[0].Replace("[", "").Replace("]", "").Replace(" ", "");

        return new List<string>(preCondInterior.Split(';')); 
    }

    List<string> FetchEffects()
    {
        var tagSection = Body.Split('\n')[1].Replace("tags: ", "");
        var effectsSection = tagSection.Split('|')[2].Trim();
        var effectsInterior = effectsSection.Split('\n')[0].Replace("[", "").Replace("]", "").Replace(" ", "");

        return new List<string>(effectsInterior.Split(';'));
    }

    public int GetIndex()
    {
        return Index;
    }

    public string GetBody()
    {
        return Body;
    }
}
