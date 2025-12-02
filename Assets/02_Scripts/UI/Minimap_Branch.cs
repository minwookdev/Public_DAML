using System.Collections;
using System.Collections.Generic;
using RandomDungeonWithBluePrint;
using UnityEngine;

namespace CoffeeCat
{
    public class Minimap_Branch : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTr = null;
        [SerializeField] private LineRenderer lineRenderer = null;
        private int fromIndex = 0;
        private int toIndex = 0;
        private bool enterdFrom = false;
        private bool enterdTo = false;
        
        public void SetBranch(Section fromSection, Section toSection, float ratio)
        {
            fromIndex = fromSection.Index;
            toIndex = toSection.Index;
            
            rectTr.localScale = Vector3.one;
            rectTr.anchoredPosition = Vector2.zero;
            
            lineRenderer.SetPosition(0, fromSection.Rect.center * ratio);
            lineRenderer.SetPosition(1, toSection.Rect.center * ratio);
            
            IsNotExistRoom(fromSection, toSection);
        }

        public void ClearBranch()
        {
            fromIndex = 0;
            toIndex = 0;
            enterdFrom = false;
            enterdTo = false;
        }

        private void IsNotExistRoom(Section fromSection, Section toSection)
        {
            if (!fromSection.IsExistRoom)enterdFrom = true;
            
            if (!toSection.IsExistRoom)
                enterdTo = true;
        }

        public void EnterdRoom(int roomIndex)
        {
            if (roomIndex == fromIndex)
                enterdFrom = true;
            
            else if (roomIndex == toIndex)
                enterdTo = true;
        }
        
        public void CheckConnectSection()
        {
            if (enterdFrom && enterdTo)
            {
                gameObject.SetActive(true);
            }
        }
    }
}