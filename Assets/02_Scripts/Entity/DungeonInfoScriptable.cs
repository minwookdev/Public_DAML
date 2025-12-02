using System;
using UnityEngine;
using RandomDungeonWithBluePrint;

[CreateAssetMenu(fileName = "Dungeon_info_scriptable", menuName = "CoffeeCat/Scriptable Object/Dungeon Info Scriptable")]
public class DungeonInfoScriptable : ScriptableObject
{
    public DungeonInfo[] DungeonInfos = null;
}

[Serializable]
public class DungeonInfo
{
    public string dungeonId = "";
    public string dungeonName = "";
    public string dungeonDesc = "";
    public string dungeonIconKey = "";
    public int difficultyValue = 0;
    public DungeonBluePrint bp = null;
}
