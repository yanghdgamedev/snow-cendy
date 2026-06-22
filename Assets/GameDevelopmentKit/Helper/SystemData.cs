using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SystemData
{
    private static Dictionary<ResourceType, ResourceTypeEvent> resourceListeners;

    private static string KEY_LEVEL = "Level";
    private static string KEY_REMOVE_ADS = "RemoveAds";
    private static string KEY_FIRST_OPEN = "FirstOpen";
    // private static string KEY_LAST_OPEN_DATE = "LastOpenDate";
    private static string KEY_COUNT_OPEN_IN_ONE_DAY = "CountOpenInOneDay";
    private static string KEY_COUNT_APP_OPEN = "CountAppOpen";
    private static string KEY_SOUND_ON = "SoundOn";
    private static string KEY_MUSIC_ON = "MusicOn";
    private static string KEY_VIBRATION_ON = "VibrationOn";
    private static string PREFIX_RESOURCE = "Resource_";
    public static int CountOpenInOneDay
    {
        get => PlayerPrefs.GetInt(KEY_COUNT_OPEN_IN_ONE_DAY, 0);
        set => PlayerPrefs.SetInt(KEY_COUNT_OPEN_IN_ONE_DAY, value);
    }
    public static int CountAppOpen
    {
        get => PlayerPrefs.GetInt(KEY_COUNT_APP_OPEN, 0);
        set => PlayerPrefs.SetInt(KEY_COUNT_APP_OPEN, value);
    }
    
    private static void SetDateTime(string key, DateTime value) {
        DateTimeOffset dateTimeOffset = new DateTimeOffset(value);
        PlayerPrefs.SetString(key, dateTimeOffset.ToUnixTimeSeconds().ToString());
    }

    private static DateTime GetDateTime(string key, DateTime defaultValue) {
        string strValue = PlayerPrefs.GetString(key, string.Empty);
        if (string.IsNullOrEmpty(strValue)) {
            return defaultValue;
        }
        
        if (long.TryParse(strValue, out long unixTime)) {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTime);
            return dateTimeOffset.ToLocalTime().DateTime;
        }
        
        return defaultValue;
    }
    
    public static bool FirstOpen
    {
        get => PlayerPrefs.GetInt(KEY_FIRST_OPEN, 0) == 1;
        set => PlayerPrefs.SetInt(KEY_FIRST_OPEN, value? 1:0);
    }
    
    public static bool RemoveAds
    {
        get => PlayerPrefs.GetInt(KEY_REMOVE_ADS, 0) == 1;
        set => PlayerPrefs.SetInt(KEY_REMOVE_ADS, value? 1:0);
    }
    
    public static int Level
    {
        get => PlayerPrefs.GetInt(KEY_LEVEL, 1);
        set => PlayerPrefs.SetInt(KEY_LEVEL, value);
    }

    public static int Coin
    {
        get => GetTotalResource(ResourceType.Coin);
        set => SetTotalResource(ResourceType.Coin, value);
    }

    public static int PercentFeatureHideBall
    {
        get => PlayerPrefs.GetInt("PercentFeatureHideBall", 0);
        set => PlayerPrefs.SetInt("PercentFeatureHideBall", value);
    }

    public static void SetTotalResource(ResourceType resourceType, int total)
    {
        string resourceKey = GetResourceKey(resourceType);
        int currentResourceCount = PlayerPrefs.GetInt(resourceKey);
        PlayerPrefs.SetInt(resourceKey, total);
        if (currentResourceCount != total)
        {
            OnResourceTypeChange(resourceType);
        }
    }
    

    public static void RegisterResourceListener(ResourceType resourceType, UnityAction action)
    {
        if (resourceListeners == null)
        {
            resourceListeners = new Dictionary<ResourceType, ResourceTypeEvent>();
        }

        ResourceTypeEvent e = null;
        if (!resourceListeners.ContainsKey(resourceType))
        {
            e = new ResourceTypeEvent();
            resourceListeners[resourceType] = e;
        }
        else
        {
            e = resourceListeners[resourceType];
        }

        e.AddListener(action);
    }

    public static void RemoveResourceListener(ResourceType resourceType, UnityAction action)
    {
        if (resourceListeners != null && resourceListeners.ContainsKey(resourceType))
        {
            ResourceTypeEvent e = resourceListeners[resourceType];
            e.RemoveListener(action);
        }
    }

    private static void OnResourceTypeChange(ResourceType resourceType)
    {
        if (resourceListeners != null && resourceListeners.ContainsKey(resourceType))
        {
            resourceListeners[resourceType].Invoke();
        }
    }
    

    public static string GetResourceKey(ResourceType resourceType)
    {
        return PREFIX_RESOURCE + resourceType;
    }
    
    public static int GetTotalResource(ResourceType resourceType, int def = 0)
    {
        return PlayerPrefs.GetInt(GetResourceKey(resourceType), def);
    }
    
    public static bool SoundOn
    {
        get => PlayerPrefs.GetInt(KEY_SOUND_ON, 1) == 1;
        set => PlayerPrefs.SetInt(KEY_SOUND_ON, value? 1:0);
    }
    
    public static bool MusicOn
    {
        get => PlayerPrefs.GetInt(KEY_MUSIC_ON, 1) == 1;
        set => PlayerPrefs.SetInt(KEY_MUSIC_ON, value? 1:0);
    }
    
    public static bool VibrationOn
    {
        get => PlayerPrefs.GetInt(KEY_VIBRATION_ON, 1) == 1;
        set => PlayerPrefs.SetInt(KEY_VIBRATION_ON, value? 1:0);
    }
}
public enum ResourceType
{
    None = 0,
    Coin = 1,
}


public class ResourceTypeEvent : UnityEvent
{
}