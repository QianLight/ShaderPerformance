using System;
using System.Collections.Generic;
using System.IO;
using CFUtilPoolLib;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using CFEngine;
using CFEngine.Editor;
using XLevel;

namespace XEditor.Level
{
    class LevelDisplayWalkableMode : LevelDisplayBaseMode
    {
        public List<QuadTreeElement> m_SelectGrid = new List<QuadTreeElement> ();

        public static List<QuadTreeElement> m_debugGrid = new List<QuadTreeElement> ();

        Rect windowRect = new Rect (20, 100, 150, 50);

        private LevelNavigation Nav = null;

        private bool bTestNavigation = false;

        SceneContext context = new SceneContext ();
        public LevelDisplayWalkableMode (LevelGridTool tool) : base (tool)
        {
            Nav = new LevelNavigation (this);

            SceneAssets.GetCurrentSceneContext (ref context);
        }

        public override void OnDisable ()
        {
            m_SelectGrid.Clear ();
            Nav.Clear ();
        }

        public override void OnEnterMode ()
        {
            base.OnEnterMode ();
            m_debugGrid.Clear ();
        }

        public override void OnLeaveMode ()
        {
            m_SelectGrid.Clear ();
            Nav.Clear ();

            m_debugGrid.Clear ();
        }

        public override void OnUpdate ()
        {
            Nav.OnUpdate ();
        }

        public override void OnGUI ()
        {
            EditorGUILayout.Space ();

            EditorGUILayout.BeginHorizontal ();

            EditorGUILayout.BeginVertical (new GUILayoutOption[] { GUILayout.Width (500) });

            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("加载导航点文件", new GUILayoutOption[] { GUILayout.Width (150) }))
            {
                Nav.LoadFromFile (ref context);
            }

            if (Nav.IsNavigationPathMatrixReady ())
            {
                GUILayout.Space (30);
                if (GUILayout.Button ("保存导航图", new GUILayoutOption[] { GUILayout.Width (150) }))
                {
                    Nav.SaveToFile (ref context);
                }

                if (GUILayout.Button ("保存导航图(服务器）", new GUILayoutOption[] { GUILayout.Width (150) }))
                {
                    Nav.SaveToServerXML (ref context);
                }

            }

            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.Space ();
            GUILayout.Label ("导航点使用方法：");
            GUILayout.Label ("   计算或者加载客户端网格之后方可添加导航点");
            GUILayout.Label ("   1. 双击放置导航点");
            GUILayout.Label ("   2. 选中导航点后拖动");
            GUILayout.Label ("   3. 选中导航点后delete删除");

            EditorGUILayout.EndVertical ();

            EditorGUILayout.BeginVertical ();

            EditorGUILayout.Space ();

            GUIContent ButtonContent = new GUIContent ("生成导航图", "生成导航图");
            if (GUILayout.Button (ButtonContent, new GUILayoutOption[] { GUILayout.Width (200), GUILayout.Height (100) }))
            {
                Nav.GenerateNaviPointMap ();
            }

            EditorGUILayout.EndVertical ();

            EditorGUILayout.EndHorizontal ();

            //if (GUILayout.Button(new GUIContent("debug", "debug"), new GUILayoutOption[] { GUILayout.Width(150) }))
            //{
            //    m_tool.m_Drawer.SmartDrawGrid();
            //}

            //if (GUILayout.Button(new GUIContent("清除所有导航点", "清除所有导航点"), new GUILayoutOption[] { GUILayout.Width(150) }))
            //{
            //    Nav.ClearNaviPoint();
            //    Nav.ClearNaviLine();
            //    m_tool.m_Drawer.SmartDrawGrid();
            //}

            if (Nav.IsNavigationPathMatrixReady ())
            {
                EditorGUILayout.Space ();

                GUILayout.Box ("", new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (1) });

                EditorGUILayout.Space ();
                EditorGUI.BeginChangeCheck ();
                bTestNavigation = GUILayout.Toggle (bTestNavigation, "测试寻路", new GUILayoutOption[] { GUILayout.Width (150) });
                if (EditorGUI.EndChangeCheck ())
                {
                    if (!bTestNavigation)
                    {
                        Nav.ClearTestNavi ();
                        Nav.ClearTestNaviLine ();
                    }

                }
            }

            //EditorGUI.BeginChangeCheck();
            //EditorGUI.EndChangeCheck();

        }

        public override bool OnMouseClick (SceneView sceneView)
        {
            Ray r = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);

            RaycastHit hitInfo;

            bool bHit = Physics.Raycast (r, out hitInfo, 10000.0f, layer_mask);

            if (bHit)
            {
                if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer ("Terrain"))
                {
                    Vector3 clickPoint = hitInfo.point;
                    LevelBlock preBlock = null;
                    if (m_SelectGrid.Count > 0)
                    {
                        preBlock = m_tool.m_Data.GetBlockByCoord (m_SelectGrid[0].pos);
                    }

                    m_SelectGrid.Clear ();

                    LevelBlock selectBlock = m_tool.m_Data.GetBlockByCoord (clickPoint);
                    if (selectBlock != null)
                    {
                        QuadTreeElement selectGrid = selectBlock.GetGridByCoord(clickPoint);
                        if (selectGrid != null)
                        {
                            m_SelectGrid.Add(selectGrid);

                            float h = m_SelectGrid[0].GetPointHeight(clickPoint);

                            Debug.Log("accurate H =" + h.ToString() + "  selectGrid:" + selectGrid.pos);

                            if (preBlock != null) m_tool.m_Drawer.ReDrawBlockColors(preBlock);
                            m_tool.m_Drawer.ReDrawBlockColors(selectBlock);

                            if (m_tool.m_Data.IsDebugAudioSurface)
                                AudioSurfaceRecognition.Instance.GetSurfaceType(hitInfo, r, true);
                        }
                    }



                    //Nav.UnSelected();

                    return true;
                }
                else if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer ("BigGuy"))
                {
                    if (Event.current.control)
                    {
                        Nav.AddLink (hitInfo.collider.gameObject);
                    }
                    else
                    {
                        Nav.OnSelect (hitInfo.collider.gameObject);
                    }
                }

            }

            return false;
        }

        public override bool OnMouseDoubleClick (SceneView sceneView)
        {
            if (!m_tool.m_Data.IsDataReady)
            {
                EditorUtility.DisplayDialog("导航点增加错误", "请先生成地形网格或者加载客户端网格", "OK");
                return false;
            }

            Ray r = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);

            RaycastHit hitInfo;

            bool bHit = Physics.Raycast (r, out hitInfo, 10000.0f, layer_mask);

            if (bHit)
            {
                Vector3 clickPoint = hitInfo.point;

                if (bTestNavigation)
                    Nav.AddTestPoint (clickPoint);
                else
                    Nav.AddNaviPoint (clickPoint, -1);
            }

            return false;
        }

        public override bool OnKeyDown (SceneView sceneView)
        {
            Event e = Event.current;

            if (e.keyCode == KeyCode.Backspace)
            {
                //Nav.RemoveLastPoint();
            }
            else if (e.keyCode == KeyCode.Delete)
            {
                Nav.DeleteCurrentSelected ();
            }
            return false;
        }

        public override void OnSceneGUI ()
        {
            if (m_SelectGrid.Count > 0)
            {
                int windowID = 1234;

                windowRect = GUILayout.Window (windowID, windowRect, DrawGridInfoWindow, "GridInfo", GUILayout.Width (100));
            }

        }
        void DrawGridInfoWindow (int windowID)
        {
            GUI.contentColor = Color.yellow;
            QuadTreeElement grid = m_SelectGrid[0];

            GUILayout.BeginHorizontal ();
            GUILayout.Label ("(" + grid.pos.x + "  " + grid.pos.y + "  " + grid.pos.z + ")");
            GUILayout.EndHorizontal ();

            if (GUILayout.Button ("Test RayCast", GUILayout.Width (100)))
            {
                m_tool.m_Data.TestRaycast (grid.pos);
            }

            GUI.DragWindow ();
        }
        public override bool IsCurrentGridSelect (QuadTreeElement grid)
        {
            for (int i = 0; i < m_SelectGrid.Count; ++i)
            {
                if (m_SelectGrid[i].pos == grid.pos) return true;
            }

            return false;
        }

        public override bool IsCurrentGridDebug (QuadTreeElement grid)
        {
            for (int i = 0; i < m_debugGrid.Count; ++i)
            {
                if (m_debugGrid[i].pos == grid.pos) return true;
            }

            return false;
        }
    }
}