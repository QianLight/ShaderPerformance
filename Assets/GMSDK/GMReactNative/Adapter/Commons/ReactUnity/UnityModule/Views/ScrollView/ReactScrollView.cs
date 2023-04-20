using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace GSDK.RNU
{
    public class ReactScrollView : SimpleBaseView
    {
        private GameObject realGameObject;

        private GameObject viewPort;
        private ReactScrollViewContent content;
        private ScrollRect scrollRect;

        private static string viewPortName = "ViewPort";
        private static string contentName = "Content";
        private static string scrollbarHorName = "barHorizontal";
        private static string scrollbarVerName = "barVertical";

        private bool isHorizontal = false;

        private Vector2 rect = Vector2.zero;
        
        private ScrollViewHandler eventHandler;

        private ReactScrollViewContent ScrollViewContent;
        
        private bool hasOnScrollFlag = false;

        // 每几帧返回一次 onScroll 进度，默认每 10 帧返回一次
        private int scrollEventThrottle = 10;
        private int scrollEventindex = -1;

        
        // ---------
        // SimpleBaseView
        public override GameObject GetGameObject()
        {
            return realGameObject;
        }

        public override GameObject GetContentObject()
        {
            return viewPort;
        }

        public override void Add(BaseView child)
        {
            GameObject curGo = GetContentObject();
            GameObject childGo = child.GetGameObject();
            if (curGo == null)
            {
                Util.Log("curGo is null, return");
                return;
            }
        
            if (childGo == null)
            {
                Util.Log("childGo is null, return");
                return;
            }
            childGo.transform.SetParent(curGo.transform, false);

            if (childGo.gameObject.name == ReactScrollViewContentManager.viewName)
            {
                Util.Log("add content");
                SetScrollViewContent(child);
            }
        }

        public override void Destroy()
        {
            Object.Destroy(viewPort);
            base.Destroy();
        }

        // ----------
        // ReactScrollView
        public ReactScrollView(string name)
        {
            realGameObject = new GameObject(name, typeof(ScrollRect), typeof(Image));

            scrollRect = realGameObject.GetComponent<ScrollRect>();
            
            viewPort = new GameObject(viewPortName, typeof(Image), typeof(Mask));
            viewPort.transform.SetParent(realGameObject.transform, false);
            RectTransform viewPortRect = viewPort.GetComponent<RectTransform>();
            viewPortRect.anchorMin = new Vector2(0, 0);
            viewPortRect.anchorMax = new Vector2(1, 1);
            viewPortRect.pivot = new Vector2(0.5f, 0.5f);
            viewPortRect.anchoredPosition3D = new Vector3(0, 0, 0);
            viewPortRect.sizeDelta = new Vector2(0, 0);


            AddScrollViewHandlerEvent();
            
            Image img = realGameObject.GetComponent<Image>();
            if (img == null)
            {
                Util.Log("image is null, return");
                return;
            }
            // 默认设置为 透明 背景
            img.color = new Color(255, 255, 255, 0);
            
            
            /*
             * 当遮挡组件为 Mask 时，透明度为 0 即 color 为 (0, 0, 0, 0) 会有被完全遮挡的情况
             * 所以此处给了一个很小的透明度值 1
             * RectMask2D 表现一切正常
             */
            Image image = viewPort.GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color32(0,0,0,1);
            }
            
            if (viewPort.GetComponent<Mask>() != null)
            {
                viewPort.GetComponent<Mask>().showMaskGraphic = false;
            }

            
            FillReactScrolView();

            SetScrollbarHorizontal(isHorizontal);

        }

        private void AddScrollRectEvent()
        {
            scrollRect.onValueChanged.RemoveAllListeners();
            if (hasOnScrollFlag)
            {
                scrollRect.onValueChanged.AddListener((value)=>
                {
                    scrollEventindex = (scrollEventindex + 1) % scrollEventThrottle;
                    if (scrollEventindex == 0)
                    {
                        ArrayList args = new ArrayList();
                        args.Add(scrollRect.content.anchoredPosition);
                        OnScrollEvent(args);
                    }
                });
            }
        }
        public void SetHasOnScroll(bool hasOnScroll)
        {
            hasOnScrollFlag = hasOnScroll;
            AddScrollRectEvent();
        }
        public void SetScrollEventThrottle(int value)
        {
            scrollEventThrottle = value;
        }
        

        
        private void AddScrollViewHandlerEvent()
        {
            eventHandler = realGameObject.GetComponent<ScrollViewHandler>();
            if (eventHandler == null)
            {
                eventHandler = realGameObject.AddComponent<ScrollViewHandler>();

            }
            eventHandler.SetScrollHorizontal(isHorizontal);
            eventHandler.SetScroll(realGameObject.GetComponent<ScrollRect>());

            eventHandler.mScrollToStart = () => { OnEvent(ReactScrollViewManager.sScrollToStart); };
            eventHandler.mScrollToEnd = () => { OnEvent(ReactScrollViewManager.sScrollToEnd); };
            eventHandler.mScrollBeginDrag = () => { OnEvent(ReactScrollViewManager.sScrollBeginDrag); };
            eventHandler.mScrollEndDrag = () => { OnEvent(ReactScrollViewManager.sScrollEndDrag); };
        }

        public void OnEvent(string eventName)
        {
            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add(eventName);
            args.Add(new Hashtable());

            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }



        public void OnScrollEvent(ArrayList arrayList)
        {
            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add(ReactScrollViewManager.sScroll);

            ArrayList offsetArr = new ArrayList();
            Vector2 offsetData = (Vector2)arrayList[0];
            offsetArr.Add(offsetData.x);
            offsetArr.Add(offsetData.y);

            ArrayList contentSizeArr = new ArrayList();
            Vector2 contentRect = content.GetRect();
            contentSizeArr.Add(contentRect.x);
            contentSizeArr.Add(contentRect.y);

            ArrayList layoutArr = new ArrayList();
            Vector2 contentPosition = content.GetPosition();
            Vector2 viewRect = GetRect();
            if (!isHorizontal)
            {
                layoutArr.Add(viewRect.x);
                float restHeight = contentRect.y - contentPosition.y;
                float layoutHeight;
                if (contentPosition.y >= 0)
                {
                    layoutHeight = restHeight < viewRect.y ? restHeight : viewRect.y;
                }
                else
                {
                    layoutHeight = restHeight < viewRect.y ? contentRect.y : (viewRect.y + contentPosition.y);
                }
                layoutArr.Add(layoutHeight);
            }
            else
            {
                float resWidth = contentRect.x + contentPosition.x;
                float layoutWidth;
                if (contentPosition.x <= 0)
                {
                    layoutWidth = resWidth < viewRect.x ? resWidth : viewRect.x;
                }
                else
                {
                    layoutWidth = resWidth < viewRect.x ? contentRect.x : (viewRect.x - contentPosition.x);
                }

                layoutArr.Add(layoutWidth);
                layoutArr.Add(viewRect.y);
            }
            
            args.Add(new Hashtable
            {
                // {"contentInset", null},
                {"contentOffset", offsetArr},
                {"contentSize", contentSizeArr},
                {"layoutMeasurement", layoutArr},
                {"zoomScale", 1}
            });

            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }
        

        private void FillReactScrolView()
        {
            ScrollRect rect = realGameObject.GetComponent<ScrollRect>();
            rect.viewport = viewPort.GetComponent<RectTransform>();

            // rect.horizontalScrollbar = _scrollbarHorizontal.GetScrollbar();
            // rect.verticalScrollbar = _scrollbarVertical.GetScrollbar();
        }

        public void SetScrollViewContent(BaseView contentView)
        {
            content = (ReactScrollViewContent)contentView;

            ScrollRect rect = realGameObject.GetComponent<ScrollRect>();
            rect.content = content.GetGameObject().GetComponent<RectTransform>();
        }

        //React prop
        public void SetScrollbarHorizontal(bool horizontal)
        {
            isHorizontal = horizontal;
            ScrollRect rect = realGameObject.GetComponent<ScrollRect>();
            rect.vertical = !isHorizontal;
            rect.horizontal = isHorizontal;

            AddScrollViewHandlerEvent();
        }

        public void SetScrollViewRectMask2D(bool isRectMask2D)
        {
            if (isRectMask2D == false)
            {
                return;
            }

            if (viewPort.GetComponent<Mask>() != null)
            {
                GameObject.Destroy(viewPort.GetComponent<Mask>());
                viewPort.AddComponent<RectMask2D>();
            }
        }

        public void SetScrollEdgThreshold(int value)
        {
            if (eventHandler == null)
            {
                Util.Log("eventHandler is null, return");
                return;
            }
            eventHandler.SetScrollEdgThreshold(value);
        }

        public void SetScrollAnimationThreshold(int value)
        {
            if (content == null)
            {
                Util.Log("content is null, return");
                return;
            }
            content.SetAnimationThreshold(value);
        }
        
        //React prop
        public void SetScrollbarMovementType(int type)
        {
            ScrollRect rect = realGameObject.GetComponent<ScrollRect>();
            if (type == 1)
            {
                rect.movementType = ScrollRect.MovementType.Unrestricted;
            }
            else if (type == 3)
            {
                rect.movementType = ScrollRect.MovementType.Clamped;
            }
            else
            {
                rect.movementType = ScrollRect.MovementType.Elastic;
            }
        }

        public void ScrollTo(float x, float y, bool animated)
        {
            content.ReSetLayout(x, y, animated, isHorizontal);
        }

        public override void SetLayout(int x, int y, int width, int height)
        {
            base.SetLayout(x, y, width, height);
            {
                rect.x = width;
                rect.y = height;
            }
        }

        public Vector2 GetRect()
        {
            return rect;
        }
        public void ScrollToEnd(bool animated)
        {
            Vector2 contentRect = content.GetRect();
            Vector2 viewRect = GetRect();
            if (!isHorizontal)
            {
                float resY = contentRect.y - viewRect.y <= 0 ? 0 : contentRect.y - viewRect.y;
                ScrollTo(0, resY, animated);
            }
            else
            {
                float resX = contentRect.x - viewRect.x <= 0 ? 0 : viewRect.x - contentRect.x;
                ScrollTo(resX, 0, animated);
            }
        }
        
    }
}
