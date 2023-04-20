#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CFEngine
{
    public interface IScquenceTarget
    {
        string CompType { get; }
        Component Comp { get; }
    }
    public class SequenceComponent : ISharedObject
    {
        public IScquenceTarget target;
        private List<SequenceSubComponent> subComponents = new List<SequenceSubComponent>();

        private const float COMP_HEIGHT = 21;
        private const float TEXT_WIDTH = 80;
        public void Reset()
        {

        }
        virtual public float defaultHeight
        {
            get { return COMP_HEIGHT + 2; }
        }
        protected void DoDefaultInfoGUI(Event e, ref Rect trackRect)
        {
            GUI.color = Color.white;

            var textInfoRect = Rect.MinMaxRect(2, 1, TEXT_WIDTH, COMP_HEIGHT);

            var nameString = string.Format("<size=11>{0}</size>", target.CompType);
            GUI.Label(textInfoRect, nameString);

            var oRect = Rect.MinMaxRect(TEXT_WIDTH + 1, 1, trackRect.width - 2, COMP_HEIGHT);
            UnityEditor.EditorGUI.ObjectField(oRect, target.Comp, typeof(Component), true);
        }


        virtual public void OnTrackInfoGUI(ref Rect trackRect)
        {
            var e = Event.current;
            DoDefaultInfoGUI(e, ref trackRect);
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
        }
    }

    public class SequenceSubComponent : ISharedObject
    {
        public void Reset()
        {

        }
    }

    public class Sequence
    {
        public string name = "";
        public List<SequenceComponent> components = new List<SequenceComponent>();
        public float duration = 10;
        private float viewTimeMin = 0;
        private float viewTimeMax = 20;
        private float currentTime = 0;
        public float Duration
        {
            get
            {
                return duration < 10 ? 10 : duration;
            }
        }

        public float ViewTimeMax
        {
            get
            {
                return viewTimeMax;
            }
            set
            {
                viewTimeMax = value;
            }
        }
        public float ViewTimeMin
        {
            get
            {
                return viewTimeMin;
            }
            set
            {
                viewTimeMin = Mathf.Max(0, value);
            }
        }
        public float CurrentTime
        {
            get { return currentTime; }
            set { currentTime = Mathf.Clamp(value, 0, duration); }
        }
        public virtual void Reset()
        {
            for (int i = 0; i < components.Count; ++i)
            {
                SharedObjectPool<SequenceComponent>.Release(components[i]);
            }
            components.Clear();
        }
    }
}
#endif