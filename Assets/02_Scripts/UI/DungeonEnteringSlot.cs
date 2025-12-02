using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;

namespace CoffeeCat
{
    public class DungeonEnteringSlot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tmp_title = null;
        [SerializeField] private TextMeshProUGUI tmp_desc = null;
        [SerializeField] private Image img = null;
        [SerializeField] private Button btn = null;
        
        [SerializeField, ReadOnly] private string dungeonKey = "";
        [SerializeField, ReadOnly] private string dungeonName = "";
        
        public void Active(DungeonInfo info)
        {
            dungeonKey = info.dungeonId;
            dungeonName = info.dungeonName;
            
            tmp_title.text = dungeonName;
            tmp_desc.text = info.dungeonDesc;
            
            // add button event
            btn.onClick.AddListener(OnClick);
            
            // active 
            gameObject.SetActive(true);
            
            // load icon sprite
            ResourceManager.Inst.AddressablesAsyncLoad<Sprite>(info.dungeonIconKey, false, (sprite) =>
            {
                if (!sprite)
                {
                    img.color = Color.clear;
                    return;
                }
                
                img.sprite = sprite;
                img.color = Color.white;
            });
        }

        private void OnClick()
        {
            TownUIPresenter.Inst.ActiveDuneonConfirmPanel(dungeonName, dungeonKey);
        }

        public void Clear()
        {
            // clear data
            dungeonKey = string.Empty;
            tmp_title.text = string.Empty;
            tmp_desc.text = string.Empty;
            img.sprite = null;
            btn.onClick.RemoveAllListeners();
            
            gameObject.SetActive(false);
        }
    }
}