using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XEditor
{
    public class HugeBoxEntity:MonoBehaviour
    {
        public static Color mNormalColor=new Color(1f, 1f, 0f, 1f);
        public static Color mSelectedColor=new Color(0f, 0f, 1f, 1f);
        public Color mCurrentColor=mNormalColor;

        public Vector3 mXYZ=Vector3.zero;
        public float mRadius=0;
        public float mHeight=0;

        public bool DrawInBox = false;
        private void OnDrawGizmos()
        {
            var parent = transform.parent;
            var scale = parent.localScale.x;
            var offset = parent.rotation * (mXYZ * scale);
            var radius = mRadius * scale;
            var height = mHeight * scale;
            if(DrawInBox)
                DrawBox(transform.position+offset,radius,height);
            else
                DrawCylinder(transform.position + offset, radius, height);
        }

        private void DrawBox(Vector3 center, float radius, float height)
        {
            Gizmos.color = mCurrentColor;

            Gizmos.DrawWireCube(center, new Vector3(radius, height/2, radius) * 2);
        }
        private void DrawCylinder(Vector3 center, float radius, float height, int lineNum = 60)
        {
            Gizmos.color = mCurrentColor;

            Vector3 bottomCenter = center + new Vector3(0, -height/2, 0);
            Vector3 topCenter = center + new Vector3(0, height/2, 0);

            Vector3 forwardLine = Vector3.forward * radius;
            Vector3 curPosBottom = bottomCenter + forwardLine;
            Vector3 prePosBottom = curPosBottom;
            Vector3 curPosTop = topCenter + forwardLine;
            Vector3 prePosTop = curPosTop;
            for (int i = 0; i < lineNum; i++)
            {
                forwardLine = radius * XCommon.singleton.HorizontalRotateVetor3(forwardLine, 360f / lineNum);

                curPosBottom = forwardLine + bottomCenter;
                Gizmos.DrawLine(prePosBottom, curPosBottom);
                prePosBottom = curPosBottom; 

                curPosTop = forwardLine + topCenter;
                Gizmos.DrawLine(prePosTop, curPosTop);
                prePosTop = curPosTop;

                Gizmos.DrawLine(curPosTop, curPosBottom);
                //Gizmos.DrawLine(prePosTop, curPosTop);
            }
        }
    }
}
