using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class RenderBinding : MonoBehaviour, IRenderBinding
{
    private static List<RenderBinding> bindings = new List<RenderBinding>();
    public static IReadOnlyList<RenderBinding> AllInstance => bindings;
    public List<FaceBinding> faces = new List<FaceBinding>();

    // private FaceBinding _face;

    // public FaceBinding Face => _face;

    private List<FaceData> _thisfaceDatas = new List<FaceData>();
    private static List<FaceData> faceDatas = new List<FaceData>();
    public static List<FaceData> FaceDatas => faceDatas;

    private void OnEnable()
    {
        PickOneFaceWhichRealNeedRender();

        if (bindings.IndexOf(this) < 0)
        {
            bindings.Add(this);
        }
    }

    private void PickOneFaceWhichRealNeedRender()
    {
        foreach (var face in faces)
        {
            if (face != null)
            {
                // _face = face;
                foreach (var faceRenderer in face.renderers)
                {
                    if (faceRenderer != null)
                    {
                        // _face.FaceRenderer = faceRenderer;
                        FaceData faceData = new FaceData();
                        faceData.Bone = face.bone;
                        faceData.Renderer = faceRenderer;
                        _thisfaceDatas.Add(faceData);
                    }
                }
                break;
            }
        }

        foreach (var faceData in _thisfaceDatas)
        {
            faceDatas.Add(faceData);
        }
    }

    private void OnDisable()
    {
        if (bindings.IndexOf(this) >= 0)
        {
            bindings.Remove(this);

        }
        foreach (var faceData in _thisfaceDatas)
        {
            faceDatas.Remove(faceData);
        }
    }
}

public struct FaceData
{
    public Transform Bone;
    public Renderer Renderer;
}