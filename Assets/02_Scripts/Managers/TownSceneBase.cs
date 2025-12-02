using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat
{
    public class TownSceneBase : SceneSingleton<TownSceneBase>
    {
        [Title("Town Scene Base")]
        [SerializeField] private CameraController cameraController = null;
        [SerializeField] private Transform playerSpawnTr = null;
        [SerializeField] private Transform dungeonEntranceTr = null;
        [SerializeField] private Transform characterSelectTr = null;
        [SerializeField] private Transform playerUpgradeTr = null;
        [SerializeField] private Transform playerAbility = null;
        
        [Title("Audio Clip")]
        public StringAudioClipDictionary AudioClipDictionary = null;

        [Title("Object Pool")]
        [SerializeField] private PoolInfo[] poolInfos = null;

        private PlayerAddressablesKey playerKey = PlayerAddressablesKey.Flower_Town;
        public Player_Town SpawnedPlayer { get; private set; } = null;

        protected override void Initialize()
        {
            FirebaseManager.Create();
            ResourceManager.Create();
            DataManager.Create();
            UserDataManager.Create();
            SceneManager.Create();
            ObjectPoolManager.Create();
            InputManager.Create();
            RogueLiteManager.Create();
            SoundManager.Create();
        }

        private void Start()
        {
            // Init PoolObjects
            ObjectPoolManager.Inst.AddToPool(poolInfos);
            
            // Init Scriptable Object
            SoundManager.Inst.RegistAudioClips(AudioClipDictionary);
            
            // TownUI Presenter Init
            TownUIPresenter.Inst.Init();
            
            // Spawn Town Requirements
            SetPlayerSpawnKey();
            SpawnedPlayer = ObjectPoolManager.Inst.Spawn<Player_Town>(playerKey.ToKey(), playerSpawnTr.position, Quaternion.identity);
            var dungeonInteractable = ObjectPoolManager.Inst.Spawn<Interaction>(InteractionType.EntranceDungeon.ToKey(), dungeonEntranceTr.position, Quaternion.identity);
            var characterSelectInteractable = ObjectPoolManager.Inst.Spawn<Interaction>(InteractionType.PlayerSelect.ToKey(), characterSelectTr.position, Quaternion.identity);
            var playerUpgradeInteractable = ObjectPoolManager.Inst.Spawn<Interaction>(InteractionType.PlayerUpgrade.ToKey(), playerUpgradeTr.position, Quaternion.identity);
            var playerAbilityInteractable = ObjectPoolManager.Inst.Spawn<Interaction>(InteractionType.PlayerAbility.ToKey(), playerAbility.position, Quaternion.identity);
            
            dungeonInteractable.PlayParticle();
            characterSelectInteractable.PlayParticle();
            playerUpgradeInteractable.PlayParticle();
            playerAbilityInteractable.PlayParticle();

            // Set Camera Target
            cameraController.SetTarget(SpawnedPlayer.Tr);
            
            // SafeLoader Start
            SafeLoader.StartProcess(gameObject);

            // play town bgm
            SoundManager.Inst.PlayBgm(SoundKey.BGM_Town.ToKey(), 0.5f);
            
            RogueLiteManager.Inst.SetIsEnteredDungeon(false);
        }
        
        public void PlayerRespawn(PlayerAddressablesKey key)
        {
            SetPlayerSpawnKey();
            ObjectPoolManager.Inst.Despawn(SpawnedPlayer.gameObject);
            SpawnedPlayer = ObjectPoolManager.Inst.Spawn<Player_Town>(playerKey.ToKey(), playerSpawnTr.position, Quaternion.identity);
            cameraController.SetTarget(SpawnedPlayer.Tr);
        }

        private void SetPlayerSpawnKey()
        {
            var characterIndex = UserDataManager.Inst.SelectedCharacterIndex;
            
            var characterSelectDatas = DataManager.Inst.CharacterSelectDatas;
            var characterSelectData = characterSelectDatas.GetCharacterInfo(characterIndex);
            playerKey = characterSelectData.TownSpawnKey;
        }
        
        private void OnDisable()
        {
            SafeLoader.StopProcess();
        }
    }
}