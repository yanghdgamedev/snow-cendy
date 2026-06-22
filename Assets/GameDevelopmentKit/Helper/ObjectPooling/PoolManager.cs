using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PoolManager : SerializedMonoBehaviour
{
    public static PoolManager Instance;
    [SerializeField] private SOPrefab soPrefab;
    [ReadOnly][ShowInInspector] private Dictionary<PoolType, PoolObject> dicPools = new Dictionary<PoolType, PoolObject>();

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameObject GetObjectFromPool(PoolType poolType, bool isCreateNewObject = false)
    {
        if (!dicPools.ContainsKey(poolType))
        {
            dicPools.Add(poolType, new PoolObject(soPrefab.Prefabs[poolType], 0));
        }

        return dicPools[poolType].GetObject(isCreateNewObject);
    }
}

public enum PoolType
{
    None = 0,
}