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
        string[] files = StandaloneFileBrowser.OpenFilePanel(title, directory, extensions, false);

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
        Toggle activeToggle = StorytellingManager.Instance.UIManager.GetActiveToggle();
        bool isHappy = (activeToggle.gameObject.name == "HappyButton");

        StorytellingManager.Instance.UIManager.SetYarnfileSelector(false);
        StorytellingManager.Instance.UIManager.SetToneSelector(false);

        StorytellingManager.Instance.InitiateStorytellingProcess(isHappy);
    }

    public void PlayButton()
    {
        // TO DO
    }
}
