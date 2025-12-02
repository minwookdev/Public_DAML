using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "CoffeeCat/Scriptable Object/PlayerAbilityData")]
public class PlayerAbilityData : ScriptableObject 
{
    [SerializeField] private List<PlayerAbility> playerAbilities = new();
    public List<PlayerAbility> Datas => playerAbilities;
}

[Serializable]
public class PlayerAbility
{
    public PlayerStatEnum type;
    public int Index;
    public string AbilityName;
    public int MaxLevel;
    public float Value;
    public int Price;
    public int PriceIncrease;
    public string Description;
    
    public bool IsMaxLevel(int currentLevel)
    {
        return MaxLevel == currentLevel;
    }
}
