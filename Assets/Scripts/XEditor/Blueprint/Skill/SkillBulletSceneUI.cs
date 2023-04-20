#if UNITY_EDITOR
using System.Collections.Generic;
using CFUtilPoolLib;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VirtualSkill
{
    public class SkillBulletSceneUI : MonoBehaviour
    {
        [SerializeField]
        public List<EcsData.XBulletData> dataList = new List<EcsData.XBulletData>();

        public List<ulong> idList = new List<ulong>();
        public List<Transform> transList = new List<Transform>();
        public List<Vector3> posList = new List<Vector3>();
        public List<float> createTime = new List<float>();

        public List<List<GameObject>> AttackRangeObjList = new List<List<GameObject>>();
        public List<bool> initList = new List<bool>();

        public void Init(int index, bool del = false)
        {
            initList[index] = false;

            for (int i = 0; i < AttackRangeObjList[index].Count; ++i)
            {
                ReleaseObj(AttackRangeObjList[index][i]);
            }
            AttackRangeObjList[index].Clear();

            if(del)
            {
                idList.RemoveAt(index);
                dataList.RemoveAt(index);
                transList.RemoveAt(index);
                createTime.RemoveAt(index);
                posList.RemoveAt(index);
                AttackRangeObjList.RemoveAt(index);
                initList.RemoveAt(index);
            }
        }

        private void Update()
        {
            for (int index = dataList.Count - 1; index >= 0; --index)
            {
                EcsData.XBulletData data = dataList[index];
                Transform trans = transList[index];
                Vector3 pos = posList[index];
                List<GameObject> AttackRangeObj = AttackRangeObjList[index];

                if (SkillHoster.hideSceneUI)
                {
                    Init(index);
                    continue;
                }

                {
                    if (data == null || trans.position == CFEngine.EngineUtility.Far_Far_Away)
                    {
                        Init(index, true);
                        continue;
                    }
                    Vector3 m = trans.forward;

                    if (!initList[index])
                    {
                        initList[index] = true;

                        switch ((XBulletType)data.Type)
                        {
                            case XBulletType.Ring:
                                {
                                    GameObject go = GetObj("Blue");
                                    AttackRangeObj.Add(go);
                                    go = GetObj("Black");
                                    AttackRangeObj.Add(go);
                                }
                                break;
                            case XBulletType.Cuboid:
                                {
                                    GameObject go = GetObj("Cube");
                                    AttackRangeObj.Add(go);
                                }
                                break;
                            default:
                                {
                                    GameObject go = GetObj("Blue");
                                    AttackRangeObj.Add(go);
                                }
                                break;
                        }
                    }

                    if ((XBulletType)data.Type == XBulletType.Ring)
                    {
                        for (int i = 0; i < AttackRangeObj.Count; ++i)
                        {
                            AttackRangeObj[i].transform.position = pos;

                            float dis = Mathf.Max(0, data.InitDis + (Time.time - createTime[index]) * data.Velocity);

                            switch (AttackRangeObj[i].name)
                            {
                                case "Blue":
                                    AttackRangeObj[i].transform.localScale = new Vector3(2 * (data.Radius + dis),
                                        data.HalfHeight, 2 * (data.Radius + dis));
                                    break;
                                case "Black":
                                    AttackRangeObj[i].transform.localScale = Vector3.zero;
                                    if (dis > data.Radius)
                                    {
                                        AttackRangeObj[i].transform.localScale = new Vector3(2 * (dis - data.Radius),
                                            data.HalfHeight, 2 * (dis - data.Radius));
                                    }
                                    break;
                            }
                        }
                    }
                    else if ((XBulletType)data.Type == XBulletType.Cuboid)
                    {
                        for (int i = 0; i < AttackRangeObj.Count; ++i)
                        {
                            if (data.CurveDataIndex == -1) AttackRangeObj[i].transform.position = trans.position;

                            switch (AttackRangeObj[i].name)
                            {
                                case "Cube":
                                    AttackRangeObj[i].transform.localScale = new Vector3(2 * data.Radius, 2 * data.HalfHeight, 2 * data.HalfLength);
                                    AttackRangeObj[i].transform.localRotation = trans.localRotation;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < AttackRangeObj.Count; ++i)
                        {
                            if (data.CurveDataIndex == -1) AttackRangeObj[i].transform.position = trans.position;

                            switch (AttackRangeObj[i].name)
                            {
                                case "Blue":
                                    AttackRangeObj[i].transform.localScale = new Vector3(2 * data.Radius, data.HalfHeight, 2 * data.Radius);
                                    break;
                            }
                        }
                    }
                }
            }

        }

        private static GameObject root = null;
        public static GameObject Root
        {
            get
            {
                if (root == null)
                {
                    root = CFEngine.XGameObject.GetGameObject();
                    root.transform.position = Vector3.zero;
                    root.transform.localScale = Vector3.one;
                    root.transform.rotation = Quaternion.identity;
                }
                return root;
            }
        }
        private static Queue<GameObject> BlueCylinderPool = new Queue<GameObject>();
        private static Queue<GameObject> BlackCylinderPool = new Queue<GameObject>();
        private static Queue<GameObject> CubePool = new Queue<GameObject>();
        private static GameObject _cylinder = null;
        private static GameObject Cylinder
        {
            get
            {
                if (_cylinder == null)
                {
                    _cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    _cylinder.transform.position = new Vector3(-10000, -10000, -10000);
                }
                return _cylinder;
            }
        }
        private static GameObject _cube = null;
        private static GameObject Cube
        {
            get
            {
                if (_cube == null)
                {
                    _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    _cube.transform.position = new Vector3(-10000, -10000, -10000);
                }
                return _cube;
            }
        }

        private GameObject GetObj(string str)
        {
            switch (str)
            {
                case "Black": return GetBlackCylinder();
                case "Blue": return GetBlueCylinder();
                case "Cube": return GetCube();
            }
            return null;
        }

        private GameObject GetBlueCylinder()
        {
            if (BlueCylinderPool.Count > 0) return BlueCylinderPool.Dequeue();
            GameObject go = GameObject.Instantiate(Cylinder);
            go.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Blueprint/SkillEditor/Materials/Blue.mat");
            go.name = "Blue";
            go.layer = 31;
            go.transform.parent = Root.transform;
            return go;
        }

        private GameObject GetBlackCylinder()
        {
            if (BlackCylinderPool.Count > 0) return BlackCylinderPool.Dequeue();
            GameObject go = GameObject.Instantiate(Cylinder);
            go.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Blueprint/SkillEditor/Materials/Black.mat");
            go.name = "Black";
            go.layer = 31;
            go.transform.parent = Root.transform;
            return go;
        }

        private GameObject GetCube()
        {
            if (CubePool.Count > 0) return CubePool.Dequeue();
            GameObject go = GameObject.Instantiate(Cube);
            go.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/Blueprint/SkillEditor/Materials/Blue.mat");
            go.name = "Cube";
            go.layer = 31;
            go.transform.parent = Root.transform;
            return go;
        }

        private void ReleaseObj(GameObject go)
        {
            switch (go.name)
            {
                case "Blue":
                    BlueCylinderPool.Enqueue(go);
                    break;
                case "Black":
                    BlackCylinderPool.Enqueue(go);
                    break;
                case "Cube":
                    CubePool.Enqueue(go);
                    break;
            }

            go.transform.position = new Vector3(-10000, -10000, -10000);
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.zero;
            go.layer = 31;
        }
    }
}
#endif