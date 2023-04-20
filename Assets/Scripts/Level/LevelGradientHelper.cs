using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CFUtilPoolLib;

namespace XLevel
{
    class LevelGradientHelper
    {
        public static void GetGradientForGrid(RaycastHit centerRaycastHit, ref byte alpha, ref byte beta, ref float baseHeight)
        {
            float gs = LevelMapData.GridSize;
            Vector3 corner1 = centerRaycastHit.point + new Vector3(-gs / 2, 0, -gs / 2);
            Vector3 corner2 = centerRaycastHit.point + new Vector3(gs / 2, 0, -gs / 2);
            Vector3 corner3 = centerRaycastHit.point + new Vector3(-gs / 2, 0, gs / 2);
            Vector3 corner4 = centerRaycastHit.point + new Vector3(gs / 2, 0, gs / 2);

            Vector3 yVector = new Vector3(0, gs * 2, 0);
            RaycastHit hitInfo1, hitInfo2, hitInfo3, hitInfo4;
            bool bHit1, bHit2, bHit3, bHit4;
            bHit1 = Physics.Raycast(corner1 + yVector, Vector3.down, out hitInfo1, gs * 4, LevelDataGenerator.layer_mask);
            bHit2 = Physics.Raycast(corner2 + yVector, Vector3.down, out hitInfo2, gs * 4, LevelDataGenerator.layer_mask);
            bHit3 = Physics.Raycast(corner3 + yVector, Vector3.down, out hitInfo3, gs * 4, LevelDataGenerator.layer_mask);
            bHit4 = Physics.Raycast(corner4 + yVector, Vector3.down, out hitInfo4, gs * 4, LevelDataGenerator.layer_mask);
            bool bHitFinal1 = bHit1 && (hitInfo1.collider.name == centerRaycastHit.collider.name);
            bool bHitFinal2 = bHit2 && (hitInfo2.collider.name == centerRaycastHit.collider.name);
            bool bHitFinal3 = bHit3 && (hitInfo3.collider.name == centerRaycastHit.collider.name);
            bool bHitFinal4 = bHit4 && (hitInfo4.collider.name == centerRaycastHit.collider.name);

            bool GridOk = false;

            if(bHitFinal1 && bHitFinal2 && bHitFinal3 && bHitFinal4)
            {
                GridOk = true;
            }
            else
            {
                if(bHit1 && bHit2 && bHit3 && bHit4)
                {
                    float h = (hitInfo1.point.y + hitInfo2.point.y + hitInfo3.point.y + hitInfo4.point.y) / 4;

                    if(h - centerRaycastHit.point.y < 0.01)
                    {
                        GridOk = true;
                    }
                }
            }

            if (GridOk)
            {
                float Alpha = Mathf.Atan((hitInfo2.point.y - hitInfo1.point.y) / gs) * 180 / Mathf.PI;
                float Beta = Mathf.Atan((hitInfo3.point.y - hitInfo1.point.y) / gs) * 180 / Mathf.PI;

                alpha = (Alpha < 0 ? (byte)(Alpha + 180) : (byte)Alpha);
                beta = (Beta < 0 ? (byte)(Beta + 180) : (byte)Beta);
                baseHeight = hitInfo1.point.y;
                return;
            }
            else
            {
                if (bHitFinal1)
                {
                    Vector3 corner5 = centerRaycastHit.point + new Vector3(0, 0, -gs / 2);
                    Vector3 corner6 = centerRaycastHit.point + new Vector3(-gs / 2, 0, 0);
                    RaycastHit hitInfo5, hitInfo6;
                    bool bHit5, bHit6;
                    bHit5 = Physics.Raycast(corner5 + yVector, Vector3.down, out hitInfo5, gs * 4, LevelDataGenerator.layer_mask);
                    bHit6 = Physics.Raycast(corner6 + yVector, Vector3.down, out hitInfo6, gs * 4, LevelDataGenerator.layer_mask);
                    bHit5 = bHit5 && (hitInfo5.collider.name == centerRaycastHit.collider.name);
                    bHit6 = bHit6 && (hitInfo6.collider.name == centerRaycastHit.collider.name);

                    if (bHit5 && bHit6)
                    {
                        float Alpha = Mathf.Atan(2 * (hitInfo5.point.y - hitInfo1.point.y) / gs) * 180 / Mathf.PI;
                        float Beta = Mathf.Atan(2 * (hitInfo6.point.y - hitInfo1.point.y) / gs) * 180 / Mathf.PI;

                        alpha = (Alpha < 0 ? (byte)(Alpha + 180) : (byte)Alpha);
                        beta = (Beta < 0 ? (byte)(Beta + 180) : (byte)Beta);
                        baseHeight = hitInfo1.point.y;
                        return;
                    }
                }
                else if (bHitFinal4)
                {
                    Vector3 corner7 = centerRaycastHit.point + new Vector3(gs / 2, 0, 0);
                    Vector3 corner8 = centerRaycastHit.point + new Vector3(0, 0, gs / 2);
                    RaycastHit hitInfo7, hitInfo8;
                    bool bHit7, bHit8;
                    bHit7 = Physics.Raycast(corner7 + yVector, Vector3.down, out hitInfo7, gs * 4, LevelDataGenerator.layer_mask);
                    bHit8 = Physics.Raycast(corner8 + yVector, Vector3.down, out hitInfo8, gs * 4, LevelDataGenerator.layer_mask);
                    bHit7 = bHit7 && (hitInfo7.collider.name == centerRaycastHit.collider.name);
                    bHit8 = bHit8 && (hitInfo8.collider.name == centerRaycastHit.collider.name);

                    if (bHit7 && bHit8)
                    {
                        float Alpha = Mathf.Atan(2 * (hitInfo7.point.y - centerRaycastHit.point.y) / gs) * 180 / Mathf.PI;
                        float Beta = Mathf.Atan(2 * (hitInfo8.point.y - centerRaycastHit.point.y) / gs) * 180 / Mathf.PI;

                        alpha = (Alpha < 0 ? (byte)(Alpha + 180) : (byte)Alpha);
                        beta = (Beta < 0 ? (byte)(Beta + 180) : (byte)Beta);
                        baseHeight = centerRaycastHit.point.y;
                        return;
                    }
                }
            }

            alpha = 0;
            beta = 0;
            baseHeight = centerRaycastHit.point.y;

        }

        public static void ExtendGrid(Vector3 grid_center, List<QuadTreeElement> existGrid, List<List<GridData>> tempData)
        {
            for(int j = 1; j <= 4; ++j)
            {
                for(int i = 0; i < tempData[j].Count; ++i)
                {
                    GridData gd = tempData[j][i];
                    if (gd.type == GridType.LowerSurface) continue;
                    if (gd.type == GridType.AimThrowSurface) continue;
                    float h = gd.height;

                    if (!IsGridExist(h, existGrid))
                    {
                        //if (count <= 2)
                        {
                            QuadTreeElement grid = new QuadTreeElement();
                            grid.pos = new Vector3(grid_center.x, h, grid_center.z);
                            grid.info = 0;
                            grid.alpha = 0;
                            grid.beta = 0;
                            existGrid.Add(grid);
                        }
                    }
                }
            }
        }

        private static bool IsGridExist(float h, List<QuadTreeElement> existGrid)
        {
            float heightDelta = LevelMapData.GridSize * 2;

            for (int i = 0; i < existGrid.Count; ++i)
            {
                if(Mathf.Abs(h - existGrid[i].pos.y) <= heightDelta)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsGridExist(float h, List<GridData> queryGrid)
        {
            float heightDelta = LevelMapData.GridSize * 2;

            for (int i = 0; i < queryGrid.Count; ++i)
            {
                if (Mathf.Abs(h - queryGrid[i].height) <= heightDelta)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ReduceGrid(Vector3 grid_center, List<List<GridData>> tempData)
        {
            for(int i = tempData[0].Count - 1; i >= 0; i--)
            {
                for(int j = 1; j <= 4; ++j)
                {
                    if(!IsGridExist(tempData[0][i].height, tempData[j]))
                    {
                        tempData[0].RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}
