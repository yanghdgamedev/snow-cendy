using System.Collections;
using UnityEngine;

using GameDevelopmentKit.Scripts;
using UnityEngine;

public class LoadingCanvas : BaseLoading
{
    [SerializeField] private RectTransform slider;
    
    protected override void CheckOpen()
    {
        
    }

    protected override IEnumerator OnStartLoad()
    {
        yield return null;
    }

    protected override void OnLoadDone()
    {
        
    }

    protected override void OnChangePercent(float percent)
    {
        slider.sizeDelta = new Vector2(percent * 720, slider.sizeDelta.y);
    }

    protected override bool ShouldLoadFaster()
    {
        return true;
    }
}
