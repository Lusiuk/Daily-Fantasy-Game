using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Outcome
{
    [System.Serializable]
    public struct FlagChange
    {
        public string flagName;
        public bool value;
    }

    public FlagChange[] flagChanges;    // установить/снять флаги
    public Sprite newSprite;            // новый спрайт для объекта (если нужно)
    public GameObject targetObject;     // объект, который нужно активировать/деактивировать
    public bool setActive = true;       // что сделать с targetObject
    public UnityEvent onComplete;       // дополнительные действия
}