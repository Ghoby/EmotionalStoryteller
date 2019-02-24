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

    public StorytellingYarnfileNodeScript(int index, string body)
    {
        Index = index;
        Body = body;
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
