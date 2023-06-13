using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    //the actual ratio of full the bar should be after the animation ends
    [SerializeField] private float value = 0f;
    [SerializeField] private AnimationCurve valueTransitionCurve;
    [SerializeField] private float valueTransitionTime = 0.75f;
    [SerializeField] private float valueChangeThresholdForAnimation = 0.02f;
    private float transitionStartTime = 0.0f;
    private float ratioAtStart = 0.0f;
    private float currentRatio = 0.0f;

    [SerializeField] private GameObject valueSurface;
    [SerializeField] private GameObject transitionSurface;


    void Start()
    {
        this.valueSurface.transform.localScale.Set(0.01f, 1f, 1f);
        this.transitionSurface.transform.localScale.Set(0.01f, 1f, 1f);
    }

    public void ChangeValue(float targetRatio)
    {
        targetRatio = Mathf.Clamp(targetRatio, 0.0f, 1.0f);
        var ratioChange = targetRatio - this.currentRatio;
        this.ratioAtStart = this.currentRatio;
        this.value = targetRatio;

        if (0.0f < ratioChange && ratioChange < this.valueChangeThresholdForAnimation) 
        {
            SetLocalScaleX(this.valueSurface, this.value);
            SetLocalScaleX(this.transitionSurface, this.value);
        }
        else if (this.value > this.ratioAtStart)
        {
            this.transitionStartTime = Time.time;
            SetLocalScaleX(this.valueSurface, this.currentRatio);
            SetLocalScaleX(this.transitionSurface, this.value);
        }
        else
        {
            this.transitionStartTime = Time.time;
            SetLocalScaleX(this.valueSurface, this.value);
            SetLocalScaleX(this.transitionSurface, this.currentRatio);
        }
    }

    
    void Update()
    {
        if ( (Time.time - this.transitionStartTime) < this.valueTransitionTime)
        {
            if(this.value > this.ratioAtStart)
            {           
                //bar fills
                //transition bar is behind and longer than value bar
                //value bar grows to match transition bar   
                var growRange = this.value - this.ratioAtStart;
                var ratioOfTimeDone = (Time.time - this.transitionStartTime + 0.00001f) / this.valueTransitionTime;
                var ratioOfTransition = growRange * this.valueTransitionCurve.Evaluate(ratioOfTimeDone);
                SetLocalScaleX(this.valueSurface, this.ratioAtStart + ratioOfTransition);
                this.currentRatio = this.ratioAtStart + ratioOfTransition;
            }
            else
            {
                //bar empties
                //transition bar is behind and longer than value bar
                //transition bar shrinks to match value bar 
                //this.ValueSurface.transform.localScale = 0.01f;   
                var growRange = this.ratioAtStart - this.value;
                var ratioOfTimeDone =  ((Time.time - this.transitionStartTime) / this.valueTransitionTime);
                var ratioOfTransition = growRange * this.valueTransitionCurve.Evaluate(ratioOfTimeDone);
                SetLocalScaleX(this.transitionSurface, this.ratioAtStart - ratioOfTransition);
                this.currentRatio = this.ratioAtStart - ratioOfTransition;
            }
        }
    }

    static private void SetLocalScaleX(GameObject go, float value)
    {
        var localScale = go.transform.localScale;
        localScale.x = value;
        go.transform.localScale = localScale;        
    }
}
