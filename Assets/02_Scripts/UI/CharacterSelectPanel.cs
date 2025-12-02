using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeCat.FrameWork;
using Sirenix.OdinInspector;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat.UI
{
    // TODO : 세이브 파일 필요
    public class CharacterSelectPanel : MonoBehaviour
    {
        private int selectingIndex = 0;
        private int maxIndex = 0;

        [Title("Top")]
        public Button btnExit = null;

        public SkeletonAnimation character;
        public Button bunLeft = null;
        public Button btnRight = null;
        public TextMeshProUGUI tmpCharacterName = null;
        public TextMeshProUGUI tmpCharacterJop = null;
        public TextMeshProUGUI tmpSelecting = null;

        [Title("Middle")]
        public TextMeshProUGUI tmpDescription = null;

        public Transform powerGrade = null;
        public Transform viabilityGrade = null;
        public Transform utilicityGrade = null;

        [Title("Bottom")]
        public Button btnClose = null;

        public Button btnBuy = null;
        public TextMeshProUGUI tmpPrice = null;
        public Button btnSelected = null;
        public Button btnChange = null;

        private void OnDestroy()
        {
            TownEvtManager.RemovePlayerUpgradeCompleteListener(SelectingCharacterUpgraded);
        }

        public void Active()
        {
            gameObject.SetActive(true);
        }

        public void Init()
        {
            var characterDatas = DataManager.Inst.CharacterSelectDatas;
            var selectedIndex = UserDataManager.Inst.SelectedCharacterIndex;
            selectingIndex = selectedIndex;
            maxIndex = characterDatas.GetLastCharacterIndex();
            btnExit.onClick.AddListener(Close);
            btnRight.onClick.AddListener(OnRightBtnClick);
            bunLeft.onClick.AddListener(OnLeftBtnClick);
            btnClose.onClick.AddListener(Close);
            TownEvtManager.AddPlayerUpgradeCompleteListener(SelectingCharacterUpgraded);

            RefreshCharacterInfo();
        }

        private void RefreshCharacterInfo()
        {
            var characterDatas = DataManager.Inst.CharacterSelectDatas;
            var charData = characterDatas.GetCharacterInfo(selectingIndex);
            var selectState = GetSelectState();
            tmpCharacterName.text = charData.CharacterName;
            tmpCharacterJop.text = charData.CharacterJob;
            tmpDescription.text = charData.Description;
            ClearGradeGroups();
            SetCharacterView(charData.SkeletonAddressableKey);
            SetGradeGroups(charData.Power, charData.Vitality, charData.Utility);
            SetActiveteButtons(charData, selectState);
        }

        private void SetCharacterView(string skeletonKey)
        {
            ResourceManager.Inst.AddressablesAsyncLoad<SkeletonDataAsset>(skeletonKey, false, (skeletonData) =>
            {
                if (!skeletonData)
                {
                    CatLog.WLog("Skeleton Data is Null");
                    return;
                }

                character.skeletonDataAsset = skeletonData;
                var skinName = skeletonKey.RemoveSuffix("_SkeletonData");
                character.initialSkinName = skinName;
                character.Initialize(true);
            });
        }

        private void SetCharacterView(SkeletonDataAsset dataAsset)
        {
            character.skeletonDataAsset = dataAsset;
            var skinName = dataAsset.name.RemoveSuffix("_SkeletonData");
            character.initialSkinName = skinName;
            character.Initialize(true);
        }

        private void ClearGradeGroups()
        {
            for (int i = 0; i < powerGrade.childCount - 1; i++)
                powerGrade.GetChild(i).gameObject.SetActive(false);

            for (int i = 0; i < viabilityGrade.childCount - 1; i++)
                viabilityGrade.GetChild(i).gameObject.SetActive(false);

            for (int i = 0; i < utilicityGrade.childCount - 1; i++)
                utilicityGrade.GetChild(i).gameObject.SetActive(false);
        }

        private void SetGradeGroups(int power, int viability, int utilicity)
        {
            if (power <= 0) CatLog.WLog("Power Grade is 0");
            if (viability <= 0) CatLog.WLog("Viability Grade is 0");
            if (utilicity <= 0) CatLog.WLog("Utilicity Grade is 0");

            for (int i = 0; i < power; i++)
                powerGrade.GetChild(i).gameObject.SetActive(true);

            for (int i = 0; i < viability; i++)
                viabilityGrade.GetChild(i).gameObject.SetActive(true);

            for (int i = 0; i < utilicity; i++)
                utilicityGrade.GetChild(i).gameObject.SetActive(true);
        }

        private CharacterSelectState GetSelectState()
        {
            var characterData = UserDataManager.Inst.GetCharacterData(selectingIndex);

            if (characterData == null)
            {
                return CharacterSelectState.NOT_OWNED;
            }

            return characterData.SelectState;
        }

        private void SetActiveteButtons(CharacterSelectData charData, CharacterSelectState state)
        {
            switch (state)
            {
                case CharacterSelectState.SELECTABLE:
                    tmpSelecting.gameObject.SetActive(false);
                    btnBuy.gameObject.SetActive(false);
                    btnSelected.gameObject.SetActive(false);
                    btnChange.gameObject.SetActive(true);
                    btnChange.onClick.RemoveAllListeners();
                    btnChange.onClick.AddListener(() => OnSelectBtnClick(charData));
                    break;
                case CharacterSelectState.SELECTED:
                    tmpSelecting.gameObject.SetActive(true);
                    btnBuy.gameObject.SetActive(false);
                    btnChange.gameObject.SetActive(false);
                    btnSelected.gameObject.SetActive(true);
                    btnSelected.onClick.RemoveAllListeners();
                    btnSelected.onClick.AddListener(() => OnSelectingBtnClick(charData));
                    break;
                case CharacterSelectState.NOT_OWNED:
                    tmpSelecting.gameObject.SetActive(false);
                    btnBuy.gameObject.SetActive(true);
                    btnBuy.onClick.RemoveAllListeners();
                    btnBuy.onClick.AddListener(() => OnBuyBtnClick(charData));
                    btnChange.gameObject.SetActive(false);
                    btnSelected.gameObject.SetActive(false);
                    SetPrice(charData.CharacterPrice);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void SetPrice(int price)
        {
            var priceStr = price.ToStringN0();

            tmpPrice.text = $"구매\n" +
                            $"[ {priceStr} 골드 ]";
        }

        private void OnSelectBtnClick(CharacterSelectData charInfo)
        {
            SoundManager.Inst.PlayButtonSE(true);
            UserDataManager.Inst.SelectedCharacter(charInfo.CharacterIndex);
            TownSceneBase.Inst.PlayerRespawn(charInfo.TownSpawnKey);
            TownUIPresenter.Inst.RefreshCharacterUpgradePanel();
            RogueLiteManager.Inst.SetDungeonPlayerKey(charInfo.DungeonSpawnKey);
            SetActiveteButtons(charInfo, CharacterSelectState.SELECTED);
            Close();
        }

        private void OnSelectingBtnClick(CharacterSelectData charInfo)
        {
            SoundManager.Inst.PlayButtonSE(false);
        }

        private void OnBuyBtnClick(CharacterSelectData charInfo)
        {
            var currency = UserDataManager.Inst.Currency;
            if (currency < charInfo.CharacterPrice)
            {
                SoundManager.Inst.PlayButtonSE(false);
                CatLog.Log("currency is not enough");
                return;
            }

            SoundManager.Inst.PlayButtonSE(true);
            UserDataManager.Inst.DecreaseCurrency(charInfo.CharacterPrice);
            TownEvtManager.InvokeCurrencyChanged(UserDataManager.Inst.Currency);
            UserDataManager.Inst.PurchasedCharacter(charInfo.CharacterIndex);
            SetActiveteButtons(charInfo, CharacterSelectState.SELECTABLE);
        }

        private void SelectingCharacterUpgraded(SkeletonDataAsset dataAsset)
        {
            SetCharacterView(dataAsset);
        }

        private void OnLeftBtnClick()
        {
            SoundManager.Inst.PlayButtonSE(true);
            selectingIndex--;
            if (selectingIndex < 0)
            {
                selectingIndex = maxIndex;
            }

            RefreshCharacterInfo();
        }

        private void OnRightBtnClick()
        {
            SoundManager.Inst.PlayButtonSE(true);
            selectingIndex++;
            if (selectingIndex > maxIndex)
            {
                selectingIndex = 0;
            }

            RefreshCharacterInfo();
        }

        private void Close()
        {
            SoundManager.Inst.PlayButtonSE(false);
            gameObject.SetActive(false);
            selectingIndex = UserDataManager.Inst.SelectedCharacterIndex;
            RefreshCharacterInfo();
        }
    }
}