using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Core;
using System.IO;

public class StorytellingYarnfileManager : MonoBehaviour
{
    string Path = "Yarnfiles/preview.yarn"; // TO DO
    TextAsset YarnfileAsset;
    Dictionary<string, StorytellingYarnfileNodeScript> Nodes;

    void Start()
    {
        Nodes = new Dictionary<string, StorytellingYarnfileNodeScript>();

        ReadYarnfile();
        List<string> lines = new List<string>(YarnfileAsset.text.Split('\n'));
        CreateNodesFromYarnfile(lines);
    }

    void ReadYarnfile()
    {
        print(Path);
        YarnfileAsset = (TextAsset) Resources.Load(Path);
    }

    void CreateNodesFromYarnfile(List<string> lines)
    {
        string nodeNameTemp = "";
        string nodeTemp = "";
        
        foreach(string line in lines)
        {
            if (line.Contains("title: "))
            {
                nodeNameTemp = GetNameForNode(line);
            }
            nodeTemp += line + "\n"; // stack node
            if (line == "===\r") // flush node
            {
                StorytellingYarnfileNodeScript yarnNode = new StorytellingYarnfileNodeScript(nodeNameTemp, nodeTemp);
                AddNode(yarnNode);

                nodeTemp = "";
            }
        }

        print(Nodes["Start.Start"].GetBody());
        print(Nodes["Grades.good"].GetBody());
    }

    string GetNameForNode(string firstLine)
    {
        string name = firstLine.Replace("title: ", string.Empty);
        name = name.Replace("\r", string.Empty);
        return name;
    }

    void AddNode(StorytellingYarnfileNodeScript node)
    {
        Nodes.Add(node.GetName(), node);
    }
}
