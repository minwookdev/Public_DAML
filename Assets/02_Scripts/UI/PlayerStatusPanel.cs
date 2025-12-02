using System;
using CoffeeCat.FrameWork;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat {
    public class PlayerStatusPanel : MonoBehaviour {
        [SerializeField] private Button btnOpenInfoPanel = null;
        [SerializeField] private Button btnOpenSkillPanel = null;
        [SerializeField] private Button btnOpenSettingPanel = null;
        [SerializeField] private Button btnClose = null;
        [SerializeField] private PlayerInfoPanel playerInfoPanel = null;
        [SerializeField] private PlayerSkillPanel playerSkillPanel = null;
        [SerializeField] private SkeletonGraphic characterSkeletonGraphic = null;

        private void Start() => SetButtonEvents();

        private void OnEnable() => DungeonSceneBase.Inst.TimeScaleZero();

        private void OnDisable() => DungeonSceneBase.Inst.RestoreTimeScale();

        private void SetButtonEvents() {
            btnOpenInfoPanel.onClick.RemoveAllListeners();
            btnOpenSkillPanel.onClick.RemoveAllListeners();
            btnOpenSettingPanel.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
            SetCharacterView();
            
            btnOpenInfoPanel.onClick.AddListener(() => {
                playerInfoPanel.gameObject.SetActive(true);
                playerSkillPanel.gameObject.SetActive(false);
            });
            btnOpenSkillPanel.onClick.AddListener(() => {
                playerInfoPanel.gameObject.SetActive(false);
                playerSkillPanel.gameObject.SetActive(true);
            });
            // btnOpenSettingPanel.onClick.AddListener(() => {
            //     
            // });
            btnClose.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void SetCharacterView()
        {
            var selectedIndex = UserDataManager.Inst.SelectedCharacterIndex;
            var characterDatas = DataManager.Inst.CharacterSelectDatas.GetDict();
            var characterData = characterDatas[selectedIndex];
            var key = characterData.SkeletonAddressableKey;
            var isCompleteB = UserDataManager.Inst.IsCompleteUpgrade(StatGrade.Beginner);

            if (isCompleteB) key = key.IncrementSkeletonDataVersion();
            
            ResourceManager.Inst.AddressablesAsyncLoad<SkeletonDataAsset>(key, false, (skeletonDataAsset) =>
            {
                if (!skeletonDataAsset)
                {
                    CatLog.Log("SkeletonDataAsset Load Failed !");
                    return;
                }

                characterSkeletonGraphic.skeletonDataAsset = skeletonDataAsset;
                characterSkeletonGraphic.initialSkinName = key.RemoveSuffix("_SkeletonData");
                characterSkeletonGraphic.Initialize(true);
            });
        }
    } 
}