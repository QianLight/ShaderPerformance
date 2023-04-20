using UnityEngine;

/// <summary>
/// 基于倒数实现的曲线
/// 作用：([0,1],[0,1])范围内数值的remap函数。
/// 原理：从倒数曲线中截取一段并缩放到([0,1],[0,1])区域。
/// 优势：能满足Shader中大部分数值过渡调整的效果的前提下有很低的开销(3 cycle)。
/// Designed by 张欣颖 黑仔
/// </summary>
public class ReciprocalCurve
{
    /// <summary>
    /// 曲线的曲折度。
    /// </summary>
    public float Curveness
    {
        get => curveness;
        set
        {
            if (value == 0)
            {
                Debug.LogError("Setting BlackCurve error : Curveness could not be zero.");
                return;
            }

            if (value == curveness)
                return;
            curveness = value;
            dirty = true;
        }
    }
    /// <summary>
    /// 垂直翻转。
    /// </summary>
    public bool FlipVertical
    {
        get => flipVertical;
        set
        {
            if (value == flipVertical)
                return;
            flipVertical = value;
            dirty = true;
        }
    }
    /// <summary>
    /// 水平翻转。
    /// </summary>
    public bool FlipHorizontal
    {
        get => flipHorizontal;
        set
        {
            if (value == flipHorizontal)
                return;
            flipHorizontal = value;
            dirty = true;
        }
    }
    public float A
    {
        get
        {
            BakeIfDirty();
            return a;
        }
    }
    public float B
    {
        get
        {
            BakeIfDirty();
            return b;
        }
    }
    public float C
    {
        get
        {
            BakeIfDirty();
            return c;
        }
    }
    public Vector3 Parameters
    {
        get
        {
            BakeIfDirty();
            return new Vector3(a, b, c);
        }
    }

    public const float MIN_CURVENESS = 1e-7f;
    private float curveness = MIN_CURVENESS;
    private bool flipVertical;
    private bool flipHorizontal;

    private bool dirty = true;
    private float a;
    private float b;
    private float c;

    public static readonly ReciprocalCurve Default = Create(MIN_CURVENESS, true, false);

    /// <summary>
    /// 禁用所有构造函数以方便报错返回控制。
    /// </summary>
    private ReciprocalCurve() { }

    /// <summary>
    /// 创建曲线
    /// </summary>
    /// <param name="curveness">曲线的曲折度</param>
    /// <param name="flipVertical">垂直翻转曲线</param>
    /// <param name="flipHorizontal">水平翻转曲线</param>
    /// <returns></returns>
    public static ReciprocalCurve Create(float curveness, bool flipVertical, bool flipHorizontal)
    {
        if (curveness == 0)
        {
            Debug.LogError("Setting BlackCurve error : Curveness could not be zero.");
            return null;
        }

        ReciprocalCurve curve = new ReciprocalCurve();
        curve.curveness = curveness;
        curve.flipVertical = flipVertical;
        curve.flipHorizontal = flipHorizontal;
        return curve;
    }

    private void BakeIfDirty()
    {
        if (!dirty)
            return;

        float offset = curveness + 1;
        a = 1 / offset;
        b = offset;

        float temp = 1 / a - 1 / b;
        float atemp = a * temp;
        float btemp = b * temp;

        if (flipHorizontal)
        {
            a = atemp - btemp;
            b = btemp;
            c = -1 / btemp;
        }
        else
        {
            a = btemp - atemp;
            b = atemp;
            c = -1 / btemp;
        }

        if (flipVertical)
        {
            a *= -1;
            b *= -1;
            c *= -1;
            c += 1;
        }

        dirty = false;
    }

    public float Evaluate(float t)
    {
        BakeIfDirty();
        return 1 / (a * t + b) + c;
    }
}
