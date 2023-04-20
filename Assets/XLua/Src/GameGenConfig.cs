/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using System.Net;
using UnityEngine;
using XLua;

public static class GameGenConfig
{
    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>() {
                // system
                typeof(System.Object),
                typeof(System.IO.MemoryStream),

                // unityengine
                typeof(UnityEngine.Object),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Quaternion),
                typeof(Color),
                typeof(Ray),
                typeof(Bounds),
                // typeof(Ray2D),
                typeof(Time),
                typeof(GameObject),
                typeof(Behaviour),
                typeof(Transform),
                typeof(TextAsset),
                typeof(Keyframe),
                typeof(AnimationCurve),
                typeof(AnimationClip),
                typeof(MonoBehaviour),
                typeof(ParticleSystem),
                // typeof(SkinnedMeshRenderer),
                // typeof(Renderer),
                typeof(Animator),
                // typeof(CapsuleCollider),
                // typeof(Light),
                typeof(WebRequest),
                typeof(Mathf),
                typeof(System.Collections.Generic.List<int>),
                typeof(Action<string>),
                typeof(UnityEngine.Debug),
                typeof(UnityEngine.Application),
                typeof(UnityEngine.AnimatorStateInfo),
                // cfui
                // typeof(HUDMesh),
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(UnityEngine.Events.UnityEvent<Vector2>),
                typeof(UnityEngine.CFUI.CFImage),
                typeof(UnityEngine.CFUI.CFText),
                typeof(UnityEngine.CFUI.CFInput),
                typeof(UnityEngine.CFUI.CFRawImage),
                typeof(UnityEngine.CFUI.Empty4Raycast),
                typeof(UnityEngine.CFUI.CFToggle),
                typeof(UnityEngine.CFUI.CFButton),
                typeof(UnityEngine.CFUI.CFSlider),
                typeof(UnityEngine.CFUI.Empty4DragRaycast),
                typeof(UnityEngine.CFEventSystems.UIBehaviour),
                typeof(UnityEngine.CFEventSystems.CFEventSystem),
                typeof(UnityEngine.CFUI.XAnimation),
                typeof(UnityEngine.CFUI.XAnimationGroup),
                typeof(UnityEngine.CFUI.XTextGroup),
                typeof(UnityEngine.CFUI.XDisplayContext),
                typeof(UnityEngine.CFUI.UIUtils),
                typeof(UnityEngine.CFUI.CFToggleGroup),
                typeof(UnityEngine.CFUI.CFSequenceFrameEx),
                typeof(UnityEngine.CFUI.LongPressButton),
               typeof(CFUtilPoolLib.SeqListRef<uint>),
               typeof(CFUtilPoolLib.SeqListRef<float>),
               typeof(CFUtilPoolLib.SeqRef<float>),
               typeof(CFUtilPoolLib.SeqRef<uint>),
               typeof(CFUtilPoolLib.XTimerMgrExtension),
               typeof(CFUtilPoolLib.XCommon),
               typeof(CFUtilPoolLib.LuaRpcRespond),

               // cfengine
               typeof(UnityEngine.CFUI.CFWrapContent),
               typeof(UnityEngine.CFUI.CFWrapContentWithSkin),
               typeof(UnityEngine.CFUI.CFLoopScrollRectControl),
               typeof(UnityEngine.CFUI.CFScrollRect),
               typeof(UnityEngine.CFUI.CFLoopScrollRect),
               typeof(UnityEngine.CFUI.CFSimpleLoopScrollRect),
               typeof(UnityEngine.CFUI.XSRControlNormal),
               typeof(UnityEngine.CFUI.CFLoopInsideScrollRect),
               typeof(UnityEngine.CFUI.CFAnimUnit),
               typeof(UnityEngine.CFUI.CFAnimation),
               typeof(UnityEngine.CFUI.XAnimation),
               typeof(UnityEngine.CFUI.XAnimationGroup),
               typeof(UnityEngine.CFUI.XAnimUnit),
               typeof(UnityEngine.CFUI.CFUIDummy),
               typeof(UnityEngine.CFUI.CFUIPool),
               typeof(UnityEngine.CFUI.XUIHeader),
               typeof(UnityEngine.CFUI.XSfxGroupPlay),
            //    typeof(CFEngine.LoadMgr),
               typeof(CFEngine.EngineUtility),
               typeof(UnityEngine.CFUI.CFLayoutRebuilder),
                typeof(CFClient.UI.CFTitanHandler),
                typeof(CFClient.UI.XItemCommControl),
                typeof(CFClient.UI.XTabElementBase),
                typeof(CFClient.UI.XCommonTabSystemControl),
                typeof(CFClient.UI.XCommonTabCommonControl),
                typeof(CFClient.UI.XCopyCenterTabControl),
                typeof(CFClient.UI.XFleetBossCenterTabControl),
                typeof(CFClient.UI.XUITools),  
                typeof(CFClient.UI.XSystemHelper),       
                typeof(CFClient.XStringDefineProxy),
                typeof(CFClient.XGlobalConfigExtension),
                // typeof(CFClient.GlobalTableName),
                typeof(CFClient.UI.XItemHelper),
                typeof(CFClient.UI.XSkinParam),
                typeof(CFClient.UI.ItemTipParam)

    };

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>() {
                typeof(Action),
                typeof(Action<bool>),
                typeof(Action<float>),
                typeof(Action<uint>),
                typeof(Action<float,float>),  
                typeof(Action<float,int>),
                typeof(Action<string>),
                typeof(Action<double>),
                typeof(Action<object>),
                typeof(Action<ulong>),
                typeof(Action<Transform,int>),
                typeof(Action<Transform,object>),
                typeof(Action<ulong,uint, uint>),
                typeof(Action<uint,uint>),
                typeof(Func<double, double, double>),
                typeof(Func<bool>),
				typeof(Func<uint,bool>),
                typeof(Func<uint,uint>),
                typeof(Func<uint,float>),
                typeof(Func<int,float>),
                typeof(UnityEngine.Events.UnityAction),
                typeof(UnityEngine.Events.UnityAction<bool>),
                typeof(UnityEngine.Events.UnityAction<object>),
                typeof(UnityEngine.Events.UnityAction<int>),
                typeof(System.Collections.IEnumerator),
            };

    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"CFEngine.SFX", "SetCreator","System.String","System.String"},
                new List<string>(){"CFEngine.SFX", "SetCreator","System.String","uint"},
                new List<string>(){"CFEngine.SFX", "OnDrawGizmo"},
                new List<string>(){"UnityEngine.CFUI.CFText", "OnRebuildRequested"},
                new List<string>(){"UnityEngine.CFUI.Empty4DragRaycast", "test"},
                new List<string>(){"UnityEngine.CFUI.Empty4Raycast", "test"},
                new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
                new List<string>(){"UnityEngine.WWW", "movie"},
    #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
    #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.WWW", "MovieTexture"},
                new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
    };
}
