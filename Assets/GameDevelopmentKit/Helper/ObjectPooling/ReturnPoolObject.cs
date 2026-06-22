using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnPoolObject : MonoBehaviour
{
    public PoolObject poolObject;

    private void OnDisable()
    {
        gameObject.SetActive(false);
        poolObject.AddToPool(gameObject);
    }
}
