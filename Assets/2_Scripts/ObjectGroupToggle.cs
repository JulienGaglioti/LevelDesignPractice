using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGroupToggle : MonoBehaviour
{
    public List<GameObject> GameObjectsToToggle;
    private ButtonObject _linkedButton;
    private List<bool> _objectsStartingStates = new List<bool>();
    private void Awake()
    {
        _linkedButton = GetComponentInChildren<ButtonObject>();
        foreach (var item in GameObjectsToToggle)
        {
            if (item != null)
            {
                _objectsStartingStates.Add(item.activeSelf);
            }
            else
            {
                _objectsStartingStates.Add(true);
            }
        }
        if (_linkedButton)
        {
            _linkedButton.OnToggle += ToggleObjects;
        }
    }
    private void OnDestroy()
    {
        if (_linkedButton)
        {
            _linkedButton.OnToggle -= ToggleObjects;
        }
    }
    public void ToggleObjects(bool active)
    {
        for (int i = 0; i < GameObjectsToToggle.Count; i++)
        {
            if (GameObjectsToToggle[i] != null)
            {
                bool newState;
                newState = active ? !_objectsStartingStates[i] : _objectsStartingStates[i];
                GameObjectsToToggle[i].SetActive(newState);
            }
        }
    }
}
