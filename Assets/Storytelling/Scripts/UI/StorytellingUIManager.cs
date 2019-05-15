using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class StorytellingUIManager : MonoBehaviour
{
    Image YarnfileSelectorObject;
    Image ToneSelectorObject;
    Text ProcessingFileObject;
    Button PlayButton;
    ToggleGroup Group;
    
    public StorytellingUIManager(Image YarnfileSelector, Image ToneSelector, Text ProcessingFileText, Button Play)
    {
        YarnfileSelectorObject = YarnfileSelector;
        ToneSelectorObject = ToneSelector;
        ProcessingFileObject = ProcessingFileText;
        PlayButton = Play;

        Group = ToneSelectorObject.gameObject.GetComponentInChildren<ToggleGroup>();
        ToneSelectorObject.gameObject.GetComponentInChildren<Toggle>().isOn = true;
        YarnfileSelectorObject.gameObject.GetComponentInChildren<InputField>().text = "";

        SetYarnfileSelector(true);
        SetToneSelector(false);
        SetProcessingFile(false);
    }

    public void SetYarnfileSelector(bool isActive)
    {
        YarnfileSelectorObject.gameObject.SetActive(isActive);
    }

    public bool CheckIfYarnfileSelectorIsActive()
    {
        return YarnfileSelectorObject.gameObject.activeSelf;
    }

    public void SetToneSelector(bool isActive)
    {
        ToneSelectorObject.gameObject.SetActive(isActive);
    }

    public bool CheckIfToneSelectorIsActive()
    {
        return ToneSelectorObject.gameObject.activeSelf;
    }

    public void SetProcessingFile(bool isActive)
    {
        ProcessingFileObject.gameObject.SetActive(isActive);
    }

    public bool CheckIfProcessingFileIsActive()
    {
        return ProcessingFileObject.gameObject.activeSelf;
    }

    public void SetPlayButton(bool isActive)
    {
        PlayButton.gameObject.SetActive(isActive);
    }

    public bool CheckIfPlayButtonIsActive()
    {
        return PlayButton.gameObject.activeSelf;
    }

    public void SetProcessingFileText(string text, bool activatePlayButton)
    {
        ProcessingFileObject.text = text;
        SetProcessingFile(true);
        SetPlayButton(activatePlayButton);
    }

    public Toggle GetActiveToggle()
    {
        List<Toggle> toggles = new List<Toggle>(Group.ActiveToggles());
        return toggles[0];
    }

    public string GetInputPath()
    {
        return YarnfileSelectorObject.gameObject.GetComponentInChildren<InputField>().text;
    }

    public void SetInputPath(string path)
    {        
        YarnfileSelectorObject.gameObject.GetComponentInChildren<InputField>().text = path;
    }
}
