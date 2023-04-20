using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;


namespace GSDK.RNU
{
    public abstract class BaseViewManager : ViewManager
    {
        private Dictionary<string, MethodInfo> propSetters = new Dictionary<string, MethodInfo>();
        private Dictionary<string, ReactProp> propAttributeSetters = new Dictionary<string, ReactProp>();

        private Dictionary<string, MethodInfo> commandSetters = new Dictionary<string, MethodInfo>();


        public static string sPressInEventname = "onPressIn";
        public static string sPressUpEventname = "onPressOut";
        public static string sLongPressEventname = "onLongPress";
        public static string sPressEventname = "onPress";
        public static string sDoubleClickEventname = "onDoubleClick";

        public static string sRegistration = "registrationName";

        public static string sBorderRadius = "_borderRadius";
        public static string sBorderWidth = "_borderWidth";
        public static string sBorderStartColor = "_borderStartColor";
        public static string sBorderEndColor = "_borderEndColor";
        public static string sBorderTopColor = "_borderTopColor";
        public static string sBorderBottomColor = "_borderBottomColor";

        public BaseViewManager()
        {
            Type typz = this.GetType();
            foreach (MethodInfo m in typz.GetMethods())
            {
                foreach (Attribute attr in m.GetCustomAttributes(false))
                {
                    if (attr is ReactProp)
                    {
                        ParameterInfo[] infos = m.GetParameters();
                        if (infos.Length != 2)
                        {
                            throw new Exception("ReactProp method must have 2 parameter");
                        }

                        string key = ((ReactProp)attr).name;
                        propSetters.Add(key, m);
                        propAttributeSetters.Add(key, (ReactProp)attr);
                    }

                    if (attr is ReactCommand)
                    {
                        commandSetters.Add(m.Name, m);
                    }
                }
            }
        }

        /**
        * @return a map of constants this module exports to JS. Supports JSON types.
        */
        public Hashtable GetConstants()
        {
            return null;
        }

        public abstract string GetName();

        [ReactProp(name = "material")]
        public virtual void setMaterial(BaseView view, string material)
        {
            GameObject panel = view.GetGameObject();

            if (panel == null)
            {
                Util.Log("panel is null setMaterial failed, return");
                return;
            }

            Image image = panel.GetComponent<Image>();
            if (image != null)
            {
                image.material = RNUMainCore.LoadMaterial(material);
                return;
            }

            Util.Log("add material failed for gameObject without Image");

        }


        [ReactProp(name = "backgroundImage")]
        public void setBackgroundImage(BaseView view, string uri)
        {
            GameObject go = view.GetGameObject();

            Image image = go.GetComponent<Image>();
            RawImage rawImage = go.GetComponent<RawImage>();
            if (image == null && rawImage == null)
            {
                image = go.AddComponent<Image>();
                if (image != null)
                {
                    // 首次添加图片 默认透明
                    image.color = new Color32(0, 0, 0, 0);
                    image.raycastTarget = false;
                }
            }

            if (image != null)
            {
                StaticCommonScript.LoadTexture(uri, texture =>
                {
                    if (!image.IsDestroyed())
                    {
                        image.color = Color.white;
                        image.sprite = texture == null ? null : Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    }
                });
            }
            else if (rawImage != null)
            {
                StaticCommonScript.LoadTexture(uri, texture =>
                {
                    if (!rawImage.IsDestroyed())
                    {
                        rawImage.texture = texture;
                    }
                });
            }

        }


        [ReactProp(name = "backgroundColor", defaultInt = 0)]
        public virtual void setBackgroundColor(BaseView view, long backgroundColor)
        {
            GameObject panel = view.GetGameObject();

            MaskableGraphic image = getOrAddImage(panel);
            if (image != null)
            {
                image.color = UnityUtils.GetColor(backgroundColor);
            }
        }
        private MaskableGraphic getOrAddImage(GameObject go)
        {
            Image image = go.GetComponent<Image>();
            RawImage rawImage = go.GetComponent<RawImage>();
            if (image == null && rawImage == null)
            {
                image = go.AddComponent<Image>();
                if (image != null)
                {
                     // 首次添加图片 默认透明
                    image.color = new Color32(0, 0, 0, 0);
                    image.raycastTarget = false;
                }
            }

            if (image != null)
            {
                return image;
            }

            return rawImage;
        }


        [ReactProp(name = "raycastTarget", defaultBoolean = false)]
        public void SetRaycastTarget(BaseView view, bool raycastTarget)
        {
            GameObject panel = view.GetGameObject();

            MaskableGraphic image = getOrAddImage(panel);
            if (image != null)
            {
                image.raycastTarget = raycastTarget;
            }
        }

        [ReactProp(name = "active", defaultBoolean = true)]
        public void SetActive(BaseView view, bool isActive)
        {
            GameObject panel = view.GetGameObject();
            panel.SetActive(isActive);
        }

        [ReactProp(name = "name")]
        public virtual void SetName(BaseView view, string name)
        {
            GameObject panel = view.GetGameObject();
            panel.name = name;
        }

        [ReactProp(name = "opacity", defaultFloat = 1.0F)]
        public void setOpacity(BaseView view, float opacity)
        {
            GameObject panel = view.GetGameObject();

            var i = panel.GetComponent<CanvasGroup>();
            if (i == null)
            {
                i = panel.AddComponent<CanvasGroup>();
            }

            i.alpha = opacity;
        }

        // shadow参数，shadowColor shadowOffset shadowOpacity
        [ReactProp(name = "shadowColor", defaultLong = 0)]
        public void setShadowColor(BaseView view, long shadowColor)
        {

            GameObject panel = view.GetGameObject();

            Shadow i = panel.GetComponent<Shadow>();
            if (i == null)
            {   // 添加shadow组件时，将默认的offset设置为(0, 0)
                i = panel.AddComponent<Shadow>();
                i.effectDistance = new Vector2(0, 0);
            }
            i.effectColor = UnityUtils.GetColor(shadowColor);
        }

        [ReactProp(name = "shadowOffset")]
        public void setShadowOffset(BaseView view, Dictionary<string, object> shadowOffset)
        {
            if (!shadowOffset.ContainsKey("width") || !shadowOffset.ContainsKey("height"))
            {
                Util.Log("Missing width or height, return");
                return;
            }
            GameObject panel = view.GetGameObject();

            Shadow i = panel.GetComponent<Shadow>();
            if (i == null)
            {
                i = panel.AddComponent<Shadow>();
            }
            float a = Convert.ToSingle(shadowOffset["width"]);
            float b = Convert.ToSingle(shadowOffset["height"]);
            i.effectDistance = new Vector2(a, -b);
        }

        [ReactProp(name = "shadowOpacity", defaultFloat = 1.0F)]
        public void setShadowOpacity(BaseView view, float shadowOpacity)
        {
            GameObject panel = view.GetGameObject();

            Shadow i = panel.GetComponent<Shadow>();
            if (i == null)
            {
                i = panel.AddComponent<Shadow>();
                i.effectDistance = new Vector2(0, 0);
            }
            i.effectColor = new Color(i.effectColor[0], i.effectColor[1], i.effectColor[2], shadowOpacity);
        }
        //shadow end

        // outline参数，outlineColor outlineOffset outlineOpacity
        [ReactProp(name = "outlineColor", defaultLong = 0)]
        public void setOutlineColor(BaseView view, long outlineColor)
        {
            GameObject panel = view.GetGameObject();

            Outline i = panel.GetComponent<Outline>();
            if (i == null)
            {    // 添加outline组件时，将默认的offset设置为(0, 0)
                i = panel.AddComponent<Outline>();
                i.effectDistance = new Vector2(0, 0);
            }
            i.effectColor = UnityUtils.GetColor(outlineColor);
        }

        [ReactProp(name = "outlineOffset")]
        public void setOutlineOffset(BaseView view, Dictionary<string, object> outlineOffset)
        {
            if (!outlineOffset.ContainsKey("width") || !outlineOffset.ContainsKey("height"))
            {
                Util.Log("Missing width or height, return");
                return;
            }
            GameObject panel = view.GetGameObject();

            Outline i = panel.GetComponent<Outline>();
            if (i == null)
            {
                i = panel.AddComponent<Outline>();
            }
            float a = Convert.ToSingle(outlineOffset["width"]);
            float b = Convert.ToSingle(outlineOffset["height"]);
            i.effectDistance = new Vector2(a, b);
        }

        [ReactProp(name = "outlineOpacity", defaultFloat = 1.0F)]
        public void setOutlineOpacity(BaseView view, float outlineOpacity)
        {
            GameObject panel = view.GetGameObject();

            Outline i = panel.GetComponent<Outline>();
            if (i == null)
            {
                i = panel.AddComponent<Outline>();
                i.effectDistance = new Vector2(0, 0);
            }
            i.effectColor = new Color(i.effectColor[0], i.effectColor[1], i.effectColor[2], outlineOpacity);
        }
        //outline end

        [ReactProp(name = "zIndex")]
        public void setZIndex(BaseView view, float zIndex)
        {
        }

        // translateX, translateY, rotateX, rotateY, rotateX, scaleX, scaleY
        [ReactProp(name = "transform")]
        public void SetTransform(BaseView view, ArrayList transfrom)
        {
            view.SetTransformInfo(TransformInfo.InitTransformInfoByJsList(transfrom));
        }

        [ReactProp(name = "borderRadius", defaultFloat = 0.0F)]
        public void setBorderRadius(BaseView view, float borderRadius)
        {
            Material imageBorder = AddImageBorder(view);
            imageBorder.SetVector(sBorderRadius, new Vector4(borderRadius, borderRadius, borderRadius, borderRadius));
        }

        [ReactProp(name = "borderTopStartRadius", defaultFloat = 0.0F)]
        public void setBorderTopStartRadius(BaseView view, float borderRadius)
        {
            Material imageBorder = AddImageBorder(view);
            Vector4 v = imageBorder.GetVector(sBorderRadius);
            v.x = borderRadius;
            imageBorder.SetVector(sBorderRadius, v);
        }

        [ReactProp(name = "borderTopEndRadius", defaultFloat = 0.0F)]
        public void setBorderTopEndRadius(BaseView view, float borderRadius)
        {
            Material imageBorder = AddImageBorder(view);
            Vector4 v = imageBorder.GetVector(sBorderRadius);
            v.y = borderRadius;
            imageBorder.SetVector(sBorderRadius, v);
        }

        [ReactProp(name = "borderBottomStartRadius", defaultFloat = 0.0F)]
        public void setBorderBottomStartRadius(BaseView view, float borderRadius)
        {
            Material imageBorder = AddImageBorder(view);
            Vector4 v = imageBorder.GetVector(sBorderRadius);
            v.z = borderRadius;
            imageBorder.SetVector(sBorderRadius, v);
        }

        [ReactProp(name = "borderBottomEndRadius", defaultFloat = 0.0F)]
        public void setBorderBottomEndRadius(BaseView view, float borderRadius)
        {
            Material imageBorder = AddImageBorder(view);
            Vector4 v = imageBorder.GetVector(sBorderRadius);
            v.w = borderRadius;
            Debug.Log("setBorderBottomEndRadius" + v.ToString());
            imageBorder.SetVector(sBorderRadius, v);
        }


        [ReactProp(name = "borderTopLeftRadius", defaultFloat = 0.0F)]
        public void setBorderTopLeftRadius(BaseView view, float borderRadius)
        {
            setBorderTopStartRadius(view, borderRadius);
        }

        [ReactProp(name = "borderTopRightRadius", defaultFloat = 0.0F)]
        public void setBorderTopRightRadius(BaseView view, float borderRadius)
        {
            setBorderTopEndRadius(view, borderRadius);
        }

        [ReactProp(name = "borderBottomLeftRadius", defaultFloat = 0.0F)]
        public void setBorderBottomLeftRadius(BaseView view, float borderRadius)
        {
            setBorderBottomStartRadius(view, borderRadius);
        }

        [ReactProp(name = "borderBottomRightRadius", defaultFloat = 0.0F)]
        public void setBorderBottomRightRadius(BaseView view, float borderRadius)
        {
            setBorderBottomEndRadius(view, borderRadius);
        }


        [ReactProp(name = "touchable")]
        public void setTouchable(BaseView view, ArrayList eventName)
        {
                GameObject panel = view.GetGameObject();
                IPointerEvent gLongPress = panel.GetComponent<IPointerEvent>();
                if (gLongPress == null)
                {
                    gLongPress = panel.AddComponent<IPointerEvent>();
                }

                if (gLongPress == null)
                {
                    Util.Log("AddComponent IPointerEvent failed, return");
                    return;
                }

                if (eventName.Contains(sPressInEventname))
                {
                    gLongPress.mPressIn = (position) =>
                    {
                        OnEvent(view, sPressInEventname, position);
                    };
                }

                if (eventName.Contains(sPressUpEventname))
                {
                    gLongPress.mPressUp = (position) =>
                    {
                        OnEvent(view, sPressUpEventname, position);
                    };
                }

                if (eventName.Contains(sLongPressEventname))
                {
                    gLongPress.mLongPress = (position) =>
                    {
                        OnEvent(view, sLongPressEventname, position);
                    };
                }

                if (eventName.Contains(sDoubleClickEventname))
                {
                    gLongPress.mDoubleClick = (position) =>
                    {
                        OnEvent(view, sDoubleClickEventname, position);
                    };
                }

                if (eventName.Contains(sPressEventname))
                {
                    gLongPress.mPress = (position) =>
                    {
                        OnEvent(view, sPressEventname, position);
                    };
                }

                SetRaycastTarget(view, true);
        }


        private void OnEvent(BaseView view, string eventName, Vector2 position)
        {
            ArrayList args = new ArrayList();
            args.Add(view.GetId());
            args.Add(eventName);
            args.Add(new Hashtable
            {
                {"pageX", position.x},
                {"pageY", position.y}
            });

            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }

        [ReactProp(name = "borderWidth", defaultFloat = 0.0F)]
        public void setBorderWidth(BaseView view, float borderWidth)
        {
            Material imageBorder = AddImageBorder(view);
            imageBorder.SetVector(sBorderWidth, new Vector4(borderWidth, borderWidth, borderWidth, borderWidth));
        }

        [ReactProp(name = "borderStartWidth", defaultFloat = 0.0F)]
        public void setBorderStartWidth(BaseView view, float borderWidth)
        {
            Material imageBorder = AddImageBorder(view);
            Vector4 v = imageBorder.GetVector(sBorderWidth);
            v.x = borderWidth;
            imageBorder.SetVector(sBorderWidth, v);
        }

        [ReactProp(name = "borderEndWidth", defaultFloat = 0.0F)]
        public void setBorderEndWidth(BaseView view, float borderWidth)
        {
            Material imageBorder = AddImageBorder(view);
            Vector4 v = imageBorder.GetVector(sBorderWidth);
            v.y = borderWidth;
            imageBorder.SetVector(sBorderWidth, v);
        }

        [ReactProp(name = "borderTopWidth", defaultFloat = 0.0F)]
        public void setBorderTopWidth(BaseView view, float borderWidth)
        {
            Material imageBorder = AddImageBorder(view);
            Vector4 v = imageBorder.GetVector(sBorderWidth);
            v.z = borderWidth;
            imageBorder.SetVector(sBorderWidth, v);
        }

        [ReactProp(name = "borderBottomWidth", defaultFloat = 0.0F)]
        public void setBorderBottomWidth(BaseView view, float borderWidth)
        {
            Material imageBorder = AddImageBorder(view);
            Vector4 v = imageBorder.GetVector(sBorderWidth);
            v.w = borderWidth;
            imageBorder.SetVector(sBorderWidth, v);
        }


        [ReactProp(name = "borderLeftWidth", defaultFloat = 0.0F)]
        public void setBorderLeftWidth(BaseView view, float borderWidth)
        {
            setBorderStartWidth(view, borderWidth);
        }

        [ReactProp(name = "borderRightWidth", defaultFloat = 0.0F)]
        public void setBorderRightWidth(BaseView view, float borderWidth)
        {
            setBorderEndWidth(view, borderWidth);
        }

        [ReactProp(name = "borderColor", defaultLong = 0)]
        public void setBorderColor(BaseView view, long borderColor)
        {
            Material imageBorder = AddImageBorder(view);
            imageBorder.SetColor(sBorderStartColor, UnityUtils.GetColor(borderColor));
            imageBorder.SetColor(sBorderEndColor, UnityUtils.GetColor(borderColor));
            imageBorder.SetColor(sBorderTopColor, UnityUtils.GetColor(borderColor));
            imageBorder.SetColor(sBorderBottomColor, UnityUtils.GetColor(borderColor));
        }

        [ReactProp(name = "borderStartColor", defaultLong = 0)]
        public void setBorderStartColor(BaseView view, long borderColor)
        {
            Material imageBorder = AddImageBorder(view);
            imageBorder.SetColor(sBorderStartColor, UnityUtils.GetColor(borderColor));
        }

        [ReactProp(name = "borderEndColor", defaultLong = 0)]
        public void setBorderEndColor(BaseView view, long borderColor)
        {
            Material imageBorder = AddImageBorder(view);
            imageBorder.SetColor(sBorderEndColor, UnityUtils.GetColor(borderColor));
        }

        [ReactProp(name = "borderTopColor", defaultLong = 0)]
        public void setBorderTopColor(BaseView view, long borderColor)
        {
            Material imageBorder = AddImageBorder(view);
            imageBorder.SetColor(sBorderTopColor, UnityUtils.GetColor(borderColor));
        }

        [ReactProp(name = "borderBottomColor", defaultLong = 0)]
        public void setBorderBottomColor(BaseView view, long borderColor)
        {
            Material imageBorder = AddImageBorder(view);
            imageBorder.SetColor(sBorderBottomColor, UnityUtils.GetColor(borderColor));
        }


        [ReactProp(name = "borderLeftColor", defaultLong = 0)]
        public void setBorderLeftColor(BaseView view, long borderColor)
        {
            setBorderStartColor(view, borderColor);
        }

        [ReactProp(name = "borderRightColor", defaultLong = 0)]
        public void setBorderRightColor(BaseView view, long borderColor)
        {
            setBorderEndColor(view, borderColor);
        }

        [ReactProp(name = "overflowMask", defaultString = "visible")]
        public virtual void setOverflowMask(BaseView view, string overflow)
        {
            GameObject panel = view.GetGameObject();
            Mask mask = panel.GetComponent<Mask>();
            if (mask == null)
            {
                mask = panel.AddComponent<Mask>();
            }

            if (mask == null)
            {
                Util.Log("add mask failed, return");
                return;
            }
            MaskableGraphic image = panel.GetComponent<Image>();
            if (image == null)
            {
                image = panel.GetComponent<RawImage>();
            }

            if (image == null)
            {
                image = panel.AddComponent<Image>();
            }

            if (image == null)
            {
                Util.Log("image component is null, return");
                return;
            }
            image.color = new Color(0, 0, 0, 255);

            if (overflow == "hidden")
            {
                mask.enabled = true;
                image.enabled = true;
            }
            else if (overflow == "visible")
            {
                mask.enabled = false;
                image.enabled = false;
            }

            mask.showMaskGraphic = false;
        }


        [ReactProp(name = "overflow", defaultString = "visible")]
        public virtual void setOverflow(BaseView view, string overflow)
        {
            GameObject panel = view.GetGameObject();
            RectMask2D mask2D = panel.GetComponent<RectMask2D>();
            if (mask2D == null)
            {
                mask2D = panel.AddComponent<RectMask2D>();
            }

            if (mask2D == null)
            {
                Util.Log("add mask2ds failed, return");
                return;
            }
            if (overflow == "hidden")
            {
                mask2D.enabled = true;
            }
            else if (overflow == "visible")
            {
                mask2D.enabled = false;
            }

        }

        private Material AddImageBorder(BaseView view)
        {
            GameObject panel = view.GetGameObject();
            MaskableGraphic image = getOrAddImage(panel);
            if (!image.material.HasProperty(sBorderRadius)){
                // 临时使用material是否含有_borderRadius属性，来判断material是否设置正确
                var borderMaterial  = RNUMainCore.LoadMaterial("borderMaterial");
                if (borderMaterial != null) {
                    image.material  = new Material(borderMaterial);
                } else {
                    Debug.LogError("没有找到材质borderMaterial.mat");
                }
            }
            return image.material;
        }

        public virtual void UpdateProperties(BaseView viewToUpdate, Dictionary<string, object> props)
        {
            var currentKey = "";
            try
            {
                foreach (KeyValuePair<string, object> kv in props)
                {
                    currentKey = kv.Key;
                    if (!propSetters.ContainsKey(kv.Key))
                    {
                        Util.Log("UpdateProperties not support key: " + kv.Key);
                        continue;
                    }

                    MethodInfo method = propSetters[kv.Key];

                    ParameterInfo[] infos = method.GetParameters();
                    // 由于FullName在 Dictionary<K,V>上太长，故用Namespace + Name
                    var typeStr = infos[1].ParameterType.Namespace + '.' + infos[1].ParameterType.Name;

                    switch (typeStr)
                    {
                        case "System.Int32":
                            {
                                if (kv.Value == null)
                                {
                                    ReactProp rp = propAttributeSetters[kv.Key];
                                    method.Invoke(this, new object[] { viewToUpdate, rp.defaultInt });
                                    break;
                                }

                                method.Invoke(this, new object[] { viewToUpdate, (int)kv.Value });
                                break;
                            }
                        case "System.Int64":
                            {
                                if (kv.Value == null)
                                {
                                    ReactProp rp = propAttributeSetters[kv.Key];
                                    method.Invoke(this, new object[] { viewToUpdate, rp.defaultLong });
                                    break;
                                }

                                method.Invoke(this, new object[] { viewToUpdate, Convert.ToInt64(kv.Value) });
                                break;
                            }
                        case "System.String":
                            {
                                if (kv.Value == null)
                                {
                                    ReactProp rp = propAttributeSetters[kv.Key];
                                    method.Invoke(this, new object[] { viewToUpdate, rp.defaultString });
                                    break;
                                }

                                method.Invoke(this, new object[] { viewToUpdate, (string)kv.Value });
                                break;
                            }
                        case "System.Boolean":
                            {
                                if (kv.Value == null)
                                {
                                    ReactProp rp = propAttributeSetters[kv.Key];
                                    method.Invoke(this, new object[] { viewToUpdate, rp.defaultBoolean });
                                    break;
                                }

                                method.Invoke(this, new object[] { viewToUpdate, (bool)kv.Value });
                                break;
                            }
                        case "System.Single":
                            {
                                if (kv.Value == null)
                                {
                                    ReactProp rp = propAttributeSetters[kv.Key];
                                    method.Invoke(this, new object[] { viewToUpdate, rp.defaultFloat });
                                    break;
                                }

                                method.Invoke(this, new object[] { viewToUpdate, Convert.ToSingle(kv.Value) });
                                break;
                            }
                        case "System.Double":
                            {
                                if (kv.Value == null)
                                {
                                    ReactProp rp = propAttributeSetters[kv.Key];
                                    method.Invoke(this, new object[] { viewToUpdate, rp.defaultDouble });
                                    break;
                                }

                                method.Invoke(this, new object[] { viewToUpdate, (double)kv.Value });
                                break;
                            }
                        case "System.Collections.Generic.Dictionary`2":
                            {
                                if (kv.Value == null)
                                {
                                    ReactProp rp = propAttributeSetters[kv.Key];
                                    method.Invoke(this, new object[] { viewToUpdate, rp.defaultMap });
                                    break;
                                }

                                method.Invoke(this, new object[] { viewToUpdate, (Dictionary<string, object>)kv.Value });
                                break;
                            }
                        case "System.Collections.ArrayList":
                            {
                                if (kv.Value == null)
                                {
                                    ReactProp rp = propAttributeSetters[kv.Key];
                                    method.Invoke(this, new object[] { viewToUpdate, rp.defaultArray });
                                    break;
                                }

                                method.Invoke(this, new object[] { viewToUpdate, (ArrayList)kv.Value });
                                break;
                            }
                        default:
                            {
                                method.Invoke(this, new object[] { viewToUpdate, kv.Value });
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                Util.Log("UpdateProperties error: {0} {1}", currentKey, e.Message);
                // 属性设置的异常，不影响全局的逻辑，故不把异常抛出
                // throw e;
            }
        }

        public void UpdateLayout(BaseView viewToUpdate, int x, int y, int width, int height)
        {
            viewToUpdate.SetLayout(x, y, width, height);

            // 如果存在border属性，传递w/h 到material
            var panel = viewToUpdate.GetGameObject();
            var image = panel.GetComponent<Image>();
            if (image != null && image.material.HasProperty(sBorderRadius))
            {
                image.material.SetFloat("_width", width);
                image.material.SetFloat("_height", height);
            }

            var rawImage = panel.GetComponent<RawImage>();
            if (rawImage != null && rawImage.material.HasProperty(sBorderRadius))
            {
                rawImage.material.SetFloat("_width", width);
                rawImage.material.SetFloat("_height", height);
            }
        }

        public void ReceiveCommand(BaseView view, string command, ArrayList args, Promise promise)
        {
            try
            {
                var methodInfo = commandSetters[command];
                args.Insert(0, view);

                if (promise != null)
                {
                    args.Add(promise);
                }
                methodInfo.Invoke(this, args.ToArray());
            }
            catch (Exception e)
            {
                Util.LogAndReport("View commnad: {0} exe error!", command);
            }
        }

        //TODO 改为由yoga计算结束，通过onLayout事件回调给JS
        [ReactCommand]
        public void getLayout(BaseView view, Promise promise)
        {
            StaticCommonScript.StaticStartCoroutine(GetLayoutIEnumerator(view, promise));
        }
        private static IEnumerator GetLayoutIEnumerator(BaseView view, Promise promise)
        {
            yield return null;
            var go = view.GetGameObject();
            var transform = go.GetComponent<RectTransform>();
            var rect = transform.rect;
            promise.Resolve(new ArrayList(){rect.x, rect.y, rect.width, rect.height});
        }
        
        /**
         * Creates a view and installs event emitters on it.
         */
        public BaseView CreateView()
        {
            BaseView view = createViewInstance();
            return view;
        }


        /**
        * Creates a view and installs event emitters on it.
        */
        protected abstract BaseView createViewInstance();

        public Dictionary<string, MethodInfo> GetPropSetters()
        {
            return propSetters;
        }

        public virtual Hashtable GetExportedCustomDirectEventTypeConstants()
        {
            return new Hashtable();
        }

        public virtual ReactSimpleShadowNode CreateShadowNodeInstance(int tag)
        {
            return new ReactViewLikeShadowNode(tag);
        }

        public ReactSimpleShadowNode CreateShadowNode(int tag)
        {
            return CreateShadowNodeInstance(tag);
        }
        
        public void Destroy()
        {
            //no-op
        }

        public Hashtable GetFlexProps()
        {
            return new Hashtable
            {
                {"marginTop", "mixed"},
                {"marginEnd", "mixed"},
                {"padding", "mixed"},
                {"height", "mixed"},
                {"flexWrap", "String"},
                {"paddingBottom", "mixed"},
                {"alignContent", "String"},
                {"borderRightWidth", "number"},
                {"left", "mixed"},
                {"alignItems", "String"},
                {"borderLeftWidth", "number"},
                {"marginHorizontal", "mixed"},
                {"minWidth", "mixed"},
                {"end", "mixed"},
                {"borderTopWidth", "number"},
                {"borderStartWidth", "number"},
                {"paddingHorizontal", "mixed"},
                {"marginVertical", "mixed"},
                {"marginRight", "mixed"},
                {"rotation", "number"},
                {"top", "mixed"},
                {"paddingStart", "mixed"},
                {"justifyContent", "String"},
                {"alignSelf", "String"},
                {"maxWidth", "mixed"},
                {"borderBottomWidth", "number"},
                {"flexShrink", "number"},
                {"scaleX", "number"},
                {"position", "String"},
                {"width", "mixed"},
                {"right", "mixed"},
                {"display", "String"},
                {"flexDirection", "String"},
                {"translateX", "number"},
                {"bottom", "mixed"},
                {"margin", "mixed"},
                {"scaleY", "number"},
                {"minHeight", "mixed"},
                {"paddingRight", "mixed"},
                {"start", "mixed"},
                {"flexBasis", "mixed"},
                {"marginStart", "mixed"},
                {"paddingTop", "mixed"},
                {"maxHeight", "mixed"},
                {"marginBottom", "mixed"},
                {"translateY", "number"},
                {"flex", "number"},
                {"borderWidth", "number"},
                {"paddingEnd", "mixed"},
                {"flexGrow", "number"},
                {"paddingLeft", "mixed"},
                {"marginLeft", "mixed"},
                {"borderEndWidth", "number"},
                {"paddingVertical", "mixed"},
            };
        }
    }
}
