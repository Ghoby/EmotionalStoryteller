using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Core;
using System.IO;
using Yarn;
using Utilities;

public class StorytellingYarnfileManager : MonoBehaviour
{
    string OriginalPath = "Yarnfiles/test.yarn"; // TO DO - get it from interface input
    string SolutionPath = "Assets/Storytelling/Resources/Yarnfiles/NewYarnfile.yarn.txt";
    TextAsset YarnfileAsset;
    Yarn.Dialogue plot;
    Dictionary<int, StorytellingYarnfileNodeScript> Nodes;

    bool CanCreateOutput;

    void Start()
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

        CanCreateOutput = false;

        Nodes = new Dictionary<int, StorytellingYarnfileNodeScript>();

        ReadYarnfile();
        List<string> lines = new List<string>(YarnfileAsset.text.Split('\n'));
        CreateNodesFromYarnfile(lines);
    }

    void Update()
    {
        
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
        foreach (KeyValuePair<int, StorytellingYarnfileNodeScript> node in Nodes)
        {
            writer.Write(node.Value.GetBody());
        }
        writer.Flush();
        writer.Close();
        print("Done");
    }

    void CreateNodesFromYarnfile(List<string> lines)
    {
        int index = 0;
        string nodeTemp = "";
        
        foreach(string line in lines)
        {
            if (line.Contains("title: "))
            {
                index++;
            }
            nodeTemp += line + "\n"; // stack node
            if (line == "===\r" || line == "===") // flush node
            {
                StorytellingYarnfileNodeScript yarnNode = new StorytellingYarnfileNodeScript(index, nodeTemp);
                AddNode(index, yarnNode);

                nodeTemp = "";
            }
        }

        //print(Nodes["Start.Start"].GetBody());
        //print(Nodes["Grades.good"].GetBody());        
    }

    string GetNameForNode(string firstLine)
    {
        string name = firstLine.Replace("title: ", string.Empty);
        name = name.Replace("\r", string.Empty);
        return name;
    }

    void AddNode(int index, StorytellingYarnfileNodeScript node)
    {
        Nodes.Add(index, node);
    }

    public void GOAPFinished()
    {
        CreateOutputYarnfile();
    }
}
