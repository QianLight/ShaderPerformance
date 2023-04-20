#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CFClient;
using ICSharpCode.NRefactory.Ast;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Zeus.Framework;
using AsyncOperation = System.ComponentModel.AsyncOperation;
using Object = UnityEngine.Object;
using Task = System.Threading.Tasks.Task;

namespace CFEngine.Editor
{
    public static class RoleEditorComponetContext
    {
        public static List<BandposeData> bandposeDatas = new List<BandposeData>();
        public static bool bandposeListInited;
        public static string label = "";
        public class CoroutineBehaviour : MonoBehaviour { }
        public static CoroutineBehaviour coroutineBehaviour;
        public static void Clear()
        {
            bandposeListInited = false;
            bandposeDatas.Clear();
        }
    }
    public class RoleEditorComponet<T> : EditorComponet<T> 
        where T: EditorComponet<T>, new()
    {
        public override void Init() { }

        public override void DrawGUI() { }

        public override void Destroy() { }
        public override string Name() { return "RoleEditorComponet"; }
        
        public void BandposeDataField(BandposeData data)
        {
            if(GUILayout.Button(data.name,EditorStyles.objectField))
                PropertyEditor.OpenPropertyEditor(data);
        }
        
        public void GetBandposeData()
        {
            if (RoleEditorComponetContext.bandposeListInited == true)
                return;
            
            _editorWindow.StartCoroutine(AsyncGetBandposeData());
        }

        IEnumerator AsyncGetBandposeData()
        {
            int process = 0;
            string[] paths = AssetDatabase.FindAssets(string.Format("t:{0}",nameof(BandposeData)));
            yield return 0;
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
                // Debug.Log(paths[i]);
                RoleEditorComponetContext.label = paths[i];
            }

            foreach (var path in paths)
            {
                BandposeData data = AssetDatabase.LoadAssetAtPath<BandposeData>(path);
                if (data)
                {
                    RoleEditorComponetContext.bandposeDatas.Add(data);
                    RoleEditorComponetContext.label = data.name;
                    process++;
                    if (process % 4 == 0)
                        yield return 0;
                }
            }
            RoleEditorComponetContext.bandposeListInited = true;
            RoleEditorComponetContext.label = "";
        }
    }
    public class BandposeStatistics :RoleEditorComponet<BandposeStatistics>
    {
        private SearchField _searcher = new SearchField();
        private string _searchKeyword = "";
        private Vector2 _scrollPosition;
        public override void Init()
        {
        }

        public override void DrawGUI()
        {
            if (!RoleEditorComponetContext.bandposeListInited)
            {
                if (GUILayout.Button("Find All"))
                {
                    GetBandposeData();
                }
                GUILayout.Label(RoleEditorComponetContext.label);
                _editorWindow.Repaint();
                return;
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Search");
                _searchKeyword = _searcher.OnGUI(_searchKeyword);
            }
            GUILayout.EndHorizontal();

            int count = 0;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,GUILayout.Width(0),GUILayout.Height(0));
            if (_searchKeyword.Equals(""))
            {
                count = 0;
                foreach (var bandposeData in RoleEditorComponetContext.bandposeDatas)
                {
                    BandposeDataField(bandposeData);
                    // EditorGUILayout.ObjectField(bandposeData, typeof(BandposeData));
                    count++;
                }
            }
            else
            {
                count = 0;
                foreach (var bandposeData in RoleEditorComponetContext.bandposeDatas)
                {
                    if (bandposeData.name.ToLower().Contains(_searchKeyword.ToLower()))
                    {
                        BandposeDataField(bandposeData);
                        // EditorGUILayout.ObjectField(bandposeData, typeof(BandposeData));
                        count++;
                    }
                }
            }
            GUILayout.EndScrollView();
            
            GUILayout.FlexibleSpace();
            GUILayout.Label("Bandpose Count: "+count);

        }

        public override void Destroy()
        {
            
        }

        public override string Name()
        {
            return "Bandpose Statistics";
        }
    }
    
    public class RoleStatistics :RoleEditorComponet<RoleStatistics>
    {
        private List<string> _roleNames = new List<string>(){};
        private List<string> _roleHasLod = new List<string>(){};
        private List<string> _roleNamesEnCH = new List<string>(){};
        private Dictionary<string, List<BandposeData>> _bandposeDic = new Dictionary<string, List<BandposeData>>();
        private List<BandposeData> _lodDatas = new List<BandposeData>();
        private List<List<string>> _prefabNames = new List<List<string>>();
        private int _currentRoleName;
        private Vector2 _namesScrollPosition;
        private Vector2 _infoScrollPosition;
        private SearchField _searcher = new SearchField();
        private string _searchKeyword = "";
        
        private bool inited;
        public override void Init()
        {
            
        }

        public void GetRoleName()
        {
            uint[] ids;
            string[] prefabs;
            XEntityPresentationReader.GetAllEntities(out ids, out prefabs);
            foreach (var id in ids)
            {
                var name = XEntityPresentationReader.GetAnimLocationByPresentId(id);
                if (name != String.Empty)
                {
                    name = name.Replace("Role_", "");
                    name = name.Replace("Monster_", "");
                    name = name.Replace("/", "");
                    name = name.ToUpperInvariant();
                    if (!_roleNames.Contains(name))
                    {
                        _roleNames.Add(name);
                        var nameCH = XEntityPresentationReader.GetData(id).Name;
                        _roleNamesEnCH.Add(name+" "+nameCH);
                    }
                }
            }
        }
        public void CategorizeBandposeByRoleName()
        {
            foreach (var bandposeData in RoleEditorComponetContext.bandposeDatas)
            {
                foreach (var roleName in _roleNames)
                {
                    var dataName = "_" +  bandposeData.name.ToLower() + "_";
                    var roleNameCompare = "_" + roleName.ToLower() + "_";
                    if (dataName.Contains(roleNameCompare))
                    {
                        if (!_bandposeDic.ContainsKey(roleName))
                        {
                            _bandposeDic.Add(roleName,new List<BandposeData>());
                        }
                        _bandposeDic[roleName].Add(bandposeData);
                    }
                }
            }
        }
        public override void DrawGUI()
        {
            if (!RoleEditorComponetContext.bandposeListInited)
            {
                if (GUILayout.Button("Find All"))
                {
                    GetBandposeData();
                }
                GUILayout.Label(RoleEditorComponetContext.label);
                
                _editorWindow.Repaint();
                return;
            }
            
            if (!inited)
            {
                Check();
            }

            GUILayout.BeginHorizontal();
            {
                int column_0 = 280;
                int column_1 = 60;
                GUILayout.BeginVertical(GUILayout.Width(column_0+column_1+25));
                {
                    DrawSearcher();
                    if (_searchKeyword.Equals(""))
                        DrawRoleList();
                }
                GUILayout.EndVertical();
                GUILayout.Space(5);

                DrawRoleInfo();
            }
            GUILayout.EndHorizontal();
        }

        public void Check()
        {
            GetRoleName();
            CategorizeBandposeByRoleName();
            MarkItemhasLod();
            inited = true;
        }

        void DrawSearcher()
        {
            GUILayout.BeginHorizontal(GUILayout.Width(365));
            {
                GUILayout.Label("Search");
                _searchKeyword = _searcher.OnGUI(_searchKeyword);
            }
            GUILayout.EndHorizontal();

            if (!_searchKeyword.Equals(""))
            {
                _namesScrollPosition = GUILayout.BeginScrollView(_namesScrollPosition);
                {
                    for (int i = 0; i < _roleNamesEnCH.Count; i++)
                    {
                        if (!_roleNamesEnCH[i].ToLower().Contains(_searchKeyword.ToLower()))
                            continue;
                        
                        if (GUILayout.Button(_roleNamesEnCH[i]))
                            _currentRoleName = i;
                    }
                }
                GUILayout.EndScrollView();
            }
        }
        void DrawRoleList()
        {
            int column_0 = 280;
            int column_1 = 60;
            
            GUILayout.Label("Roles List: "+_roleNamesEnCH.Count,EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name: ",EditorStyles.helpBox,GUILayout.Width(column_0));
                GUILayout.Label("Has LOD: ",EditorStyles.helpBox,GUILayout.Width(column_1));
            }
            GUILayout.EndHorizontal();
            
            _namesScrollPosition = GUILayout.BeginScrollView(_namesScrollPosition);
            {
                GUILayout.BeginHorizontal();
                {
                    _currentRoleName = GUILayout.SelectionGrid(_currentRoleName, _roleNamesEnCH.ToArray(), 1,GUILayout.Width(column_0));
                    _currentRoleName = GUILayout.SelectionGrid(_currentRoleName, _roleHasLod.ToArray(), 1,GUILayout.Width(column_1));
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            
        }
        
        void DrawRoleInfo()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label(_roleNamesEnCH[_currentRoleName], "SettingsHeader");
                GUILayout.Space(10);
                DrawBandpose();
            }
            GUILayout.EndVertical();
        }
        void DrawBandpose()
        {
            if (_bandposeDic.ContainsKey(_roleNames[_currentRoleName]))
            {
                _lodDatas.Clear();
                _infoScrollPosition = GUILayout.BeginScrollView(_infoScrollPosition,GUILayout.Width(0),GUILayout.Height(0));
                {
                    
                    GUILayout.Label("All Bandpose Count: "+_bandposeDic[_roleNames[_currentRoleName]].Count,EditorStyles.boldLabel);
                    var datas = _bandposeDic[_roleNames[_currentRoleName]];
                    foreach (var bandposeData in datas)
                    {
                        BandposeDataField(bandposeData);
                        
                        if (bandposeData.name.ToLower().Contains("_lod"))
                        {
                            _lodDatas.Add(bandposeData);
                        }
                    }

                    GUILayout.Space(10);
                    
                    foreach (var lodData in _lodDatas)
                    {
                        foreach (var data in RoleEditorComponetContext.bandposeDatas)
                        {
                            var lodName = lodData.name.ToLower().Replace("_lod", "");
                            if (data.name.ToLower().Equals(lodName))
                            {
                                GUILayout.Label("LOD Group: ",EditorStyles.boldLabel);
                                BandposeDataField(data);
                                BandposeDataField(lodData);
                                DrawLODGroup(data,lodData);
                                GUILayout.Space(5);
                            }
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
        }

        public void MarkItemhasLod()
        {
            foreach (var roleName in _roleNames)
            {
                _lodDatas.Clear();
                if (_bandposeDic.ContainsKey(roleName))
                {
                    var datas = _bandposeDic[roleName];
                    foreach (var bandposeData in datas)
                    {
                        if (bandposeData.name.ToLower().Contains("_lod"))
                        {
                            _lodDatas.Add(bandposeData);
                            break;
                        }
                        
                    }
                }

                if (_lodDatas.Count>0)
                {
                    _roleHasLod.Add("♢");
                }
                else
                {
                    _roleHasLod.Add("");
                }
            }
        }
        private void FindLodPrefabName(BandposeData data, BandposeData lodData)
        {
            _prefabNames.Clear();
            foreach (var config in data.exportConfig)
            {
                var prefabName = config.prefabName;
                List<string> prefabGroup = new List<string>();
                prefabGroup.Add(prefabName);
                _prefabNames.Add(prefabGroup);
            }

            foreach (var lodConfig in lodData.exportConfig)
            {
                var lodPrefabName = lodConfig.prefabName;
                foreach (var prefab in data.exportConfig)
                {
                    var prefabName = prefab.prefabName;
                    var name = lodPrefabName.ToLower().Replace("_lod1", "");
                    name = name.Replace("_lod2", "");
                    name = name.Replace("_lod3", "");
                    if (name.Equals(prefabName.ToLower()))
                    {
                        _prefabNames[data.exportConfig.IndexOf(prefab)].Add(lodPrefabName);
                    }
                }
            }
        }
        
        void DrawLODGroup(BandposeData data, BandposeData lodData)
        {
            FindLodPrefabName(data, lodData);

            foreach (var prefabGroup in _prefabNames)
            {
                for (int i = 0; i < prefabGroup.Count; i++)
                {
                    bool isLastGroup = _prefabNames.IndexOf(prefabGroup) == _prefabNames.Count - 1;
                    if (isLastGroup)
                    {
                        if (prefabGroup[i].ToLower().Contains("_lod1"))
                        {
                            prefabGroup[i] = "               └── " + prefabGroup[i];
                        }
                        else if (prefabGroup[i].ToLower().Contains("_lod2"))
                        {
                            prefabGroup[i] = "                            └── " + prefabGroup[i];
                        }
                        else if (prefabGroup[i].ToLower().Contains("_lod3"))
                        {
                            prefabGroup[i] = "                                              └── " + prefabGroup[i];
                        
                        }
                        else
                        {
                            prefabGroup[i] = "└── " + prefabGroup[i];
                        }
                    }
                    else
                    {
                        if (prefabGroup[i].ToLower().Contains("_lod1"))
                        {
                            prefabGroup[i] = "│             └── " + prefabGroup[i];
                        }
                        else if (prefabGroup[i].ToLower().Contains("_lod2"))
                        {
                            prefabGroup[i] = "│                          └── " + prefabGroup[i];
                        }
                        else if (prefabGroup[i].ToLower().Contains("_lod3"))
                        {
                            prefabGroup[i] = "│                                            └── " + prefabGroup[i];
                        
                        }
                        else
                        {
                            prefabGroup[i] = "├── " + prefabGroup[i];
                        }
                    }
                    GUILayout.Label(prefabGroup[i]);
                }
            }
        }
        
        public override void Destroy()
        {
            _roleNames.Clear();
            _roleNamesEnCH.Clear();
            _bandposeDic.Clear();
            _lodDatas.Clear();
            
            _roleNames = null;
            _roleNamesEnCH = null;
            _bandposeDic = null;
            _lodDatas = null;
        }

        public override string Name()
        {
            return "Role Statistics";
        }
    }

    public class RoleInSceneStatistics :RoleEditorComponet<RoleInSceneStatistics>
    {
        private Vector2 _namesScrollPosition;
        private List<XGameObject> _roles = new List<XGameObject>();
        private bool _passView;
        public override void Init()
        {
        }

        void GetRoles()
        {
            EntityExtSystem.entitysUpdate += GetEntitys;
        }
        
        private void GetEntitys(EngineContext context, List<XEntity> xentities)
        {
            _roles.Clear();
            foreach (var xentity in xentities)
            {
                _roles.Add(xentity.EngineObject);
            }
            EntityExtSystem.entitysUpdate -= GetEntitys;
        }

        public override void DrawGUI()
        {
            base.DrawGUI();
            if (GUILayout.Button("Find Roles in Scene"))
            {
                GetRoles();
            }

            int triCount = 0;
            int vertexCount = 0;
            int roleCount = 0;
            if (_roles.Count == 0)
                return;
            
            _namesScrollPosition = GUILayout.BeginScrollView(_namesScrollPosition);
            foreach (var role in _roles)
            {
                int triangles = 0;
                int sdTriangles = 0;
                int vertexs = 0;
                bool drawGUI = false;
                bool hasSD = false;
                string passName = "";
                QueueData riIt = role.BeginGetRender(out int index);
                while (role.GetRender(ref riIt, ref index, out RendererInstance ri))
                {
                    if (ri.render && ri.render.enabled && ri.render is SkinnedMeshRenderer renderer)
                    {
                        var mesh = renderer.sharedMesh;
                        var mat = ri.shareMaterial;
                        if (renderer.name.Contains("_sd_"))
                            hasSD = true;
                        if (mesh && mat)
                        {
                            int passCount = 1;

                            if (_passView)
                            {
                                passCount = 0;
                                for (int i = 0; i < mat.passCount; i++)
                                {
                                    var pn = mat.GetPassName(i);
                                    if (!PassesWhiteList(pn) && mat.GetShaderPassEnabled(pn))
                                    {
                                        passCount++;
                                        if (!passName.Contains(pn))
                                        {
                                            passName += $"{pn}; ";
                                        }
                                    }
                                }
                            }

                            if (renderer.name.Contains("_sd_"))
                            {
                                sdTriangles += mesh.triangles.Length / 3;
                                if (!_passView)
                                    break;
                            }
                            
                            triangles += passCount * mesh.triangles.Length / 3;
                            vertexs += passCount * mesh.vertexCount;
                            
                            drawGUI = true;
                        }
                    }
                    
                }

                bool PassesWhiteList(string name)
                {
                    return name.Equals("PlanarShadow") || name.Equals("DepthOnly") || name.Contains("Overdraw");
                }
                

                if (drawGUI)
                {
                    // GUILayout.Label(role.GoName,EditorStyles.boldLabel);
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.ObjectField(role.GetGO(), typeof(GameObject));
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Tri: {triangles}");
                    GUILayout.Label($"Vertex: {vertexs}");
                    if (!_passView)
                    {
                        GUILayout.Label($"Has SD: {hasSD}");
                        GUILayout.Label($"SD Tri: {sdTriangles}");
                    }
                    GUILayout.EndHorizontal();
                    
                    if (_passView)
                    {
                        GUILayout.Label($"Pass: {passName}");
                    }
                    GUILayout.EndVertical();
                    triCount += triangles;
                    vertexCount += vertexs;
                    roleCount += 1;
                }
                
            }
            GUILayout.EndScrollView();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Role Sum: {roleCount}");
            GUILayout.Label($"Tri Sum: {triCount}");
            GUILayout.Label($"Vertex Sum: {vertexCount}");
            GUILayout.EndHorizontal();

            if (!_passView)
            {
                if (GUILayout.Button("Render Pass View"))
                {
                    _passView = true;
                }
            }
            else
            {
                if (GUILayout.Button("Mesh View"))
                {
                    _passView = false;
                }
            }
        }

        public override void Destroy()
        {
            _roles.Clear();
        }



        public override string Name()
        {
            return "Role In Scene Statistics";
        }
    }
}

#endif
