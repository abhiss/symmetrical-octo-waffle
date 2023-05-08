using UnityEngine;
using UnityEngine.UI;

public class LightBrightnessControl : MonoBehaviour
{
    public Slider brightnessSlider;
    public Light directionalLight;
    public Light pointLight;

    private float defaultDirectionalLightIntensity;
    private float defaultPointLightIntensity;

    void Start()
    {
        defaultDirectionalLightIntensity = directionalLight.intensity;
        defaultPointLightIntensity = pointLight.intensity;
        brightnessSlider.onValueChanged.AddListener(AdjustBrightness);
    }

    void AdjustBrightness(float value)
    {
        directionalLight.intensity = defaultDirectionalLightIntensity * value;
        pointLight.intensity = defaultPointLightIntensity * value;
    }
}
