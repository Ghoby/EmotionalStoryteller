using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Unity.FSM;
using ReGoap.Utilities;

// Using the state machine interface
public class StorytellingYarnfileNodeScript : SmState
{
    int Index;
    string Body;
    List<string> PreConditions;

    public StorytellingYarnfileNodeScript(int index, string body)
    {
        base.Awake();
        Index = index;
        Body = body;
       
        PreConditions = FetchPreConditions();
        foreach(var p in PreConditions)
        {
            print(p);
        }
    }

    List<string> FetchPreConditions()
    {
        string tagSection = Body.Split('\n')[1].Replace("tags: ", "");
        string preCondSection = tagSection.Split('|')[1].Trim();

        return new List<string>(preCondSection.Split(' '));
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
