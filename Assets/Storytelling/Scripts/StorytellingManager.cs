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
    string SolutionPath = "/Storytelling/Resources/Yarnfiles/";
    TextAsset YarnfileAsset;
    List<KeyValuePair<int, StorytellingYarnfileNode>> Nodes;
    public bool isHappyTone;
    
    bool GoapInitiated;
    bool GoapFinished;

    public StorytellingUIManager UIManager;

    int NarrativesIndex;

    readonly float EqualVariationLimit = 0.2f;
    KeyValuePair<float, float>[] NarrativeMoodVariationArray; // array that will be used in the emotional variation of narrators

    readonly string MariaNeutralExpression = "<<Feel Maria Neutral 0.8 None>>\n";    
    readonly string MariaHappyExpression = "<<Feel Maria Neutral 0.8 None>>\n<<Feel Maria Happiness 0.8 None>>\n";
    readonly string MariaSadExpression = "<<Feel Maria Neutral 0.8 None>>\n<<Feel Maria Sadness 0.8 None>>\n";
    readonly string JoaoNeutralExpression = "<<Feel Joao Neutral 0.8 None>>\n";
    readonly string JoaoFearExpression = "<<Feel Joao Neutral 0.8 None>>\n<<Feel Joao Fear 0.8 None>>\n";
    readonly string JoaoSurpriseExpression = "<<Feel Joao Neutral 0.8 None>>\n<<Feel Joao Surprise 0.8 None>>\n";


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
        
        // PLANNING PHASE //////////////////////
        //InitiateGoap();
    }

    void Update()
    {
        
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
        FixNarratorEmotionalExpressions(ref narrative);
        FixNarrativeConnections(ref narrative);        

        print(solutionPath);

        StreamWriter writer = new StreamWriter("Assets" + solutionPath, false);
        
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

    void FixNarratorEmotionalExpressions(ref List<KeyValuePair<int, StorytellingYarnfileNode>> narrative)
    {
        for (int i = 0; i < narrative.Count; i++)
        {
            string[] nodeLines = narrative[i].Value.GetBody().Split('\n');
            string newBody = "";

            for (int j = 0; j < nodeLines.Length - 1; j++)
            {
                if (j >= 5)
                {
                    if (nodeLines[j].Contains("<<objective>>"))
                    {
                        if (NarrativeMoodVariationArray[i].Value >= 2f)
                        {
                            newBody += JoaoSurpriseExpression;
                        }
                        if(NarrativeMoodVariationArray[i].Key > 0f)
                        {
                            newBody += MariaHappyExpression;
                        }

                        if (NarrativeMoodVariationArray[i].Value <= -2f)
                        {
                            newBody += JoaoFearExpression;
                        }
                        if (NarrativeMoodVariationArray[i].Key < 0f)
                        {
                            newBody += MariaSadExpression;
                        }

                        if (NarrativeMoodVariationArray[i].Key == 0f)
                        {
                            newBody += JoaoNeutralExpression + MariaNeutralExpression;
                        }
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
        
        foreach(var node in narrative)
        {
            string currentName = narratorNames[nameIndex % 2] + ":";
            string[] nodeLines = node.Value.GetBody().Split('\n');
            string newBody = "";

            // Until nodeLines.Length-2 because nodeLines.Length-1 is "", so there's no use in checking it out
            for (int i = 0; i < nodeLines.Length - 1; i++)
            {
                // First condition checks if the loop as passed the header section of the file
                // Second condition checks if the line is in fact a interior tag
                if(i >= 5 && !nodeLines[i].Contains("<<"))
                {
                    nodeLines[i] = currentName + nodeLines[i].Split(new char[] { ':' }, 2)[1];
                }
                newBody += nodeLines[i] + "\n";
            }

            node.Value.SetBody(newBody);
            nameIndex++;
        }        
    }

    void FixNarratorOrderWithMood(ref List<KeyValuePair<int, StorytellingYarnfileNode>> narrative)
    {
        string[] narratorNames = { "Maria", "Joao" };
        string currentName = "";

        for (int i = 0; i < narrative.Count; i++)
        {
            string[] nodeLines = narrative[i].Value.GetBody().Split('\n');
            string newBody = "";

            if(i == 0)
            {
                if (NarrativeMoodVariationArray[i].Key > 0f)       { currentName = narratorNames[0] + ":"; }
                else if (NarrativeMoodVariationArray[i].Key < 0f)  { currentName = narratorNames[1] + ":"; }
                else                                               { currentName = narratorNames[UnityEngine.Random.Range(0, 2)] + ":"; }
            }            

            for (int j = 0; j < nodeLines.Length - 1; j++)
            {
                if (j >= 5)
                {
                    if (!nodeLines[j].Contains("<<"))
                    {
                        nodeLines[j] = currentName + nodeLines[j].Split(new char[] { ':' }, 2)[1];
                    }
                    else
                    {
                        if(nodeLines[j].Contains("<<objective>>"))
                        {
                            if (NarrativeMoodVariationArray[i].Value >= 2f || NarrativeMoodVariationArray[i].Key > 0f)
                            {
                                currentName = narratorNames[0] + ":";
                            }
                            else if (NarrativeMoodVariationArray[i].Value <= -2f || NarrativeMoodVariationArray[i].Key < 0f)
                            {
                                currentName = narratorNames[1] + ":";
                            }
                        }                                         
                    }                    
                }
                newBody += nodeLines[j] + "\n";
            }
            narrative[i].Value.SetBody(newBody);
        }        
    }


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
        

        /// BRUTE FORCE (OMISSION ONLY)

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

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////// NARRATIVE EVALUATION ///////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

    float GetMoodVariationValue(float obj1, float obj2, float obj3)
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

    float EvaluateNarrativeSimple(List<KeyValuePair<int, StorytellingYarnfileNode>> narrative)
    {
        float objectiveValue = 0f;
        for (int i = 0; i < narrative.Count; i += 3)
        {
            float obj1 = 0f;
            float obj2 = 0f;
            float obj3 = 0f;
            int groupNodeCount = 1;

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

    float EvaluateNarrativeBetter(List<KeyValuePair<int, StorytellingYarnfileNode>> narrative, bool registerVariation)
    {
        float objectiveValue = 0f;
        if(registerVariation)
        {
            NarrativeMoodVariationArray = new KeyValuePair<float, float>[(int) narrative.Count];
        }
        
        for (int i = 0; i < narrative.Count; i += 1)
        {
            float obj1 = 0f;
            float obj2 = 0f;
            float obj3 = 0f;
                                   
            if (i - 2 >= 0 && narrative[i].Value.Effects.Count > 0)
            {
                obj1 = GetNodeEffectValue(narrative[i - 2].Value.Effects);
            }
            else
            {
                obj1 = 0f;
            }
            if (i - 1 >= 0 && narrative[i].Value.Effects.Count > 0)
            {
                obj2 = GetNodeEffectValue(narrative[i - 1].Value.Effects);
            }
            else
            {
                obj2 = 0f;
            }
            if (narrative[i].Value.Effects.Count > 0)
            {
                obj3 = GetNodeEffectValue(narrative[i].Value.Effects);
            }

            float varVal = GetMoodVariationValue(obj1, obj2, obj3);                        
            objectiveValue += obj3 + varVal;

            if (registerVariation)
            {               
                NarrativeMoodVariationArray[i] = new KeyValuePair<float, float>(obj3, varVal);
            }
        }

        return objectiveValue;
    }


    List<KeyValuePair<int, StorytellingYarnfileNode>> GetBestNarrative(List<KeyValuePair<int, StorytellingYarnfileNode>>[] narratives, bool isHappy)
    {
        List<KeyValuePair<int, StorytellingYarnfileNode>> bestNarrative = null;
        float bestObjectiveValue = 0f;
        float currentObjectiveValue = 0f;
        int bestIndex = 0;
        for (int i = 0; i < narratives.Length; i++)
        {
            //currentObjectiveValue = EvaluateNarrativeSimple(narratives[i]);

            if (i == 7349)
            {
                print("x");
            }

            currentObjectiveValue = EvaluateNarrativeBetter(narratives[i], false);
            if ((isHappy && currentObjectiveValue >= bestObjectiveValue) || (!isHappy && currentObjectiveValue <= bestObjectiveValue))
            {
                bestNarrative = narratives[i];
                bestObjectiveValue = currentObjectiveValue;
                bestIndex = i;
            }
        }

        if (bestNarrative != null)
        {
            EvaluateNarrativeBetter(bestNarrative, true);

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
                        float temp = EvaluateNarrativeSimple(narratives[i]);
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


    // CORRIGIR
    void OmissionRecursiveGeneration(ref List<KeyValuePair<int, StorytellingYarnfileNode>>[] narrativesArray, 
        List<KeyValuePair<int, StorytellingYarnfileNode>> nodes, int index, int aux)
    {
        if(index == nodes.Count - 1)
        {
            var x = narrativesArray[NarrativesIndex];
            if (!nodes[index].Value.GetIsEssential())
            {
                NarrativesIndex++;
            }
            narrativesArray[NarrativesIndex] = new List<KeyValuePair<int, StorytellingYarnfileNode>>(narrativesArray[NarrativesIndex - 1]);
            narrativesArray[NarrativesIndex].Add(nodes[index]);
            x = narrativesArray[NarrativesIndex];
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

    // Memory storage functions

    public void DeleteMemory()
    {
        PlayerPrefs.DeleteKey("YarnfilePath");
        PlayerPrefs.DeleteKey("InitialNodeName");
    }

    public void WriteInMemory(string solutionPath, string initialNode)
    {
        PlayerPrefs.SetString("YarnfilePath", Application.dataPath + solutionPath);
        PlayerPrefs.SetString("InitialNodeName", initialNode);

        print(PlayerPrefs.GetString("YarnfilePath"));
        print(PlayerPrefs.GetString("InitialNodeName"));
    }
}
