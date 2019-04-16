using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class StorytellingUIManager : MonoBehaviour
{
    Image YarnfileSelectorObject;
    Image ToneSelectorObject;
    ToggleGroup Group;
    
    public StorytellingUIManager(Image YarnfileSelector, Image ToneSelector)
    {
        YarnfileSelectorObject = YarnfileSelector;
        ToneSelectorObject = ToneSelector;
        Group = ToneSelectorObject.gameObject.GetComponentInChildren<ToggleGroup>();
        ToneSelectorObject.gameObject.GetComponentInChildren<Toggle>().isOn = true;
        YarnfileSelectorObject.gameObject.GetComponentInChildren<InputField>().text = StorytellingManager.Instance.OriginalPath;

        SetYarnfileSelector(true);
        SetToneSelector(false);
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
