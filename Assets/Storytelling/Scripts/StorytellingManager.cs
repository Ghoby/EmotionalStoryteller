using System;
using System.Collections.Generic;
using UnityEngine;
using ReGoap.Core;
using System.IO;
using Yarn;
using UnityEngine.UI;

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
    private int MaxNarrativeNum = 10000;

    // UI elements & related
    [SerializeField]
    private Image YarnfileSelector; // set in editor
    [SerializeField]
    private Image ToneSelector; // set in editor
    [SerializeField]
    private Text ProcessingFileText; // set in editor
    [SerializeField]
    private Button PlayButton; // set in editor

    readonly string FileIsProcessingText = "File is processing.\n\nPlease wait...";
    readonly string FileIsCompletedText = "Processing complete!";

    // yarnfile related attributes
    public string OriginalPath = "C:/Users/Duarte Ferreira/Documents/_tese/EmotionalStoryteller/Assets/Storytelling/Resources/Yarnfiles/PlaceholderFile.yarn.txt";
    string HappySolutionPath = "Assets/Storytelling/Resources/Yarnfiles/RandomHappy.yarn.txt";
    string DourSolutionPath = "Assets/Storytelling/Resources/Yarnfiles/RandomDour.yarn.txt";
    string SolutionPath = "StorytellingSolutions/";
    TextAsset YarnfileAsset;
    List<KeyValuePair<int, StorytellingYarnfileNode>> Nodes;
    public bool isHappyTone;
    
    bool GoapInitiated;
    bool GoapFinished;

    public StorytellingUIManager UIManager;

    int NarrativesIndex;

    readonly float EqualVariationLimit = 0.2f;
    KeyValuePair<float, float>[] NarrativeMoodVariationArray; // array that will be used in the emotional variation of narrators
    List<KeyValuePair<float, float>> NarrativeMoodVariationIntraNodesList;

    readonly string MariaNeutralExpression = "<<Feel Maria Neutral 0.8 None>>\n";    
    readonly string MariaHappyExpression = "<<Feel Maria Neutral 0.8 None>>\n<<Feel Maria Happiness 0.8 None>>\n";
    readonly string MariaSadExpression = "<<Feel Maria Neutral 0.8 None>>\n<<Feel Maria Sadness 0.8 None>>\n";
    readonly string JoaoNeutralExpression = "<<Feel Joao Neutral 0.8 None>>\n";
    readonly string JoaoFearExpression = "<<Feel Joao Neutral 0.8 None>>\n<<Feel Joao Fear 0.3 None>>\n";
    readonly string JoaoSurpriseExpression = "<<Feel Joao Neutral 0.8 None>>\n<<Feel Joao Surprise 0.5 None>>\n";

    readonly int UntilNodPeriodMin = 2;
    readonly int UntilNodPeriodMax = 7;
    readonly int NodPeriod = 1;
    readonly string Nod = "<<Nod ";
    readonly string NodStart = " Start>>";
    readonly string NodStop = " Stop>>";

    void Awake()
    {
        // DELETE OR COMMENT WHEN DONE ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        DeleteMemory();

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
        UIManager = new StorytellingUIManager(YarnfileSelector, ToneSelector, ProcessingFileText, PlayButton);

        if(PlayerPrefs.GetString("OriginalPath") != string.Empty)
        {
            UIManager.SetInputPath(PlayerPrefs.GetString("OriginalPath"));
        }

        if (!Directory.Exists("Assets/" + SolutionPath))
        {            
            Directory.CreateDirectory("Assets/" + SolutionPath);
            print("Solutions folder created");
        }

        // PLANNING PHASE //////////////////////
        //InitiateGoap();
    }
    
    public void InitiateStorytellingProcess(bool isHappy)
    {
        isHappyTone = isHappy;
        
        List<string> lines = new List<string>(ReadYarnfile().Split('\n'));
        CreateNodesFromYarnfile(lines);

        if(isHappyTone)
        {
            print("Happy");
        }
        else
        {
            print("Dour");
        }

        // Update UI
        UIManager.SetProcessingFileText(FileIsProcessingText, false);

        InitiateNarrativeGeneration();
    }

    public void SetYarnfilePath(string path)
    {
        OriginalPath = path;
    }

    public void SetNarrativeTone(bool isHappy)
    {
        isHappyTone = isHappy;
    }

    string ReadYarnfile()
    {
        print(OriginalPath);

        StreamReader reader = new StreamReader(OriginalPath);
        return reader.ReadToEnd();
    }

    void CreateOutputYarnfile(List<KeyValuePair<int, StorytellingYarnfileNode>> narrative, string solutionPath)
    { 
        //FixNarratorOrder(ref narrative, false);
        FixNarratorOrder(ref narrative, true);
        AddNarratorEmotionalExpressions(ref narrative);
        FixNarrativeConnections(ref narrative);        

        print(solutionPath);

        StreamWriter writer = new StreamWriter("Assets/" + solutionPath, false);
        
        foreach (KeyValuePair<int, StorytellingYarnfileNode> node in narrative)
        {
            writer.Write(node.Value.GetBody());
        }
        writer.Flush();
        writer.Close();
        print("Done");

        WriteInMemory(solutionPath, narrative[0].Value.GetTitle());

        // UI updated
        UIManager.SetProcessingFileText(FileIsCompletedText, true);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////// FILE FIXES BEFORE OUTPUT ////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void FixNarrativeConnections(ref List<KeyValuePair<int, StorytellingYarnfileNode>> narrative)
    {
        for(int i = 0; i < narrative.Count; i++)
        {
            if(i < narrative.Count - 1)
            {
                narrative[i].Value.SetTransitions(narrative[i + 1].Value.GetTitle(), false);
            }
            else
            {
                narrative[i].Value.SetTransitions("", true);
            }
        }
    }

    void AddNarratorEmotionalExpressions(ref List<KeyValuePair<int, StorytellingYarnfileNode>> narrative)
    {
        int effectIndex = 0;

        for (int i = 0, n = 0; i < narrative.Count; i++)
        {
            string[] nodeLines = narrative[i].Value.GetBody().Split('\n');
            string newBody = "";

            float obj3 = NarrativeMoodVariationIntraNodesList[n].Value;

            for (int j = 0; j < nodeLines.Length - 1; j++)
            {
                // ADD FACIAL EXPRESSIONS AND BUBBLE COMMANDS
                if (j >= 5)
                {
                    if (nodeLines[j].Contains("<<OBJECTIVE"))
                    {
                        if (NarrativeMoodVariationIntraNodesList[effectIndex].Value >= 1.5f)
                        {
                            newBody += JoaoSurpriseExpression;
                        }
                        else if (NarrativeMoodVariationIntraNodesList[effectIndex].Value <= -1.5f)
                        {
                            newBody += JoaoFearExpression;
                        }
                        else
                        {
                            newBody += JoaoNeutralExpression;
                        }

                        if (NarrativeMoodVariationIntraNodesList[effectIndex].Key >= 0.1f)
                        {
                            newBody += MariaHappyExpression;
                        }                       
                        else if (NarrativeMoodVariationIntraNodesList[effectIndex].Key <= -0.1f)
                        {
                            newBody += MariaSadExpression;
                        }
                        else
                        {
                            newBody += MariaNeutralExpression;
                        }
                        effectIndex++;
                    }
                }
                newBody += nodeLines[j] + "\n";
            }
            narrative[i].Value.SetBody(newBody);
        }
    }

    void FixNarratorOrder(ref List<KeyValuePair<int, StorytellingYarnfileNode>> narrative, bool useMoodInNarration)
    {
        if(useMoodInNarration)
        {
            FixNarratorOrderWithMood(ref narrative);
            return;
        }
        FixNarratorOrderSimple(ref narrative);
    }

    void FixNarratorOrderSimple(ref List<KeyValuePair<int, StorytellingYarnfileNode>> narrative)
    {
        string[] narratorNames = { "Maria", "Joao" };
        int nameIndex = UnityEngine.Random.Range(0, 2);
        int linesUntilNod = UnityEngine.Random.Range(UntilNodPeriodMin, UntilNodPeriodMax);
        int linesUntilNodStop = NodPeriod;
        bool nodNeedsClosing = false;
        string currentNoddingNPC = "";

        foreach (var node in narrative)
        {
            string currentName = narratorNames[nameIndex % 2] + ":";
            string[] nodeLines = node.Value.GetBody().Split('\n');
            string newBody = "";

            // Until nodeLines.Length-2 because nodeLines.Length-1 is "", so there's no use in checking it out
            for (int i = 0; i < nodeLines.Length - 1; i++)
            {
                // First condition checks if the loop as passed the header section of the file
                // Second condition checks if the line is in fact a interior tag
                if(i >= 5)
                {
                    if(!nodeLines[i].Contains("<<"))
                    {
                        nodeLines[i] = currentName + nodeLines[i].Split(new char[] { ':' }, 2)[1];
                    }
                    else
                    {
                        linesUntilNod++; // so this line doesn't count in the nodding pattern
                    }

                    AddNodAux(ref newBody, currentName, narratorNames, ref linesUntilNod, ref linesUntilNodStop, ref currentNoddingNPC, ref nodNeedsClosing);

                }
                newBody += nodeLines[i] + "\n";
            }

            if (nodNeedsClosing)
            {
                linesUntilNodStop = 0;
                AddNodAux(ref newBody, currentName, narratorNames, ref linesUntilNod, ref linesUntilNodStop, ref currentNoddingNPC, ref nodNeedsClosing);
            }

            node.Value.SetBody(newBody);
            nameIndex++;
        }        
    }

    void FixNarratorOrderWithMood(ref List<KeyValuePair<int, StorytellingYarnfileNode>> narrative)
    {
        string[] narratorNames = { "Maria", "Joao" };
        string currentName = "";
        int effectIndex = 0;

        int linesUntilNod = UnityEngine.Random.Range(UntilNodPeriodMin, UntilNodPeriodMax);
        int linesUntilNodStop = NodPeriod;
        bool nodNeedsClosing = false;
        string currentNoddingNPC = "";

        for (int i = 0; i < narrative.Count; i++)
        {
            string[] nodeLines = narrative[i].Value.GetBody().Split('\n');
            string newBody = "";

            if(i == 0)
            {
                if (NarrativeMoodVariationIntraNodesList[i].Key >= 0.1f)       { currentName = narratorNames[0] + ":"; }
                else if (NarrativeMoodVariationIntraNodesList[i].Key <= -0.1f)  { currentName = narratorNames[1] + ":"; }
                else                                               { currentName = narratorNames[UnityEngine.Random.Range(0, 2)] + ":"; }
            }

            for (int j = 0; j < nodeLines.Length - 1; j++)
            {
                // CHANGE NARRATORS ACCORDING WITH CURRENT MOOD
                if (j >= 5)
                {
                    if (!nodeLines[j].Contains("<<"))
                    {
                        nodeLines[j] = currentName + nodeLines[j].Split(new char[] { ':' }, 2)[1];
                    }
                    else
                    {
                        if (nodeLines[j].Contains("<<OBJECTIVE"))
                        {
                            if (NarrativeMoodVariationIntraNodesList[effectIndex].Value >= 1.5f ||
                            NarrativeMoodVariationIntraNodesList[effectIndex].Key >= 0.1f)
                            {
                                currentName = narratorNames[0] + ":";
                            }
                            else if (NarrativeMoodVariationIntraNodesList[effectIndex].Value <= -1.5f ||
                                     NarrativeMoodVariationIntraNodesList[effectIndex].Key <= 0.1f)
                            {
                                currentName = narratorNames[1] + ":";
                            }
                        }                        
                        effectIndex++;
                        linesUntilNod++; // so this line doesn't count in the nodding pattern
                    }

                    AddNodAux(ref newBody, currentName, narratorNames, ref linesUntilNod, ref linesUntilNodStop, ref currentNoddingNPC, ref nodNeedsClosing);          
                }

                newBody += nodeLines[j] + "\n";
            }

            if(nodNeedsClosing)
            {
                linesUntilNodStop = 0;
                AddNodAux(ref newBody, currentName, narratorNames, ref linesUntilNod, ref linesUntilNodStop, ref currentNoddingNPC, ref nodNeedsClosing);
            }

            narrative[i].Value.SetBody(newBody);
        }        
    }

    void AddNodAux(ref string newBody, string currentName, string[] narratorNames, ref int linesUntilNod, ref int linesUntilNodStop, ref string currentNoddingNPC, ref bool nodNeedsClosing)
    {
        if (linesUntilNodStop == 0)
        {
            newBody += Nod + currentNoddingNPC + NodStop + "\n";

            linesUntilNod = UnityEngine.Random.Range(UntilNodPeriodMin, UntilNodPeriodMax);
            linesUntilNodStop = NodPeriod;
            nodNeedsClosing = false;
        }
        else if (linesUntilNod <= 0)
        {
            if (linesUntilNod == 0 && !nodNeedsClosing)
            {
                if (currentName == narratorNames[0] + ":")
                {
                    newBody += Nod + narratorNames[1] + NodStart + "\n";
                    currentNoddingNPC = narratorNames[1];
                }
                else
                {
                    newBody += Nod + narratorNames[0] + NodStart + "\n";
                    currentNoddingNPC = narratorNames[0];
                }
                nodNeedsClosing = true;
            }
            if(linesUntilNod < 0)
            {
                linesUntilNodStop--;
            }            
        }

        linesUntilNod--;
    }


    // version that creates nodes whose index in the dictionary is the timestamp they contain
    // NEEDS SECURITY CHECKS
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

        Nodes.Sort(SortByTimestamp);
    }

    static int SortByTimestamp(KeyValuePair<int, StorytellingYarnfileNode> n1, KeyValuePair<int, StorytellingYarnfileNode> n2)
    {
        return n1.Key.CompareTo(n2.Key);
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

    void InitiateNarrativeGeneration()
    {
        /// RANDOM APPROACH

        //List<KeyValuePair<int, StorytellingYarnfileNode>>[] randomNarratives = GenerateRandomNarratives();
        //List<KeyValuePair<int, StorytellingYarnfileNode>> bestNarrativeHappy = GetBestNarrative(randomNarratives, true);
        //List<KeyValuePair<int, StorytellingYarnfileNode>> bestNarrativeDour = GetBestNarrative(randomNarratives, false);
        //CreateOutputYarnfile(bestNarrativeHappy, RandomHappySolutionPath);
        //CreateOutputYarnfile(bestNarrativeDour, RandomDourSolutionPath);

        /// RANDOM WITH REORGANIZATION

        //List<KeyValuePair<int, StorytellingYarnfileNode>>[] EssentialOnlyNarratives = GenerateEssentialOnlyNarratives(MaxNarrativeNum);
        //List<KeyValuePair<int, StorytellingYarnfileNode>> bestNarrative = GetBestRandomNarrativeReorganize(EssentialOnlyNarratives, isHappyTone);
        //foreach (var node in bestNarrative)
        //{
        //    node.Value.PrintInfo();
        //}
        

        // BRUTE FORCE (OMISSION ONLY)

        List<KeyValuePair<int, StorytellingYarnfileNode>> bestNarrative = BruteForceBestNarrativeOmissionOnly(isHappyTone);
        foreach(var node in bestNarrative)
        {
            node.Value.PrintInfo();
        }

        string solutionPath = SolutionPath;
        if (isHappyTone)
        {
            solutionPath += "_Happy";
        }
        else
        {
            solutionPath += "_Dour";
        }
        CreateOutputYarnfile(bestNarrative, solutionPath + "SolutionNarrative.yarn.txt");
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////// NARRATIVE EMOTIONAL EVALUATION ///////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    float GetNodeEffectValue(StorytellingYarnfileNode node)
    {
        return node.GetGlobalEffect();
    }

    public float GetMoodVariationValue(float obj1, float obj2, float obj3)
    {
        float result = 0f;

        if(obj2 - obj1 > EqualVariationLimit)
        {
            if(obj3 - obj2 > EqualVariationLimit)
            {
                result = 2f;
            }
            else if (obj3 - obj2 < -EqualVariationLimit)
            {
                result = -3f;
            }
            else
            {
                result = 1f;
            }
        }
        else if(obj2 - obj1 < -EqualVariationLimit)
        {
            if (obj3 - obj2 > EqualVariationLimit)
            {
                result = 3f;
            }
            else if (obj3 - obj2 < -EqualVariationLimit)
            {
                result = -2f;
            }
            else
            {
                result = -1f;
            }
        }
        else
        {
            if (obj3 - obj2 > EqualVariationLimit)
            {
                result = 1f;
            }
            else if (obj3 - obj2 < -EqualVariationLimit)
            {
                result = -1f;
            }
            else
            {
                result = 0f;
            }
        }

        return result;
    }

    float EvaluateNarrativeInterNodes(List<KeyValuePair<int, StorytellingYarnfileNode>> narrative, bool registerVariation)
    {
        float objectiveValue = 0f;
        if(registerVariation)
        {
            NarrativeMoodVariationArray = new KeyValuePair<float, float>[(int) narrative.Count];
        }
        
        for (int i = 0; i < narrative.Count; i++)
        {
            float obj1 = 0f;
            float obj2 = 0f;
            float obj3 = 0f;
                                   
            if (i - 2 >= 0 && narrative[i - 2].Value.Effects.Count > 0)
            {
                obj1 = GetNodeEffectValue(narrative[i - 2].Value);
            }
            if (i - 1 >= 0 && narrative[i - 1].Value.Effects.Count > 0)
            {
                obj2 = GetNodeEffectValue(narrative[i - 1].Value);
            }
            if (narrative[i].Value.Effects.Count > 0)
            {
                obj3 = GetNodeEffectValue(narrative[i].Value);
            }

            float varVal = GetMoodVariationValue(obj1, obj2, obj3);                        
            objectiveValue += obj3 + varVal;

            if (registerVariation)
            {               
                NarrativeMoodVariationArray[i] = new KeyValuePair<float, float>(obj3, obj3 + varVal);
            }
        }
        return objectiveValue;
    }

    void EvaluateNarrativeIntraNodes(List<KeyValuePair<int, StorytellingYarnfileNode>> narrative)
    {
        NarrativeMoodVariationIntraNodesList = new List<KeyValuePair<float, float>>();
        var auxList = new List<KeyValuePair<int, float>>();

        for (int i = 0; i < narrative.Count; i++)
        {
            auxList.AddRange(narrative[i].Value.GetEffects());
        }

        for (int i = 0; i < auxList.Count; i++)
        {
            float obj1 = 0f;
            float obj2 = 0f;
            float obj3 = 0f;

            if (i - 2 >= 0)
            {
                obj1 = auxList[i - 2].Value;
            }
            if (i - 1 >= 0)
            {
                obj2 = auxList[i - 1].Value;
            }
            obj3 = auxList[i].Value;

            float varVal = GetMoodVariationValue(obj1, obj2, obj3);
            NarrativeMoodVariationIntraNodesList.Add(new KeyValuePair<float, float>(obj3, obj3 + varVal));
        }
    }

    List<KeyValuePair<int, StorytellingYarnfileNode>> GetBestNarrative(List<KeyValuePair<int, StorytellingYarnfileNode>>[] narratives, bool isHappy)
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>> bestNarrative = null;
        float bestObjectiveValue = 0f;
        float currentObjectiveValue = 0f;
        int bestIndex = 0;
        for (int i = 0; i < narratives.Length; i++)
        {
            //currentObjectiveValue = EvaluateNarrativeInterNodesSimple(narratives[i]);

            if (i == 7349)
            {
                print("x");
            }

            currentObjectiveValue = EvaluateNarrativeInterNodes(narratives[i], false);
            if ((isHappy && currentObjectiveValue >= bestObjectiveValue) || (!isHappy && currentObjectiveValue <= bestObjectiveValue))
            {
                bestNarrative = narratives[i];
                bestObjectiveValue = currentObjectiveValue;
                bestIndex = i;
            }
        }

        if (bestNarrative != null)
        {
            EvaluateNarrativeInterNodes(bestNarrative, true);
            EvaluateNarrativeIntraNodes(bestNarrative);

            if (isHappy)
                print("best objective value happy: " + bestObjectiveValue);
            else
                print("best objective value dour: " + bestObjectiveValue);

            print("best index: " + bestIndex);
            return bestNarrative;
        }
        else
        {
            print("null");
        }
        return new List<KeyValuePair<int, StorytellingYarnfileNode>>();
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

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////// Generation of Narratives (Omission and Reorganizing) //////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    List<KeyValuePair<int, StorytellingYarnfileNode>>[] GenerateEssentialOnlyNarratives(int max)
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>>[] randomEssentialNarrativeArray = new List<KeyValuePair<int, StorytellingYarnfileNode>>[max];
        for (int i = 0; i < MaxNarrativeNum; i++)
        {
            randomEssentialNarrativeArray[i] = GenerateOneTypeOnlyNarrative(true);
        }

        return randomEssentialNarrativeArray;
    }

    List<KeyValuePair<int, StorytellingYarnfileNode>> GenerateOneTypeOnlyNarrative(bool getEssential)
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>> narrative = new List<KeyValuePair<int, StorytellingYarnfileNode>>();

        foreach (KeyValuePair<int, StorytellingYarnfileNode> node in Nodes)
        {
            if (node.Value.GetIsEssential() == getEssential)
            {
                narrative.Add(new KeyValuePair<int, StorytellingYarnfileNode>(node.Value.GetIndex(), node.Value));
            }
        }

        return narrative;
    }

    List<KeyValuePair<int, StorytellingYarnfileNode>> GetBestRandomNarrativeReorganize(List<KeyValuePair<int, StorytellingYarnfileNode>>[] narratives, bool isHappy)
    {
        float[] bestObjectiveValuesArray = new float[narratives.Length];    

        for(int i = 0; i < narratives.Length; i++)
        {
            float bestObjectiveValue = 0f;
            int bestIndex = 0;

            if (isHappy) { bestObjectiveValue = float.NegativeInfinity; }
            else { bestObjectiveValue = float.PositiveInfinity; }

            foreach (var node in Nodes)
            {
                if (UnityEngine.Random.Range(0, 2) == 1 && !node.Value.GetIsEssential())
                {
                    for (int j = 0; j <= narratives[i].Count; j++)
                    {
                        narratives[i].Insert(j, node);
                        float temp = EvaluateNarrativeInterNodes(narratives[i], false);
                        if ((isHappy && temp > bestObjectiveValue) || (!isHappy && temp < bestObjectiveValue))
                        {
                            bestObjectiveValue = temp;
                            bestIndex = j;
                        }
                        narratives[i].RemoveAt(j);
                    }
                    narratives[i].Insert(bestIndex, node);                    
                }
            }
            bestObjectiveValuesArray[i] = bestObjectiveValue;
        }

        var max = bestObjectiveValuesArray[0];
        var maxIndex = 0;
        for(int i = 1; i < bestObjectiveValuesArray.Length; i++)
        {
            if((isHappy && bestObjectiveValuesArray[i] > max) || (!isHappy && bestObjectiveValuesArray[i] < max))
            {
                max = bestObjectiveValuesArray[i];
                maxIndex = i;
            }
        }

        return narratives[maxIndex];
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////// All Possibilities (Brute-Force approach) //////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public double integerFactorialRecursion(int number)
    {        
        double result = 1;
        while (number > 1)
        {
            result = result * number;
            number = number - 1;
        }
        return result;
    }

    double CalculateFullPossibilitySpaceSize()
    {
        double result = 0;
        List<KeyValuePair<int, StorytellingYarnfileNode>> essentialOnlyNarrative = GenerateOneTypeOnlyNarrative(true);
        List<KeyValuePair<int, StorytellingYarnfileNode>> nonEssentialOnlyNarrative = GenerateOneTypeOnlyNarrative(false);

        int n = essentialOnlyNarrative.Count;
        int m = nonEssentialOnlyNarrative.Count;

        for (int i = 1; i <= m; i++)
        {
            double temp = 0;
            for(int j = 0; j <= n || j < i; j++)
            {
                temp += (n + 1) * integerFactorialRecursion(i - j) * (integerFactorialRecursion(n) / integerFactorialRecursion(n - i));
            }
            temp *= integerFactorialRecursion(m) / (integerFactorialRecursion(i) * integerFactorialRecursion(m - i));
            result += temp;
        }

        print(result);
        return result;
    }    

    List<KeyValuePair<int, StorytellingYarnfileNode>> BruteForceBestNarrativeOmissionOnly(bool isHappy)
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>> nonEssentialOnlyNarrative = GenerateOneTypeOnlyNarrative(false);
        List<KeyValuePair<int, StorytellingYarnfileNode>>[] narratives = new List<KeyValuePair<int, StorytellingYarnfileNode>>[(int)Math.Pow(2, nonEssentialOnlyNarrative.Count)];
        narratives[0] = new List<KeyValuePair<int, StorytellingYarnfileNode>>();
        NarrativesIndex = 0;

        OmissionRecursiveGeneration(ref narratives, new List<KeyValuePair<int, StorytellingYarnfileNode>>(Nodes), 0, 0);

        return GetBestNarrative(narratives, isHappyTone);
    }

    void OmissionRecursiveGeneration(ref List<KeyValuePair<int, StorytellingYarnfileNode>>[] narrativesArray, 
        List<KeyValuePair<int, StorytellingYarnfileNode>> nodes, int index, int aux)
    {
        if(index == nodes.Count - 1)
        {
            if (!nodes[index].Value.GetIsEssential())
            {
                NarrativesIndex++;
                narrativesArray[NarrativesIndex] = new List<KeyValuePair<int, StorytellingYarnfileNode>>(narrativesArray[NarrativesIndex - 1]);
            }            
            narrativesArray[NarrativesIndex].Add(nodes[index]);
        }
        else if(index < nodes.Count - 1)
        {
            if(nodes[index].Value.GetIsEssential())
            {
                narrativesArray[NarrativesIndex].Add(nodes[index]);
                OmissionRecursiveGeneration(ref narrativesArray, nodes, index + 1, aux + 1);
            }
            else
            {
                OmissionRecursiveGeneration(ref narrativesArray, nodes, index + 1, aux);
                NarrativesIndex++;
                narrativesArray[NarrativesIndex] = new List<KeyValuePair<int, StorytellingYarnfileNode>>();
                for(int x = 0; x < aux; x++)
                {
                    narrativesArray[NarrativesIndex].Add(narrativesArray[NarrativesIndex - 1][x]);
                }
                narrativesArray[NarrativesIndex].Add(nodes[index]);
                OmissionRecursiveGeneration(ref narrativesArray, nodes, index + 1, aux + 1);
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////// Memory Storage Functions /////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void DeleteMemory()
    {
        PlayerPrefs.DeleteKey("OriginalPath");
        PlayerPrefs.DeleteKey("YarnfilePath");
        PlayerPrefs.DeleteKey("InitialNodeName");
    }

    public void WriteInMemory(string solutionPath, string initialNode)
    {
        PlayerPrefs.SetString("OriginalPath", OriginalPath);
        PlayerPrefs.SetString("YarnfilePath", Application.dataPath + "/" + solutionPath);
        PlayerPrefs.SetString("InitialNodeName", initialNode);

        print(PlayerPrefs.GetString("OriginalPath"));
        print(PlayerPrefs.GetString("YarnfilePath"));
        print(PlayerPrefs.GetString("InitialNodeName"));
    }
}
