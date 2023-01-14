using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.UI
{
    public class FlexibleGridLayout : LayoutGroup
    {
        private enum GridFitType {
            Uniform,
            Width,
            Height,
            FixedRows,
            FixedColumns
        }

        private enum CellFitType {
            Fill,
            Contain,
            FitX,
            FitY
        }

        [SerializeField] private GridFitType gridFitType;
        [SerializeField, EnableIf("gridFitType", GridFitType.FixedRows), MinValue(1)] private int rows;
        [SerializeField, EnableIf("gridFitType", GridFitType.FixedColumns), MinValue(1)] private int columns;
        [SerializeField] private CellFitType cellFitType;
        [SerializeField] private Vector2 cellSpacing;
        [SerializeField, ShowIf("cellFitType", CellFitType.FitY)] private float fixedCellWidth;
        [SerializeField, ShowIf("cellFitType", CellFitType.FitX)] private float fixedCellHeight;
        [ShowInInspector, ReadOnly] private Vector2 cellSize;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            if(gridFitType == GridFitType.Uniform || gridFitType == GridFitType.Width || gridFitType == GridFitType.Height) {
                float sqrRt = Mathf.Sqrt(transform.childCount);
                rows = Mathf.CeilToInt(sqrRt);
                columns = Mathf.CeilToInt(sqrRt);
            }

            if(gridFitType == GridFitType.Width || gridFitType == GridFitType.FixedColumns) {
                rows = Mathf.CeilToInt(transform.childCount / (float) columns);
            }

            if(gridFitType == GridFitType.Height || gridFitType == GridFitType.FixedRows) {
                columns = Mathf.CeilToInt(transform.childCount / (float) rows);
            }

            float parentWidth = rectTransform.rect.width;
            float parentHeight = rectTransform.rect.height;

            float cellWidth = (parentWidth / (float) columns) - ((cellSpacing.x / (float) columns) * (columns - 1)) - (padding.left / (float) columns) - (padding.right / (float) columns);
            float cellHeight = (parentHeight / (float) rows) - ((cellSpacing.y / (float) rows) * (rows - 1)) - (padding.top / (float) rows) - (padding.bottom / (float) rows);

            if(cellFitType == CellFitType.Fill) {
                cellSize.x = cellWidth;
                cellSize.y = cellHeight;
            }

            if(cellFitType == CellFitType.Contain) {
                cellSize.x = cellWidth < cellHeight ? cellWidth : cellHeight;
                cellSize.y = cellSize.x;
            }

            if(cellFitType == CellFitType.FitX) {
                cellSize.x = cellWidth;
                cellSize.y = fixedCellHeight;
            }

            if(cellFitType == CellFitType.FitY) {
                cellSize.x = fixedCellWidth;
                cellSize.y = cellHeight;
            }

            int columnCount = 0;
            int rowCount = 0;

            for(int i = 0; i < rectChildren.Count; i++) {
                rowCount = i / columns;
                columnCount = i % columns;

                var item = rectChildren[i];

                var xPos = (cellSize.x * columnCount) + (cellSpacing.x * columnCount) + padding.left;
                var yPos = (cellSize.y * rowCount) + (cellSpacing.y * rowCount) + padding.top;

                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }
        }

        public override void CalculateLayoutInputVertical()
        {
            
        }

        public override void SetLayoutHorizontal()
        {

        }

        public override void SetLayoutVertical()
        {

        }
    }
}