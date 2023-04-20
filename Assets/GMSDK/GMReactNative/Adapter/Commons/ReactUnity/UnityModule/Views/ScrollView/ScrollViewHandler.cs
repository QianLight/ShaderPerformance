
/*
 * @Author: hexiaonuo
 * @Date: 2021-11-09
 * @Description: ScrollViewHandler script
 * @FilePath: ReactUnity/UnityModule/Scripts/ScrollViewHandler.cs
 */

using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GSDK.RNU
{
    public class ScrollViewHandler: UIBehaviour, IBeginDragHandler, IEndDragHandler
    {
        private ScrollRect mRect;
        private bool mIsHorizontal;

        public UnityAction mScrollToStart;
        public UnityAction mScrollToEnd;
        public UnityAction mScrollBeginDrag;
        public UnityAction mScrollEndDrag;


        //  触发到顶到底事件的边界阈值，默认 10 个单位的位置
        private int scrollEdgThreshold = 10;
        
        public void SetScroll(ScrollRect scrollRect)
        {
            mRect = scrollRect;
        }

        public void SetScrollHorizontal(bool isHorizontal)
        {
            mIsHorizontal = isHorizontal;
        }

        public void SetScrollEdgThreshold(int value)
        {
            scrollEdgThreshold = value;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Util.Log("-------OnBeginDrag.position {0}{1}", eventData.position.x, eventData.position.y);
            if (mScrollBeginDrag != null)
            {
                mScrollBeginDrag();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Util.Log("-------OnEndDrag.position {0}{1}", eventData.position.x, eventData.position.y);

            if (mScrollEndDrag != null)
            {
                mScrollEndDrag();
            }
            
            if (IsReachStart() && mScrollToStart != null)
            {
                mScrollToStart();
            } 
            else if (IsReachEnd() && mScrollToEnd != null)
            {
                mScrollToEnd();
            }
        }

        private bool IsReachStart()
        {
            if (mRect == null)
            {
                Util.Log("ScrollRect is null, return false default");
                return false;
            }
            // Util.Log("isReachStart {0}, {1}, {2}",mRect.content.anchoredPosition.x,mRect.content.anchoredPosition.y, mIsHorizontal);


            // 横向
            if (mIsHorizontal)
            {
                return mRect.content.anchoredPosition.x > scrollEdgThreshold;
            } 
            return mRect.content.anchoredPosition.y < -scrollEdgThreshold;
        }

        private bool IsReachEnd()
        {
            if (mRect == null)
            {
                Util.Log("ScrollRect is null, return false default");
                return false;
            }

            // Util.Log("isReachEnd {0}, {1}",mRect.content.anchoredPosition.x,mRect.content.rect.width-mRect.viewport.rect.width);
            // Util.Log("isReachEnd {0}, {1}",mRect.content.anchoredPosition.x,mRect.content.rect.height-mRect.viewport.rect.height);

            // 横向
            if (mIsHorizontal)
            {
                return (mRect.content.anchoredPosition.x + mRect.content.rect.width) < mRect.viewport.rect.width - scrollEdgThreshold;
            } 
            return mRect.content.anchoredPosition.y > (mRect.content.rect.height-mRect.viewport.rect.height + scrollEdgThreshold);
        }
    }
}