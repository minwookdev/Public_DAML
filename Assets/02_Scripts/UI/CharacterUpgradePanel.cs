using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat.UI
{
    public class CharacterUpgradePanel : MonoBehaviour
    {
        [Title("Top")]
        public Button btnClose;

        public Button btn1stUpgrade;
        public Button btn2ndUpgrade;
        public GameObject lockIcon;
        public RectTransform rectTr2ndUpgrade;

        [Title("Character Area")]
        public SkeletonAnimation character;

        public ParticleSystem completeEffect;
        public Image slider1stUpgrade;
        public Image slider2ndUpgrade;

        [Title("Upgrade Area")]
        public Transform SlotHolder;

        [Title("Bottom")]
        public Button btnConfirm;

        private List<StatUpgradeSlot> slots = new();
        private PlayerStatUpgradeData upgradeData;
        private StatGrade currentGrade = StatGrade.Beginner;

        private void OnDestroy()
        {
            TownEvtManager.RemovePlayerUpgradeProgressListener(SetProgressSlider);
        }

        public void Init()
        {
            TownEvtManager.AddPlayerUpgradeProgressListener(SetProgressSlider);
            btnClose.onClick.AddListener(Close);
            btnConfirm.onClick.AddListener(Close);
            btn1stUpgrade.onClick.AddListener(On1stUpgradeBtnClick);
            btn2ndUpgrade.onClick.AddListener(On2ndUpgradeBtnClick);
            SetCompleteState();
            RefreshUpgradePanel();
        }

        public void RefreshUpgradePanel()
        {
            var datas = DataManager.Inst.PlayerStatUpgradeDatas;
            var index = UserDataManager.Inst.SelectedCharacterIndex;
            upgradeData = Array.Find(datas, x => x.CharacterIndex == index);

            SetSlots();
            SetPlayerView(false);
            SetCompleteState();
            TownEvtManager.InvokePlayerUpgradeProgress();
        }

        private void SetPlayerView(bool isPlayEffect)
        {
            var selectData = DataManager.Inst.CharacterSelectDatas;
            var index = UserDataManager.Inst.SelectedCharacterIndex;
            var dataAssetKey = selectData.GetSkeletonDataAssetKeys(index);

            var isComplete_B = UserDataManager.Inst.IsCompleteUpgrade(StatGrade.Beginner);
            if (isComplete_B) dataAssetKey = dataAssetKey.IncrementSkeletonDataVersion();

            ResourceManager.Inst.AddressablesAsyncLoad<SkeletonDataAsset>(dataAssetKey, false, (data) =>
            {
                if (!data)
                {
                    CatLog.Log("Skeleton Data is Null");
                    return;
                }

                character.skeletonDataAsset = data;
                character.initialSkinName = dataAssetKey.RemoveSuffix("_SkeletonData");
                character.Initialize(true);

                if (!isPlayEffect) return;
                SoundManager.Inst.PlaySE(SoundKey.Plaeyr_Enhanced.ToKey(), 1f);
                StopCoroutine(PlayCompleteEffect());
                StartCoroutine(PlayCompleteEffect());
                TownEvtManager.InvokePlayerUpgradeComplete(data);
            });
        }

        private void SetSlots()
        {
            foreach (var slot in slots) ObjectPoolManager.Inst.Despawn(slot.gameObject);
            slots.Clear();

            var upgradeDatas = GetCurrentGradeData();

            foreach (var data in upgradeDatas)
            {
                var slot = ObjectPoolManager.Inst.Spawn<StatUpgradeSlot>("statSlot", SlotHolder);
                slot.SetSlot(data, currentGrade);
                slots.Add(slot);
            }
        }

        private List<StatUpgradeData> GetCurrentGradeData()
        {
            switch (currentGrade)
            {
                case StatGrade.Beginner:
                    return upgradeData.BegginerDatas;
                case StatGrade.Middle:
                    return upgradeData.MiddleDatas;
                case StatGrade.High:
                    CatLog.WLog("High Grade is not implemented yet");
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetProgressSlider()
        {
            var totalCurrent = UserDataManager.Inst.GetTotalCurrentUpgradeCount(currentGrade);
            var totalMax = upgradeData.GetTotalMaxUpgradeCount(currentGrade);
            var progress = (float)totalCurrent / totalMax;

            slider1stUpgrade.fillAmount = progress;
            slider2ndUpgrade.fillAmount = progress * 0.5f;

            var isGradeComplete = UserDataManager.Inst.IsCompleteUpgrade(currentGrade);
            if (!isGradeComplete && slider1stUpgrade.fillAmount >= 1f)
            {
                CompletedUpgrade();
            }
        }

        private void CompletedUpgrade()
        {
            UserDataManager.Inst.SetCompleteUpgrade(currentGrade);

            switch (currentGrade)
            {
                case StatGrade.Beginner:
                    lockIcon.gameObject.SetActive(false);
                    rectTr2ndUpgrade.offsetMax = Vector2.zero;
                    break;
                case StatGrade.Middle:
                    CatLog.Log("next grade is not implemented yet");
                    break;
                case StatGrade.High:
                    CatLog.Log("High Grade is max grade");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetPlayerView(true);
        }

        private IEnumerator PlayCompleteEffect()
        {
            completeEffect.Play();
            var track = character.AnimationState;
            track.SetAnimation(0, "Win_1", false);
            yield return new WaitForSeconds(2f);
            track.SetAnimation(0, "Idle_2", true);
        }

        private void SetCompleteState()
        {
            var begginer = UserDataManager.Inst.IsCompleteUpgrade(StatGrade.Beginner);
            var middle = UserDataManager.Inst.IsCompleteUpgrade(StatGrade.Middle);

            if (begginer)
            {
                lockIcon.gameObject.SetActive(false);
                rectTr2ndUpgrade.offsetMax = Vector2.zero;
            }

            if (middle)
            {
                CatLog.Log("High Grade is not implemented yet");
            }
        }

        private void On1stUpgradeBtnClick()
        {
            if (currentGrade == StatGrade.Beginner) return;

            SoundManager.Inst.PlayButtonSE(true);
            currentGrade = StatGrade.Beginner;
            SetSlots();
        }

        private void On2ndUpgradeBtnClick()
        {
            var isComplete_B = UserDataManager.Inst.IsCompleteUpgrade(StatGrade.Beginner);
            if (currentGrade == StatGrade.Middle || !isComplete_B)
            {
                SoundManager.Inst.PlayButtonSE(false);
                return;
            }

            SoundManager.Inst.PlayButtonSE(true);
            currentGrade = StatGrade.Middle;
            SetSlots();
        }

        public void Active()
        {
            gameObject.SetActive(true);
        }

        private void Close()
        {
            SoundManager.Inst.PlayButtonSE(false);
            gameObject.SetActive(false);
        }
    }
}