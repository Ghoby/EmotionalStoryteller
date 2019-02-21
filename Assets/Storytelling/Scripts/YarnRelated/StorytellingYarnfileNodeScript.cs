using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorytellingYarnfileNodeScript : MonoBehaviour
{
    string Name;
    string Body;



    public StorytellingYarnfileNodeScript(string name, string body)
    {
        Name = name;
        Body = body;
    }

    public string GetName()
    {
        return Name;
    }

    public string GetBody()
    {
        return Body;
    }
}
