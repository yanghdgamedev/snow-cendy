using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class PoolObject
{
    [SerializeField] private Stack<GameObject> stack = new Stack<GameObject>();
    [SerializeField] private GameObject baseObj;
    [SerializeField] private GameObject tmp;
    [SerializeField] private ReturnPoolObject returnPoolObject;
    
    public PoolObject(GameObject baseObj, int count)
    {
        this.baseObj = baseObj;
        stack = new Stack<GameObject>();
        for (int i = 0; i < count; i++)
        {
            tmp = Object.Instantiate(baseObj);
            tmp.SetActive(false);
            returnPoolObject = tmp.AddComponent<ReturnPoolObject>();
            returnPoolObject.poolObject = this;
            stack.Push(tmp);
        }
    }
    
    public GameObject GetObject(bool isCreateNewObject = false)
    {
        if (stack.Count == 0 || isCreateNewObject)
        {
            tmp = Object.Instantiate(baseObj);
            returnPoolObject = tmp.AddComponent<ReturnPoolObject>();
            returnPoolObject.poolObject = this;
            return tmp;
        }
        return stack.Pop();
    }
    
    public void AddToPool(GameObject obj)
    {
        stack.Push(obj);
    }
}
