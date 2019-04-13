using System;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Core;
using System.IO;
using Yarn;
using Utilities;

public class StorytellingManager : MonoBehaviour
{
    // for singleton
    public static StorytellingManager Instance = null;

    // Unity Editor objects 
    [SerializeField]
    private GameObject StorytellingPlanningManagerObj;
    [SerializeField]
    private GameObject StorytellingAgentObj;
    [SerializeField]
    private int MaxNarrativeNum = 100;
    [SerializeField]
    private bool isHappyTone = true;

    // yarnfile related attributes
    string OriginalPath = "Yarnfiles/KnightPlot.yarn"; // TO DO - get it from interface input
    string RandomHappySolutionPath = "Assets/Storytelling/Resources/Yarnfiles/RandomHappy.yarn.txt";
    string RandomDourSolutionPath = "Assets/Storytelling/Resources/Yarnfiles/RandomDour.yarn.txt";
    TextAsset YarnfileAsset;
    List<KeyValuePair<int, StorytellingYarnfileNode>> Nodes;

    bool GoapInitiated;
    bool GoapFinished;

    void Awake()
    {
        //Check if instance already exists
        if (Instance == null)
        {
            //if not, set instance to this
            Instance = this;
        }
        //If Instance already exists and it's not this:
        else if (Instance != this)
        {
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one Instance of a GameManager.
            Destroy(gameObject);
        }
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        GoapInitiated = false;
        GoapFinished = false;

        Nodes = new List<KeyValuePair<int, StorytellingYarnfileNode>>();

        ReadYarnfile();
        List<string> lines = new List<string>(YarnfileAsset.text.Split('\n'));
        CreateNodesFromYarnfile(lines);

        // PLANNING PHASE //////////////////////
        //InitiateGoap();


        //List<KeyValuePair<int, StorytellingYarnfileNode>>[] randomNarratives = GenerateRandomNarratives();
        //List<KeyValuePair<int, StorytellingYarnfileNode>> bestNarrativeHappy = GetBestRandomNarrativeOmissionOnly(randomNarratives, true);
        //List<KeyValuePair<int, StorytellingYarnfileNode>> bestNarrativeDour = GetBestRandomNarrativeOmissionOnly(randomNarratives, false);
        //CreateOutputYarnfile(bestNarrativeHappy, RandomHappySolutionPath);
        //CreateOutputYarnfile(bestNarrativeDour, RandomDourSolutionPath);

        List<KeyValuePair<int, StorytellingYarnfileNode>>[] EssentialOnlyNarratives = GenerateEssentialOnlyNarratives();
    }

    void Update()
    {
        if(GoapFinished)
        {
            // TO DO: COMPLETE REST OF FINAL OPERATIONS
            //CreateOutputYarnfile();
        }
    }

    void ReadYarnfile()
    {
        print(OriginalPath);
        YarnfileAsset = (TextAsset) Resources.Load(OriginalPath);
    }

    void CreateOutputYarnfile(List<KeyValuePair<int, StorytellingYarnfileNode>> narrative, string solutionPath)
    {
        StreamWriter writer = new StreamWriter(solutionPath, true);

        //TO DO - get it from new dictionary OR in the proper order of reading
        foreach (KeyValuePair<int, StorytellingYarnfileNode> node in narrative)
        {
            writer.Write(node.Value.GetBody());
        }
        writer.Flush();
        writer.Close();
        print("Done");
    }


    // version that creates nodes whose index is the number by which it appeared in the yarnfile
    //void CreateNodesFromYarnfile(List<string> lines)
    //{
    //    int index = 0;
    //    string nodeTemp = "";

    //    foreach (string line in lines)
    //    {
    //        if (line.Contains("title: "))
    //        {
    //            index++;
    //        }
    //        nodeTemp += line + "\n"; // stack node
    //        if (line == "===\r" || line == "===") // flush node
    //        {
    //            StorytellingYarnfileNode yarnNode = new StorytellingYarnfileNode(index, nodeTemp);
    //            AddNode(index, yarnNode);
    //            nodeTemp = "";
    //        }
    //    }
    //}


    // version that creates nodes whose index in the dictionary is the timestamp they contain
    void CreateNodesFromYarnfile(List<string> lines)
    {
        int nodeTimestamp = 0;
        int lineCounter = 0;
        string nodeTemp = "";
        
        foreach(string line in lines)
        {
            if (line.Contains("title: "))
            {
                nodeTimestamp = Convert.ToInt32(lines[lineCounter + 1].Replace("tags: ", "").Split('|')[0].Trim());
            }
            nodeTemp += line + "\n"; // stack node
            if (line == "===\r" || line == "===") // flush node
            {
                StorytellingYarnfileNode yarnNode = new StorytellingYarnfileNode(nodeTimestamp, nodeTemp);
                AddNode(nodeTimestamp, yarnNode);
                nodeTemp = "";
            }

            lineCounter++;
        }
    }

    string GetNameForNode(string firstLine)
    {
        string name = firstLine.Replace("title: ", string.Empty);
        name = name.Replace("\r", string.Empty);
        return name;
    }

    void AddNode(int index, StorytellingYarnfileNode node)
    {
        Nodes.Add(new KeyValuePair<int, StorytellingYarnfileNode>(index, node));
    }

    

    void InitiateGoap()
    {
        StorytellingAgentObj.SetActive(true);
        StorytellingPlanningManagerObj.SetActive(true);

        GoapInitiated = true;
    }

    public void GOAPFinished()
    {
        //CreateOutputYarnfile();
    }

    float GetNodeEffectValue(List<KeyValuePair<string, float>> Effects)
    {
        float obj = 0f;

        foreach (var effect in Effects)
        {
            if (effect.Key.ToUpper() == effect.Key)
            {
                obj += effect.Value;
            }
            else
            {
                obj += effect.Value * 0.5f;
            }
        }
        return obj;
    }

    float EvaluateNarrative(List<KeyValuePair<int, StorytellingYarnfileNode>> narrative)
    {
        float objectiveValue = 0f;
        List<float> tempObjValues = new List<float>();
        for (int i = 0; i < narrative.Count; i += 3)
        {
            float obj1 = 0f;
            float obj2 = 0f;
            float obj3 = 0f;
            int groupNodeCount = 1;
            tempObjValues.Clear();

            if (narrative[i].Value.Effects.Count > 0)
            {
                obj1 = GetNodeEffectValue(narrative[i].Value.Effects);
            }
            if (i+1 < narrative.Count && narrative[i].Value.Effects.Count > 0)
            {
                obj2 = GetNodeEffectValue(narrative[i].Value.Effects);
                groupNodeCount++;
            }
            if (i+2 < narrative.Count && narrative[i].Value.Effects.Count > 0)
            {
                obj3 = GetNodeEffectValue(narrative[i].Value.Effects);
                groupNodeCount++;
            }

            // evaluation 1
            if( groupNodeCount == 1 || (groupNodeCount == 2 && obj1 == obj2) || (groupNodeCount == 3 && obj1 == obj2 && obj2 == obj3) )
            {
                objectiveValue += groupNodeCount * obj1;
            }
            // evaluation 2
            else if( groupNodeCount == 2 && obj1 != obj2 )
            {
                objectiveValue += groupNodeCount * (obj2 - obj1);
            }
            // evaluation 2
            else if( groupNodeCount == 3 && ( (obj1 <= obj2 && obj2 <= obj3) || (obj1 >= obj2 && obj2 >= obj3) ) )
            {
                objectiveValue += groupNodeCount * (obj3 - obj1);
            }
            // evaluation 3
            else if (groupNodeCount == 3 && ((obj1 <= obj2 && obj2 >= obj3) || (obj1 >= obj2 && obj2 <= obj3)))
            {
                objectiveValue += groupNodeCount * (obj1 + obj3) * 0.5f;
            }
        }

        return objectiveValue;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////// Random Generation of Narratives (Omission Only) ///////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    List<KeyValuePair<int, StorytellingYarnfileNode>>[] GenerateRandomNarratives()
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>>[] randomNarrativeArray = new List<KeyValuePair<int, StorytellingYarnfileNode>>[MaxNarrativeNum];
        for (int i = 0; i < MaxNarrativeNum; i++)
        {
            randomNarrativeArray[i] = GenerateSingleRandomNarrative();
        }

        return randomNarrativeArray;
    }

    List<KeyValuePair<int, StorytellingYarnfileNode>> GenerateSingleRandomNarrative()
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>> narrative = new List<KeyValuePair<int, StorytellingYarnfileNode>>();

        foreach (KeyValuePair<int, StorytellingYarnfileNode> node in Nodes)
        {
            if (node.Value.GetIsEssential())
            {
                narrative.Add(new KeyValuePair<int, StorytellingYarnfileNode>(node.Value.GetIndex(), node.Value));
            }
            else
            {
                var randomValue = UnityEngine.Random.Range(0, 2);
                if (randomValue == 1)
                {
                    narrative.Add(new KeyValuePair<int, StorytellingYarnfileNode>(node.Value.GetIndex(), node.Value));
                }
            }
        }

        return narrative;
    }

    List<KeyValuePair<int, StorytellingYarnfileNode>> GetBestRandomNarrativeOmissionOnly(List<KeyValuePair<int, StorytellingYarnfileNode>>[] narratives, bool isHappy)
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>> bestNarrative = null;
        float bestObjectiveValue = 0f;
        float currentObjectiveValue = 0f;
        for (int i = 0; i < narratives.Length; i++)
        {
            currentObjectiveValue = EvaluateNarrative(narratives[i]);
            if ((isHappy && currentObjectiveValue >= bestObjectiveValue) || (!isHappy && currentObjectiveValue <= bestObjectiveValue))
            {
                bestNarrative = narratives[i];
                bestObjectiveValue = currentObjectiveValue;
            }
        }

        if (bestNarrative != null)
        {
            if (isHappy)
                print("best objective value happy: " + bestObjectiveValue);
            else
                print("best objective value dour: " + bestObjectiveValue);
            return bestNarrative;
        }
        else
        {
            print("null");
        }
        return new List<KeyValuePair<int, StorytellingYarnfileNode>>();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////// Generation of Narratives (Omission and Reorganizing) //////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    List<KeyValuePair<int, StorytellingYarnfileNode>>[] GenerateEssentialOnlyNarratives()
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>>[] randomNarrativeArray = new List<KeyValuePair<int, StorytellingYarnfileNode>>[MaxNarrativeNum];
        for (int i = 0; i < MaxNarrativeNum; i++)
        {
            randomNarrativeArray[i] = GenerateSingleEssentialOnlyNarrative();
        }

        return randomNarrativeArray;
    }

    List<KeyValuePair<int, StorytellingYarnfileNode>> GenerateSingleEssentialOnlyNarrative()
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>> narrative = new List<KeyValuePair<int, StorytellingYarnfileNode>>();

        foreach (KeyValuePair<int, StorytellingYarnfileNode> node in Nodes)
        {
            if (node.Value.GetIsEssential())
            {
                narrative.Add(new KeyValuePair<int, StorytellingYarnfileNode>(node.Value.GetIndex(), node.Value));
            }
        }

        return narrative;
    }

    List<KeyValuePair<int, StorytellingYarnfileNode>> GetBestRandomNarrativeReorganize(List<KeyValuePair<int, StorytellingYarnfileNode>> narrative, bool isHappy)
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>> bestNarrative = new List<KeyValuePair<int, StorytellingYarnfileNode>>(narrative);
        float currentObjectiveValue = 0f;
        float bestObjectiveValue = 0f;
        int bestIndex = 0;

        if (isHappy)
        {
            bestObjectiveValue = float.NegativeInfinity;
        }
        else
        {
            bestObjectiveValue = float.PositiveInfinity;
        }

        foreach(var node in Nodes)
        {
            currentObjectiveValue = EvaluateNarrative(narrative);
            if(!node.Value.GetIsEssential())
            {
                for(int i = 0; i <= narrative.Count; i++)
                {
                    bestNarrative.Insert(i, node);
                    float temp = EvaluateNarrative(bestNarrative);
                    if((isHappy && temp >=  bestObjectiveValue) || (!isHappy && temp <= bestObjectiveValue))
                    {
                        bestObjectiveValue = temp;
                        bestIndex = i;
                    }
                    bestNarrative.RemoveAt(i);
                }

                bestNarrative.Insert(bestIndex, node);
            }
        }

        return bestNarrative;
    }

}
