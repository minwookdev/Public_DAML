using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeCat;
using UnityEngine;

namespace RandomDungeonWithBluePrint
{
    [Serializable]
    public class SectionLinked {
        public Section ToSection;
        public Section FromSection;
        public Gate FromGate;
        public Gate ToGate;
        public List<Vector2Int> Ways;

        public Vector2 GetDirectionButtonWorldPosition() {
            if (Ways.Count < 3) {
                throw new Exception("Invalid Ways Counts.");
            }

            Vector2 pos = Ways[2];
            pos.x += Constants.TileRadius;
            pos.y += Constants.TileRadius;
            return pos;
        }
    }
    
    public class Section
    {
        public int Index;               // Section의 인덱스
        public RectInt Rect;            // Section의 사이즈(x,y,width,height)
        public Room Room;               // Section 안의 Room
        public Relay Relay;             // 길의 연결부위
        public Vector2Int MinRoomSize;  // Section의 최소 사이즈
        public int MakeRoomWeight;      // Room 생성 가중치
        public bool RoomIndispensable;  // Room 생성 필수 여부
        public List<SectionLinked> SectionLinkeds = new();
        private List<SectionLinked> tempSectionLinkeds = new();

        public int Width => Rect.width;
        public int Height => Rect.height;
        public bool IsExistRoom => Room != null;

        public Section() { }

        public Section(FieldBluePrint.Section bluePrint)
        {
            Index = bluePrint.Index;
            Rect = bluePrint.Rect;
            MakeRoomWeight = bluePrint.MakeRoomWeight;
            RoomIndispensable = bluePrint.RoomIndispensable;
            MinRoomSize = bluePrint.MinRoomSize;
        }

        public int AdjoiningWithDirection(Section other)
        {
            return Rect.AdjoiningWithDirection(other.Rect);
        }

        public bool AdjoinWith(Section other)
        {
            return AdjoiningWithDirection(other) != Constants.Direction.Error;
        }

        public Vector2Int GetEdge(Section other, Vector2Int initial = default)
        {
            return Rect.GetEdge(AdjoiningWithDirection(other), initial);
        }

        public IEnumerable<Joint> GetUnConnectedJoints(int direction)
        {
            return Room.GetUnconnectedJoints(direction);
        }

        public IEnumerable<Joint> GetConnectedJoints(int direction)
        {
            return Room.GetConnectedJoints(direction);
        }

        public bool ExistUnconnectedJoints(int direction)
        {
            return !IsExistRoom || GetUnConnectedJoints(direction).Any();
        }
        
        public List<SectionLinked> GetOtherLinkedSections(Section targetSection) {
            tempSectionLinkeds.Clear();
            for (int i = 0; i < SectionLinkeds.Count; i++) {
                if (SectionLinkeds[i].ToSection != targetSection) {
                    tempSectionLinkeds.Add(SectionLinkeds[i]);
                }
            }
            return tempSectionLinkeds;
        }

        public SectionLinked GetToLinkedSection(Section toSection) {
            return SectionLinkeds.FirstOrDefault(s => s.ToSection == toSection);
        }
        
        public void Dispose() {
            Room?.Dispose();
            Room = null;    
        }
    }
}