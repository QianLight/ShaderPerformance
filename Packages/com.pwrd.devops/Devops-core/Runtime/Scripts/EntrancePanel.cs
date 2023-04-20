using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Devops.Core
{
    public class EntrancePanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        static EntrancePanel instance;
        public static EntrancePanel Instance()
        {
            return instance;
        }
        bool mIsDrag = false;
        bool mIsDown = false;
        float mDownTimer = 0.0f;
        RectTransform selfRect;
        public Image logoImage;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            selfRect = transform as RectTransform;
        }
        public void SetEnable(bool enable)
        {
            gameObject.SetActive(enable);
        }
        private void Update()
        {
            if (mIsDown && !mIsDrag)
            {
                mDownTimer += Time.deltaTime;
                if (mDownTimer >= 0.3f)
                {
                    mDownTimer = 0.0f;
                    mIsDrag = true;
                }
            }
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (mIsDrag)
            {
                selfRect.position = eventData.position;
                Vector2 vP = Vector2.zero;
                vP.x = Mathf.Round(selfRect.position.x / Screen.width);
                vP.y = Mathf.Round(selfRect.position.y / Screen.height);
                selfRect.ResetAnchor(vP);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(!mIsDown)
            {
                mIsDown = true;
                OnBeginDrag();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!mIsDrag)
            {
                OnClick();
            }
            mIsDrag = false;
            mDownTimer = 0.0f;
            if (mIsDown)
            {
                mIsDown = false;
                OnEndDrag();
            }
        }

        void OnBeginDrag()
        {
            ResetWelt();
        }

        void OnEndDrag()
        {
            _CheckWelt();
        }

        void ResetWelt()
        {
            logoImage.rectTransform.anchoredPosition = Vector2.zero;
        }

        void _CheckWelt()
        {
            if(selfRect.IsNearSide_horizontal(out float distance, out RectTransformEx.eAnchorType anchorType))
            {
                if(anchorType == RectTransformEx.eAnchorType.Left || anchorType == RectTransformEx.eAnchorType.LeftBottom || anchorType == RectTransformEx.eAnchorType.LeftTop)
                {
                    if(distance < selfRect.sizeDelta.x / 2.0f)
                    {
                        selfRect.anchoredPosition = new Vector2(0.0f, selfRect.anchoredPosition.y);
                    }
                }
                else if(anchorType == RectTransformEx.eAnchorType.Right || anchorType == RectTransformEx.eAnchorType.RightBottom || anchorType == RectTransformEx.eAnchorType.RightTop)
                {
                    if (distance < selfRect.sizeDelta.x / 2.0f)
                    {
                        selfRect.anchoredPosition = new Vector2(0.0f, selfRect.anchoredPosition.y);
                    }
                }
                else
                { }
            }
        }

        void OnClick()
        {
            FunctionsPanel.Instance().SetEnable(true);
            SetEnable(false);
        }
    }
}