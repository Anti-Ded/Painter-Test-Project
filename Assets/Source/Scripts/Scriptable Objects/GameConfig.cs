using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Config/GameConfig")]
public sealed class GameConfig : ScriptableObject
{
    public Color startBrushColor = Color.red;
    public int startBrushSize = 8;
    public int drawMaxCount = 32;
}

