using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Core;
using System.IO;

public class StorytellingYarnfileReader : MonoBehaviour
{
    TextAsset text;

    void Start()
    {
        string a = ReadYarnfile();
        Debug.Log(a);
    }

    string ReadYarnfile()
    {
        TextAsset text = (TextAsset) Resources.Load("Yarnfiles/WaitExampleTest.yarn");
        print(text);
        return text.text;
    }
}
