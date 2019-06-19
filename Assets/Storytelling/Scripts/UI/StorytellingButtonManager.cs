using UnityEngine;
using UnityEngine.UI;
using SFB;

public class StorytellingButtonManager : MonoBehaviour
{
    [SerializeField] private InputField _inputField;
    [SerializeField] private Text _titleText;

    private string _name;
    private string _title;

    public string DialogDirectory { get; set; }
    public string DialogExtensions { get; set; }

    bool isHappy;
    bool isEmotional;


    void SetName(string submitedName)
    {
        string path;

        path = submitedName;
#if UNITY_STANDALONE_OSX
		path = _name.Replace("file://", "");
		path = _name.Replace("%20", " ");
#endif
        StorytellingManager.Instance.UIManager.SetInputPath(path);
    }

    public void BrowseFiles()
    {
        string title = string.IsNullOrEmpty(_title) ? "Select file" : _title;
        string directory = string.IsNullOrEmpty(DialogDirectory) ? string.Empty : DialogDirectory;
        string extensions = string.IsNullOrEmpty(DialogExtensions) ? string.Empty : DialogExtensions;
        string[] files = StandaloneFileBrowser.OpenFilePanel(title, directory, "yarn.txt", false);

        if (files.Length > 0 && !string.IsNullOrEmpty(files[0]))
        {
            SetName(files[0]);
        }
    }

    public void NextButton()
    {
        string path = StorytellingManager.Instance.UIManager.GetInputPath();

        if(path != null && path != "")
        {
            StorytellingManager.Instance.SetYarnfilePath(path);
            StorytellingManager.Instance.UIManager.SetYarnfileSelector(false);
            StorytellingManager.Instance.UIManager.SetToneSelector(true);
        }        
    }

    public void Next2Button()
    {
        Toggle activeToggle = StorytellingManager.Instance.UIManager.GetMoodActiveToggle();
        isHappy = (activeToggle.gameObject.name == "HappyButton");

        StorytellingManager.Instance.UIManager.SetYarnfileSelector(false);
        StorytellingManager.Instance.UIManager.SetToneSelector(false);
        StorytellingManager.Instance.UIManager.SetNarratorSelector(true);
    }

    public void Next3Button()
    {
        Toggle activeToggle = StorytellingManager.Instance.UIManager.GetNarratorActiveToggle();
        isEmotional = (activeToggle.gameObject.name == "EmotionalButton");

        StorytellingManager.Instance.UIManager.SetYarnfileSelector(false);
        StorytellingManager.Instance.UIManager.SetToneSelector(false);
        StorytellingManager.Instance.UIManager.SetNarratorSelector(false);

        StorytellingManager.Instance.InitiateStorytellingProcess(isHappy, isEmotional);
    }

    public void PlayButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Preview", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
