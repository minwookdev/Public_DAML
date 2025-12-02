using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat;
using CoffeeCat.FrameWork;

namespace RandomDungeonWithBluePrint {
    public class GateObject : MonoBehaviour {
        // Managed Field
        [SerializeField] private GameObject lockedObject = null;
        [ShowInInspector, ReadOnly] private int thisDirection = 0;
        
        // Fields
        private Transform tr = null;
        private BoxCollider2D boxCollider = null;
        private Room refRoom = null;
        private List<Vector2> ways = new();
        private const float TILE_PER_MOVE_SECONDS = 0.05f;
        private const float LAST_TILE_MOVE_SECONDS = 0.5f;

        // Fast Move Variables
        private bool isCompleteConnectedWays = false;
        private bool isSectionLinkedSelected = false;
        private SectionLinked thisSectionLinked = null;
        private SectionLinked selectedSectionLinked = null;
        private List<SectionLinked> selectableSectionLinkeds = null;
        
        public void Initialize(Gate gate, Room room, SectionLinked sectionLinked) {
            refRoom = room;
            thisDirection = gate.Direction;
            thisSectionLinked = sectionLinked;
            tr = GetComponent<Transform>();
            tr.position = GetPosition(gate.Direction, gate.Position);
            tr.rotation = GetRotation(gate.Direction);
            boxCollider = GetComponent<BoxCollider2D>();
        }
        
        private async UniTaskVoid CreateWaysAndPlayerMoveAsync() {
            InputManager.Inst.DisablePublishInputEvent();
            ClearWaysAndSelectData();
            
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            var startSectionLinked = thisSectionLinked;
            
            while (!isCompleteConnectedWays) {
                UpdateWaysToRecursive(startSectionLinked);
            
                // Move Player to Way Positions Routine
                int loopCounts = isCompleteConnectedWays ? ways.Count - 1 : ways.Count;
                var isCompletedTransformMoved = await player.LerpMoveTransformAsync(ways, loopCounts, TILE_PER_MOVE_SECONDS);
                if (!isCompletedTransformMoved) {
                    InputManager.Inst.RestorePublishInputEvent();
                    return;
                }
                
                // escape loop if completely connected ways
                if (isCompleteConnectedWays) {
                    continue;
                }
                
                // init way direction selecte buttons and wait until select
                isSectionLinkedSelected = false;
                var inputCanvas = InputManager.Inst.GetInputCanvas();
                for (int i = 0; i < selectableSectionLinkeds.Count; i++) {
                    var sectionLinked = selectableSectionLinkeds[i];
                    var buttonWorldPos = sectionLinked.GetDirectionButtonWorldPosition();
                    inputCanvas.InitWayDirectionButton(i, OnButtonSelectSectionLinked, buttonWorldPos);
                }
                
                var isCancelledSelectWait = await UniTask.WaitUntil(() => isSectionLinkedSelected, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
                if (isCancelledSelectWait) {
                    InputManager.Inst.RestorePublishInputEvent();
                    return;
                }
                
                startSectionLinked = selectedSectionLinked;
                selectedSectionLinked = null;
                isSectionLinkedSelected = false;
            }
            
            // player move to last way point by rigidbody move position
            var lastWayPoint = ways[^1];
            await player.LerpMoveRigidBodyAsync(player.Tr.position, lastWayPoint, LAST_TILE_MOVE_SECONDS);
            InputManager.Inst.RestorePublishInputEvent();
        }

        private void ClearWaysAndSelectData() {
            isCompleteConnectedWays = false;
            isSectionLinkedSelected = false;
            selectedSectionLinked = null;
            selectableSectionLinkeds = null;
            ways.Clear();
        }

        private void OnButtonSelectSectionLinked(int index) {
            selectedSectionLinked = selectableSectionLinkeds[index];
            isSectionLinkedSelected = true;
        }
        
        private void UpdateWaysToRecursive(SectionLinked startLinked) {
            ways.Clear();
            
            // safety recursive loop
            const int MAX_LOOP_COUNT = 10;
            int currentLoopCount = 0;
            var currentLinked = startLinked;
            var toSection = currentLinked.ToSection;
            
            // recursiuve loop for connecting ways to destination section
            while (true) {
                AddWays(currentLinked.Ways);
                    
                // finded destination
                if (toSection.IsExistRoom) {
                    isCompleteConnectedWays = true;
                    break;
                }
                
                // finding to sections other linked
                var nextSectionLinkeds = toSection.GetOtherLinkedSections(currentLinked.FromSection);
                var count = nextSectionLinkeds.Count;

                // There is currently no path connected to the from section
                if (count <= 0) {
                    OnNotConnectedAnyWayToSection();
                    isCompleteConnectedWays = true;
                    return;
                }

                // find multiple direction section
                if (count > 1) {
                    selectableSectionLinkeds = toSection.SectionLinkeds;
                    isCompleteConnectedWays = false;
                    break;
                }

                // set next to section and linked
                currentLinked = nextSectionLinkeds[0];
                toSection = currentLinked.ToSection;

                currentLoopCount++;
                if (currentLoopCount >= MAX_LOOP_COUNT) {
                    throw new Exception("Arrived Max Loop Count !");
                }
            }
            
            FixWayPositions();
            ways = RemoveDuplicateWays(ways);

            if (!isCompleteConnectedWays) {
                return;
            }

            // add last way position for player move inside to room
            var targetGate = currentLinked.ToGate;
            var lastWayPosition = GetLastPositionOnCompleteConnectedWays(targetGate);
            ways.Add(lastWayPosition);
        }
        
        private void AddWays(List<Vector2Int> addWays) {
            for (int i = 0; i < addWays.Count; i++) {
                ways.Add(addWays[i]);
            }
        }
        
        private void OnNotConnectedAnyWayToSection() {
            var waysCount = ways.Count;
            for (int i = waysCount - 2; i >= 0; i--) {
                var additionalWay = ways[i];
                ways.Add(additionalWay);
            }

            FixWayPositions();
            ways.Add(GetSelfCenterPos());
        }
        
        private void FixWayPositions() {
            for (int i = 0; i < ways.Count; i++) {
                var way = ways[i];
                var fixedPosition = new Vector2(way.x + Constants.TileRadius, way.y + Constants.TileRadius);
                ways[i] = fixedPosition;
            }
        }
        
        private List<Vector2> RemoveDuplicateWays(List<Vector2> list) {
            List<Vector2> result = new();
            for (int i = 0; i < list.Count; i++) {
                if (!result.Contains(list[i])) {
                    result.Add(list[i]);
                }
            }
            return result;
        }

        private Vector2 GetPosition(int constDirection, Vector2Int worldPos) {
            return constDirection switch {
                Constants.Direction.Up => new Vector2(worldPos.x + Constants.TileRadius, worldPos.y + Constants.TileDiameter * 2f),
                Constants.Direction.Down => new Vector2(worldPos.x + Constants.TileRadius, worldPos.y - Constants.TileDiameter),
                Constants.Direction.Left => new Vector2(worldPos.x + Constants.TileDiameter * 2f, worldPos.y + Constants.TileRadius),
                Constants.Direction.Right => new Vector2(worldPos.x - Constants.TileDiameter, worldPos.y + Constants.TileRadius),
                _ => throw new NotImplementedException($"Not Implemented This Constant Direction : {constDirection}")
            };
        }

        private Quaternion GetRotation(int constDirection) {
            return constDirection switch {
                Constants.Direction.Up => Quaternion.Euler(0f, 0f, 180f),
                Constants.Direction.Down => Quaternion.identity,
                Constants.Direction.Right => Quaternion.Euler(0f, 0f, -90f),
                Constants.Direction.Left => Quaternion.Euler(0f, 0f, 90f),
                _ => throw new NotImplementedException($"Not Implemented This Constant Direction : {constDirection}")
            };
        }

        private Vector2 GetSelfCenterPos() {
            return thisDirection switch {
                Constants.Direction.Up    => new Vector2(tr.position.x, tr.position.y + Constants.TileRadius),
                Constants.Direction.Down  => new Vector2(tr.position.x, tr.position.y - Constants.TileRadius),
                Constants.Direction.Left  => new Vector2(tr.position.x + Constants.TileRadius, tr.position.y),
                Constants.Direction.Right => new Vector2(tr.position.x - Constants.TileRadius, tr.position.y),
                _                         => throw new NotImplementedException($"Not Implemented This Constant Direction : {thisDirection}")
            };
        }

        private Vector2 GetLastPositionOnCompleteConnectedWays(Gate targetGate) {
            var position = targetGate.Position;
            const float valueDownRight = Constants.TileDiameter + Constants.TileRadius;
            const float valueUpLeft = Constants.TileDiameter * 2 + Constants.TileRadius;
            return targetGate.Direction switch {
                Constants.Direction.Up    => new Vector2(position.x + Constants.TileRadius, position.y + valueUpLeft),
                Constants.Direction.Left  => new Vector2(position.x + valueUpLeft, position.y + Constants.TileRadius),
                Constants.Direction.Down  => new Vector2(position.x + Constants.TileRadius, position.y - valueDownRight),
                Constants.Direction.Right => new Vector2(position.x - valueDownRight, position.y + Constants.TileRadius),
                _                         => throw new NotImplementedException($"Not Implemented This Constant Direction : {targetGate.Direction}")
            };
        }
        
        private void OnTriggerEnter2D(Collider2D collision) {
            // Check Only Players Collider
            if (!collision.gameObject.layer.Equals(LayerMask.NameToLayer("Player"))) { }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            // Check Only Players Collider
            if (!collision.gameObject.layer.Equals(LayerMask.NameToLayer("Player"))) {
                return;
            }

            // Player Position
            Vector2 playerPosition = collision.transform.position;
            // Entered Player in Room
            if (refRoom.IsInsideRoom(playerPosition)) {
                if (refRoom.RoomData.IsPlayerInside) {
                    // 플레이어가 이미 방에 있던 상태
                    return;
                }
                refRoom.RoomData.EnteredPlayer();
                DungeonSceneBase.Inst.SetPlayersRoom(refRoom);
                
                CatLog.Log("Entered Player in Room.");
            }
            // Leaves Player From Room
            else {
                if (!refRoom.RoomData.IsPlayerInside) {
                    // 플레이어가 이미 방에서 탈출한 상태 Or 아직 방에 진입하지 않은 상태
                    return;
                }
                refRoom.RoomData.LeavesPlayer();
                DungeonSceneBase.Inst.ClearPlayersRoom(refRoom);
                
                CatLog.Log("Exit Player.");
                
                // Create and Move Player to Ways
                CreateWaysAndPlayerMoveAsync().Forget();
            }
        }
        
        public void Lock(bool isLock) {
            lockedObject.gameObject.SetActive(isLock);
            boxCollider.isTrigger = !isLock;
        }
    }
}