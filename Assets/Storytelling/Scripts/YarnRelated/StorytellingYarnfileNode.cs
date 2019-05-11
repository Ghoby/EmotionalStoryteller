using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Unity.FSM;
using ReGoap.Utilities;

// Using the state machine interface
public class StorytellingYarnfileNode : MonoBehaviour
{
    int Index;
    bool IsEssential;
    public List<KeyValuePair<string, float>> Effects;
    string Body;
    string Title;

    public StorytellingYarnfileNode(int index, string body)
    {
        Index = index;
        Body = ExtractCoreBody(body);
        Title = ExtractTitle(body);

        IsEssential = CheckIfEssential();
        Effects = FetchEffects();
    }

    bool CheckIfEssential()
    {
        var tagSection = Body.Split('\n')[1].Replace("tags: ", "");
        var isEssentialTagString = tagSection.Split('|')[1].Trim();
        var isEssentialTagStringValue = isEssentialTagString.Split(':')[1];

        return isEssentialTagStringValue == "True";
    }

    string ExtractCoreBody(string body)
    {
        string coreBody = "";
        string[] splitBody = body.Split('\n');
        for(int i = 0; i < splitBody.Length; i++)
        {   
            if (splitBody[i] == "\r")
            {
                break;
            }
            coreBody += splitBody[i] + "\n";
        }

        return coreBody;
    }

    string ExtractTitle(string body)
    {
        return body.Split('\n')[0].Replace("title: ", "").Trim();
    }

    List<KeyValuePair<string, float>> FetchEffects()
    {
        var tagSection = Body.Split('\n')[1].Replace("tags: ", "");
        var effectsSection = tagSection.Split('|')[2].Trim();
        var effectsInterior = effectsSection.Split('\n')[0].Replace("[", "").Replace("]", "").Replace(" ", "");
        var effectArray = effectsInterior.Split(';');

        var list = new List<KeyValuePair<string, float>>();
        for(int i = 0; i < effectArray.Length; i++)
        {
            var effect = effectArray[i].Split(':');            
            list.Add(new KeyValuePair<string, float>(effect[0], 
                float.Parse(effect[1], System.Globalization.CultureInfo.InvariantCulture)));
        }

        return list;
    }

    public void SetTransitions(string nextNodeName, bool isExit)
    {       
        if(!isExit)
        {
            Body += "\n[[>|" + nextNodeName + "]]\n[[Repeat|" + Title + "]]\n===\n";
        }
        else
        {
            Body += "\n<<exit>>\n===\n";
        }
    }

    public int GetIndex()
    {
        return Index;
    }

    public void SetBody(string body)
    {
        Body = body;
    }

    public string GetBody()
    {
        return Body;
    }

    public string GetTitle()
    {
        return Title;
    }

    public bool GetIsEssential()
    {
        return IsEssential;
    }

    public void PrintInfo()
    {
        string info = "";
        info += Index + "; ";
        info += IsEssential + "; ";
        foreach (var effect in Effects)
        {
            info += effect.Key + "->" + effect.Value + "; ";
        }
        print(info);
    }
}
