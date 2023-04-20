using Devops.Performance;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devops.Core
{
    public class MoveUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public RectTransform MovePanel;
        private Vector2 mSrcPos = Vector2.zero;
        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform selfRect;
        private Vector3 offset = Vector3.zero;

        void Start()
        {
            canvas = transform.GetComponentInParent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();
            selfRect = gameObject.GetComponent<RectTransform>();
            GetOffset();
        }

        void GetOffset()
        {
            RectTransform current = transform.GetComponent<RectTransform>();
            while (current != null && current != MovePanel)
            {
                offset += current.anchoredPosition3D;
                if (current.parent != null)
                {
                    current = current.parent.GetComponent<RectTransform>();
                }
                else
                {
                    current = null;
                }
            }
            //offset = offset / 2;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Drag(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Drag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Drag(eventData);
        }

        void Drag(PointerEventData eventData)
        {
            Vector3 globalVariableMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out globalVariableMousePos))
            {
                Vector3 pos = Camera.main.WorldToScreenPoint(globalVariableMousePos);
                MovePanel.position = globalVariableMousePos - offset;

                Vector2 vP = Vector2.zero;
                vP.x = Mathf.Round(MovePanel.position.x / Screen.width);
                vP.y = Mathf.Round(MovePanel.position.y / Screen.height);
                MovePanel.ResetAnchor(vP);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Drag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Drag(eventData);
        }
    }
}