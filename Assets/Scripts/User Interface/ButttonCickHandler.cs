using UnityEngine;
using UnityEngine.UI;

public class ButtonClickHandler : MonoBehaviour
{
    public AudioSource buttonClickAudioSource;
    public Button[] buttons;

    void Start()
    {
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(PlayButtonClickSound);
        }
    }

    public void PlayButtonClickSound()
    {
        buttonClickAudioSource.Play();
    }
}
