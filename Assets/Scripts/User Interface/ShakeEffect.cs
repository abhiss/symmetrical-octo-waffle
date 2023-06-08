using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.7f;

    // The original position of the object
    Vector3 originalPos;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void TriggerShake()
    {
        StartCoroutine(Shake());
    }

    System.Collections.IEnumerator Shake()
    {
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        // Reset to original position
        transform.localPosition = originalPos;
    }
}
