using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using RandomDungeonWithBluePrint;
using CoffeeCat.FrameWork;
using CoffeeCat.RogueLite;

namespace CoffeeCat
{
    public class Minimap : MonoBehaviour
    {
        private readonly Dictionary<int, Minimap_Room> minimapRooms = new Dictionary<int, Minimap_Room>();
        [ShowInInspector] private List<Minimap_Branch> minimapBranches = new List<Minimap_Branch>();
        private const float minimapRatio = 4f;
        private const string roomPanelKey = "RoomPanel";
        private const string branchKey = "MinimapBranch";

        [SerializeField] private RectTransform panelTr = null;
        [SerializeField] private RectTransform branchTr = null;
        [SerializeField] private Button btnClose = null;
        
        private void Start() => btnClose.onClick.AddListener(Close);
        
        public void GenerateMiniMap(Field field)
        {
            panelTr.sizeDelta = new Vector2(field.Size.x, field.Size.y) * minimapRatio;
            
            foreach (var room in field.Rooms)
            {
                var roomObj = ObjectPoolManager.Inst.Spawn(roomPanelKey, panelTr);
                var minimapRoom = roomObj.GetComponent<Minimap_Room>();
                minimapRoom.Initialize(room, minimapRatio);
                minimapRooms.Add(room.RoomData.RoomIndex, minimapRoom);
            }

            foreach (var connection in field.Connections)
            {
                var spawnObj = ObjectPoolManager.Inst.Spawn(branchKey, branchTr);
                var minimapBranch = spawnObj.GetComponent<Minimap_Branch>();
                var fromSection = field.GetSection(connection.From);
                var toSection = field.GetSection(connection.To);
                
                minimapBranches.Add(minimapBranch);
                minimapBranch.SetBranch(fromSection, toSection, minimapRatio);
                minimapBranch.gameObject.SetActive(false);
            }
            
            AddRoomEvents();
        }

        public void ClearMinimap()
        {
            RemoveRoomEvents();

            foreach (var room in minimapRooms.Values) 
            {
                ObjectPoolManager.Inst.Despawn(room.gameObject);
            }

            foreach (var branch in minimapBranches)
            {
                branch.ClearBranch();
                ObjectPoolManager.Inst.Despawn(branch.gameObject);
            }
            
            minimapRooms.Clear();
            minimapBranches.Clear();
        }
        
        private void AddRoomEvents() {
            DungeonEvtManager.AddEventRoomEnteringEvent(EnterdRoom);
            DungeonEvtManager.AddEventRoomLeftEvent(LeftRoom);
            DungeonEvtManager.AddEventClearedRoomEvent(ClearedRoom);
        }

        private void RemoveRoomEvents() {
            DungeonEvtManager.RemoveEventRoomEntering(EnterdRoom);
            DungeonEvtManager.RemoveEventRoomLeft(LeftRoom);
            DungeonEvtManager.RemoveEventClearedRoom(ClearedRoom);
        }

        private void EnterdRoom(RoomDataStruct roomData)
        {
            var roomPanel = minimapRooms[roomData.RoomIndex];
            roomPanel.EnterdRoom();
            
            foreach (var branch in minimapBranches)
            {
                branch.EnterdRoom(roomData.RoomIndex);
                branch.CheckConnectSection();
            }
        }
        
        private void LeftRoom(RoomDataStruct roomData)
        {
            var roomPanel = minimapRooms[roomData.RoomIndex];
            roomPanel.LeftRoom();
        }
        
        private void ClearedRoom(RoomDataStruct roomData)
        {
            var roomPanel = minimapRooms[roomData.RoomIndex];
            roomPanel.ClearedRoom();
        }

        public void Open() 
        {
            gameObject.SetActive(true);
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}