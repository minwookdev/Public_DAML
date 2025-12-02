using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using Spine.Unity;
using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.RogueLite;
using CoffeeCat.Utils.Defines;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace CoffeeCat
{
    public partial class Player_Dungeon : MonoBehaviour
    {
        [Title("Status")]
        [SerializeField] private SkeletonAnimation skeletonAnimation = null;
        [SerializeField] protected PlayerLevelData playerLevelData = null;
        [SerializeField] protected PlayerNormalAttackKey normalAttackKey = PlayerNormalAttackKey.NONE;
        [ShowInInspector, ReadOnly] protected PlayerStat enhancedStat = null;
        private PlayerStat baseStat = null;
        private PlayerStat buffedStat = null;
        private PlayerStat equipStat = null;
        private int currentResurrectionCount = 0;
        private const int MAX_RESURRECTION_COUNT = 1;
        private const float RESURRECTION_INVINCIBLE_TIME = 2f;
        private const float DEATH_WAIT_SECONDS = 2f;

        [Title("Transform")]
        [SerializeField] protected Transform tr = null;
        [SerializeField] protected Transform projectileTr = null;
        [SerializeField] protected CircleCollider2D itemAquisitionCollider = null;
        private Rigidbody2D rigid = null;
        private bool isPlayerInBattle = false;
        private bool hasFiredProjectile = false;
        private bool isPlayerDamaged = false;
        private bool isInvincible = false;
        private bool isDead = false;
        public bool IsMoveForced = false;
        private CameraController mainCamController = null;
        private readonly DamageResult damageResult = new();
        
        [Title("Evasion")]
        [SerializeField] private bool isEvasionEnabled = true;
        [SerializeField] private string evasionEffectKey = "evasion_effect";
        [SerializeField] private LayerMask evasionCollisionLayers;
        [SerializeField, PropertyRange(1f, 10f)] private float evasionDistance = 3f;
        [SerializeField, PropertyRange(1f, 10f)] private float evasionChargingTimer = 2f;
        [SerializeField, PropertyRange(0, 5)] private int evasionMaxChargingCount = 3;
        private readonly RaycastHit2D[] evasionRaycastHitResults = new RaycastHit2D[3];
        private const float EVASION_FIX_DISTANCE = 0.25f;
        private int evasionCurrentChargingCount = 0;
        private float evasionCurrentChargingTime = 0f;
        
        // Property
        public Transform Tr => tr;
        public Transform ProjectileTr => projectileTr;
        public PlayerStat BuffedStat => buffedStat;
        public PlayerStat EnhancedStat => enhancedStat;
        public PlayerNormalAttackKey NormalAttackKey => normalAttackKey;
        public int CurrentLevel => playerLevelData.GetCurrentLevel();

        private CancellationTokenSource lerpMoveCts = null;
        private CancellationTokenSource movingSoundCts = null;

        private void Start()
        {
            rigid = GetComponent<Rigidbody2D>();
            playerLevelData.Initialize();
            mainCamController = RogueLiteManager.Inst.MainCamController;
            
            LoadResources();
            SetStat();
            CheckInvincibleTime();
            SetSkillData();
            InitEvasionSystem();
            RegistDefaultAudioClips();

            DungeonEvtManager.AddEventMonsterKilledByPlayer(KilledMonster);
            DungeonEvtManager.AddEventRoomFirstEnteringEvent(PlayerEnteredRoom);
            DungeonEvtManager.AddEventClearedRoomEvent(PlayerClearedRoom);
            DungeonEvtManager.AddEventSkillSelectCompleted(EnableSkillSelectPanelIfPossible);
            DungeonEvtManager.AddEventOnPlayerEquipmentsChanged(EquipedItem);
            DungeonEvtManager.AddEventDecreasePlayerHP(PlayHitRandomSE);
        }
        
        private void OnEnable() 
        {
            InputManager.AddEventJoyStickInputBegan(MoveStart);
            InputManager.AddEventJoyStickInputFixedUpdate(Move);
            InputManager.AddEventJoyStickInputEnded(MoveEnd);
            if (isEvasionEnabled) {
                InputManager.AddEventEvasionInput(Evasion);
            }
            
#if UNITY_EDITOR || UNITY_STANDALONE
            InputManager.AddEventStandaloneInputBegan(MoveStart);
            InputManager.AddEventStandaloneInputFixedUpdate(Move);
            InputManager.AddEventStandaloneInputEnded(MoveEnd);
#endif
        }

        private void Update() => UpdateEvasionChargingCount();

        private void OnDisable() 
        {
            InputManager.RemoveEventJoyStickInputBegan(MoveStart);
            InputManager.RemoveEventJoyStickInputFixedUpdate(Move);
            InputManager.RemoveEventJoyStickInputEnded(MoveEnd);
            if (isEvasionEnabled) {
                InputManager.RemoveEventEvasionInput(Evasion);
            }
            
#if UNITY_EDITOR || UNITY_STANDALONE
            InputManager.RemoveEventStandaloneInputBegan(MoveStart);
            InputManager.RemoveEventStandaloneInputFixedUpdate(Move);
            InputManager.RemoveEventStandaloneInputEnded(MoveEnd);
#endif
            
            damageResult.Clear();
            CancelLerpMove();
            CancelMovingSound();
        }

        private void OnDestroy() 
        {
            DungeonEvtManager.RemoveEventMonsterKilledByPlayer(KilledMonster);
            DungeonEvtManager.RemoveEventRoomFirstEntering(PlayerEnteredRoom);
            DungeonEvtManager.RemoveEventClearedRoom(PlayerClearedRoom);
            DungeonEvtManager.RemoveEventSkillSelectCompleted(EnableSkillSelectPanelIfPossible);
            DungeonEvtManager.RemoveEventOnPlayerEquipmentsChanged(EquipedItem);
            DungeonEvtManager.RemoveEventDecreasePlayerHP(PlayHitRandomSE);
        }

        private void LoadResources()
        {
            var requests = new SafeLoader.Req[]
            {
                new() {
                    Key = "LevelUp",
                    SpawnCount = 2
                },
                new() {
                    Key = "Iron Skin",
                    SpawnCount = 1
                },
                new() {
                    Key = "Concentration",
                    SpawnCount = 1
                }
            };

            SafeLoader.RegistAll(requests, success =>
            {
                if (!success)
                {
                    CatLog.ELog("Player Loading Failed !");
                }
            });
        }

        private void SetStat()
        {
            var key = RogueLiteManager.Inst.playerKey;
            var baseStatOrigin = DataManager.Inst.PlayerStats.DataDictionary[key.ToKey()];
            baseStat = baseStatOrigin.Clone();

            // get current user's character data
            var chracterDatas = UserDataManager.Inst.CharacterDatas;
            var selectedCharacterIndex = UserDataManager.Inst.SelectedCharacterIndex;
            var selectedCharacterData = chracterDatas[selectedCharacterIndex];
            var characterStatUpgradeDatas = DataManager.Inst.PlayerStatUpgradeDatas;
            var characterStatUpgradeData = characterStatUpgradeDatas[selectedCharacterIndex];
            var characterAbilityData = DataManager.Inst.PlayerAbilityData;
            if (characterStatUpgradeData.CharacterIndex != selectedCharacterIndex) { // check matched character index
                throw new Exception($"Character Index is not matched: {characterStatUpgradeData.CharacterIndex} != {selectedCharacterIndex}");
            }
            
            // get current user's character stat upgrade data
            var characterUpgradeProgressA = selectedCharacterData.UpgradeProgress_B;
            var characterUpgradeProgressB = selectedCharacterData.UpgradeProgress_M;
            var characterStatUpgradeDataA = characterStatUpgradeData.BegginerDatas;
            var characterStatUpgradeDataB = characterStatUpgradeData.MiddleDatas;
            var characterAbilityProgress = UserDataManager.Inst.GetAbilityProgress();
            foreach (var pair in characterUpgradeProgressA) {
                var progressKey = pair.Key;
                var findedStatData = characterStatUpgradeDataA.SingleOrDefault(data => data.Index == progressKey);
                if (findedStatData == null) {
                    throw new Exception($"Not found Stat Data: {progressKey}");
                }

                var increaseValue = findedStatData.ValueIncrease * pair.Value;
                switch (findedStatData.Name) {
                    case PlayerStatEnum.MaxHp:                baseStat.MaxHp += increaseValue;                break;
                    case PlayerStatEnum.Defense:              baseStat.Defense += increaseValue;              break;
                    case PlayerStatEnum.MoveSpeed:            baseStat.MoveSpeed += increaseValue;            break;
                    case PlayerStatEnum.AttackPower:          baseStat.AttackPower += increaseValue;          break;
                    case PlayerStatEnum.InvincibleTime:       baseStat.InvincibleTime += increaseValue;       break;
                    case PlayerStatEnum.CriticalChance:       baseStat.CriticalChance += increaseValue;       break;
                    case PlayerStatEnum.CriticalResistance:   baseStat.CriticalResistance += increaseValue;   break;
                    case PlayerStatEnum.CriticalMultiplier:   baseStat.CriticalMultiplier += increaseValue;   break;
                    case PlayerStatEnum.Penetration:          baseStat.Penetration += increaseValue;          break;
                    case PlayerStatEnum.ItemAcquisitionRange: baseStat.ItemAcquisitionRange += increaseValue; break;
                    case PlayerStatEnum.CoolTimeReduce:       baseStat.CoolTimeReduce += increaseValue;       break;
                    case PlayerStatEnum.SearchRange:          baseStat.SearchRange += increaseValue;          break;
                    case PlayerStatEnum.AddProjectile:        baseStat.AddProjectile += (int)increaseValue;   break;
                    case PlayerStatEnum.DamageDeviation:
                    default: throw new NotImplementedException($"Not implemented Progress Key: {findedStatData.Name}");
                }
            }

            foreach (var pair in characterUpgradeProgressB) {
                var progressKey = pair.Key;
                var findedStatData = characterStatUpgradeDataB.SingleOrDefault(data => data.Index == progressKey);
                
                if (findedStatData == null) {
                    throw new Exception($"Not found Stat Data: {progressKey}");
                }

                var increaseValue = findedStatData.ValueIncrease * pair.Value;
                switch (findedStatData.Name) {
                    case PlayerStatEnum.MaxHp:                baseStat.MaxHp += increaseValue;                break;
                    case PlayerStatEnum.Defense:              baseStat.Defense += increaseValue;              break;
                    case PlayerStatEnum.MoveSpeed:            baseStat.MoveSpeed += increaseValue;            break;
                    case PlayerStatEnum.AttackPower:          baseStat.AttackPower += increaseValue;          break;
                    case PlayerStatEnum.InvincibleTime:       baseStat.InvincibleTime += increaseValue;       break;
                    case PlayerStatEnum.CriticalChance:       baseStat.CriticalChance += increaseValue;       break;
                    case PlayerStatEnum.CriticalResistance:   baseStat.CriticalResistance += increaseValue;   break;
                    case PlayerStatEnum.CriticalMultiplier:   baseStat.CriticalMultiplier += increaseValue;   break;
                    case PlayerStatEnum.Penetration:          baseStat.Penetration += increaseValue;          break;
                    case PlayerStatEnum.ItemAcquisitionRange: baseStat.ItemAcquisitionRange += increaseValue; break;
                    case PlayerStatEnum.CoolTimeReduce:       baseStat.CoolTimeReduce += increaseValue;       break;
                    case PlayerStatEnum.SearchRange:          baseStat.SearchRange += increaseValue;          break;
                    case PlayerStatEnum.AddProjectile:        baseStat.AddProjectile += (int)increaseValue;   break;
                    case PlayerStatEnum.DamageDeviation:
                    default: throw new NotImplementedException($"Not implemented Progress Key: {findedStatData.Name}");
                }
            }

            foreach (var pair in characterAbilityProgress)
            {
                var progressKey = pair.Key;
                var findedStatData = characterAbilityData.Datas.SingleOrDefault(data => data.Index == progressKey);
                if (findedStatData == null)
                {
                    throw new Exception($"Not found Stat Data: {progressKey}");
                }
                
                var increaseValue = findedStatData.Value * pair.Value;
                switch (findedStatData.type) {
                    case PlayerStatEnum.MaxHp:                baseStat.MaxHp += increaseValue;                break;
                    case PlayerStatEnum.Defense:              baseStat.Defense += increaseValue;              break;
                    case PlayerStatEnum.MoveSpeed:            baseStat.MoveSpeed += increaseValue;            break;
                    case PlayerStatEnum.AttackPower:          baseStat.AttackPower += increaseValue;          break;
                    case PlayerStatEnum.InvincibleTime:       baseStat.InvincibleTime += increaseValue;       break;
                    case PlayerStatEnum.CriticalChance:       baseStat.CriticalChance += increaseValue;       break;
                    case PlayerStatEnum.CriticalResistance:   baseStat.CriticalResistance += increaseValue;   break;
                    case PlayerStatEnum.CriticalMultiplier:   baseStat.CriticalMultiplier += increaseValue;   break;
                    case PlayerStatEnum.Penetration:          baseStat.Penetration += increaseValue;          break;
                    case PlayerStatEnum.ItemAcquisitionRange: baseStat.ItemAcquisitionRange += increaseValue; break;
                    case PlayerStatEnum.CoolTimeReduce:       baseStat.CoolTimeReduce += increaseValue;       break;
                    case PlayerStatEnum.SearchRange:          baseStat.SearchRange += increaseValue;          break;
                    case PlayerStatEnum.AddProjectile:        baseStat.AddProjectile += (int)increaseValue;   break;
                    case PlayerStatEnum.DamageDeviation:
                    default: throw new NotImplementedException($"Not implemented Progress Key: {findedStatData.type}");
                }
            }
            
            buffedStat = new PlayerStat();
            equipStat = new PlayerStat();
            enhancedStat = new PlayerStat();
            CalculateEnhancedStat();
            enhancedStat.SetHpToMax();

            DungeonEvtManager.InvokeEventIncreasePlayerHP(enhancedStat.CurrentHp, enhancedStat.MaxHp);
        }

        private void MoveStart(float magnitude, Vector2 direction) => PlayMovingSoundAsync().Forget();

        private void Move(float magnitude, Vector2 direction) 
        {
            if (isDead)
                return;
            
            var moveDirection = new Vector2(direction.x, direction.y);
            rigid.velocity = (moveDirection * enhancedStat.MoveSpeed) * magnitude;
            
            // if (isPlayerInBattle) 
            //     return;
            
            if (direction.x != 0f || direction.y != 0f) 
            {
                SwitchingPlayerDirection(rigid.velocity.x < 0);    
            }
        }

        private void MoveEnd() 
        {
            rigid.velocity = Vector2.zero;
            CancelMovingSound();
        }

        private void SwitchingPlayerDirection(bool isSwitching)
        {
            // Default Direction is Right
            // isSwitching : true -> Left, false -> Right
            var lossyScale = tr.lossyScale;
            tr.localScale = isSwitching switch
            {
                true  => new Vector3(-2f, lossyScale.y, lossyScale.z),
                false => new Vector3(2f, lossyScale.y, lossyScale.z)
            };
        }

        private void InitEvasionSystem() {
            if (!isEvasionEnabled) {
                DungeonUIPresenter.Inst.DisableEvasionViewer();
                return;
            }
            evasionCurrentChargingCount = evasionMaxChargingCount;
            SafeLoader.Regist(evasionEffectKey, (success) => {
                if (success) {
                    return;
                }
                CatLog.WLog($"Invalid Evasion Key: {evasionEffectKey}");
                isEvasionEnabled = false;
            }, 2);
        }

        private void UpdateEvasionChargingCount() {
            if (!isEvasionEnabled || isDead) {
                return;
            }

            DungeonUIPresenter.Inst.UpdateEvasionViwer(evasionCurrentChargingCount, evasionCurrentChargingTime, evasionChargingTimer);
            if (evasionCurrentChargingCount >= evasionMaxChargingCount) {
                return;
            }
            
            evasionCurrentChargingTime += Time.deltaTime;
            if (evasionCurrentChargingTime < evasionChargingTimer) {
                return;
            }
            evasionCurrentChargingTime = 0f;
            evasionCurrentChargingCount++;
        }

        private void Evasion(Vector2 direction) {
            if (!isEvasionEnabled || evasionCurrentChargingCount <= 0 || isDead || IsMoveForced) {
                return;
            }

            evasionCurrentChargingCount--;
            var count = Physics2D.RaycastNonAlloc(tr.position, direction, evasionRaycastHitResults, evasionDistance, evasionCollisionLayers);
            if (count <= 0) {
                var destPos = tr.position + (Vector3)direction * evasionDistance;
                PlayEvasionEffect(destPos);
                rigid.MovePosition(destPos);
                return;
            }
            
            // collision with object
            var resultHit = evasionRaycastHitResults[0];
            for (int i = 0; i < count; i++) {
                var tempHit = evasionRaycastHitResults[i];
                if (tempHit.distance < resultHit.distance) {
                    resultHit = tempHit;
                }
            }
            var distance = resultHit.distance - EVASION_FIX_DISTANCE;
            if (distance < 0f) {
                distance = 0f;
            }
            
            var fixedPosition = tr.position + (Vector3)direction * distance;
            PlayEvasionEffect(fixedPosition);
            rigid.MovePosition(fixedPosition);
        }

        private void PlayEvasionEffect(Vector2 endPoint) {
            var startPointEffector = ObjectPoolManager.Inst.Spawn<Effector>(evasionEffectKey, tr.position);
            startPointEffector.Play();
            var endPointEffector = ObjectPoolManager.Inst.Spawn<Effector>(evasionEffectKey, endPoint);
            endPointEffector.Play();
            PlayEvasionRandomSE();
        }

        private void PlayEvasionRandomSE() {
            var random = Random.Range(0, 2);
            var key = random == 0 ? SoundKey.Player_Evasion_0 : SoundKey.Player_Evasion_1;
            SoundManager.Inst.PlaySE(key.ToKey(), 0.9f);
        }
        
        private void CheckInvincibleTime()
        {
            this.ObserveEveryValueChanged(_ => isPlayerDamaged)
                .Skip(TimeSpan.Zero)
                .Where(_ => isPlayerDamaged)
                .Subscribe(_ =>
                {
                    SetInvincibleWithDuration(enhancedStat.InvincibleTime);
                }).AddTo(this);
        }

        private void SetInvincibleWithDuration(float duration) {
            isInvincible = true;
            Observable.Timer(TimeSpan.FromSeconds(duration))
                      .Subscribe(__ => isInvincible = false);
        }
        
        private void KilledMonster(MonsterStatus.KilledInfo killedInfo) 
        {
            // increase exp
            AddExp(killedInfo.ExpAmount);
        }

        private void OnDead()
        {
            isDead = true;
            rigid.velocity = Vector2.zero;
            DungeonEvtManager.InvokeEventPlayerKilled();
            OnPlayerDeathAsync().Forget();
        }

        private void Resurrect() 
        {
            SetInvincibleWithDuration(RESURRECTION_INVINCIBLE_TIME);
            enhancedStat.SetHpToHalf();
            isDead = false;
            currentResurrectionCount++;
        }
        
        private async UniTaskVoid OnPlayerDeathAsync() 
        {
            var isCanceled = await UniTask.WaitForSeconds(DEATH_WAIT_SECONDS, false, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
            if (isCanceled) 
            {
                return;
            }

            bool isResurrectable = currentResurrectionCount < MAX_RESURRECTION_COUNT;
            if (isResurrectable) 
            {
                EnableResurrectPanel();
            }
            else 
            {
                EnableWayToTownPanel();
            }
        }

        private void EnableResurrectPanel() 
        {
            const string panelMessage = "GAME OVER \n \n 광고를 시청해 \n 체력 일부를 회복하고 \n 부활할 수 있습니다.";
            DungeonUIPresenter.Inst.OpenConfirmPanel(panelMessage, Resurrect, () => { DungeonSceneBase.Inst.RequestToTownScene(); });
        }

        private void EnableWayToTownPanel() 
        {
            const string panelMessage = "GAME OVER \n \n 확인 버튼을 눌러 \n 마을로 이동합니다.";
            DungeonUIPresenter.Inst.OpenConfirmPanel(panelMessage, () => { DungeonSceneBase.Inst.RequestToTownScene(); });
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Monster와 충돌
            if (other.gameObject.TryGetComponent(out MonsterStatus monsterStat))
            {
                damageResult.SetData(monsterStat.CurrentStat, enhancedStat);
                OnDamaged(damageResult);
            }
        }

        private void PlayerEnteredRoom(RoomDataStruct roomData)
        {
            switch (roomData.RoomType)
            {
                case RoomType.PlayerSpawnRoom:
                    break;
                case RoomType.MonsterSpawnRoom:
                    isPlayerInBattle = true;
                    ActiveSkillEffect();
                    break;
                case RoomType.ShopRoom:
                    break;
                case RoomType.BossRoom:
                    break;
                case RoomType.RewardRoom:
                    break;
                case RoomType.EmptyRoom:
                    break;
                case RoomType.ExitRoom:
                    break;
            }
        }

        private void PlayerClearedRoom(RoomDataStruct roomData)
        {
            switch (roomData.RoomType)
            {
                case RoomType.MonsterSpawnRoom:
                    isPlayerInBattle = false;
                    break;
                case RoomType.BossRoom:
                    break;
            }
        }

        private void CheckWithIncreaseLevel() {
            if (!playerLevelData.isReadyLevelUp()) {
                return;
            }
            playerLevelData.LevelUp();
            OnLevelIncreased();
        }

        private void OnLevelIncreased() {
            var levelUpEffect = ObjectPoolManager.Inst.Spawn("LevelUp", tr);
            levelUpEffect.transform.localPosition = Vector3.zero;
            currentSkillPoint++;
            DungeonUIPresenter.Inst.EnableSkillPointButton(currentSkillPoint);
        }

        private void EquipedItem(EquipmentSet equipSet)
        {
            if (equipSet == null)
            {
                CatLog.WLog("EquipSet is null");
                return;
            }

            equipStat.Clear();
            equipmentSkills.Clear();
            ClearEquipmentSkillEffects();

            if (equipSet.Weapon)    ApplyEquipEffect(equipSet.Weapon);
            if (equipSet.Accessory) ApplyEquipEffect(equipSet.Accessory);
            if (equipSet.Artifact)  ApplyEquipEffect(equipSet.Artifact);
            if (equipSet.Armor)     ApplyEquipEffect(equipSet.Armor);
            if (equipSet.Gloves)    ApplyEquipEffect(equipSet.Gloves);
            if (equipSet.Shoes)     ApplyEquipEffect(equipSet.Shoes);
            CalculateEnhancedStat();
            RefreshEquipmentSkillEffect();
        }
        
        private void ApplyEquipEffect(Equipment equipment)
        {
            if (equipment.AddMaxHP > 0)                    equipStat.MaxHp += equipment.AddMaxHP;
            if (equipment.AddDamage > 0)                   equipStat.AttackPower += equipment.AddDamage;                  
            if (equipment.AddDefence > 0)                  equipStat.Defense += equipment.AddDefence;
            if (equipment.AddGrantSkillId > 0)             AddGrantSkill(equipment.AddGrantSkillId);
            if (equipment.AddMoveSpeed > 0)                equipStat.MoveSpeed += equipment.AddMoveSpeed;                
            if (equipment.AddCoolTimeReductionRatio > 0f)  equipStat.CoolTimeReduce += equipment.AddCoolTimeReductionRatio; 
            if (equipment.AddItemAcquisitionRange > 0f)    equipStat.ItemAcquisitionRange += equipment.AddItemAcquisitionRange;   
            //if (equipment.AddSkillChainCount > 0)         
            //if (equipment.AddProjectileCount > 0)         
            //if (equipment.AddPenetration > 0)             
            //if (equipment.AddMaxHPRatio > 0f)             
            //if (equipment.AddHPRegenPerSeconds > 0f)      
            //if (equipment.AddCriticalChance > 0f)         
            //if (equipment.AddCriticalMultiplier > 0f)     
            //if (equipment.AddMonsterFindRange > 0f)       
            //if (equipment.AddSkillRangeRatio > 0f)        
            //if (equipment.AddExpGainRatio > 0f)           
            //if (equipment.AddGoldGainRatio > 0f)          
            //if (equipment.AddItemDropRateRatio > 0f)      
        }

        private void AddGrantSkill(int index)
        {
            var skill = skillData.ItemSkillDict[index];
            SkillEffectManager.Inst.EquipmentSkillEffect(equipStat, skill);
            equipmentSkills.Add(skill);
        }

        private void ClearEquipmentSkillEffects()
        {
            for (int i = 0; i < equipmentSkillEffects.Count; i++)
            {
                var skill = equipmentSkillEffects[i];
                equipmentSkillEffects.Remove(skill);
                ObjectPoolManager.Inst.Despawn(skill);
            }
        }

        private void RefreshEquipmentSkillEffect()
        {
            for (int i = 0; i < equipmentSkills.Count; i++)
            {
                var effect = ObjectPoolManager.Inst.Spawn(equipmentSkills[i].SkillName, tr);
                effect.transform.localPosition = Vector3.zero;
                equipmentSkillEffects.Add(effect);
            }
        }

        private void RegistDefaultAudioClips() {
            Span<SoundKey> keys = stackalloc SoundKey[] {
                SoundKey.Player_Evasion_0,
                SoundKey.Player_Evasion_1,
                SoundKey.Player_Hit_0,
                SoundKey.Player_Moving
            };
            SoundManager.Inst.RegistAudioClips(keys);
        }

        private void PlayHitRandomSE(float currentHp, float maxHp) {
            SoundManager.Inst.PlaySE(SoundKey.Player_Hit_0.ToKey(), 0.8f);
        }

        private async UniTaskVoid PlayMovingSoundAsync(float specifiedWaitDuration = -1) {
            CancelMovingSound();
            movingSoundCts = new CancellationTokenSource();
            
            while (true) {
                var currenVelocity = rigid.velocity;
                var velocityMagnitude = currenVelocity.magnitude;
                float waitSeconds = 0.5f - 0.02f * (velocityMagnitude / 5f); // time = baseSpeed - decayRate * speed
                var isCanceled = await UniTask.WaitForSeconds(waitSeconds, false, PlayerLoopTiming.Update, movingSoundCts.Token).SuppressCancellationThrow();
                if (isCanceled) {
                    return;
                }
                SoundManager.Inst.PlaySE(SoundKey.Player_Moving.ToKey(), 0.3f);
            }
        }

        private void CancelMovingSound() {
            if (movingSoundCts == null) {
                return;
            }
            movingSoundCts.Cancel();
            movingSoundCts.Dispose();
            movingSoundCts = null;
        }
        
        #region Public Methods

        public bool IsWalking() => rigid.velocity.x != 0 || rigid.velocity.y != 0 || IsMoveForced;

        public void StartAttack() => hasFiredProjectile = true;

        public bool IsAttacking() => hasFiredProjectile;

        public void FinishAttackAnimation() => hasFiredProjectile = false;

        public bool IsDamaged() => isPlayerDamaged;

        public void FinishHitAnimation() => isPlayerDamaged = false;

        public bool IsDead() => isDead;

        public void OnDamaged(DamageResult damageResult)
        {
            if (isInvincible)
                return;
            
            var calculatedDamage = damageResult.CalculatedDamage;
            enhancedStat.CurrentHp -= calculatedDamage;
            DamageTextManager.Inst.OnFloatingText(calculatedDamage, tr.position, true);
            isPlayerDamaged = true;

            if (enhancedStat.CurrentHp <= 0)
            {
                enhancedStat.CurrentHp = 0;
                OnDead();
            }
            
            DungeonEvtManager.InvokeEventDecreasePlayerHP(enhancedStat.CurrentHp, enhancedStat.MaxHp);
        }

        public void IncreasePlayerHealth(float incAmount) {
            var value = enhancedStat.CurrentHp + incAmount;
            enhancedStat.CurrentHp = value > enhancedStat.MaxHp ? enhancedStat.MaxHp : value;
            DungeonEvtManager.InvokeEventIncreasePlayerHP(enhancedStat.CurrentHp, enhancedStat.MaxHp);
        }

        private void SetDirectionToPosition(Vector2 toPosition) {
            var isMatchedAxisX = Mathf.Approximately(tr.position.x, toPosition.x);
            if (isMatchedAxisX) {
                return;
            }
            var isLeft = tr.position.x > toPosition.x;
            SwitchingPlayerDirection(isLeft);
        }
        
        public async UniTask<bool> LerpMoveTransformAsync(List<Vector2> positions, int loopCount, float stepDuration) {
            // cancel and assign new token
            CancelLerpMove();
            IsMoveForced = true;
            lerpMoveCts = new CancellationTokenSource();
            mainCamController.SetUpdateType(CameraUpdateType.LateUpdate, CameraMoveType.Normal);
            CancelMovingSound();
            
            // set velocity to zero
            rigid.velocity = Vector2.zero;
            var isCanceled1 = await UniTask.NextFrame(PlayerLoopTiming.Update, lerpMoveCts.Token).SuppressCancellationThrow();
            if (isCanceled1) {
                return false;
            }
            
            var startPos = transform.position;
            for (int i = 0; i < loopCount; i++) {
                var position = positions[i];
                var elapsedTime = 0f;
                var lerpT = 0f;
                SetDirectionToPosition(position);
                
                while (lerpT < 1f) {
                    elapsedTime += Time.fixedDeltaTime;
                    lerpT = Mathf.Clamp01(elapsedTime / stepDuration);
                    var movedPosition = Vector2.Lerp(startPos, position, lerpT);
                    rigid.MovePosition(movedPosition);
                    
                    var isCanceled2 = await UniTask.Yield(PlayerLoopTiming.FixedUpdate, lerpMoveCts.Token).SuppressCancellationThrow();
                    if (isCanceled2) {
                        return false;
                    }
                }

                rigid.MovePosition(position);
                startPos = position;
            }
            
            mainCamController.SetUpdateType(CameraUpdateType.LateUpdate, CameraMoveType.Smooth);
            IsMoveForced = false;
            return true;
        }

        public async UniTask<bool> LerpMoveRigidBodyAsync(Vector2 startPos, Vector2 endPos, float duration) {
            // cancel and assign new token
            CancelLerpMove();
            IsMoveForced = true;
            lerpMoveCts = new CancellationTokenSource();
            CancelMovingSound();
            
            // lerp move position rigidbody
            float elapsedTime = 0f;
            float lerpT = 0f;
            while (lerpT < 1f) {
                elapsedTime += Time.fixedDeltaTime;
                lerpT = Mathf.Clamp01(elapsedTime / duration);
                var movePos = Vector2.Lerp(startPos, endPos, lerpT);
                rigid.MovePosition(movePos);
                
                var isCancelled = await UniTask.Yield(PlayerLoopTiming.FixedUpdate, lerpMoveCts.Token).SuppressCancellationThrow();
                if (isCancelled) {
                    return false;
                }
            }

            rigid.MovePosition(endPos);
            IsMoveForced = false;
            return true;
        }

        private void CancelLerpMove() {
            if (lerpMoveCts == null) {
                return;
            }
            
            lerpMoveCts.Cancel();
            lerpMoveCts.Dispose();
            lerpMoveCts = null;
            IsMoveForced = false;
            mainCamController.SetUpdateType(CameraUpdateType.LateUpdate, CameraMoveType.Smooth);
        }

        public void AddExp(float value) {
            playerLevelData.AddExp(value);
            CheckWithIncreaseLevel();
            DungeonEvtManager.InvokeEventIncreasePlayerExp(playerLevelData.GetCurrentExp(), playerLevelData.GetExpToNextLevel()); 
        }

        public void SetBattleMode(bool isOn) {
            isPlayerInBattle = isOn;
        }
        
        public void ForcedIncreaseLevel() {
            playerLevelData.ForcedLevelUp();
            OnLevelIncreased();
        }
        
        public void CalculateEnhancedStat()
        {
            enhancedStat = enhancedStat.AddStat(baseStat, buffedStat, equipStat);
            itemAquisitionCollider.radius = enhancedStat.ItemAcquisitionRange;
        }

        public void SetSkeletonData(string key)
        {
            var assetKey = key;
            assetKey = assetKey.IncrementSkeletonDataVersion();
            ResourceManager.Inst.AddressablesAsyncLoad<SkeletonDataAsset>(assetKey, false, (skeletonDataAsset) =>
            {
                if (!skeletonDataAsset)
                {
                    CatLog.Log("SkeletonDataAsset Load Failed !");
                    return;
                }

                skeletonAnimation.skeletonDataAsset = skeletonDataAsset;
                skeletonAnimation.initialSkinName = assetKey.RemoveSuffix("_SkeletonData");
                skeletonAnimation.Initialize(true);
            });
        }

        #endregion
    }
}