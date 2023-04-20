using System.Collections.Generic;
using UnityEngine;

namespace XEditor
{
    public class DynamicBoneSave : MonoBehaviour
    {
        public static string Save(GameObject go, GameObject save)
        {
            for (int i = save.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(save.transform.GetChild(i).gameObject);

            //save
            //deal with CFDynamicBone
            Dictionary<string, GameObject> dict = new Dictionary<string, GameObject>();
            HashSet<CFDynamicBoneColliderBase> dealwithedDBCs = new HashSet<CFDynamicBoneColliderBase>();
            CFDynamicBone[] oldDbs = go.GetComponents<CFDynamicBone>();
            if (oldDbs == null || oldDbs.Length == 0)
                return "Have not CFDynamicBone Script";
            foreach (CFDynamicBone oldDb in oldDbs)
            {
                if (oldDb.root == null)
                    return "Some CFDynamicBone m_Root is null";
                GameObject dbGo = QueryGameObject(dict, oldDb.root.name, save);
                CFDynamicBone dbComp = CopyCompontent<CFDynamicBone>(oldDb, save);
                dbComp.root = dbGo.transform;
                dbComp.colliders.Clear();
                //deal with DynamicBoneColliders in CFDynamicBone
                for (int i = 0; i < oldDb.colliders.Count; i++)
                {
                    if (oldDb.colliders[i] == null)
                        return "Some CFDynamicBone m_Colliders has null";
                    GameObject dbcGo = QueryGameObject(dict, oldDb.colliders[i].name, save);
                    CFDynamicBoneColliderBase dbcComp = CopyCompontent<CFDynamicBoneColliderBase>(oldDb.colliders[i], dbcGo);
                    dbComp.colliders.Add(dbcComp);
                    dealwithedDBCs.Add(oldDb.colliders[i]);
                }
            }

            //check other DynamicBoneCollider
            CFDynamicBoneColliderBase[] dbcs = go.GetComponentsInChildren<CFDynamicBoneColliderBase>(true);
            foreach(CFDynamicBoneColliderBase dbc in dbcs)
            {
                if (dealwithedDBCs.Contains(dbc))
                    continue;
                return "DynamicBoneCollider " + dbc.name + " have not be use.";
            }

            return "Save CFDynamicBone Script success.";
        }

        public static string Load(GameObject go, GameObject load)
        {
            //destroy old comp
            CFDynamicBone[] dbs = go.GetComponentsInChildren<CFDynamicBone>();
            foreach (CFDynamicBone db in dbs)
                DestroyImmediate(db);
            CFDynamicBoneColliderBase[] dbcs = go.GetComponentsInChildren<CFDynamicBoneColliderBase>();
            foreach (CFDynamicBoneColliderBase dbc in dbcs)
                DestroyImmediate(dbc);

            //create dict
            Dictionary<string, Transform> dict = new Dictionary<string, Transform>();
            CreateDict(go.transform, dict);

            //copy CFDynamicBoneColliderBase
            dbcs = load.GetComponentsInChildren<CFDynamicBoneColliderBase>();
            foreach (CFDynamicBoneColliderBase dbc in dbcs)
            {
                if (!dict.ContainsKey(dbc.name))
                {
                    ClearDynamicBoneInfo(go);
                    return "新prefab上无法找到保存的DynamicBoneCollider所在节点: " + dbc.name;
                }
                CopyCompontent<CFDynamicBoneColliderBase>(dbc, dict[dbc.name].gameObject);
            }

            //copy CFDynamicBone
            dbs = load.GetComponentsInChildren<CFDynamicBone>();
            foreach(CFDynamicBone db in dbs)
            {
                CFDynamicBone newDb = CopyCompontent<CFDynamicBone>(db, go);
                if (!dict.ContainsKey(db.root.name))
                {
                    ClearDynamicBoneInfo(go);
                    return "新prefab上无法找到保存的m_Root节点: " + db.root.name;
                }
                newDb.root = dict[db.root.name];
                newDb.colliders.Clear();
                for(int i = 0; i < db.colliders.Count; i++)
                {
                    if (!dict.ContainsKey(db.colliders[i].name))
                    {
                        ClearDynamicBoneInfo(go);
                        return "新prefab上无法找到保存的m_Colliders节点: " + db.colliders[i].name;
                    }
                    CFDynamicBoneColliderBase newDbc = dict[db.colliders[i].name].GetComponent<CFDynamicBoneColliderBase>();
                    if (newDbc == null)
                    {
                        ClearDynamicBoneInfo(go);
                        return "新prefab的m_Colliders没有DynamicBoneCollider脚本，请联系pyc";
                    }
                    newDb.colliders.Add(newDbc);
                }
            }

            return "Load CFDynamicBone Script success.";
        }

        private static void ClearDynamicBoneInfo(GameObject go)
        {
            CFDynamicBone[] dbs = go.GetComponentsInChildren<CFDynamicBone>();
            foreach (CFDynamicBone db in dbs)
                DestroyImmediate(db);
            CFDynamicBoneColliderBase[] dbcs = go.GetComponentsInChildren<CFDynamicBoneColliderBase>();
            foreach (CFDynamicBoneColliderBase dbc in dbcs)
                DestroyImmediate(dbc);
        }


        private static GameObject QueryGameObject(Dictionary<string, GameObject> dict, string name, GameObject root)
        {
            if (dict.ContainsKey(name))
                return dict[name];
            else
            {
                GameObject go = new GameObject(name);
                go.transform.parent = root.transform;
                dict[name] = go;
                return go;
            }
        }

        private static T CopyCompontent<T>(T oldComp, GameObject newGo) where T : Component
        {
            if (oldComp != null)
            {
                T newComp = newGo.AddComponent<T>();
                UnityEditorInternal.ComponentUtility.CopyComponent(oldComp);
                UnityEditorInternal.ComponentUtility.PasteComponentValues(newComp);
                return newComp;
            }
            return null;
        }

        private static void CreateDict(Transform ts, Dictionary<string, Transform> dict)
        {
            if (dict.ContainsKey(ts.name))
            {
                Debug.LogError("Prefab has the same bone node : " + ts.name);
            }
            else
            {
                dict.Add(ts.name, ts);
            }
            for (int i = 0; i < ts.childCount; i++)
            {
                CreateDict(ts.GetChild(i), dict);
            }
        }
    }
}