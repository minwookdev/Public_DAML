using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoffeeCat;

namespace RandomDungeonWithBluePrint
{
    public class Gate
    {
        public int Direction;
        public Section Section;
        public Vector2Int Position;
    }

    public class ConnectionDetail 
    {
        public Section From;
        public Section To;
        public Gate FromGate;
        public Gate ToGate;
        public List<Vector2Int> Ways = new();
    }

    public class Field
    {
        public int MaxRoomNum;
        public Vector2Int Size;
        public List<Section> Sections;
        public List<Connection> Connections; 
        public List<ConnectionDetail> ConnectionDetails = new();
        public List<Vector2Int> Branches;
        public List<Gate> Gates;
        public Dictionary<RoomType, List<Section>> RoomDictionary = null;

        public Grid Grid { get; } = new();
        public List<Room> Rooms => Sections.Where(s => s.IsExistRoom).Select(s => s.Room).ToList();
        public bool RoomIsFull => MaxRoomNum <= Rooms.Count;
        // 섹션에 Room이 존재하지 않으면서 Room 생성 가중치가 0보다 큰 섹션들에
        // Room이 존재하는지에 대한 조건값을 모두가 충족하는지의 반대값
        // All => 모든 요소가 ExistRoom 이라면 ExistRoomToBeMake = false;
        //     => 하나의 요소라도 ExistRoom 이 아니라면 ExistRoomToBeMake = true;
        public bool ExistRoomToBeMake => !Sections.Where(s => !s.IsExistRoom && s.MakeRoomWeight > 0).All(s => s.IsExistRoom);

        public void BuildGrid()
        {
            Grid.Build(Size, Rooms, Branches, Gates);
        }

        public Section GetSection(int index)
        {
            return Sections?.FirstOrDefault(s => s.Index == index);
        }

        // section이 어느 section과도 이어지지 않는지
        public bool IsIsolatedSection(Section section)
        {
            // ConnectedAny : 매개변수로 받은 section의 Index가 Connection에 정의한 From Index, To Index와 일치하는지 비교
            // 어디에도 일치하지 않는다면 Connection이 없는 Section
            return !Connections.Any(c => c.ConnectedAny(section.Index));
        }

        // IsolatedSection이면서 주위에 Connection을 가진 Section이 있는 것을 찾음
        public IEnumerable<Section> IsolatedAndExistConnectedSectionAroundSections()
        {
            return Sections.Where(s => IsIsolatedSection(s) && ExistConnectedSectionAround(s));
        }

        // Sections 안에서 section에 인접한 것을 반환
        public IEnumerable<Section> GetSectionsAdjoinWith(Section section)
        {
            return Sections.Where(s => section != s && section.AdjoinWith(s));
        }

        // section에 인접하고, 어느 곳이든지 Connection 연결되어 있는 Section들을 찾아 리스트로 반환
        public IEnumerable<Section> GetSectionsAdjoinWithConnected(Section section)
        {
            return GetSectionsAdjoinWith(section).Where(s => Connections.Any(c => c.ConnectedAny(s.Index)));
            // return GetSectionsAdjoinWith(section).Where(s => !IsIsolatedSection(s));
        }

        // section주변에 Connection이 있는 Section이 존재하는지
        // 인접한 Section을 찾아서 그 중 하나라도 Connection이 존재한다면 True
        public bool ExistConnectedSectionAround(Section section)
        {
            return GetSectionsAdjoinWith(section).Any(s => !IsIsolatedSection(s));
        }

        public bool Connected(Section a, Section b)
        {
            return Connections.Any(c => c.Connected(a.Index, b.Index));
        }

        public bool TryFindRoomFromType(RoomType roomType, out Room result) {
            result = null;
            if (!RoomDictionary.TryGetValue(roomType, out List<Section> sections)) {
                return false;
            }

            result = sections.FirstOrDefault()?.Room;
            return result != null;
        }
        
        public bool IsConnectable(Section a, Section b)
        {
            if (a == null || b == null) return false;
            if (a == b) return false;
            if (!a.AdjoinWith(b)) return false;
            if (Connected(a, b)) return false;
            if (!a.ExistUnconnectedJoints(a.AdjoiningWithDirection(b))) return false;
            if (!b.ExistUnconnectedJoints(b.AdjoiningWithDirection(a))) return false;
            return true;
        }
        
        public float GetDistanceBySection(Section a, Section b)
        {
            return Vector2.Distance(a.Rect.center, b.Rect.center);
        }

        public SectionLinked GetLinkedSectionByFrom(Gate gate) {
            for (int i = 0; i < Sections.Count; i++) {
                var linkedSections = Sections[i].SectionLinkeds;
                for (int j = 0; j < linkedSections.Count; j++) {
                    if (linkedSections[j].FromGate == gate) {
                        return linkedSections[j];
                    }
                }
            }
            return null;
        }

        public void Dispose() {
            for (int i = 0; i < Sections.Count; i++) {
                Sections[i].Dispose();
            }
            Sections.Clear();
        }
    }
}