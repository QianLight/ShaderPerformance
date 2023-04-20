/*
 * @Author: hexiaonuo
 * @Date: 2021-11-08
 * @Description: longPress script
 * @FilePath: ReactUnity/UnityModule/Scripts/LongPress.cs
 */


using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;


namespace GSDK.RNU
{
    public class IPointerEvent: UIBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
    {
        private bool isPointDown;
        private float startPressTime;

        private float longPressTimeDef = 1f;
        private bool isLongPressStatus;

        private float doubleClickTimeDef = 0.2f;
        private int clickCount = 0;
        private bool isPress;

        
        private Vector2 currentPosition;
        
        public UnityAction<Vector2> mLongPress;
        public UnityAction<Vector2> mPressIn;
        public UnityAction<Vector2> mPressUp;
        public UnityAction<Vector2> mPress;
        public UnityAction<Vector2> mDoubleClick;
        

        private void Update()
        {
            if (isPointDown && !isLongPressStatus && (Time.time - startPressTime >= longPressTimeDef))
            {
                OnLongPress();
            }

            if (clickCount > 0)
            {
                if (Time.time - startPressTime > doubleClickTimeDef)
                {
                    clickCount = 0;
                }

                if (clickCount >= 2)
                {
                    if (mDoubleClick != null)
                    {
                        mDoubleClick(currentPosition);
                    }

                    clickCount = 0;
                }
            }
        }
        
        public void SetLongPressTime(float time)
        {
            longPressTimeDef = time;
            Util.Log("------longPressTime is {0}", longPressTimeDef);
        }

        public void SetDoubleClickTime(float time)
        {
            doubleClickTimeDef = time;
            Util.Log("------doubleClickTimeDef is {0}", doubleClickTimeDef);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            isPointDown = true;
            isPress = false;
            startPressTime = Time.time;

            currentPosition = eventData.position;
            
            if (mPressIn != null)
            {                    
                mPressIn(currentPosition);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPointDown = false;
            isLongPressStatus = false;

            if (!isPress)
            {
                clickCount++;
            }

            currentPosition = eventData.position;
            if (mPressUp != null)
            {
                mPressUp(currentPosition);
            }
        }

        public void OnLongPress()
        {
            isLongPressStatus = true;
            isPress = true;

            startPressTime = Time.time;
            if (mLongPress != null)
            {
                mLongPress(currentPosition);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            if (mPress != null)
            {     
                mPress(eventData.position);
            }
        }
    }
}