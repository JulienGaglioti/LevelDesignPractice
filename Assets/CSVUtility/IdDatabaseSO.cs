using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "ID Database")]
public class IdDatabaseSO : SerializedScriptableObject
{
    public List<IdSO> List;
    public Dictionary<string, IdSO> Dictionary = new Dictionary<string, IdSO>();

    [Button]
    public void MakeDictionary()
    {
        Dictionary = new Dictionary<string, IdSO>();

        foreach (var element in List)
        {
            if (Dictionary.TryGetValue(element.Id, out IdSO ScriptableObject))
            {
                Dictionary[element.Id] = ScriptableObject;
            }
            else
            {
                Dictionary.Add(element.Id, element);
            }
        }
    }
}
