using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CoffeeCat.Utils.Defines;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace CoffeeCat
{
    public static class Extensions
    {
        private const string N0_NON_ROUNDING_FORMAT = "#,0";
        private const string N1_NON_ROUNDING_FORMAT = "#,0.0";
        private const string N2_NON_ROUNDING_FORMAT = "#,0.00";

        private static readonly List<ItemType> resultItemTypes = new(); 
        
        public static string ToKey(this InteractionType type)
        {
            return type switch
            {
                InteractionType.DungeonNextFloor => "portal_floor",
                InteractionType.DungeonBoss      => "portal_floor_boss",
                InteractionType.DungeonShop      => "object_shop",
                InteractionType.DungeonReward    => "object_reward",
                InteractionType.EntranceTown     => "portal_town",
                InteractionType.EntranceDungeon  => "portal_dungeon",
                InteractionType.PlayerUpgrade    => "interaction_player_upgrade",
                InteractionType.PlayerAbility    => "interaction_player_ability",
                InteractionType.PlayerSelect     => "interaction_player_select",
                InteractionType.None             => throw new ArgumentOutOfRangeException(nameof(type), type, null),
                _                                => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static string ToKey(this AddressablesKey key) {
            return key switch {
                AddressablesKey.NONE                             => "",
                AddressablesKey.Effect_hit_1                     => "Effect_hit_1",
                AddressablesKey.Effect_hit_2                     => "Effect_hit_2",
                AddressablesKey.Effect_hit_3                     => "Effect_hit_3",
                AddressablesKey.Skeleton_Mage_Projectile_Default => "monster_attack_fireball",
                AddressablesKey.Skeleton_Mage_Projectile_Skill   => "monster_skill_expsphere",
                AddressablesKey.Monster_Skeleton                 => "Skeleton",
                AddressablesKey.Monster_Skeleton_Warrior         => "Skeleton_Warrior",
                AddressablesKey.Monster_Skeleton_Mage            => "Skeleton_Mage",
                AddressablesKey.GroupSpawnPositions              => "GroupSpawnPositions",
                AddressablesKey.InteractableSign                 => "InteractableIcon",
                AddressablesKey.InputCanvas                      => "Canvas_Input",
                AddressablesKey.DungeonInfoSO                    => "Dungeon_Information",
                AddressablesKey.HealthBar                        => "monster_health_bar",
                AddressablesKey.Object_Item                      => "object_item",
                AddressablesKey.Object_Start_Room                => "object_start_room",
                AddressablesKey.GateObject                       => "dungeon_door",
                _                                                => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }

        public static string ToKey(this SoundKey key) {
            return key switch {
                SoundKey.None               => "",
                SoundKey.BGM_Dungeon        => "bgm_dungeon",
                SoundKey.BGM_Town           => "bgm_town",
                SoundKey.Button_0           => "se_btn_0",
                SoundKey.Button_1           => "se_btn_1",
                SoundKey.Currency_0         => "se_item_gold_0",
                SoundKey.Currency_1         => "se_item_gold_1",
                SoundKey.Currency_2         => "se_item_gold_2",
                SoundKey.EXP_0              => "se_item_exp_0",
                SoundKey.EXP_1              => "se_item_exp_1",
                SoundKey.EXP_2              => "se_item_exp_2",
                SoundKey.Item_Consumable    => "se_item_consumable",
                SoundKey.Item_Equipment     => "se_item_equipment",
                SoundKey.Plaeyr_Enhanced    => "se_character_enhanced",
                SoundKey.Player_Evasion_0   => "se_player_evasion_0",
                SoundKey.Player_Evasion_1   => "se_player_evasion_1",
                SoundKey.Player_Hit_0       => "se_player_taken_damage_0",
                SoundKey.Monster_Skel_Hit_0 => "se_skeleton_hit_0",
                SoundKey.Monster_Skel_Hit_1 => "se_skeleton_hit_1",
                SoundKey.Dropped_Item       => "se_item_dropped",
                SoundKey.Currency_Used      => "se_currency_used",
                SoundKey.Player_Moving      => "se_player_moving",
                _                           => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }

        public static string ToKey(this PlayerAddressablesKey key)
        {
            return key switch
            {
                PlayerAddressablesKey.NONE           => "",
                PlayerAddressablesKey.Flower_Dungeon => "Flower_Dungeon",
                PlayerAddressablesKey.Flame_Dungeon  => "Flame_Dungeon",
                PlayerAddressablesKey.Clown_Dungeon  => "Clown_Dungeon",
                PlayerAddressablesKey.Flower_Town    => "Flower_Town",
                PlayerAddressablesKey.Flame_Town     => "Flame_Town",
                PlayerAddressablesKey.Clown_Town     => "Clown_Town",
                _                                    => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }

        public static string ToKey(this PlayerNormalAttackKey key)
        {
            return key switch
            {
                PlayerNormalAttackKey.NONE                => "",
                PlayerNormalAttackKey.NormalAttack_Flower => "NormalAttack_Flower",
                PlayerNormalAttackKey.NormalAttack_Flame  => "NormalAttack_Flame",
                PlayerNormalAttackKey.NormalAttack_Clown  => "NormalAttack_Clown",
                _                                         => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }

        public static string ToKey(this SceneName key)
        {
            return key switch
            {
                SceneName.DungeonScene   => "DungeonScene",
                SceneName.TownScene      => "TownScene",
                SceneName.LoadingScene   => "LoadingScene",
                SceneName.BossScene_T1_E => "BossScene_Mine",
                SceneName.BossScene_T1_N => "BossScene_Mine",
                SceneName.BossScene_T1_H => "BossScene_Mine",
                SceneName.NONE           => throw new ArgumentOutOfRangeException(nameof(key), key, null),
                _                        => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };
        }
        
        public static string ToName(this PlayerStatEnum stat)
        {
            return stat switch
            {
                PlayerStatEnum.MaxHp                => "최대 체력",
                PlayerStatEnum.Defense              => "방어력",
                PlayerStatEnum.MoveSpeed            => "이동 속도",
                PlayerStatEnum.AttackPower          => "공격력",
                PlayerStatEnum.InvincibleTime       => "무적 시간",
                PlayerStatEnum.CriticalChance       => "치명타 확률",
                PlayerStatEnum.CriticalResistance   => "치명타 저항",
                PlayerStatEnum.CriticalMultiplier   => "치명타 배수",
                PlayerStatEnum.DamageDeviation      => "대미지 격차",
                PlayerStatEnum.Penetration          => "관통력",
                PlayerStatEnum.ItemAcquisitionRange => "아이템 획득 범위",
                PlayerStatEnum.CoolTimeReduce       => "쿨타임 감소",
                PlayerStatEnum.SearchRange          => "탐색 범위",
                PlayerStatEnum.AddProjectile        => "추가 발사체",
                _                                   => throw new ArgumentOutOfRangeException()
            };
        }
        
        public static string RemoveSuffix(this string str, string suffix)
        {
            return str.EndsWith(suffix) ? str.Substring(0, str.Length - suffix.Length) : str;
        }
        
        public static string IncrementSkeletonDataVersion(this string key)
        {
            return Regex.Replace(key, @"\((\d+)\)", match =>
            {
                int number = int.Parse(match.Groups[1].Value);
                return $"({number + 1})";
            });
        }

        public static int Roll1To100I() {
            return UnityEngine.Random.Range(1, 101);
        }
        
        public static float Roll1To100F() {
            return UnityEngine.Random.Range(1f, 100f);
        }
        
        public static bool EvaluateChance100I(int chanceValue) {
            return Roll1To100I() <= chanceValue;
        }
        
        public static bool EvaluateChance100F(float chanceValue) {
            return Roll1To100F() <= chanceValue;
        }

        public static bool EvaluateChance1F(float chanceValue) {
            return EvaluateChance100F(chanceValue * 100f);
        }

        /// <summary>
        /// ToString Non Rounding Format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringN0(this int value) => value.ToString(N0_NON_ROUNDING_FORMAT);

        /// <summary>
        /// ToString Non Rounding Format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringN1(this int value) => value.ToString(N1_NON_ROUNDING_FORMAT);
        
        /// <summary>
        /// ToString Non Rounding Format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringN2(this int value) => value.ToString(N2_NON_ROUNDING_FORMAT);

        /// <summary>
        /// ToString Non Rounding Format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringN0(this float value) => value.ToString(N0_NON_ROUNDING_FORMAT);
        
        /// <summary>
        /// ToString Non Rounding Format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringN1(this float value) => value.ToString(N1_NON_ROUNDING_FORMAT);

        /// <summary>
        /// ToString Non Rounding Format
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringN2(this float value) => value.ToString(N2_NON_ROUNDING_FORMAT);

        public static ItemType ConvertToItemType(this ItemSortType sortType) {
            switch (sortType) {
                case ItemSortType.Resource:   return ItemType.Resource;
                case ItemSortType.Consumable: return ItemType.Consumable;
                
                case ItemSortType.AllEquipments:
                case ItemSortType.Weapon:
                case ItemSortType.Armor:
                case ItemSortType.Gloves:
                case ItemSortType.Shoes:    
                case ItemSortType.Ring:
                case ItemSortType.Artifact:   return ItemType.Equipment;
                
                case ItemSortType.All:
                case ItemSortType.None:
                default: throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null);
            }
        }

        public static EquipmentType ConvertToEquipType(this ItemSortType sortType) {
            switch (sortType) {
                case ItemSortType.Weapon:   return EquipmentType.Weapon;
                case ItemSortType.Armor:    return EquipmentType.Armor;
                case ItemSortType.Gloves:   return EquipmentType.Gloves;
                case ItemSortType.Shoes:    return EquipmentType.Shoes;
                case ItemSortType.Ring:     return EquipmentType.Ring;
                case ItemSortType.Artifact: return EquipmentType.Artifact;
                
                case ItemSortType.None:
                case ItemSortType.All:
                case ItemSortType.Resource:
                case ItemSortType.Consumable:
                case ItemSortType.AllEquipments:
                default: throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null);
            }
        }

        public static bool IsDetailedEquipmentType(this ItemSortType sortType) {
            return sortType switch {
                ItemSortType.All           => false,
                ItemSortType.AllEquipments => false,
                ItemSortType.Resource      => false,
                ItemSortType.Consumable    => false,
                ItemSortType.Weapon        => true,
                ItemSortType.Armor         => true,
                ItemSortType.Gloves        => true,
                ItemSortType.Shoes         => true,
                ItemSortType.Ring          => true,
                ItemSortType.Artifact      => true,
                ItemSortType.None          => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null),
                _                          => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null)
            };
        }

        public static bool IsIncludeMutipleItemType(this ItemSortType sortType) {
            return sortType switch {
                ItemSortType.ResourceAndConsumable => true,
                ItemSortType.None                  => false,
                ItemSortType.All                   => false,
                ItemSortType.AllEquipments         => false,
                ItemSortType.Weapon                => false,
                ItemSortType.Armor                 => false,
                ItemSortType.Gloves                => false,
                ItemSortType.Shoes                 => false,
                ItemSortType.Ring                  => false,
                ItemSortType.Artifact              => false,
                ItemSortType.Resource              => false,
                ItemSortType.Consumable            => false,
                _                                  => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null)
            };
        }

        public static List<ItemType> ConvertToMultipleTypes(this ItemSortType sortType) {
            resultItemTypes.Clear();
            switch (sortType) {
                case ItemSortType.ResourceAndConsumable:
                    resultItemTypes.Add(ItemType.Resource);
                    resultItemTypes.Add(ItemType.Consumable);
                    break;
                case ItemSortType.None:
                case ItemSortType.All:
                case ItemSortType.AllEquipments:
                case ItemSortType.Weapon:
                case ItemSortType.Armor:
                case ItemSortType.Gloves:
                case ItemSortType.Shoes:
                case ItemSortType.Ring:
                case ItemSortType.Artifact:
                case ItemSortType.Resource:
                case ItemSortType.Consumable:
                default: throw new NotImplementedException($"Not Implemented this Type. {sortType.ToString()}");
            }
            return resultItemTypes;
        }

        public static string ToStringEx(this ItemType type) {
            return type switch {
                ItemType.Resource   => "RESOURCE",
                ItemType.Consumable => "CONSUMABLE",
                ItemType.Equipment  => "EQUIPMENT",
                _                   => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        public static string ToStringEx(this ItemGrade grade) {
            return grade switch {
                ItemGrade.None      => "NONE",
                ItemGrade.Common    => "COMMON",
                ItemGrade.Uncommon  => "UNCOMMON",
                ItemGrade.Rare      => "RARE",
                ItemGrade.Epic      => "EPIC",
                ItemGrade.Legendary => "LEGENADRY",
                _                   => throw new ArgumentOutOfRangeException(nameof(grade), grade, null)
            };
        }
        
        public static string ToStringEx(this EquipmentType type) {
            return type switch {
                EquipmentType.Weapon   => "WEAPON",
                EquipmentType.Armor    => "ARMOR",
                EquipmentType.Gloves   => "GLOVES",
                EquipmentType.Shoes    => "SHOES",
                EquipmentType.Ring     => "RING",
                EquipmentType.Artifact => "ARTIFACT",
                _                      => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        /// <summary>
        /// Null Safe
        /// </summary>
        /// <param name="button"></param>
        /// <param name="isInteractanble"></param>
        public static void SetInteractableSafeNull(this Button button, bool isInteractanble) {
            if (!button) {
                return;
            }
            button.interactable = isInteractanble;
        }

        public static Vector2 GetPositionV2(this Transform transform) => new(transform.position.x, transform.position.y);
        
        
    }
}