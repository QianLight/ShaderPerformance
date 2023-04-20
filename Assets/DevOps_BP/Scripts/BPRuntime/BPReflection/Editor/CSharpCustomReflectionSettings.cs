using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Blueprint.CSharpReflection
{
    public class CSharpCustomReflectionSettings
    {
        //自定义类
        public static BindType[] customClassList =
        {
            //Unity
            _GT(typeof(UnityEngine.GameObject)),
            _GT(typeof(UnityEngine.Component)),
            _GT(typeof(UnityEngine.Events.UnityEvent)),
            _GT(typeof(UnityEngine.Debug)),
            _GT(typeof(UnityEngine.Color)),
            _GT(typeof(UnityEngine.Time)),
            _GT(typeof(UnityEngine.Random)),
            _GT(typeof(UnityEngine.SceneManagement.SceneManager)),
            _GT(typeof(UnityEngine.SceneManagement.LoadSceneMode)),
            _GT(typeof(UnityEngine.Color)),
            _GT(typeof(UnityEngine.Transform)),
            _GT(typeof(UnityEngine.RectTransform)),
            _GT(typeof(UnityEngine.Vector2)),
            _GT(typeof(UnityEngine.Vector3)),
            _GT(typeof(UnityEngine.Quaternion)),
            _GT(typeof(BoxCollider)),
            _GT(typeof(CharacterController)),
            _GT(typeof(Animator)),
            _GT(typeof(LayerMask)),
            _GT(typeof(Physics)),
            _GT(typeof(PlayableDirector)),
            _GT(typeof(Mathf)),
            _GT(typeof(Camera)),
            _GT(typeof(Input)),
            _GT(typeof(KeyCode)),
            _GT(typeof(Resources)),
            _GT(typeof(Sprite)),
            _GT(typeof(EventTrigger)),
            _GT(typeof(EventTrigger.TriggerEvent)),
            _GT(typeof(EventTrigger.Entry)),
            _GT(typeof(EventTriggerType)),
            _GT(typeof(List<EventTrigger.Entry>)),
            _GT(typeof(BaseEventData)),
            _GT(typeof(UnityEngine.AI.NavMeshAgent)),
            _GT(typeof(UnityEngine.AI.NavMesh)),
            _GT(typeof(BaseEventData)),
            _GT(typeof(RectTransformUtility)),
            _GT(typeof(PointerEventData)),
            _GT(typeof(Application)),
            _GT(typeof(UnityEditor.EditorApplication)),
            _GT(typeof(UnityEngine.Rigidbody)),
            _GT(typeof(UnityEngine.Rigidbody2D)),
            _GT(typeof(BoxCollider2D)),
            _GT(typeof(CircleCollider2D)),
            _GT(typeof(SphereCollider)),
            _GT(typeof(CapsuleCollider)),
            _GT(typeof(ParticleSystem)),
            _GT(typeof(Space)),
            _GT(typeof(Rect)),
            _GT(typeof(SpriteRenderer)),
            _GT(typeof(Texture)),
            _GT(typeof(Texture2D)),
            _GT(typeof(Collider2D)),
            _GT(typeof(Collision2D)),
            _GT(typeof(AnimatorStateInfo)),
            _GT(typeof(Physics2D)),
            _GT(typeof(AsyncOperation)),
            _GT(typeof(Array)),
            _GT(typeof(Renderer)),
            _GT(typeof(Material)),
            _GT(typeof(Shader)),
            _GT(typeof(RigidbodyConstraints2D)),
            _GT(typeof(CapsuleCollider2D)),
            _GT(typeof(RigidbodyType2D)),
            _GT(typeof(DateTime)),
            _GT(typeof(DayOfWeek)),
            _GT(typeof(Convert)),
            _GT(typeof(Screen)),
            _GT(typeof(AnimationClip)),
            _GT(typeof(RuntimeAnimatorController)),
            _GT(typeof(TimeSpan)),
            _GT(typeof(ContactPoint)),
            _GT(typeof(LineRenderer)),
            _GT(typeof(GUIUtility)),
            _GT(typeof(Canvas)),
            _GT(typeof(Handheld)),
            _GT(typeof(ICanvasRaycastFilter)),

            // Unity UI
            _GT(typeof(UnityEngine.UI.Button)),
            _GT(typeof(UnityEngine.UI.InputField)),
            _GT(typeof(UnityEngine.UI.Toggle)),
            _GT(typeof(UnityEngine.UI.ToggleGroup)),
            _GT(typeof(UnityEngine.UI.Text)),
            _GT(typeof(UnityEngine.UI.Image)),
            _GT(typeof(UnityEngine.UI.Slider)),
            _GT(typeof(UnityEngine.UI.Slider.Direction)),
            _GT(typeof(UnityEngine.UI.Toggle.ToggleEvent)),
            _GT(typeof(UnityEngine.UI.GridLayoutGroup)),
            _GT(typeof(UnityEngine.UI.HorizontalLayoutGroup)),
            _GT(typeof(UnityEngine.UI.VerticalLayoutGroup)),
            _GT(typeof(UnityEngine.UI.LayoutRebuilder)),
            _GT(typeof(UnityEngine.UI.ScrollRect)),

            // Devops
            _GT(typeof(PTestNode)),
            _GT(typeof(Devops.Core.DevopsInfoSettings)),

            // Plugin
            _GT(typeof(BpPluginNode)),
            // OnePunch


        };

        //结构
        public static BindType[] customStructList =
        {
           
        };

        //枚举
        public static BindType[] customEnumList =
        {
            _GT(typeof(ScreenType)),
            _GT(typeof(KBEnum)),
        };
        
        //带子类
        public static BindType[] customTypeAutoSubclassInAssemblyCSharp =
        {
            
        };

        public static BindType _GT(Type t)
        {
            return new BindType(t);
        }
    }
}