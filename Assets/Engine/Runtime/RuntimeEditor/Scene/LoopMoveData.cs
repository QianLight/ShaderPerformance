#if ANIMATION_OBJECT_V0
#if UNITY_EDITOR
using System.IO;

namespace CFEngine
{
    [System.Serializable]
    public class LoopMoveData : SceneAnimationData
    {
        public LoopMoveAnim lma = new LoopMoveAnim ();
        public string startPoint;
        public string endPoint;
        public string animationGroupPath;
        public string animationMatGroupPath;
        // public List<string> matobjects = new List<string> ();
        public override void Save (BinaryWriter bw)
        {
            bw.Write (AnimationObject.AnimType_LoopMove);
            base.Save (bw);
            bw.Write (lma.moveSpeed);
            bw.Write (lma.angle);
            EditorCommon.WriteVector (bw, lma.start);
            EditorCommon.WriteVector (bw, lma.end);
            EditorCommon.WriteVector (bw, lma.uvMove);
        }
    }
}
#endif
#endif