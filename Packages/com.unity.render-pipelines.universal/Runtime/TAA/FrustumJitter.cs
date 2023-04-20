using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine.Rendering.Universal
{
    public class FrustumJitter
    {
        public Vector4 JitterSample = Vector4.zero;
        public Matrix4x4 PM,DefaultPM;
        public Matrix4x4 CurrV, PrevV;
        public Vector4 ProjectionExtents = Vector4.zero;
        public float PatternScale = 1.0f;

        #region source point data
        private float[] points_Still()
        {
            return new float[] {
         0.5f, 0.5f,
    };
        }
        private float[] points_Uniform2()
        {
            return new float[] {
        -0.25f, -0.25f,//ll
         0.25f,  0.25f,//ur
    };
        }

        private float[] points_Uniform4()
        {
            return new float[] {
        -0.25f, -0.25f,//ll
         0.25f, -0.25f,//lr
	     0.25f,  0.25f,//ur
        -0.25f,  0.25f,//ul
    };
        }

        private float[] points_Uniform4_Helix()
        {
            return new float[] {
        -0.25f, -0.25f,//ll  3  1
	     0.25f,  0.25f,//ur   \/|
         0.25f, -0.25f,//lr   /\|
        -0.25f,  0.25f,//ul  0  2
    };
        }

        private float[] points_Uniform4_DoubleHelix()
        {
            return new float[] {
        -0.25f, -0.25f,//ll  3  1
	     0.25f,  0.25f,//ur   \/|
         0.25f, -0.25f,//lr   /\|
        -0.25f,  0.25f,//ul  0  2
	    -0.25f, -0.25f,//ll  6--7
         0.25f, -0.25f,//lr   \
        -0.25f,  0.25f,//ul    \
	     0.25f,  0.25f,//ur  4--5
    };
        }
        private float[] points_SkewButterfly()
        {
            return new float[] {
        -0.250f, -0.250f,
         0.250f,  0.250f,
         0.125f, -0.125f,
        -0.125f,  0.125f,
    };
        }
        private float[] points_Rotated4()
        {
            return new float[] {
        -0.125f, -0.375f,//ll
         0.375f, -0.125f,//lr
	     0.125f,  0.375f,//ur
        -0.375f,  0.125f,//ul
    };
        }
        private float[] points_Rotated4_Helix()
        {
            return new float[] {
        -0.125f, -0.375f,//ll  3  1
	     0.125f,  0.375f,//ur   \/|
         0.375f, -0.125f,//lr   /\|
        -0.375f,  0.125f,//ul  0  2
    };
        }
        private float[] points_Rotated4_Helix2()
        {
            return new float[] {
        -0.125f, -0.375f,//ll  2--1
	     0.125f,  0.375f,//ur   \/
        -0.375f,  0.125f,//ul   /\
         0.375f, -0.125f,//lr  0  3
    };
        }
        private float[] points_Poisson10()
        {
            return new float[] {
        -0.16795960f*0.25f,  0.65544910f*0.25f,
        -0.69096030f*0.25f,  0.59015970f*0.25f,
         0.49843820f*0.25f,  0.83099720f*0.25f,
         0.17230150f*0.25f, -0.03882703f*0.25f,
        -0.60772670f*0.25f, -0.06013587f*0.25f,
         0.65606390f*0.25f,  0.24007600f*0.25f,
         0.80348370f*0.25f, -0.48096900f*0.25f,
         0.33436540f*0.25f, -0.73007030f*0.25f,
        -0.47839520f*0.25f, -0.56005300f*0.25f,
        -0.12388120f*0.25f, -0.96633990f*0.25f,
    };
        }
        private float[] points_Pentagram()
        {
            return new float[] {
         0.000000f*0.5f,  0.525731f*0.5f,// head
        -0.309017f*0.5f, -0.425325f*0.5f,// lleg
         0.500000f*0.5f,  0.162460f*0.5f,// rarm
        -0.500000f*0.5f,  0.162460f*0.5f,// larm
         0.309017f*0.5f, -0.425325f*0.5f,// rleg
    };
        }
        private float[] points_Halton_2_3_x8()
        {
            float[] result = new float[8 * 2];
            return result;
        }
        private float[] points_Halton_2_3_x16()
        {
            float[] result = new float[16 * 2];
            return result;
        }
        private float[] points_Halton_2_3_x32()
        {
            float[] result = new float[32 * 2];
            return result;
        }
        private float[] points_Halton_2_3_x256()
        {
            float[] result = new float[256 * 2];
            return result;
        }
        private float[] points_MotionPerp2()
        {
            return new float[] {
         0.00f, -0.25f,
         0.00f,  0.25f,
    };
        }

        private static void TransformPattern(float[] seq, float theta, float scale)
        {
            float cs = Mathf.Cos(theta);
            float sn = Mathf.Sin(theta);
            for (int i = 0, j = 1, n = seq.Length; i != n; i += 2, j += 2)
            {
                float x = scale * seq[i];
                float y = scale * seq[j];
                seq[i] = x * cs - y * sn;
                seq[j] = x * sn + y * cs;
            }
        }

        // http://en.wikipedia.org/wiki/Halton_sequence
        private static float HaltonSeq(int prime, int index = 1/* NOT! zero-based */)
        {
            float r = 0.0f;
            float f = 1.0f;
            int i = index;
            while (i > 0)
            {
                f /= prime;
                r += f * (i % prime);
                i = (int)Mathf.Floor(i / (float)prime);
            }
            return r;
        }

        private static void InitializeHalton_2_3(float[] seq)
        {
            for (int i = 0, n = seq.Length / 2; i != n; i++)
            {
                float u = HaltonSeq(2, i + 1) - 0.5f;
                float v = HaltonSeq(3, i + 1) - 0.5f;
                seq[2 * i + 0] = u;
                seq[2 * i + 1] = v;
            }
        }

        public FrustumJitter()
        {


        }
        #endregion

        #region Static point data accessors

        private Pattern lastPattern = Pattern.None;
        private float[] lastData;
        private Vector3 focalMotionPos = Vector3.zero;
        private Vector3 focalMotionDir = Vector3.right;
        private int activeIndex = -2;
        private Camera lastCamera;

        private float[] accessPointData(Pattern pattern)
        {
            if (lastPattern != pattern)
            {
                lastPattern = pattern;
                lastCamera = null;
                switch (pattern)
                {
                    case Pattern.Still:
                        {
                            lastData = points_Still();
                            break;
                        }
                    case Pattern.Uniform2:
                        {
                            lastData = points_Uniform2();
                            break;
                        }
                    case Pattern.Uniform4:
                        {
                            lastData = points_Uniform4();
                            break;
                        }
                    case Pattern.Uniform4_Helix:
                        {
                            lastData = points_Uniform4_Helix();
                            break;
                        }
                    case Pattern.Uniform4_DoubleHelix:
                        {
                            lastData = points_Uniform4_DoubleHelix();
                            break;
                        }
                    case Pattern.SkewButterfly:
                        {
                            lastData = points_SkewButterfly();
                            break;
                        }
                    case Pattern.Rotated4:
                        {
                            lastData = points_Rotated4();
                            break;
                        }
                    case Pattern.Rotated4_Helix:
                        {
                            lastData = points_Rotated4_Helix();
                            break;
                        }
                    case Pattern.Rotated4_Helix2:
                        {
                            lastData = points_Rotated4_Helix2();
                            break;
                        }
                    case Pattern.Poisson10:
                        {
                            lastData = points_Poisson10();
                            break;
                        }
                    case Pattern.Pentagram:
                        {
                            lastData = points_Pentagram();
                            // points_Pentagram
                            Vector2 vh = new Vector2(lastData[0] - lastData[2], lastData[1] - lastData[3]);
                            Vector2 vu = new Vector2(0.0f, 1.0f);
                            TransformPattern(lastData, Mathf.Deg2Rad * (0.5f * Vector2.Angle(vu, vh)), 1.0f);
                            break;
                        }
                    case Pattern.MotionPerp2:
                        {
                            lastData = points_MotionPerp2();
                            break;
                        }
                    case Pattern.Halton_2_3_X8:
                        {
                            lastData = points_Halton_2_3_x8();
                            InitializeHalton_2_3(lastData);
                            break;
                        }

                    case Pattern.Halton_2_3_X32:
                        {
                            lastData = points_Halton_2_3_x32();
                            InitializeHalton_2_3(lastData);
                            break;
                        }
                    case Pattern.Halton_2_3_X256:
                        {
                            lastData = points_Halton_2_3_x256();
                            InitializeHalton_2_3(lastData);
                            break;
                        }
                    default:
                        {
                            //Pattern.Halton_2_3_X16:
                            lastData = points_Halton_2_3_x16();
                            InitializeHalton_2_3(lastData);
                            break;
                        }
                }
            }
            else
            {
                if(pattern == Pattern.None)
                {
                    lastData = points_Halton_2_3_x16();
                    InitializeHalton_2_3(lastData);
                }
            }

            return lastData;
        }

        private int accessLength(Pattern pattern)
        {
            return accessPointData(pattern).Length / 2;
        }

        private Vector2 sampleData(Camera cam, Pattern pattern, int index)
        {
            float[] points = accessPointData(pattern);
            int n = points.Length / 2;
            int i = index % n;

            float x = PatternScale * points[2 * i + 0];
            float y = PatternScale * points[2 * i + 1];

            if (pattern != Pattern.MotionPerp2)
            {
                return new Vector2(x, y);
            }
            else
            {

                // update motion dir
                Vector3 oldWorld = focalMotionPos;
                Vector3 newWorld = cam.transform.TransformVector(cam.nearClipPlane * Vector3.forward);

                Vector3 oldPoint = (cam.worldToCameraMatrix * oldWorld);
                Vector3 newPoint = (cam.worldToCameraMatrix * newWorld);
                Vector3 newDelta = (newPoint - oldPoint).WithZ(0.0f);

                var mag = newDelta.magnitude;
                if (mag != 0.0f)
                {
                    // yes, apparently this is necessary instead of newDelta.normalized... because facepalm
                    var dir = newDelta / mag;
                    if (dir.sqrMagnitude != 0.0f)
                    {
                        focalMotionPos = newWorld;
                        focalMotionDir = Vector3.Slerp(focalMotionDir, dir, 0.2f);
                        //Debug.Log("CHANGE focalMotionDir " + focalMotionDir.ToString("G4") + " delta was " + newDelta.ToString("G4") + " delta.mag " + newDelta.magnitude);
                    }
                }
                return new Vector2(x, y).Rotate(Vector2.right.SignedAngle(focalMotionDir));
            }
        }
        #endregion

        private void reSet(Camera cam)
        {
            cam.ResetProjectionMatrix();

            JitterSample = Vector4.zero;
            activeIndex = -2;
        }

        public void Jitter(Camera cam, Pattern pattern, bool urpProcess = false)
        {
            if (lastCamera != cam)
            {
                if (cam == null) return;
                lastCamera = cam;
                reSet(lastCamera);
            }

            // xy = current sample, zw = previous sample
            if (activeIndex == -2)
            {
                JitterSample = Vector4.zero;
                activeIndex += 1;

                PM = cam.GetProjectionMatrix(out ProjectionExtents);

                if(!urpProcess)
                    cam.projectionMatrix = PM;

                CurrV = cam.worldToCameraMatrix;
                PrevV = CurrV;
            }
            else
            {
                activeIndex += 1;
                activeIndex %= accessLength(pattern);
                Vector2 sample = sampleData(cam, pattern, activeIndex);
                JitterSample.z = JitterSample.x;
                JitterSample.w = JitterSample.y;
                JitterSample.x = sample.x;
                JitterSample.y = sample.y;

                PM = cam.GetProjectionMatrix(sample.x, sample.y, out ProjectionExtents);

                if (!urpProcess)
                    cam.projectionMatrix = PM;

                PrevV = CurrV;
                CurrV = cam.worldToCameraMatrix;
            }
        }
    }

    public enum Pattern
    {
        None,
        Still,
        Uniform2,
        Uniform4,
        Uniform4_Helix,
        Uniform4_DoubleHelix,
        SkewButterfly,
        Rotated4,
        Rotated4_Helix,
        Rotated4_Helix2,
        Poisson10,
        Pentagram,
        Halton_2_3_X8,
        Halton_2_3_X16,
        Halton_2_3_X32,
        Halton_2_3_X256,
        MotionPerp2,
    };
}