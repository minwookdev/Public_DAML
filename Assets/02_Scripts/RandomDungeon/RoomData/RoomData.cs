using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using RandomDungeonWithBluePrint;
using UnityRandom = UnityEngine.Random;

namespace CoffeeCat.RogueLite {
	public class RoomData {
		public RoomType RoomType { get; protected set; }                   // 룸 타입
		public int RoomIndex { get; protected set; } = 0;                  // 룸 인덱스
		public int Rarity { get; protected set; } = 0;                     // 룸의 레어도
		public bool IsCleared { get; protected set; } = false;             // 해당 룸의 클리어 여부
		public bool IsLocked { get; protected set; } = false;              // 현재 룸이 잠금 상태
		public bool IsPlayerInside { get; protected set; } = false;        // 플레이어가 방 안에 있는지
		public bool IsPlayerFirstEntered { get; protected set; }  = false; // 플레이어의 처음 방문 여부
		protected Action<bool> OnRoomLocked { get; private set; } = null;  // 방 잠금 상태 변경 시 실행할 액션
		protected Interaction Interaction = null;            // 상호작용 오브젝트
		protected Vector3 roomCenterPosition = Vector3.zero;
		protected readonly DungeonSceneBase dungeonSceneBase = null;
		protected readonly DungeonBluePrint DungeonBluePrint = null;
		
		public RoomData(RoomType roomType, int index, int rarity = 0, Room room = null) {
			RoomType = roomType;
			RoomIndex = index;
			Rarity = rarity;
			roomCenterPosition = room.FloorRectInt.center;
			dungeonSceneBase = DungeonSceneBase.Inst;
			DungeonBluePrint = dungeonSceneBase.DungeonBluePrint;
		}

		public virtual void Initialize() {
			
		}

		public void AddEventOnRoomLocked(GateObject gateObject) {
			OnRoomLocked += gateObject.Lock;
		}
		
		protected virtual void OnRoomFirstEntered() {
			
		}

		/// <summary>
		/// Room Entering Callback
		/// </summary>
		public virtual void EnteredPlayer() {
			IsPlayerInside = true;
			var loomDataStruct = new RoomDataStruct(this);
			if (!IsPlayerFirstEntered)
			{
				DungeonEvtManager.InvokeEventRoomEnteringFirst(loomDataStruct);
				IsPlayerFirstEntered = true;
				OnRoomFirstEntered();
			}
			DungeonEvtManager.InvokeEventRoomEntering(loomDataStruct);
		}

		/// <summary>
		/// Room Leaving Callback
		/// </summary>
		public virtual void LeavesPlayer() {
			IsPlayerInside = false;
			var loomDataStruct = new RoomDataStruct(this);
			DungeonEvtManager.InvokeEventRoomLeft(loomDataStruct);
		}

		protected void SetInteractable(InteractionType type) {
			if (Interaction) {
				return;
			}
			
			// Spawn Interactable Object
			Interaction = ObjectPoolManager.Inst.Spawn<Interaction>(type.ToKey(), roomCenterPosition);
		}

		protected void DisableInteractable() {
			if (!Interaction) {
				return;
			}
			Interaction.StopParticle();
			Interaction.gameObject.SetActive(false);
		}

		protected void ActiveInteractable() {
			if (!Interaction) {
				return;
			}
			Interaction.gameObject.SetActive(true);
			Interaction.PlayParticle();
		}
		
		public virtual void Dispose() {
			OnRoomLocked = null;
			if (Interaction && ObjectPoolManager.IsExist) {
				ObjectPoolManager.Inst.Despawn(Interaction.gameObject);	
			}
		}
	}

	public class BattleRoom : RoomData {
		// Monster Spawn Variables
		private static readonly float spawnIntervalTime = 0.35f;
		public Vector2[] SpawnPositions { get; private set; } = null;
		private List<MonsterSpawnData> spawnDataList;
		private List<MonsterStatus> spawnedMonsters = null;
		private string groupSpawnPositionsKey;
		private float totalWeight = 0f;
		private int groupMonsterSpawnCount = 0;
		private int keepAverageCount = 0;
		
		// Room Clear Variables
		private int MaxSpawnCount = 0;
		private float EndureSeconds = 0f;
		
		public BattleRoom(Room room, int index, BattleRoomDataEntity entity) : base(RoomType.MonsterSpawnRoom, index, entity.Rarity, room) {
			SpawnPositions = GetMonsterSpawnPositions(room, tileRadius: 0.5f);
			MaxSpawnCount = entity.MaxSpawnMonster;
			keepAverageCount = entity.KeepAverageCount;
			EndureSeconds = entity.EndureSeconds;
			var weights = entity.SpawnWeights;
			spawnDataList = new List<MonsterSpawnData>();
			spawnedMonsters = new List<MonsterStatus>();
			// Weights first
			for (int i = 0; i < weights.Length; i++) {
				// 동일한 Index의 SpawnKey가 존재하지 않거나 스폰 확률이 지정되어있지 않은 경우
				if (entity.SpawnKeys.Length <= i || entity.SpawnWeights[i] <= 0f) {
					continue;
				}

				var spawnData = new MonsterSpawnData() {
					Weight = weights[i],
					Key = entity.SpawnKeys[i]
				};
				
				// Add New Spawn Data
				spawnDataList.Add(spawnData);
				totalWeight += weights[i];
			}
			
			// Preload Monsters
			for (int i = 0; i < spawnDataList.Count; i++) {
				string key = spawnDataList[i].Key.ToKey();
				SafeLoader.Regist(key);
			}
			// Preload Group Monsters Positions
			if (entity.GroupSpawnPointKey == AddressablesKey.NONE)
				return;
			groupSpawnPositionsKey = entity.GroupSpawnPointKey.ToKey();
			SafeLoader.Regist(groupSpawnPositionsKey);
		}

		protected override void OnRoomFirstEntered() {
			// ignoring spawn monster is room cleared flagged
			if (IsCleared) {
				return;
			}
			
			// Room Locked And Monster Spawn Start
			IsLocked = true;
			OnRoomLocked?.Invoke(IsLocked);
			SpawnGroupMonster();          // 그룹 몬스터 스폰
			ObservableUpdateBattleRoom(); // 일반 몬스터 스폰

			return;
			void SpawnGroupMonster() {
				if (groupSpawnPositionsKey.Equals(string.Empty)) {
					return;
				}

				var groupSpawnPoint = ObjectPoolManager.Inst.Spawn<MonsterGroupSpawnPoint>(groupSpawnPositionsKey, roomCenterPosition);
				var points = groupSpawnPoint.SpawnPositions;
				foreach (var point in points) {
					if (groupMonsterSpawnCount >= MaxSpawnCount) {
						break;
					}

					// Spawn Monster Group Spawn Position
					var key = RaffleSpawnMonster();
					var spawnedMonster = ObjectPoolManager.Inst.Spawn<MonsterStatus>(key, point);
					spawnedMonster.Spawn();
					spawnedMonsters.Add(spawnedMonster);
					groupMonsterSpawnCount++;
				}
			}
			
			void ObservableUpdateBattleRoom() {
				// Variables
				float spawnTimer = 0f;
				float endureTimer = 0f;
				int currentSpawnCount = 0;
			
				// Subscribe Spawn Update Observable
				Observable.EveryUpdate()
				          .Skip(TimeSpan.Zero)
				          .Select(_ => dungeonSceneBase.CurrentRoomMonsterKilledCount)
				          .TakeWhile(_ => !IsCleared)
				          .DoOnCompleted(OnCleared)
				          .Subscribe(currentKillCount => {
					          endureTimer += Time.deltaTime;
					          spawnTimer += Time.deltaTime;
					          
					          // Spawn Monster
					          if (spawnTimer >= spawnIntervalTime) {
						          SpawnMonster();
						          spawnTimer = 0f;
					          }
					          
					          // Check Room Clear Condition
					          IsCleared = IsClear(currentKillCount);
				          });
				return;

				// Monster Spawn 
				void SpawnMonster() {
					if (currentSpawnCount >= MaxSpawnCount) {
						return;
					}

					var activatedMonsters = spawnedMonsters.Count(monster => monster.IsAlive);
					if (activatedMonsters >= keepAverageCount)
						return;
					
					var spawnedMonster = ObjectPoolManager.Inst.Spawn<MonsterStatus>(RaffleSpawnMonster(), GetRandomPos());
					spawnedMonster.Spawn();
					spawnedMonsters.Add(spawnedMonster);
					currentSpawnCount++;
				}

				// Battle Room Clear Condition
				bool IsClear(int killedCount) => (killedCount >= MaxSpawnCount || endureTimer >= EndureSeconds);
			}
		}

		public override void LeavesPlayer() {
			base.LeavesPlayer();

			if (!IsCleared) {
				// Return to Room
			}
		}

		private void OnCleared() {
			// Despawn All Alive Monsters
			var aliveMonsters = spawnedMonsters.Where(monster => monster.IsAlive);
			foreach (var monster in aliveMonsters) {
				monster.Kill(false);
			}
			spawnedMonsters.Clear();
			spawnedMonsters = null;
			
			IsCleared = true;
			IsLocked = false;
			OnRoomLocked?.Invoke(IsLocked);
			var roomDataStruct = new RoomDataStruct(this);
			DungeonEvtManager.InvokeEventClearedRoom(roomDataStruct);
			dungeonSceneBase.ClearCurrentRoomKillCount();
		}

		Vector2[] GetMonsterSpawnPositions(Room room, float tileRadius = 0.5f) {
			var floors = room.Floors;
			float floorXMin = floors[0].x, 
			      floorXMax = floors[0].x, 
			      floorYMin = floors[0].y, 
			      floorYMax = floors[0].y;
			foreach (var floor in floors) {
				if (floor.x < floorXMin) floorXMin = floor.x;
				if (floor.x > floorXMax) floorXMax = floor.x;
				if (floor.y < floorYMin) floorYMin = floor.y;
				if (floor.y > floorYMax) floorYMax = floor.y;
			}
			
			List<Vector2> positionList = new List<Vector2>();
			for (int i = 0; i < floors.Count; i++) {
				if (floors[i].x == floorXMin || floors[i].x == floorXMax ||
				    floors[i].y == floorYMin || floors[i].y == floorYMax) {
					continue;
				}

				Vector2 position = new Vector2(floors[i].x + tileRadius, floors[i].y + tileRadius);
				if (positionList.Contains(position)) {
					continue;
				}
				positionList.Add(position);
			}
			return positionList.ToArray();
		}

		private struct MonsterSpawnData {
			public float Weight;
			public AddressablesKey Key;
		}

		private string RaffleSpawnMonster() {
			float randomPoint = UnityRandom.value * totalWeight;
			int index = 0;
			for (int i = 0; i < spawnDataList.Count; i++) {
				if (randomPoint < spawnDataList[i].Weight) {
					index = i;
				}
				else {
					randomPoint -= spawnDataList[i].Weight;
				}
			}
			return spawnDataList[index].Key.ToKey();
		}

		private Vector2 GetRandomPos() {
			int index = UnityRandom.Range(0, SpawnPositions.Length);
			return SpawnPositions[index];
		}

		public override void Dispose() {
			base.Dispose();
			if (ObjectPoolManager.IsExist) {
				ObjectPoolManager.Inst?.DespawnAll(groupSpawnPositionsKey);
			}
			IsCleared = true;
		}
	}

	public class PlayerSpawnRoom : RoomData {
		private readonly bool isGrantItemsOnEnteredRoom = false;
		private StartRoomObject spawnedStartRoomObject = null;
		
		public PlayerSpawnRoom(Room room, int index) : base(RoomType.PlayerSpawnRoom, index, room: room) {
			isGrantItemsOnEnteredRoom = DungeonBluePrint.IsGrantItemsOnStart;
		}
		
		protected override void OnRoomFirstEntered() {
			var isFirstFloor = dungeonSceneBase.CurrentFloor == 0;
			if (!isFirstFloor) {
				return;
			}

			if (!isGrantItemsOnEnteredRoom) {
				return;
			}

			var objectSpawnKey = AddressablesKey.Object_Start_Room.ToKey();
			SafeLoader.Regist(objectSpawnKey, (result) => {
				if (!result) {
					CatLog.ELog("Failed to load Start Room Item Object.");
					return;
				}
				
				spawnedStartRoomObject = ObjectPoolManager.Inst.Spawn<StartRoomObject>(objectSpawnKey, roomCenterPosition);
				spawnedStartRoomObject.Init(DungeonBluePrint.StartRoomTable);
			});
		}

		public override void Dispose() {
			base.Dispose();
			if (spawnedStartRoomObject && ObjectPoolManager.IsExist) {
				ObjectPoolManager.Inst.Despawn(spawnedStartRoomObject.gameObject);
			}
		}
	}

	public class RewardRoom : RoomData {
		public RewardRoom(Room room, int index, int rarity) : base(RoomType.RewardRoom, index, rarity, room) { }
		
		protected override void OnRoomFirstEntered() {
			base.OnRoomFirstEntered();
			SetInteractable(InteractionType.DungeonReward);
			var rewardRoomLootTable = DungeonBluePrint.RewardItemTable[(RoomGradeType)Rarity];
			var rewardInteractable = Interaction as InteractionDungeonReward;
			if (!rewardInteractable) {
				CatLog.WLog("Failed to Casting Interactable Object to Reward Interactable.");
				return;
			}
			rewardInteractable.Init(rewardRoomLootTable);
		}
		
		public override void EnteredPlayer() {
			base.EnteredPlayer();
			ActiveInteractable();
		}

		public override void LeavesPlayer() {
			base.LeavesPlayer();
			DisableInteractable();
		}
	}

	public class ShopRoom : RoomData {
		public ShopRoom(Room room, int index, int rarity) : base(RoomType.ShopRoom, index, rarity, room) { }

		protected override void OnRoomFirstEntered() {
			base.OnRoomFirstEntered();
			SetInteractable(InteractionType.DungeonShop);
			var shopLootTable = DungeonBluePrint.ShopItemTable[(RoomGradeType)Rarity];
			var shopInteractable = Interaction as InteractionDungeonShop;
			if (!shopInteractable) {
				CatLog.WLog("Failed to Casting Interactable Object to Shop Interactable.");
				return;
			}
			shopInteractable.Init(shopLootTable);
		}

		public override void EnteredPlayer() {
			base.EnteredPlayer();
			ActiveInteractable();
		}

		public override void LeavesPlayer() {
			base.LeavesPlayer();
			DisableInteractable();
		}
	}
	
	public class EmptyRoom : RoomData {
		public EmptyRoom(Room room, int index) : base(RoomType.EmptyRoom, index, room: room) { }
	}

	public class ExitRoomInteractable : RoomData {
		private InteractionType interactionType = InteractionType.None;
		
		public ExitRoomInteractable(Room room, int index) : base(RoomType.ExitRoom, index, room: room) { }

		public override void Initialize() {
			base.Initialize();
			interactionType = dungeonSceneBase.IsNextFloorLast() ? InteractionType.DungeonBoss : InteractionType.DungeonNextFloor;
		}

		protected override void OnRoomFirstEntered() {
			base.OnRoomFirstEntered();
			SetInteractable(interactionType);
		}

		public override void EnteredPlayer() {
			base.EnteredPlayer();
			ActiveInteractable();
		}

		public override void LeavesPlayer() {
			base.LeavesPlayer();
			DisableInteractable();
		}
	}

	public struct RoomDataStruct
	{
		public RoomType RoomType;
		public int RoomIndex;

		public RoomDataStruct(RoomData roomData)
		{
			RoomType = roomData.RoomType;
			RoomIndex = roomData.RoomIndex;
		}
	}
}
