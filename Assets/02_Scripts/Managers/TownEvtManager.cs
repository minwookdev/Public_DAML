using UnityEngine;
using CoffeeCat.FrameWork;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine.Events;

namespace CoffeeCat {
    public class TownEvtManager : SceneSingleton<TownEvtManager> {
        private UnityEvent OnPlayeUpgradeProgress = new UnityEvent();
        private UnityEvent<SkeletonDataAsset> OnPlayerUpgradeComplete = new UnityEvent<SkeletonDataAsset>();
        private UnityEvent<int> OnCurrencyChanged = new UnityEvent<int>();
        private UnityEvent<int, PlayerAbility> OnAbilityUpgrade = new UnityEvent<int, PlayerAbility>();

        #region Add Event

        public static void AddPlayerUpgradeProgressListener(UnityAction action) =>
            Inst.OnPlayeUpgradeProgress.AddListener(action);
        public static void AddPlayerUpgradeCompleteListener(UnityAction<SkeletonDataAsset> action) =>
            Inst.OnPlayerUpgradeComplete.AddListener(action);
        public static void AddCurrencyChangedListener(UnityAction<int> action) =>
            Inst.OnCurrencyChanged.AddListener(action);
        public static void AddAbilityUpgradeListener(UnityAction<int, PlayerAbility> action) =>
            Inst.OnAbilityUpgrade.AddListener(action);

        #endregion

        #region Remove Event

        public static void RemovePlayerUpgradeProgressListener(UnityAction action)
        {
            if (!IsExist)
                return;
            
            Inst.OnPlayeUpgradeProgress.RemoveListener(action);
        }

        public static void RemovePlayerUpgradeCompleteListener(UnityAction<SkeletonDataAsset> action)
        {
            if (!IsExist)
                return;
            
            Inst.OnPlayerUpgradeComplete.RemoveListener(action);
        }
        
        public static void RemoveCurrencyChangedListener(UnityAction<int> action)
        {
            if (!IsExist)
                return;
            
            Inst.OnCurrencyChanged.RemoveListener(action);
        }
        
public static void RemoveAbilityUpgradeListener(UnityAction<int, PlayerAbility> action)
        {
            if (!IsExist)
                return;
            
            Inst.OnAbilityUpgrade.RemoveListener(action);
        }

        #endregion

        #region Invoke Event

        public static void InvokePlayerUpgradeProgress() => Inst.OnPlayeUpgradeProgress.Invoke();
        public static void InvokePlayerUpgradeComplete(SkeletonDataAsset dataAsset) => Inst.OnPlayerUpgradeComplete.Invoke(dataAsset);
        public static void InvokeCurrencyChanged(int amount) => Inst.OnCurrencyChanged.Invoke(amount);
        public static void InvokeAbilityUpgrade(int slotIndex, PlayerAbility ability) => Inst.OnAbilityUpgrade.Invoke(slotIndex, ability);
        
        #endregion
    }
}