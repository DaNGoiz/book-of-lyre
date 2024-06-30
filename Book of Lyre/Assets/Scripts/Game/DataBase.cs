using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Store datas for Unity settings
/// </summary>
public class DataBase : MonoBehaviour
{
    /// <summary>
    /// Layer names
    /// </summary>
    public struct LayerName
    {
        public const string defaultName = "Default";
        public const string backgroundName = "Background";
        public const string mainLayerName = "MainLayer";
        public const string foregroundName = "Foreground";
        public const string creatureName = "Creature";
        public const string playerName = "Player";
    }
    /// <summary>
    /// Tag names
    /// </summary>
    public struct TagName
    {
        public const string playerName = "Player";
        public const string mainCameraName = "MainCamera";
        public const string gameController = "GameController";
    }
    public delegate void PlatformEventContact(GameObject gameObject);
    public delegate void PlatformEventStay(GameObject gameObject);
    public delegate void PlatformEventExit(GameObject gameObject);
}
