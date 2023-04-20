using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CFEngine;
using UnityEditor;
using UnityEngine;

public class SFXAnimationEventEditor : EditorWindow
{
    class AnimationEventItem
    {
        public AnimationEventItem(AnimationEvent animationEvent)
        {
            this.animationEvent = animationEvent;
        }

        public int selectedIndex = -1;
        public AnimationEvent animationEvent;
    }

    private static SFXAnimationEventEditor _ins;
    string partTag;
    Vector2 scrollPos;
    Animator sourceAnimator;
    AnimationClip currentClip;
    string[] arrayEventMethodName;
    List<MethodInfo> listEventMethod;
    List<AnimationEventItem> listAnimEventItem;

    private List<AnimationEventItem> stateList;
    private List<AnimationEventItem> maskList;
    private List<AnimationEventItem> doList;
    
    int selectedIndex;
    
    private string message;
    private MessageType messageType;

    private GUIStyle _title;
    private GUIStyle _subTitle;
    
    [MenuItem("Tools/特效/特效角色动画事件编辑器", false, 6)]
    static void SFXAnimationEventEditorShow()
    {
        _ins = EditorWindow.GetWindow(typeof(SFXAnimationEventEditor)) as SFXAnimationEventEditor;
        _ins.minSize = new Vector2(400, 600);
        _ins.Show();
    }

    private void OnEnable()
    {
        _title = new GUIStyle() { fontStyle = FontStyle.Bold, normal = new GUIStyleState() { textColor = Color.red }, fixedHeight = 0};
        _subTitle = new GUIStyle() { fontStyle = FontStyle.Bold, normal = new GUIStyleState() { textColor = new Color(0.8f,0.5f,0,1) }, fixedHeight = 0};
    }

    private void OnDisable()
    {
        currentClip = null;
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUIUtility.labelWidth = 100;
        EditorGUILayout.HelpBox(message, messageType);
        Animator tmpAnimator = EditorGUILayout.ObjectField("Animator Object", sourceAnimator, typeof(Animator), true) as Animator;
        if (tmpAnimator == null)
        {
            sourceAnimator = null;
            listEventMethod = null;
            listAnimEventItem = null;

            return;
        }

        partTag = EditorGUILayout.TextField("Part Tag", partTag);
        if (sourceAnimator != tmpAnimator)
        {
            sourceAnimator = tmpAnimator;
            sourceAnimator.TryGetComponent(out SFXAnimationEventManager manager);
            if (manager == null)
            {
                message = "特效没有挂载SFXPartMaskEvent";
                messageType = MessageType.Error;
                tmpAnimator = null;
                sourceAnimator = null;
                return;
            }
            
            listEventMethod = new List<MethodInfo>();
            List<string> tmpNames = new List<string>();
            Type type = manager.GetType();
            MethodInfo[] arrayMethodInfo = type.GetMethods();
            IEnumerable<MethodInfo> tmpInfos = arrayMethodInfo.Where
            (
                p =>
                    p.IsPublic &&
                    p.ReturnType == typeof(void) &&
                    (p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { }) ||
                     p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { typeof(int) }) ||
                     p.GetParameters().Select(q => q.ParameterType.BaseType).SequenceEqual(new Type[] { typeof(Enum) }) ||
                     p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { typeof(float) }) ||
                     p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { typeof(string) }) ||
                     p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { typeof(UnityEngine.Object) }) ||
                     p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[]{ typeof(AnimationEvent)}))
            );
            listEventMethod.AddRange(tmpInfos);
            foreach (MethodInfo info in tmpInfos)
            {
                ParameterInfo[] paramInfo = info.GetParameters();
                if (paramInfo.Length == 0)
                {
                    tmpNames.Add(type + "." + info.Name + " ( )");
                }
                else
                {
                    tmpNames.Add(type + "." + info.Name + " ( " + paramInfo[0].ParameterType + " )");
                }
            }
            arrayEventMethodName = tmpNames.ToArray();
        }

        using (new EditorGUILayout.HorizontalScope("box"))
        {
            List<string> listClipName = new List<string>();
            foreach (AnimationClip clip in sourceAnimator.runtimeAnimatorController.animationClips)
            {
                listClipName.Add(clip.name);
            }
            selectedIndex = EditorGUILayout.Popup(selectedIndex, listClipName.ToArray());


            AnimationClip tmpClip = sourceAnimator.runtimeAnimatorController.animationClips[selectedIndex];
            if (tmpClip == null)
            {
                return;
            }
            if (currentClip != tmpClip)
            {
                currentClip = tmpClip;

                Debug.Log("currentClip=" + currentClip);

                // create list for editor UI display
                RefreshAnimEvent();
            }

            if (GUILayout.Button("刷新"))
            {
                RefreshAnimEvent();
            }
        }
       
       
        
        decimal frameTime = (1.0m / new Decimal(currentClip.frameRate));
        EditorGUILayout.LabelField("FrameTime=" + frameTime);
        if (listAnimEventItem == null)
        {
            message = "特效Animation没有任何Animation Event";
            messageType = MessageType.Warning;
            return;
        }

        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
        {
            scrollPos = scrollView.scrollPosition;
            int currentFrameText = -1;
        
            foreach (AnimationEventItem item in listAnimEventItem)
            {
                AnimationEvent animEvent = item.animationEvent;

                //
                int frame = (int)Decimal.Round(new Decimal(animEvent.time) / frameTime);
                if (frame > currentFrameText)
                {
                    currentFrameText = frame;
                    EditorGUILayout.LabelField("Frame " + currentFrameText, _title);
                }

                //
                EditorGUI.indentLevel++;

                //
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (item.selectedIndex == -1)
                    {
                        item.selectedIndex = listEventMethod.FindIndex((MethodInfo x) => (x.Name == animEvent.functionName));
                    }
                    EditorGUILayout.PrefixLabel("functionName", GUIStyle.none);
                    item.selectedIndex = EditorGUILayout.Popup(item.selectedIndex, arrayEventMethodName);
                    if (item.selectedIndex == -1)
                    {
                        Debug.LogError("functionName=" + animEvent.functionName);
                        continue;
                    }
                    else
                    {
                        animEvent.functionName = listEventMethod[item.selectedIndex].Name;
                    }
                }

                if (arrayEventMethodName[item.selectedIndex].Contains("HideRolePart"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("设置部件隐藏", _subTitle);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        animEvent.time =
                            Decimal.ToSingle(new Decimal(EditorGUILayout.IntField("开始生效帧", frame)) * frameTime);

                        if (string.IsNullOrEmpty(partTag))
                        {
                            message = "请选择角色对应Part名,否则part设置可能无法显示";
                            messageType = MessageType.Warning;
                        }
                        else
                        {
                            message = " ";
                            messageType = MessageType.None;
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        int data = animEvent.intParameter;
                        var pi = PartConfig.instance.GetPartInfo(partTag);
                        if (pi != null)
                        {
                            EditorGUILayout.PrefixLabel("隐藏部件", GUIStyle.none);
                            var position = EditorGUILayout.GetControlRect(false, 16, EditorStyles.popup);
                            data = EditorCommon.DoMaskPopup(ref position, data,
                                pi.parts, pi.partsFlags);
                        }
                        // PartConfig.instance.OnPartGUI (partTag, ref data);
                        animEvent.intParameter = (int)data;
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        int duration = (int)Decimal.Round(new Decimal(animEvent.floatParameter) / frameTime);
                        EditorGUILayout.PrefixLabel("持续时间");
                        EditorGUIUtility.labelWidth = 50;
                        duration = EditorGUILayout.IntField(new GUIContent("(帧)"), duration);
                        EditorGUIUtility.labelWidth = 100;
                        animEvent.floatParameter = (float)(duration * frameTime);
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
            }
        }

        

        // EditorGUILayout.EndScrollView();
        
        if (GUILayout.Button("Save"))
        {
            SaveAnimation();
            AssetDatabase.SaveAssets();
            Debug.Log("Save: currentClip=" + currentClip);
            RefreshAnimEvent();
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(this, "修改角色在Animation的部件显隐");
        }
    }

    private void RefreshAnimEvent()
    {
        listAnimEventItem = new List<AnimationEventItem>();
        foreach (AnimationEvent animEvent in currentClip.events)
        {
            listAnimEventItem.Add(new AnimationEventItem(animEvent));
        }
    }

    void SaveAnimation()
    {
        if (currentClip != null && listAnimEventItem != null)
        {
            List<AnimationEvent> tmpList = new List<AnimationEvent>();
            foreach (AnimationEventItem item in listAnimEventItem)
            {
                tmpList.Add(item.animationEvent);
            }
            AnimationUtility.SetAnimationEvents(currentClip, tmpList.ToArray());
        }
    }

}
