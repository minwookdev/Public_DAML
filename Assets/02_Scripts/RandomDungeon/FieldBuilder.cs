using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using CoffeeCat;
using Random = UnityEngine.Random;

namespace RandomDungeonWithBluePrint
{
    public static class FieldBuilder 
    {
        public static Field Build(FieldBluePrint bluePrint)
        {
            // bluePrint 스크립터블 오브젝트에 따라 Field를 생성
            var field = new Field
            {
                Size = bluePrint.Size,
                MaxRoomNum = Random.Range(bluePrint.MinRoomNum, bluePrint.MaxRoomNum + 1),
                Gates = new List<Gate>()
            };

            MakeSection(field, bluePrint);　     
            MakeRooms(field);                   
            MakeRoomsType(field, bluePrint);    
            MakeBranches(field, bluePrint);     
            MakeConnectionDetails(field);
            field.BuildGrid();
            
            // PrintConnectionDetails(field);
            return field;
        }

        private static void MakeSection(Field field, FieldBluePrint bluePrint)
        {
            field.Sections = bluePrint.Sections.Select(s => new Section(s)).ToList();
        }

        private static void MakeRooms(Field field)
        {
            MakeIndispensableRooms(field); // 필수적인 Room 생성
            MakeStochasticRooms(field);    // 확률적인 Room 생성
            MakeRelay(field);              // Room이 없는 Section에 Branch의 연결지점 생성 
        }

        // 필수적인 Room 생성
        private static void MakeIndispensableRooms(Field field)
        {
            var targetSections = field.Sections.Where(s => s.RoomIndispensable)
                .OrderBy(s => s.Index)
                .Take(field.MaxRoomNum);
            foreach (var section in targetSections)
            {
                MakeRoom(section);
            } 
        }

        // 필수가 아닌 Room (확률적으로) 생성
        private static void MakeStochasticRooms(Field field)
        {
            // !RoomIsFull : 방의 개수가 한계가 아닐 때
            // ExistRoomToBeMake : 방을 만들 장소가 존재할 때
            while (!field.RoomIsFull && field.ExistRoomToBeMake)
            {
                // 가중치에 따라 랜덤으로 섹션을 정해서 섹션에 Room 을 생성
                var targetSection = RaffleForMakeRoom(field);
                MakeRoom(targetSection);
            }
        }

        private static void MakeRoom(Section section)
        {
            var sectionWithPadding = section.Rect.AddPadding(2); // Padding을 적용한 새로운 RectInt
            var roomRect = GetRoomRect(sectionWithPadding, section.MinRoomSize); // MinRoomsize ~ sectionWithPadding 까지의 랜덤 사이즈로 방 사이즈를 결정
            var safeArea = sectionWithPadding.SafeAreaOfInclusion(roomRect); //sectionWithPadding의 가로 세로 값에서 roomRect의 가로 세로 값을 뺀 후 room이 생성될 수 있는 safeArea 정의
            roomRect.x = Random.Range(safeArea.xMin, safeArea.xMax);
            roomRect.y = Random.Range(safeArea.yMin, safeArea.yMax); // SafeArea의 xMax, yMax에 생성되더라도 sectionWithPadding 내에 생성됨
            section.Room = new Room(roomRect);

            // var mod = Random.Range(0, 10) % 2;
            foreach (var direction in Constants.Direction.FourDirections)
            {
                // var edgePositions = section.Room.Edge[direction].ToList(); // Room 의 방향별 가장자리
                
                section.Room.SetJoint(direction, section.Room.EdgeWithCenter[direction]);

                // for (var i = 0; i < edgePositions.Count; i++)
                // {
                //     if (i % 2 == mod)
                //     {
                //         // 가장자리 변에서 하나씩 점프하며 출구 예정지를 정의
                //         section.Room.SetJoint(direction, edgePositions[i]);
                //     }
                // }
            }
            
            // 정의된 Joint에 따라 Wall Tiles에서 Joint Position 제외
            section.Room.ExceptJointTilesInWallDictionary();
        }

        private static RectInt GetRoomRect(RectInt source, Vector2Int minSize)
        {
            if (minSize.x < 0 || minSize.y < 0 || source.width < minSize.x || source.height < minSize.y)
            {
                throw new ArgumentOutOfRangeException();
            }

            return new RectInt(source.x, source.y, Random.Range(minSize.x, source.width + 1), Random.Range(minSize.y, source.height + 1));
        }

        private static void MakeRelay(Field field)
        {
            // Room이 존재하지 않는 Section
            foreach (var section in field.Sections.Where(s => !s.IsExistRoom))
            {
                var padding = section.Rect.AddPadding(2);
                var point = new Vector2Int(Random.Range(padding.xMin, padding.xMax), Random.Range(padding.yMin, padding.yMax));
                section.Relay = new Relay(section.Index, point);
            }
        }

        // Room 을 생성하기 위한 추첨
        private static Section RaffleForMakeRoom(Field field)
        {
            var candidate = field.Sections.Where(s => !s.IsExistRoom).OrderBy(s => s.Index).ToList(); // candidate : 후보
            var rand = Random.Range(0, candidate.Sum(c => c.MakeRoomWeight));
            var pick = 0;
            for (var i = 0; i < candidate.Count; i++)
            {
                if (rand < candidate[i].MakeRoomWeight)
                {
                    pick = i;
                    break;
                }

                rand -= candidate[i].MakeRoomWeight;
            }

            return candidate[pick];
        }

        private static void MakeBranches(Field field, FieldBluePrint bluePrint)
        {
            field.Connections = bluePrint.Connections.Select(c => new Connection { From = c.From, To = c.To }).ToList();
            
            if (bluePrint.AutoGenerateDefaultConnections)
            {
                ExtendConnections(field);
                ComplementAllConnection(field);
                MakeAdditionalBranch(field, bluePrint);
            }
            
            // Check All Section is Connected
            // TODO: Check the parts that require exception handling (ex: infinite loop)
            var isAllSectionIsConnected = IsAllSectionConnectedWithMakeGroup(field, false, out var sectionGroupList);
            if (bluePrint.IsConnectAllCompletely && !isAllSectionIsConnected)
            {
                CompletelyConnectAllSections(field, sectionGroupList);
            }
            if (bluePrint.IsRemoveInvalidConnects)
            {
                RemoveAllInvalidConnections(field);
            }
            
            // MakeBranches내부에서 Joint의 Connected 변수를 변경하여 최종 연결상태를 정의하기 때문에 여기서 호출
            field.Branches = field.Connections.SelectMany(c => Join(field.GetSection(c.From), field.GetSection(c.To), field.Gates, field.ConnectionDetails)).ToList();
            field.Rooms.ForEach(room => room.ExceptJointTilesInWallDictionary());
            
            // NOTE ====================================================================================================
            // 1. 한 Section에서 연결된 Connection을 따라서 방문할 수 있었던 Section들의 정보를 저장 (Visited Sections)
            // 2. 이 방문할 수 있었던 Section들의 정보가 일치하는 Section들은 하나의 그룹으로 명명하고 sectionGroupList에 저장
            // 3. 방문지 정보(길이, 요소를 비교해서)가 다른 Section들은 다른 Group이라는 개념이 되고 sectionGroupList에 새로 추가됨
            // 4. sectionGroupList의 길이가 1이면 하나의 그룹으로써 모든 Section이 연결되어 있는 것으로 판단. 그외는 반대. 
            // =========================================================================================================
            // 1. 
        }
        
        // float배열에서 가장 길이가 짧은 대상을 찾아서 반환 Linq
        private static float FindShortest(float[] distances)
        {
            return distances.Min();
        }

        private static void ExtendConnections(Field field)
        {
            var targetSection = field.Sections[Random.Range(0, field.Sections.Count)];
            while (targetSection != null)
            {
                // targetSection과 인접하면서 Connection이 없는 Section을 찾음
                var isolatedSections = field.Sections.Where(s => s != targetSection && targetSection.AdjoinWith(s) && field.IsIsolatedSection(s)).ToList();
                // 결과가 없으면 loop를 중단
                if (!isolatedSections.Any())
                {
                    break;
                }

                var nextSection = isolatedSections[Random.Range(0, isolatedSections.Count)];

                // targetSection과 nextSection을 연결하는 Connection을 생성
                field.Connections.Add(new Connection { From = targetSection.Index, To = nextSection.Index });

                // nextSection을 타겟으로 변경
                targetSection = nextSection;
            }

            var sections = field.Sections
                                .Where(s => field.IsIsolatedSection(s) && !field.ExistConnectedSectionAround(s))
                                .ToList();

            // Connection이 없는 Section이면서 / 인접한 Section들도 모두 Connection이 없는 경우
            // Section이 고립되지 않도록
            while(sections.Any())
            {
                var isolatedSection = sections[Random.Range(0, sections.Count)];
                var fromSection = field.GetSectionsAdjoinWith(isolatedSection).FirstOrDefault();
                field.Connections.Add(new Connection { From = fromSection.Index, To = isolatedSection.Index });
                
                sections = field.Sections
                                .Where(s => field.IsIsolatedSection(s) && !field.ExistConnectedSectionAround(s))
                                .ToList();
            }
        }

        private static void ComplementAllConnection(Field field)
        {
            var resultSections = field.IsolatedAndExistConnectedSectionAroundSections().ToList();
            while (resultSections.Any())
            {
                var isolatedSection = resultSections[Random.Range(0, resultSections.Count)];

                // 인접한 Section 중에서 길이 이어져 있는 Section
                var fromSection = GetAdjoinedSection(field, isolatedSection);
                field.Connections.Add(new Connection { From = fromSection.Index, To = isolatedSection.Index });

                resultSections.Remove(isolatedSection);
            }
        }

        private static void MakeAdditionalBranch(Field field, FieldBluePrint bluePrint)
        {
            var randomBranchNum = Random.Range(bluePrint.MinRandomBranchNum, bluePrint.MaxRandomBranchNum + 1);
            for (var i = 0; i < randomBranchNum; i++)
            {
                var roomContainingSections = field.Sections.Where(s => s.IsExistRoom).ToList();
                if (!roomContainingSections.Any())
                {
                    break;
                }
                
                var targetSection = roomContainingSections[Random.Range(0, roomContainingSections.Count)];

                var unconnectedSections = field.GetSectionsAdjoinWith(targetSection)
                                               .Where(s => !field.Connected(s, targetSection)).ToList();
                var pairSection = unconnectedSections.Count > 0 ? unconnectedSections[Random.Range(0, unconnectedSections.Count)] : null;
                if (pairSection == null)
                {
                    break;
                }

                field.Connections.Add(new Connection { From = targetSection.Index, To = pairSection.Index });
            }
        }

        private static Section GetAdjoinedSection(Field field, Section target)
        {
            var adjoinedSections = field.GetSectionsAdjoinWithConnected(target).ToList();
            return adjoinedSections[Random.Range(0, adjoinedSections.Count)];
        }

        private static IEnumerable<Vector2Int> Join(Section from, Section to, List<Gate> Gates, List<ConnectionDetail> connectionDetails)
        {
            // from 과 to 가 서로 인접한 방향
            var relation = from.AdjoiningWithDirection(to);
            if (relation == Constants.Direction.Error)
            {
                return new Vector2Int[] { };
            }

            var inverse = Constants.Direction.Inverse(relation);

            // From 에서 relation 방향에 연결된 출구가 있거나
            // To 에서 inverse 방향에 연결된 출구가 있다면
            if (!from.ExistUnconnectedJoints(relation) || !to.ExistUnconnectedJoints(inverse))
            {
                return new Vector2Int[] { };
            }

            var start1 = PickJoint(from, relation, Gates, out Gate fromGate);
            var start2 = PickJoint(to, inverse, Gates, out Gate toGate);
            var end1 = from.GetEdge(to, start1);
            var end2 = to.GetEdge(from, start2);
            // ex) Left : from.xMin, start1.y

            // ======================================= Create Connection Details =======================================
            var start1LineTo = start1.LineTo(end1);
            var start2LineTo = start2.LineTo(end2);
            var end1LineTo = end1.LineTo(end2);

            var fromToWayPoints = new List<Vector2Int>();
            fromToWayPoints.AddRange(start1LineTo);
            fromToWayPoints.AddRange(end1LineTo);
            fromToWayPoints.AddRange(start2LineTo.Reverse());

            var result = fromToWayPoints.ToArray();
            
            // remove duplication in way list
            for (int i = fromToWayPoints.Count - 1; i >= 0; i--) {
                var wayPoint = fromToWayPoints[i];
                if (fromToWayPoints.Count(pos => pos == wayPoint) > 1) {
                    fromToWayPoints.RemoveAt(i);
                }
            }
            
            // print from to way points
            // string log = $"start from: {from.Index.ToString()}, to: {to.Index.ToString()}" + '\n';
            // for (var i = 0; i < fromToWayPoints.Count; i++) {
            //     var way = fromToWayPoints[i];
            //     log += way + " ";
            // }
            // CatLog.Log(log);
            
            // make new connection details
            var connectionDetail = new ConnectionDetail {
                From = from,
                To = to,
                FromGate = fromGate,
                ToGate = toGate,
                Ways = fromToWayPoints, 
            };
            connectionDetails.Add(connectionDetail);
            // =========================================================================================================
            // return new[]
            // {
            //     start1.LineTo(end1),
            //     start2.LineTo(end2),
            //     end1.LineTo(end2)
            // }.SelectMany(p => p);
            return result;
        }

        private static void MakeConnectionDetails(Field field) {

            var connectionDetails = field.ConnectionDetails;
            for (int i = 0; i < connectionDetails.Count; i++) {
                var detail = connectionDetails[i];
                var fromSection = detail.From;
                var toSection = detail.To;

                var from2To = new SectionLinked() {
                    FromGate = detail.FromGate,
                    ToGate = detail.ToGate,
                    FromSection = detail.From,
                    ToSection = detail.To,
                    Ways = detail.Ways,
                };
                fromSection.SectionLinkeds.Add(from2To);

                var reversedWays = new List<Vector2Int>(detail.Ways);
                reversedWays.Reverse();
                var to2From = new SectionLinked() {
                    FromGate = detail.ToGate,
                    ToGate = detail.FromGate,
                    FromSection = detail.To,
                    ToSection = detail.From,
                    Ways = reversedWays
                };
                toSection.SectionLinkeds.Add(to2From);
            }
        }

        private static void PrintConnectionDetails(Field field) {
            // print connection details
            for (int i = 0; i < field.ConnectionDetails.Count; i++) {
                var connectionDetial = field.ConnectionDetails[i];
                string log = $"From: {connectionDetial.From.Index.ToString()}, To: {connectionDetial.To.Index.ToString()}" + '\n';
                for (int j = 0; j < connectionDetial.Ways.Count; j++) {
                    var way = connectionDetial.Ways[j];
                    log += way + " ";
                }
                CatLog.Log(log);
            }
        }
        
        private static Vector2Int PickJoint(Section section, int direction, List<Gate> Gates, out Gate pickedGate)
        {
            pickedGate = null;
            
            // 현재 section에 Room 이 존재하지 않는다면
            if (!section.IsExistRoom)
            {
                // 길의 중계지점이 될 Position을 반환
                return section.Relay.Point;
            }

            var joints = section.ExistUnconnectedJoints(direction) ? section.GetUnConnectedJoints(direction).ToList() : section.GetConnectedJoints(direction).ToList();
            var pick = joints[Random.Range(0, joints.Count)];

            var newGate = new Gate { Direction = direction, Position = pick.Position, Section = section }; 
            Gates.Add(newGate);
            pickedGate = newGate;

            pick.Connected = true;
            return pick.Position;
        }

        private static void MakeRoomsType(Field field, FieldBluePrint bluePrint)
        {
            // 방이 존재하는 Section 가져오기
            var sectionIndexes = field.Sections.Where(section => section.IsExistRoom).Select(section => section.Index).ToList();
            var result = new Dictionary<RoomType, List<Section>>(); // RoomType, Section
            
            // BluePrint에 따라 RewardType과 ShopType의 방 개수를 랜덤하게 설정
            int rewardRoomCount = Random.Range(bluePrint.MinRewardRoomCount, bluePrint.MaxRewardRoomCount + 1);
            int shopRoomCount   = Random.Range(bluePrint.MinShopRoomCount, bluePrint.MaxShopRoomCount + 1);

            // RoomType 생성
            GenerateRoomType(bluePrint.MinEntrance, RoomType.PlayerSpawnRoom);
            GenerateRoomType(bluePrint.MinExtrance, RoomType.ExitRoom);
            GenerateRoomType(rewardRoomCount, RoomType.RewardRoom);
            GenerateRoomType(shopRoomCount, RoomType.ShopRoom);
            
            // BattleType의 Room 개수 계산 ( -1: 남은 Room을 모두 BattleType으로 )
            int battleRoomCount = (bluePrint.MaxBattleRoomCount == -1)
                ? sectionIndexes.Count
                : Random.Range(bluePrint.MinBattleRoomCount, bluePrint.MaxBattleRoomCount + 1);
            // Battle Room 생성
            GenerateRoomType(battleRoomCount, RoomType.MonsterSpawnRoom);
            GenerateEmptyFromAllLeftRooms(); // 남은 Room을 모두 EmptyType으로
            
            // RoomType에 따라 RoomData를 세팅
            foreach (var pair in result) {
                foreach (var section in pair.Value) {
                    section.Room.SetRoomData(pair.Key, section.Index, bluePrint.GetRoomEntityByWeight(pair.Key));
                }
            }

            field.RoomDictionary = result;
            return;
            
            void GenerateRoomType(int count, RoomType roomType) {
                while (count > 0) {
                    if (!result.ContainsKey(roomType)) {
                        result.Add(roomType, new List<Section>());
                    }
                    
                    // 남은 Section이 존재하지 않음
                    if (sectionIndexes.Count <= 0) {
                        // 우선순위가 높은 Room이 생성되지 못함
                        if (roomType != RoomType.MonsterSpawnRoom) {
                            CatLog.WLog("Generate RoomType Warning !");
                        }
                        return;
                    }

                    // 무작위 Section Index를 도출하고 Dictionary에 저장
                    int randomSectionIndex = sectionIndexes[Random.Range(0, sectionIndexes.Count)];
                    sectionIndexes.RemoveAll(index => index.Equals(randomSectionIndex));
                    
                    var findSection = field.Sections.Find(section => section.Index == randomSectionIndex);
                    if (findSection == null || findSection.Room.RoomData != null) {
                        CatLog.ELog("Section Find Failed Or Override Room Type Error.");
                        return;
                    }
                    
                    result[roomType].Add(findSection);
                    count--;
                }
            }

            void GenerateEmptyFromAllLeftRooms() {
                result.Add(RoomType.EmptyRoom, new List<Section>());
                for (int i = sectionIndexes.Count - 1; i >= 0; i--) {
                    int sectionIndexNum = sectionIndexes[i];
                    var findSection = field.Sections.Find(section => section.Index == sectionIndexNum);
                    if (findSection == null || findSection.Room.RoomData != null) {
                        CatLog.ELog("Section Find Failed Or Override Room Type Error.");
                        continue;
                    }
                    
                    result[RoomType.EmptyRoom].Add(findSection);
                    sectionIndexes.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Check whether all sections are perfectly connected and can be visited. If it is not completely connected, the Section Group List is returned.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="isLogging"></param>
        /// <param name="sectionGroupList"></param>
        /// <returns></returns>
        private static bool IsAllSectionConnectedWithMakeGroup(Field field, bool isLogging, out List<Section[]> sectionGroupList)
        {
            var connections = field.Connections;
            var sections = field.Sections;
            
            StringBuilder sb = isLogging ? new StringBuilder() : null;
            sb?.Clear();
            sb?.Append("Branch Process Report \n");

            List<Connection> visitedConnections = new();
            List<Section> visitedSections = new();
            Dictionary<Section, Section[]> visitedDictionary = new();
            var roomExistSections = sections.Where(s => s.IsExistRoom).ToArray();
            sb?.Append($"Room Exist SectionCount(Check Count): {roomExistSections.Length.ToString()} \n");
            for (int i = 0; i < roomExistSections.Length; i++)
            {
                visitedConnections.Clear();
                visitedSections.Clear();

                var roomExistSection = roomExistSections[i];
                IsConnectedToTargetSection(roomExistSection);
                /*visitedSections.Remove(roomExistSection); // Ignore Start Section*/

                // ** 무한루프 체크 **
                visitedDictionary.Add(roomExistSection, visitedSections.ToArray());
                sb?.Append($"Start Section {roomExistSection.Index.ToString()}, Visited Section Count: {visitedSections.Count.ToString()} \n");
            }

            // Make Section Group List
            sectionGroupList = new();
            if (visitedDictionary.Any())
            {
                sectionGroupList.Add(visitedDictionary.First().Value);
                foreach (var value in visitedDictionary.Select(pair => pair.Value))
                {
                    for (int i = 0; i < sectionGroupList.Count; i++)
                    {
                        // Matching Group
                        bool isMissMatched = value.Length != sectionGroupList[i].Length;
                        if (!isMissMatched)
                        {
                            for (int j = 0; j < sectionGroupList[i].Length; j++)
                            {
                                var targetSection = sectionGroupList[i][j];
                                var find = value.SingleOrDefault(s => s == targetSection);
                                if (find == null)
                                {
                                    isMissMatched = true;
                                    break;
                                }
                            }
                        }

                        // Find Matched Group -> Break Loop
                        if (!isMissMatched)
                        {
                            break;
                        }

                        // If you browse the Group List all the way through but cannot find it, add it to the Group List
                        if (i != sectionGroupList.Count - 1)
                            continue;
                        sectionGroupList.Add(value);
                    }
                }
            }

            // Print Group Section Report 
            bool result = sectionGroupList.Count == 1;
            if (isLogging)
            {
                if (result)
                {
                    sb.Append("Is Connected All Sections \n");
                    sb.Append("Process Ended. \n");
                    CatLog.Log(sb.ToString());
                }
                else
                {
                    sb.Append("Is Not Connected All Sections ! \n");
                    sb.Append($"Section is Splited By - {sectionGroupList.Count.ToString()} \n");
                    for (var i = 0; i < sectionGroupList.Count; i++)
                    {
                        var sectionGroup = sectionGroupList[i];
                        sb.Append($"=== Group: {sectionGroup.Length.ToString()} === \n");
                        sb.Append("=== Connected Information === \n");
                        for (int j = 0; j < sectionGroup.Length; j++)
                        {
                            var section = sectionGroup[j];
                            sb.Append($"{section.Index.ToString()}, ");
                        }

                        sb.Append('\n');
                    }

                    sb.Append("Process Ended. \n");
                    CatLog.WLog(sb.ToString());
                }
            }

            return result;

            // Recursive Method 
            void IsConnectedToTargetSection(Section visitSection)
            {
                visitedSections.Add(visitSection);
                var visitSectionConnections = connections
                                              .Where(c => c.To == visitSection.Index ||
                                                          c.From == visitSection.Index)
                                              .ToList();
                if (!visitSectionConnections.Any())
                    return;

                foreach (var connection in visitSectionConnections)
                {
                    if (visitedConnections.Contains(connection))
                        continue;
                    visitedConnections.Add(connection);

                    int nextSectionIndex =
                        visitSection.Index == connection.To ? connection.From : connection.To;
                    var nextSection = field.GetSection(nextSectionIndex);
                    if (visitedSections.Contains(nextSection))
                        continue;
                    IsConnectedToTargetSection(nextSection);
                }
            }
        }

        private static void CompletelyConnectAllSections(Field field, List<Section[]> sectionGroupList)
        {
            // TODO: Matched Nearest Section Groups Or Check Reorderable Groups When Loop's End 
            var sb2 = new StringBuilder();
            sb2.Append("Connecting Section Group Process Start ! \n");
            var groupsCount = sectionGroupList.Count;
            while (groupsCount != 1)
            {
                var group1Sections = sectionGroupList[0];
                var group2Sections = sectionGroupList[1];
                bool isConnectSuccessed = false;

                // Find Nearest Section
                for (int i = 0; i < group1Sections.Length; i++)
                {
                    var group1Section = group1Sections[i];
                    var adJoinSections = field.GetSectionsAdjoinWith(group1Section).ToArray();
                    var adjoinGroup2Sections = adJoinSections.Where(s => group2Sections.Contains(s)).ToArray();
                    for (int j = 0; j < adjoinGroup2Sections.Length; j++)
                    {
                        var group2Section = adjoinGroup2Sections[j];
                        if (!field.IsConnectable(group1Section, group2Section))
                        {
                            continue;
                        }

                        // Finded Connectable Nearest Group2 Section
                        var connection = new Connection() { From = group1Section.Index, To = group2Section.Index };
                        field.Connections.Add(connection);
                        isConnectSuccessed = true;
                        sb2.Append($"add new Nearest Connection. From: {connection.From.ToString()}, To: {connection.To.ToString()} \n");
                        break;
                    }

                    // Check Connect Successed
                    if (isConnectSuccessed)
                    {
                        break;
                    }
                }

                // Is Failed To Nearest Connection -> Shortest Connectable Recursive Search
                if (!isConnectSuccessed)
                {
                    var s2sDistanceDtoList = new List<SectionToSectionDistanceDTO>();
                    for (int i = 0; i < group1Sections.Length; i++)
                    {
                        for (int j = 0; j < group2Sections.Length; j++)
                        {
                            var group1Section = group1Sections[i];
                            var group2Section = group2Sections[j];
                            var distance = field.GetDistanceBySection(group1Section, group2Section);
                            s2sDistanceDtoList.Add(new SectionToSectionDistanceDTO()
                                                       { s1 = group1Section, s2 = group2Section, distance = distance });
                        }
                    }

                    // s2sDistanceDtoList를 오름차 순 distance로 정렬
                    var orderByDistance = s2sDistanceDtoList.OrderBy(d => d.distance).ToArray();
                    var virtualVisitedSections = new List<Section>();
                    for (int i = 0; i < orderByDistance.Length; i++)
                    {
                        virtualVisitedSections.Clear();
                        isConnectSuccessed =
                            IsConnetableToTargetSectionRecursive(virtualVisitedSections, orderByDistance[i].s1,
                                                                 orderByDistance[i].s2);
                        if (isConnectSuccessed)
                        {
                            break;
                        }
                    }

                    if (!isConnectSuccessed)
                    {
                        CatLog.ELog("Group Section Connection Failed !");
                        break;
                    }

                    // Add Connection From Virtual Visited Connection
                    for (int i = 0; i < virtualVisitedSections.Count - 1; i++)
                    {
                        var currentSection = virtualVisitedSections[i];
                        var nextSection = virtualVisitedSections[i + 1];
                        var newConnection = new Connection() { From = currentSection.Index, To = nextSection.Index };
                        field.Connections.Add(newConnection);
                        sb2.Append($"add new Nearest Connection. From: {newConnection.From.ToString()}, To: {newConnection.To.ToString()} \n");
                    }
                }

                // Find Way To Recursive
                // Update Section Group List
                var isAllConnected = IsAllSectionConnectedWithMakeGroup(field, false, out sectionGroupList);
                if (isAllConnected)
                    break;
                groupsCount = sectionGroupList.Count;
            }

            sb2.Append("End Make Connection Process.");
            CatLog.WLog(sb2.ToString());
            return;

            bool IsConnetableToTargetSectionRecursive(List<Section> originList, Section startSection, Section targetSection)
            {
                var copyList = originList.ToList();
                copyList.Add(startSection);
                var adJoinSections = field.GetSectionsAdjoinWith(startSection)
                                          .Where(s => !copyList.Contains(s) && field.IsConnectable(s, targetSection))
                                          .OrderBy(s => field.GetDistanceBySection(s, targetSection))
                                          .ToArray();
                if (adJoinSections.Contains(targetSection))
                {
                    copyList.Add(targetSection);
                    originList.Clear();
                    originList.AddRange(copyList);
                    return true;
                }

                for (int i = 0; i < adJoinSections.Length; i++)
                {
                    var newStartSection = adJoinSections[i];
                    var result = IsConnetableToTargetSectionRecursive(copyList, newStartSection, targetSection);
                    if (result)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private static void RemoveAllInvalidConnections(Field field)
        {
            // variables
            var connections = field.Connections;
            
            // Remove Invalid Connections [ Relay <-> Relay Connection ] 
            var relay2RelayConnections = connections.Where(c => !GetSection(c.To).IsExistRoom && !GetSection(c.From).IsExistRoom).ToList();
            var removeTargetConnection = FindMatchedConnection();
            while (removeTargetConnection != null)
            {
                relay2RelayConnections.Remove(removeTargetConnection);
                connections.Remove(removeTargetConnection);
                CatLog.Log("Remove Relay <-> Relay Connection: From: " + removeTargetConnection.From.ToString() +
                           " -> To: " + removeTargetConnection.To.ToString());
                removeTargetConnection = FindMatchedConnection();
            }

            // Remove Invalid Connections [ Room <-> Relay Connection  ] 
            // Get Room <-> Relay Connections
            RemoveInvalidConnections();
            return;
            
            // Inner Methods
            void RemoveInvalidConnections()
            {
                var roomToRelayConnections = connections.Where(c => (!GetSection(c.To).IsExistRoom && GetSection(c.From).IsExistRoom) || (GetSection(c.To).IsExistRoom && !GetSection(c.From).IsExistRoom)).ToArray();
                for (int i = 0; i < roomToRelayConnections.Length; i++)
                {
                    // To Section Exist Room
                    if (GetSection(roomToRelayConnections[i].To).IsExistRoom)
                    {
                        if (IsExistAnotherConnection(roomToRelayConnections[i].From, roomToRelayConnections[i]) == false)
                        {
                            connections.Remove(roomToRelayConnections[i]);
                            CatLog.Log("Remove RoomToRelay Connections : From: " + roomToRelayConnections[i].From.ToString() + " -> To: " + roomToRelayConnections[i].To.ToString());
                        }
                    }
                    // From Section Exist Room
                    else if (GetSection(roomToRelayConnections[i].From).IsExistRoom)
                    {
                        if (IsExistAnotherConnection(roomToRelayConnections[i].To, roomToRelayConnections[i]) == false)
                        {
                            connections.Remove(roomToRelayConnections[i]);
                            CatLog.Log("Remove RoomToRelay Connections : From: " + roomToRelayConnections[i].From.ToString() + " -> To: " + roomToRelayConnections[i].To.ToString());
                        }
                    }
                }
            }

            bool IsExistAnotherConnection(int sectionIndex, Connection self)
            {
                return field.Connections.Any(c => (c.To == sectionIndex || c.From == sectionIndex) && !(c.To == self.To && c.From == self.From));
            }

            Connection FindMatchedConnection()
            {
                return relay2RelayConnections.FirstOrDefault(c => !(IsExistAnotherConnection(c.From, c) && IsExistAnotherConnection(c.To, c)));
            }

            Section GetSection(int index)
            {
                return field.GetSection(index);
            }
        }
        
        private class SectionToSectionDistanceDTO {
            public Section s1;
            public Section s2;
            public float distance;
        }
    }
}
