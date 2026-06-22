using System.Collections;
using System.Collections.Generic;
using GameDevelopmentKit.Scripts;
using HDG.BaseGameController;
using HDG.EventDispatcher;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(BaseGameController))]
public class LevelLoader : MonoBehaviour
{
    [SerializeField] private string path;
    [SerializeField] protected int level;
    [SerializeField] protected BaseLevel levelLoaded;
    protected UnityAction<object> OnBeginLoad;
    protected UnityAction<object> OnFinishLoad;
    protected UnityAction<object> OnBeginDestroy;
    protected UnityAction<object> OnFinishDestroy;
    
    public bool IsLevelLoaded => levelLoaded != null;

    /// <summary>The level prefab instance currently loaded (null if none). Index is <see cref="level"/>.</summary>
    public BaseLevel CurrentLevel => levelLoaded;
    public int CurrentLevelIndex => level;

    public void RegisterListenerBeginLoad(UnityAction<object> action)
    {
        OnBeginLoad += action;
    }

    public void RegisterListenerFinishLoad(UnityAction<object> action)
    {
        OnFinishLoad += action;
    }

    public void RegisterListenerBeginDestroy(UnityAction<object> action)
    {
        OnBeginDestroy += action;
    }

    public void RegisterListenerFinishDestroy(UnityAction<object> action)
    {
        OnFinishDestroy += action;
    }

    public void RemoveListenerBeginLoad(UnityAction<object> action)
    {
        OnBeginLoad -= action;
    }

    public void RemoveListenerFinishLoad(UnityAction<object> action)
    {
        OnFinishLoad -= action;
    }

    public void RemoveListenerBeginDestroy(UnityAction<object> action)
    {
        OnBeginDestroy -= action;
    }

    public void RemoveListenerFinishDestroy(UnityAction<object> action)
    {
        OnFinishDestroy -= action;
    }

    public BaseLevel LoadLevel(int level)
    {
        if (levelLoaded != null)
        {
            DestroyLevel();
        }

        OnBeginLoad?.Invoke(level);
        var levelLoad = Resources.Load<BaseLevel>(path + level);
        this.levelLoaded = Instantiate(levelLoad);
        this.level = level;
        OnFinishLoad?.Invoke(level);
        return levelLoaded;
    }

    public virtual void DestroyLevel()
    {
        if (levelLoaded != null)
        {
            OnBeginDestroy?.Invoke(level);
            levelLoaded.gameObject.SetActive(false);
            Destroy(levelLoaded.gameObject);
            OnFinishDestroy?.Invoke(level);
            level = 0;
            levelLoaded = null;
            this.PostEvent(EventID.OnDestroyLevel);
        }
    }
}