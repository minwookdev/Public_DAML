using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;

namespace RandomDungeonWithBluePrint
{
    [CreateAssetMenu(menuName = "CoffeeCat/Scriptable Object/DungeonBluePrint")]
    public class DungeonBluePrint : ScriptableObject {
        [Title("BluePrint Queue Options", TitleAlignment = TitleAlignments.Centered)]
        [field: SerializeField] public FieldBluePrint[] NormalMapBluePrints { get; private set; } = null;
        [field: SerializeField] public FieldBluePrint[] HiddenMapBluePrints { get; private set; } = null;
        [field: SerializeField] public LootTableDictionary LootTable { get; private set; } = null;
        [field: SerializeField] public StartRoomTableDictionary StartRoomTable { get; private set; } = null;
        [field: SerializeField] public RewardRoomTableDictionary RewardItemTable { get; private set; } = null;
        [field: SerializeField] public ShopRoomTableDictionary ShopItemTable { get; private set; } = null;
        [field: SerializeField] public SceneName EventMapSceneKey { get; private set; } = SceneName.NONE;
        [field: SerializeField] public SceneName BossMapSceneKey { get; private set; } = SceneName.NONE;
        [field: SerializeField] public bool IsGrantItemsOnStart { get; private set; } = true;
    }
}