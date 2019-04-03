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
    private int MaxNarrativeNum = 10;

    // yarnfile related attributes
    string OriginalPath = "Yarnfiles/KnightPlot.yarn"; // TO DO - get it from interface input
    string SolutionPath = "Assets/Storytelling/Resources/Yarnfiles/NewYarnfile.yarn.txt";
    TextAsset YarnfileAsset;
    Dictionary<int, StorytellingYarnfileNode> Nodes;

    bool GoapInitiated;
    bool GoapFinished;

    void Awake()
    {
        /*
        plot = new Yarn.Dialogue(new Yarn.MemoryVariableStore())
        {
            LogDebugMessage = delegate (string message)
            {
                DebugLog.Log(message);
            },
            LogErrorMessage = delegate (string message)
            {
                DebugLog.Err(message);
            }
        };

        plot.LoadFile("C:/Users/Duarte Ferreira/Documents/_tese/EmotionalStoryteller/Assets/Storytelling/Resources/Yarnfiles/preview.yarn.txt");
        */

        // TO DO -> VER SE PARTE ACIMA É NECESSÁRIA; EU PREFIRO USAR UMA IMPLEMENTAÇÃO SÓ MINHA, TBH


             
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

        Nodes = new Dictionary<int, StorytellingYarnfileNode>();

        ReadYarnfile();
        List<string> lines = new List<string>(YarnfileAsset.text.Split('\n'));
        CreateNodesFromYarnfile(lines);

        // PLANNING PHASE //////////////////////
        //InitiateGoap();

        List<KeyValuePair<int, StorytellingYarnfileNode>>[] narratives = GenerateRandomNarratives();

    }

    void Update()
    {
        if(GoapFinished)
        {
            // TO DO: COMPLETE REST OF FINAL OPERATIONS
            CreateOutputYarnfile();
        }
    }

    void ReadYarnfile()
    {
        print(OriginalPath);
        YarnfileAsset = (TextAsset) Resources.Load(OriginalPath);
    }

    void CreateOutputYarnfile()
    {
        StreamWriter writer = new StreamWriter(SolutionPath, true);

        //TO DO - get it from new dictionary OR in the proper order of reading
        foreach (KeyValuePair<int, StorytellingYarnfileNode> node in Nodes)
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
        Nodes.Add(index, node);
    }

    List<KeyValuePair<int, StorytellingYarnfileNode>>[] GenerateRandomNarratives()
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>>[] randomNarrativeArray = new List<KeyValuePair<int, StorytellingYarnfileNode>>[MaxNarrativeNum];
        for(int i = 0; i < MaxNarrativeNum; i++)
        {
            randomNarrativeArray[i] = GenerateSingleRandomNarrative();
        }

        randomNarrativeArray[0][0].Value.PrintInfo();
        randomNarrativeArray[1][0].Value.PrintInfo();
        randomNarrativeArray[4][0].Value.PrintInfo();
        randomNarrativeArray[6][0].Value.PrintInfo();

        return randomNarrativeArray;
    }

    List<KeyValuePair<int, StorytellingYarnfileNode>> GenerateSingleRandomNarrative()
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>> narrative = new List<KeyValuePair<int, StorytellingYarnfileNode>>();
        var randomValueGen = new System.Random();

        foreach (KeyValuePair<int, StorytellingYarnfileNode> node in Nodes)
        {
            if(node.Value.GetIsEssential())
            {
                narrative.Add(new KeyValuePair<int, StorytellingYarnfileNode>(node.Value.GetIndex(), node.Value));
            }
            else
            {
                var randomValue = randomValueGen.Next(2);
                if(randomValue == 1)
                {
                    narrative.Add(new KeyValuePair<int, StorytellingYarnfileNode>(node.Value.GetIndex(), node.Value));
                }
            }
        }

        return narrative;
    }

    void InitiateGoap()
    {
        StorytellingAgentObj.SetActive(true);
        StorytellingPlanningManagerObj.SetActive(true);

        GoapInitiated = true;
    }

    public void GOAPFinished()
    {
        CreateOutputYarnfile();
    }
}
