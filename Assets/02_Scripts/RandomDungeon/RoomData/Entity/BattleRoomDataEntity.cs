/// CODER	      :	MIN WOOK KIM
/// MODIFIED DATE : 2023. 08. 23
/// IMPLEMENTATION: Battle Room의 Data를 위한 ScriptableObject
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.Defines;

namespace RandomDungeonWithBluePrint {
	[CreateAssetMenu(menuName = "CoffeeCat/Battle Room Entity")]
	public class BattleRoomDataEntity : RoomDataEntity {
		// Default Spawn Monster
		[HorizontalGroup("Default Monster Spawn", Title = "Default Spawn")] 
		public AddressablesKey[] SpawnKeys;
		[HorizontalGroup("Default Monster Spawn", Title = "Default Spawn")] 
		public float[] SpawnWeights;
		
		// Group Spawn Monster
		[TitleGroup("Monster Spawn Key")] 
		public AddressablesKey GroupSpawnPointKey = default;
		
		// Options
		[TitleGroup("Options")]
		public int MaxSpawnMonster = 0;
		public float EndureSeconds = 300f; // 5min
		public int KeepAverageCount = 6;
	}
}
