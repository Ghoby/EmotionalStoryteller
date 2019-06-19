using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Using the state machine interface
public class StorytellingYarnfileNode : MonoBehaviour
{
    int Index;
    bool IsEssential;    
    string Body;
    string Title;
    public List<KeyValuePair<int, float>> Effects;

    float GlobalEffect;

    public StorytellingYarnfileNode(int index, string body)
    {
        Index = index;
        Body = ExtractCoreBody(body);
        Title = ExtractTitle(body);

        IsEssential = CheckIfEssential();
        Effects = FetchEffects();
        GlobalEffect = GenerateGlobalEffectValue();
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

    List<KeyValuePair<int, float>> FetchEffects()
    {
        var list = new List<KeyValuePair<int, float>>();
        string[] splitBody = Body.Split('\n');

        for(int i = 0; i < splitBody.Length; i++)
        {
            if (i >= 5)
            {
                if(splitBody[i].Contains("<<OBJECTIVE"))
                {
                    var effectsInterior = splitBody[i].Replace("\r", "").Replace("<<", "").Replace(">>", "").Replace(" ", "");
                    var effectArray = effectsInterior.Split(';');

                    float effectValue = 0f;
                    for (int j = 0; j < effectArray.Length; j++)
                    {
                        var effect = effectArray[j].Split(':');
                        float currentValue = float.Parse(effect[1], System.Globalization.CultureInfo.InvariantCulture);
                        
                        if(effect[0].ToUpper() != effect[0])
                        {
                            currentValue *= 0.5f;
                        }
                        effectValue += currentValue;
                    }
                    list.Add(new KeyValuePair<int, float>(i, effectValue));
                }
            }
        }
        print("x");

        return list;

        //var tagSection = Body.Split('\n')[1].Replace("tags: ", "");
        //var effectsSection = tagSection.Split('|')[2].Trim();
        //var effectsInterior = effectsSection.Split('\n')[0].Replace("[", "").Replace("]", "").Replace(" ", "");
        //var effectArray = effectsInterior.Split(';');

        //var list = new List<KeyValuePair<string, float>>();
        //for(int i = 0; i < effectArray.Length; i++)
        //{
        //    var effect = effectArray[i].Split(':');            
        //    list.Add(new KeyValuePair<string, float>(effect[0], 
        //        float.Parse(effect[1], System.Globalization.CultureInfo.InvariantCulture)));
        //}

        //return list;
    }

    public List<KeyValuePair<int, float>> GetEffects()
    {
        return Effects;
    }

    float GenerateGlobalEffectValue()
    {
        float result = 0f;

        for(int i = 0; i < Effects.Count; i++)
        {
            float obj1 = 0f;
            float obj2 = 0f;
            float obj3 = 0f;

            if (i - 2 >= 0)
            {
                obj1 = Effects[i - 2].Value;
            }
            if (i - 1 >= 0)
            {
                obj2 = Effects[i - 1].Value;
            }
            obj3 = Effects[i].Value;
            result += obj3 + StorytellingManager.Instance.GetMoodVariationValue(obj1, obj2, obj3);
        }

        return result;
    }

    public float GetGlobalEffect()
    {
        return GlobalEffect;
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
