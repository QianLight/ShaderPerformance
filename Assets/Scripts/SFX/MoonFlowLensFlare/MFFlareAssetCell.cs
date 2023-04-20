using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FlareAsset", menuName = "MFLensflare/Create FlareAsset split by Cell")]
[Serializable]
public class MFFlareAssetCell : MFFlareAsset
{
   [SerializeField]public Vector2Int modelCell;
}


