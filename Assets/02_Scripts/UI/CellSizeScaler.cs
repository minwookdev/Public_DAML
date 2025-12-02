using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat {
    [RequireComponent(typeof(GridLayoutGroup))]
    public class CellSizeScaler : MonoBehaviour {
        private RectTransform rectTr = null;
        private GridLayoutGroup gridLayoutGroup = null;
        private Vector2 lastRectSize = Vector2.zero;

        private void Start() {
            rectTr = GetComponent<RectTransform>();
            gridLayoutGroup = GetComponent<GridLayoutGroup>();
        }

        private void Update() => ResizeCells();

        private void ResizeCells() {
            var currentConstraint = gridLayoutGroup.constraint;
            if (currentConstraint == GridLayoutGroup.Constraint.Flexible) {
                return;
            }
            
            Vector2 sizeDelta = new Vector2(rectTr.rect.width, rectTr.rect.height);
            if (sizeDelta == lastRectSize) {
                return;
            }

            // update size delta
            lastRectSize = sizeDelta;
            // CatLog.Log("LastSizeDelta: " + lastRectSize);

            var constraintCount = gridLayoutGroup.constraintCount;
            if (constraintCount <= 0) {
                return;
            }
            
            switch (currentConstraint) {
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    var width = lastRectSize.x;
                    var spacingX = gridLayoutGroup.spacing.x;
                    var leftPadding = gridLayoutGroup.padding.left;
                    var rightPadding = gridLayoutGroup.padding.right;
                    var totalCellSize = width - leftPadding - rightPadding - (constraintCount - 1) * spacingX;
                    var cellWidth = totalCellSize / constraintCount;
                    var fixedColumnCountCellSize = new Vector2(cellWidth, cellWidth);
                    gridLayoutGroup.cellSize = fixedColumnCountCellSize;
                    break;
                case GridLayoutGroup.Constraint.FixedRowCount:
                    var height = lastRectSize.y;
                    var spacingY = gridLayoutGroup.spacing.y;
                    var topPadding = gridLayoutGroup.padding.top;
                    var bottomPadding = gridLayoutGroup.padding.bottom;
                    var totalCellSizeY = height - topPadding - bottomPadding - (constraintCount - 1) * spacingY;
                    var cellHeight = totalCellSizeY / constraintCount;
                    var fixedRowCountCellSize = new Vector2(cellHeight, cellHeight);
                    gridLayoutGroup.cellSize = fixedRowCountCellSize;
                    break;
                case GridLayoutGroup.Constraint.Flexible:
                default: throw new NotImplementedException();
            }
        }
    }
}