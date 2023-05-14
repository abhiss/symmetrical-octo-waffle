using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [Header("Settings")]
    public float activationThreshold = 0.9f;
    public float fadeDuration = 0.5f;

    [Header("Canvas and Points")]
    public GameObject[] canvases;
    public GameObject[] lookObjects;

    private CinematicPan cinematicPan;
    private CanvasGroup[] canvasGroups;
    private int currentCanvasIndex;

    void Start()
    {
        cinematicPan = Camera.main.GetComponent<CinematicPan>();

        canvasGroups = new CanvasGroup[canvases.Length];
        for (int i = 0; i < canvases.Length; i++)
        {
            canvasGroups[i] = canvases[i].GetComponent<CanvasGroup>();
            canvasGroups[i].alpha = 0;
            canvases[i].SetActive(false);
        }

        currentCanvasIndex = 0;
        ActivateCanvas(currentCanvasIndex);
    }

    void Update()
    {
        int newIndex = cinematicPan.GetIndex();
        if (newIndex != currentCanvasIndex)
        {
            DeactivateCanvas(currentCanvasIndex);
            ActivateCanvas(newIndex);
            currentCanvasIndex = newIndex;
        }
    }
    public void ActivateCanvas(int index)
    {
        StartCoroutine(FadeCanvas(index, true, fadeDuration));
    }

    private void DeactivateCanvas(int index)
    {
        StartCoroutine(FadeCanvas(index, false, fadeDuration));
    }

    private IEnumerator FadeCanvas(int index, bool fadeIn, float duration)
    {
        float startTime = Time.time;
        float endTime = startTime + duration;
        float alpha = fadeIn ? 0 : 1;
        float targetAlpha = fadeIn ? 1 : 0;

        canvases[index].SetActive(true);

        while (Time.time <= endTime)
        {
            float t = (Time.time - startTime) / duration;
            canvasGroups[index].alpha = Mathf.Lerp(alpha, targetAlpha, t);
            yield return null;
        }

        canvasGroups[index].alpha = targetAlpha;

        if (!fadeIn)
        {
            canvases[index].SetActive(false);
        }
    }
}

