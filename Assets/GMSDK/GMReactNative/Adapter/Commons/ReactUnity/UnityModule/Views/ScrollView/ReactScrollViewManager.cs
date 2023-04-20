using System;
using System.Collections;

namespace GSDK.RNU {
    public class ReactScrollViewManager: BaseViewManager {
        
        public static string viewName = "RNUScrollView";

        public static string sScrollToStart = "scrollToStart";
        public static string sScrollToEnd = "scrollToEnd";
        public static string sScrollBeginDrag = "onScrollBeginDrag";
        public static string sScrollEndDrag = "onScrollEndDrag";
        public static string sScroll = "onScroll";
        
        public override string GetName() {
            return viewName;
        }

        protected override BaseView createViewInstance() {
            return new ReactScrollView(viewName);
        }
        
        [ReactProp(name = "horizontal")]
        public void SetHorizontal(ReactScrollView view, bool horizontal)
        {
            view.SetScrollbarHorizontal(horizontal);
        }

        [ReactProp(name = "movementType")]
        public void SetMovementType(ReactScrollView view, int type)
        {
            view.SetScrollbarMovementType(type);
        }


        [ReactProp(name = "rectMask2D")]
        public void SetRectMask2D(ReactScrollView view, bool isRectMask2D)
        {
            view.SetScrollViewRectMask2D(isRectMask2D);
        }
        
        
        // scrolbar 属性预留
        
        [ReactProp(name = "barImage")]
        public void SetBarImage(ReactScrollView view, string uri)
        {
            // view.SetBarImage(uri);
        }

        [ReactProp(name = "barColor")]
        public void SetBarColor(ReactScrollView view, long barColor)
        {
            // view.SetBarColor(barColor);
        }

        [ReactProp(name = "barImageEnable")]
        public void SetbarEnable(ReactScrollView view, bool isEnable)
        {            
            // view.SetBarImageEnable(isEnable);
        }
        
        [ReactProp(name = "overflow")]
        public override void setOverflow(BaseView view, string overflow)
        {
            throw new Exception("scrollView do not support set overflow !!!");
        }
        
        
        [ReactProp(name = "overflowMask")]
        public override void setOverflowMask(BaseView view, string overflow)
        {
            throw new Exception("scrollView do not support set overflow Mask !!!");
        }
        
        // 每几帧返回一次 onScroll 进度，默认每 10 帧返回一次
        [ReactProp(name = "scrollEventThrottle")]
        public void SetScrollEventThrottle(ReactScrollView view, int value)
        {
            view.SetScrollEventThrottle(value);
        }
        
        //  触发到顶到底事件的边界阈值，默认 10 个单位的位置
        [ReactProp(name = "scrollEdgThreshold")]
        public void SetScrollEdgThreshold(ReactScrollView view, int value)
        {
            view.SetScrollEdgThreshold(value);
        }
                
        [ReactProp(name = "animationThreshold")]
        public void SetScrollAnimationThreshold(ReactScrollView view, int value)
        {
            view.SetScrollAnimationThreshold(value);
        }
        
        
        
        [ReactProp(name = "hasOnScroll")]
        public void SetHasOnScroll(ReactScrollView view, bool hasOnScroll)
        {
            view.SetHasOnScroll(hasOnScroll);
        }
        
        public override Hashtable GetExportedCustomDirectEventTypeConstants()
        {
            return new Hashtable
            {
                {sScrollToStart, new Hashtable{{sRegistration, sScrollToStart}}},
                {sScrollToEnd, new Hashtable{{sRegistration, sScrollToEnd}}},
                {sScrollBeginDrag, new Hashtable{{sRegistration, sScrollBeginDrag}}},
                {sScrollEndDrag, new Hashtable{{sRegistration, sScrollEndDrag}}},
                {sScroll, new Hashtable{{sRegistration, sScroll}}},
            };
        }
        
        [ReactCommand]
        public void scrollTo(ReactScrollView scrollView, double x, double y, bool animated)
        {
            scrollView.ScrollTo((float)x, (float)y, animated);
        }

        [ReactCommand]
        public void srollToBegin(ReactScrollView scrollView, bool animated)
        {
            scrollView.ScrollTo(0, 0, animated);
        }

        [ReactCommand]
        public void srollToEnd(ReactScrollView scrollView, bool animated)
        {
            scrollView.ScrollToEnd(animated);
        }

    }
}