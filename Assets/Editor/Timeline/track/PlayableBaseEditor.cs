using UnityEngine;
using UnityEngine.Playables;

namespace XEditor
{

    public abstract class PlayableBaseEditor
    {
        /// <summary>
        /// Track 序列号
        /// </summary>
        protected int trackIndex;

        /// <summary>
        /// parent editor
        /// </summary>
        public PlayableDirectorInspector editor;

        /// <summary>
        /// Timeline 内部创建GameObject使用的tag
        /// </summary>
        protected static string tag { get { return "Timeline"; } }

        /// <summary>
        /// 基类构造函数
        /// </summary>
        public PlayableBaseEditor (int indx, PlayableDirectorInspector parent)
        {
            trackIndex = indx;
            parent = editor;
        }

        /// <summary>
        /// Revert 操作
        /// </summary>
        public abstract void Reset ();

        /// <summary>
        /// GUI 绘制函数
        /// </summary>
        public abstract void OnInspectorGUI (PlayableBinding pb);

        /// <summary>
        /// 加载配置到timelime编辑器
        /// </summary>
        public virtual void OnLoad (PlayableBinding pb) { }

        // /// <summary>
        // /// 解耦相关的资源。
        // /// 主要用来热更新，走配置 
        // /// </summary>
        // public virtual void UnloadRef (PlayableBinding pb) { }

        /// <summary>
        /// 内部通用数组merge操作
        /// </summary>
        /// <returns></returns>
        protected T[] MergeFrom<T> (T[] orig, int cnt)
        {
            if (orig == null)
            {
                return new T[cnt];
            }
            else
            {
                T[] ss = new T[cnt];
                for (int i = 0; i < Mathf.Min (cnt, orig.Length); i++)
                {
                    ss[i] = orig[i];
                }
                return ss;
            }
        }

        protected T[] RemvFrom<T> (T[] ori, int indx)
        {
            if (ori != null && indx < ori.Length)
            {
                T[] nt = new T[ori.Length - 1];
                for (int i = 0; i < indx; i++)
                {
                    nt[i] = ori[i];
                }
                for (int i = indx + 1; i < ori.Length; i++)
                {
                    nt[i - 1] = ori[i];
                }
                return nt;
            }
            return ori;
        }

        public uint TransformIndex (Transform tf)
        {
            if (tf.parent)
            {
                int cnt = tf.parent.childCount;
                for (uint i = 0; i < cnt; i++)
                {
                    if (tf.parent.GetChild ((int) i) == tf)
                    {
                        return i;
                    }
                }
            }
            return 0;
        }

    }

}