using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class BrightnessControl : MonoBehaviour
{
    public Slider brightnessSlider;
    public PostProcessVolume postProcessVolume;

    private PostProcessProfile brightnessProfile;
    private ColorGrading colorGrading;

    void Start()
    {
        brightnessProfile = postProcessVolume.profile;
        colorGrading = brightnessProfile.GetSetting<ColorGrading>();
        brightnessSlider.onValueChanged.AddListener(AdjustBrightness);
    }

    void AdjustBrightness(float value)
    {
        Debug.Log("Brightness value: " + value);
        colorGrading.postExposure.value = value;
    }
}
