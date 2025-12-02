using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Sirenix.OdinInspector;
using CoffeeCat.UI;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat.FrameWork {
    public class DungeonUIPresenter : SceneSingleton<DungeonUIPresenter> {
        [Title("UI")]
        [SerializeField] private SkillSelectPanel skillSelector = null;
        [SerializeField] private Minimap minimap = null;
        [SerializeField] private Image hpSliderImage = null;
        [SerializeField] private Image expSliderImage = null;
        [SerializeField] private Button btnMap = null;
        [SerializeField] private Button btnStatus = null;
        [SerializeField] private Button btnSettings = null;
        [SerializeField] private GameObject fadePanel = null;
        [SerializeField] private TextMeshProUGUI tmpCurrency = null;
        [SerializeField] private ItemInfoPanel itemInfoPanel = null;
        [SerializeField] public ConfirmPanel confirmPanel = null;
        [field: SerializeField] public DungeonShopPanel ShopPanel { get; private set; } = null;
        [SerializeField] public EvasionViewer evasionViewer = null;
        [SerializeField] private DungeonStartPanel startPanel = null;
        
        [Title("Settings")]
        [SerializeField] private PlayerStatusPanel statusPanel = null; 
        [SerializeField] private SettingsPanel settingsPanel = null;

        [Title("Skill Points")]
        [SerializeField] private Button btnSkillPoint = null;
        [SerializeField] private TextMeshProUGUI tmpSkillPoint = null;

        private void Start() {
            btnSkillPoint.gameObject.SetActive(false);
            
            btnMap.onClick.AddListener(OpenMiniMap);
            btnStatus.onClick.AddListener(OpenStatusPanel);
            btnSettings.onClick.AddListener(settingsPanel.EnablePanel);
            btnSkillPoint.onClick.AddListener(OnButtonSkillPoint);
            
            DungeonEvtManager.AddEventIncreasePlayerHP(UpdatePlayerHPSlider);
            DungeonEvtManager.AddEventDecreasePlayerHP(UpdatePlayerHPSlider);
            DungeonEvtManager.AddEventIncreasePlayerExp(UpdatePlayerExpSlider);
            DungeonEvtManager.AddEventOnPlayerCurrencyChanged(UpdateCurrencyTMP);
        }

        private void OnDisable() {
            if (!DungeonEvtManager.IsExist) {
                return;
            }
            DungeonEvtManager.RemoveEventIncreasePlayerHP(UpdatePlayerHPSlider);
            DungeonEvtManager.RemoveEventDecreasePlayerHP(UpdatePlayerHPSlider);
            DungeonEvtManager.RemoveEventIncreasePlayerExp(UpdatePlayerExpSlider);
        }

        public void OpenSkillSelectPanel(PlayerSkillSelectData[] datas) {
            skillSelector.Open(datas);
        }
        
        public void CloseSkillSelectPanel() {
            skillSelector.ClosePanel();
        }

        private void UpdatePlayerHPSlider(float current, float max) {
            hpSliderImage.fillAmount = current / max;
        }

        private void UpdatePlayerExpSlider(float current, float max) {
            expSliderImage.fillAmount = current / max;
        }

        private void OpenStatusPanel() => statusPanel.gameObject.SetActive(true);

        private void OpenMiniMap() 
        {
            minimap.Open();
        }

        public void EnableSkillPointButton(int stackNum) {
            btnSkillPoint.gameObject.SetActive(true);
            tmpSkillPoint.SetText(stackNum.ToString());
        }

        public void DisableSkillPointButton() {
            btnSkillPoint.gameObject.SetActive(false);
            tmpSkillPoint.SetText(string.Empty);
        }

        private void OnButtonSkillPoint() {
            RogueLiteManager.Inst.EnableSkillSelectIfPossible();
        }
        
        public void OpenFadePanel() {
            fadePanel.SetActive(true);
        }

        public void CloseFadePanel() {
            fadePanel.SetActive(false);
        }

        private void UpdateCurrencyTMP(int value1) {
            tmpCurrency.SetText(value1.ToString());
        }

        public void OpenItemInfoPanel(Item item, ItemInfoType infoType) => itemInfoPanel.Set(item, infoType);
        
        public void OpenConfirmPanel(string messageStr, UnityAction onConfirmAction = null, UnityAction onCanceledAction = null) => confirmPanel.Set(messageStr, onConfirmAction, onCanceledAction);

        public void UpdateEvasionViwer(int currentEvasionCount, float currentTime, float maxTime) => evasionViewer.UpdateViewer(currentEvasionCount, currentTime, maxTime);

        public void DisableEvasionViewer() => evasionViewer.SetDisable();

        public void StartPanelTweenPlay() {
            var reservedDungeonId = RogueLiteManager.Inst.ReservedDungeonId;
            startPanel.OpenPanel(reservedDungeonId);
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        public void OpenMinimapInStandalone() {
            if (minimap.gameObject.activeSelf) {
                minimap.Close();
            }
            else {
                OpenMiniMap();
            }
        }

        public void OpenPlayerSkillInStandalone() {
            OpenStatusPanel();
        }
#endif
    }
}
