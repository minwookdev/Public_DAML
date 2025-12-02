/// CODER	      :	MIN WOOK KIM
/// MODIFIED DATE : 2023.08.23
/// IMPLEMENTATION: RoomData를 위한 ScriptableObject Rarity 별로 RoomData를 다르게 주기 위해 작성
using Sirenix.OdinInspector;
using UnityEngine;

namespace RandomDungeonWithBluePrint {
	public class RoomDataEntity : ScriptableObject {
		[Title("Common")]
		public int Rarity = 0;
	}
}
