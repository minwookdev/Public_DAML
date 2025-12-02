using CoffeeCat.FrameWork;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonEnteringConfirm : MonoBehaviour {
    [SerializeField] private Button btnConfirm = null;
    [SerializeField] private Button[] btnCancels = null;
    [SerializeField] private TextMeshProUGUI tmpDungeonName = null;
    
    private string dungeonId = "";

    private void Start() {
        btnConfirm.onClick.AddListener(Confirm);
        
        for (int i = 0; i < btnCancels.Length; i++) {
            var btnCancel = btnCancels[i];
            btnCancel.onClick.AddListener(Cancel);
        }
    }

    public void Active(string dungeonName, string dungeonKey) {
        dungeonId = dungeonKey;
        tmpDungeonName.text = dungeonName;
        
        gameObject.SetActive(true);
    }

    private void Deactive() {
        tmpDungeonName.SetText("");
        dungeonId = "";
        
        gameObject.SetActive(false);
    }

    private void Confirm() {
        RogueLiteManager.Inst.EnteringDungeon(dungeonId);
    }

    private void Cancel() {
        Deactive();
    }
}