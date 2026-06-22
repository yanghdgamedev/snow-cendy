using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Prefab Data", menuName = "ScriptableObjects/PrefabData")]
public class SOPrefab : SerializedScriptableObject
{
    public Dictionary<PoolType, GameObject> Prefabs = new Dictionary<PoolType, GameObject>();
}
