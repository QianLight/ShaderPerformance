#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CFEventSystems;

public class CameraController : MonoBehaviour
{
    [HideInInspector]
    [Tooltip("跟随的目标")]
    public Transform player; 

    [HideInInspector]
    public static bool useJoystick = false;

    [Tooltip("注视点最大偏移")]
    public float offsetMaxY = 1.5f;

    [Tooltip("注视点最小偏移")]
    public float offsetMinY = 1.3f;

    [Tooltip("摄像机最小仰视角度")]
    public float minCameraRotateX = -30; 

    [Tooltip("摄像机最大俯视角度")]
    public float maxCameraRotateX = 80; 

    [Tooltip("摄像机距离人物的最小距离")]
    public float minDistance = 1;  

    [Tooltip("摄像机距离人物的最大距离")]
    public float maxDistance = 15;

    [Tooltip("摄像机初始水平旋转角度")]
    public float initialHorizontalRotation = 0f;

    [Tooltip("摄像机初始垂直旋转角度")]
    public float initialVerticalRotation = 0f;

    [Tooltip("摄像机初始距离人的距离，用于恢复初始角度使用")]
    public float initDisance = 2;

    [Tooltip("放大Lerp速度，值越小Lerp越快")]
    public float zoomInSpeed = 0.2f;

    [Tooltip("缩小Lerp速度，值越小Lerp越快")]
    public float zoomOutSpeed = 0.2f;

    [Tooltip("碰撞的层")]
    public LayerMask CollisionLayers;

    [Tooltip("人物移动速度")]
    public float moveSpeed = 5f;


    [Tooltip("以注释掉为中心，采样五个点，发射射线检测是否有碰撞物体")]
    public List<Vector3> SamplingPoints = new List<Vector3>
                {
                    Vector3.zero,
                    new Vector3(0, 0.5f, 0),
                    new Vector3(0, -0.5f, 0),
                    new Vector3(0.5f, 0, 0),
                    new Vector3(-0.5f, 0, 0),
                };

    private float scrollSpeed = 1;                          //Editor下相机视野缩放系数
    private float rotateSpeed = 20;                         //Editor下相机视野旋转系数
    private bool isRotating = false;                        //用来判断是否正在旋转
    private bool isInited = false;                          //是否已经初始化
    private float totalAddXAngles = 0;                      //俯仰角的累加值
    private float k = 0;                                    //计算注视点y偏移的k系数
    private float b = 0;                                    //计算注视点y偏移的k系数
    private float anglePerDistanceX = 0;                    //手机平台水平方向转到速度
    private float anglePerDistanceY = 0;                    //手机平台垂直方向转到速度
    private float desiredDistance = 5;                      //记录当前要达到的距离
    private float currentDistance;                          //当前摄像机距离目标点的距离
    private const float FLOAT_TOLERANCE = 0.001f;           //误差值
    private bool isZoomingIn;                               //是否在放大
    private bool isZoomingOut;                              //是否在缩小
    private float zoomStartTime;                            //缩放开始时间
    private float zoomStartDistance;                        //缩放开始距离
    private Touch[] touches;                                //记录触摸点的数组
    private int unUICount;                                  //在非UI上的触摸点数
    private List<Vector2> tempPos = new List<Vector2>();    //临时记录触摸点的位置
    private bool canScroll = false;                         //是否可以缩放
    private float initPlayerYAngle = 0;                     //初始人物的旋转角度
    private Vector2 oldPosition1;                           //上一次的第一个触摸点位置1
    private Vector2 oldPosition2;                           //上一次的第一个触摸点位置2
    private Vector3 pivotOffset = Vector3.zero;             //注视点的偏移向量，x/z分量为0，y为偏移值

    /// <summary>
    /// 放大或者缩小都是在缩放
    /// </summary>
    private bool IsZooming                                  
    {       
        get
        {
            return isZoomingIn || isZoomingOut;
        }
    }


    /// <summary>
    /// 触摸数据封装
    /// </summary>
    class TouchData
    {
        public int fingerID = -1;
        public Touch touch;
    }

    /// <summary>
    /// 当前帧的所有触摸点数
    /// </summary>
    private static List<TouchData> fingers = new List<TouchData>();
    static List<TouchData> Fingers
    {
        get
        {
            if (fingers.Count == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    TouchData mf = new TouchData();
                    mf.fingerID = -1;
                    fingers.Add(mf);
                }
            }
            return fingers;
        }
    }

   
    /// <summary>
    /// 摄像机的注视点
    /// </summary>
    private Vector3 TargetPosition
    {
        get { return player.transform.position + pivotOffset; }
    }


    /// <summary>
    /// 初始化摄像机的参数
    /// </summary>
    public void Init()
    {
        player = StartGame.Instance.joystick.model;

        anglePerDistanceX = 360.0f / Screen.width;
        anglePerDistanceY = (maxCameraRotateX - minCameraRotateX) * 1.0f / Screen.height;

        k = (offsetMaxY - offsetMinY) / (minDistance - maxDistance);
        b = offsetMinY - k * maxDistance;

        desiredDistance = initDisance;
        currentDistance = initDisance;
        initPlayerYAngle = player.transform.localEulerAngles.y;

        pivotOffset.y = k * currentDistance + b;


        transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.AngleAxis(initialHorizontalRotation, player.up) * this.transform.rotation;
        initialVerticalRotation = GetEnforcedVerticalDegrees(initialVerticalRotation);
        transform.rotation = Quaternion.AngleAxis(initialVerticalRotation, transform.right) * this.transform.rotation;
        CheckHit(0);

        unUICount = 0;
        for (int i = 0; i < Fingers.Count; ++i)
        {
            Fingers[i].fingerID = -1;
        }
        isInited = true;
    }

    /// <summary>
    /// 恢复初始摄像机的位置
    /// </summary>
    public void ResetInitConfig()
    {
        desiredDistance = initDisance;
        totalAddXAngles = 0;
        transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.AngleAxis(player.localEulerAngles.y + initialHorizontalRotation - initPlayerYAngle, player.up) * this.transform.rotation;
        initialVerticalRotation = GetEnforcedVerticalDegrees(initialVerticalRotation);
        transform.rotation = Quaternion.AngleAxis(initialVerticalRotation, transform.right) * this.transform.rotation;
        CheckHit(0);
    }

    /// <summary>
    /// 调试使用
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(this.transform.position, this.transform.position + this.transform.forward * 20);
    }

    /// <summary>
    /// 外部Update时调用
    /// </summary>
    public void UpdateCamera()
    {
        if (!isInited) return;


#if UNITY_EDITOR || UNITY_STANDALONE
        UpdateCameraEditor();
#elif UNITY_ANDROID || UNITY_IPHONE
        UpdateCameraMobile();
#endif
    }

    static List<RaycastResult> results = new List<RaycastResult>();
    static PointerEventData eventDataCurrentPosition = new PointerEventData(CFEventSystem.current);

    /// <summary>
    /// 判断当前点的位置是否在UI上
    /// </summary>
    /// <param name="point"></param>
    /// <param name="checkIgnore"></param>
    /// <returns></returns>
    public static bool PointOnUI(Vector3 point, bool checkIgnore = true)
    {
        if (CFEventSystem.current == null) return false;

        Vector2 screenPosition = point;
        eventDataCurrentPosition.position = screenPosition;
        CFEventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        if (checkIgnore && results.Count > 0)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject != null && results[i].gameObject.CompareTag("UI"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Editor下更新摄像机的方法
    /// </summary>

    void UpdateCameraEditor()
    {
        if (Input.GetMouseButtonDown(0) && !PointOnUI(Input.mousePosition))
        {
            isRotating = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }

        if (isRotating && Input.GetMouseButton(0) && useJoystick == false)
        {
            transform.rotation = Quaternion.AngleAxis(rotateSpeed * Input.GetAxis("Mouse X"), player.up) * this.transform.rotation;
            float addX = -rotateSpeed * Input.GetAxis("Mouse Y");
            addX = GetEnforcedVerticalDegrees(addX);
            transform.rotation = Quaternion.AngleAxis(addX, transform.right) * this.transform.rotation;
        }

        float add = -Input.GetAxis("Mouse ScrollWheel");
        CheckHit(add);
    }


    /// <summary>
    /// 手机平台下更新摄像机的方法
    /// </summary>
    private void UpdateCameraMobile()
    {
        touches = Input.touches;
        float add = 0;

        for (int i = 0; i < Fingers.Count; ++i)
        {
            if (Fingers[i].fingerID == -1)
            {
                continue;
            }

            bool isExist = false;
            for (int j = 0; j < touches.Length; ++j)
            {
                if (Fingers[i].fingerID == touches[j].fingerId)
                {
                    isExist = true;
                    break;
                }
            }

            if (isExist == false)
            {
                Fingers[i].fingerID = -1;
                unUICount--;
            }
        }

        for (int i = 0; i < touches.Length; ++i)
        {
            bool isExist = false;
            for (int j = 0; j < Fingers.Count; ++j)
            {
                if (touches[i].fingerId == Fingers[j].fingerID)
                {
                    isExist = true;
                    Fingers[j].touch = touches[i];
                }
            }

            if (!isExist && touches[i].phase == TouchPhase.Began && !PointOnUI(touches[i].position))
            {
                for (int j = 0; j < Fingers.Count; ++j)
                {
                    if (Fingers[j].fingerID == -1)
                    {
                        Fingers[j].fingerID = touches[i].fingerId;
                        Fingers[j].touch = touches[i];
                        unUICount++;
                        break;
                    }
                }
            }
        }

        if (unUICount == 1)
        {
            for (int i = 0; i < Fingers.Count; ++i)
            {
                if (Fingers[i].fingerID != -1)
                {
                    Vector3 originalPos = transform.position;//保存相机当前的位置
                    Quaternion originalRotation = transform.rotation;//保存相机当前的旋转

                    float y = Fingers[i].touch.deltaPosition.x * anglePerDistanceX; //* Time.deltaTime;// Fingers[i].touch.deltaTime;
                    transform.rotation = Quaternion.AngleAxis(y, player.up) * this.transform.rotation;
                    float addX = -Fingers[i].touch.deltaPosition.y * anglePerDistanceY;
                    addX = GetEnforcedVerticalDegrees(addX);
                    transform.rotation = Quaternion.AngleAxis(addX, transform.right) * this.transform.rotation;
                }
            }
        }
        else if (unUICount >= 2)
        {
            tempPos.Clear();
            canScroll = false;

            for (int i = 0; i < Fingers.Count; ++i)
            {
                if (Fingers[i].fingerID != -1)
                {
                    tempPos.Add(Fingers[i].touch.position);

                    if (tempPos.Count == 2 && Fingers[i].touch.phase == TouchPhase.Began)
                    {
                        oldPosition1 = tempPos[0];
                        oldPosition2 = tempPos[1];
                    }

                    if (Fingers[i].touch.phase == TouchPhase.Moved)
                    {
                        canScroll = true;
                    }
                }

                if (canScroll && tempPos.Count > 1)
                {
                    var tempPosition1 = tempPos[0];
                    var tempPosition2 = tempPos[1];

                    float currentTouchDistance = Vector3.Distance(tempPosition1, tempPosition2);
                    float lastTouchDistance = Vector3.Distance(oldPosition1, oldPosition2);
                    add = -(currentTouchDistance - lastTouchDistance) * Time.deltaTime * scrollSpeed;
                    oldPosition1 = tempPosition1;
                    oldPosition2 = tempPosition2;

                    break;
                }
            }
        }

        CheckHit(add);
    }


    /// <summary>
    /// 检测碰撞并设置摄像机的位置
    /// </summary>
    /// <param name="add"></param>

    public void CheckHit(float add)
    {
        if (add < 0)
        {
            desiredDistance = Mathf.Max(desiredDistance + add, 0);
            desiredDistance = Mathf.Max(desiredDistance, minDistance);
        }
        else if (add > 0)
        {
            desiredDistance = Mathf.Min(desiredDistance + add, maxDistance);
        }

        float desired = desiredDistance;
        float calculated = CalculateMaximumDistanceFromTarget(TargetPosition, Mathf.Max(desired, currentDistance)); // The maximum distance we calculated we can be based off collision and preference
        float zoomDistance = ZoomDistance(currentDistance, calculated, desiredDistance);

        pivotOffset.y = k * currentDistance + b;
        Vector3 offset = transform.forward * zoomDistance;
        transform.position = TargetPosition - offset;
        currentDistance = Vector3.Distance(transform.position, TargetPosition);
    }


    private float GetEnforcedVerticalDegrees(float degrees)
    {
        totalAddXAngles += degrees;
        if (totalAddXAngles < minCameraRotateX)
        {
            degrees = degrees + minCameraRotateX - totalAddXAngles;
            totalAddXAngles = minCameraRotateX;
        }
        else if (totalAddXAngles > maxCameraRotateX)
        {
            degrees = degrees + maxCameraRotateX - totalAddXAngles;
            totalAddXAngles = maxCameraRotateX;
        }
        return degrees;
    }

    /// <summary>
    /// 计算摄像机和注视点的最近距离，如果有碰撞则要拉近摄像机
    /// </summary>
    /// <param name="target"></param>
    /// <param name="furthestDistance"></param>
    /// <returns></returns>

    private float CalculateMaximumDistanceFromTarget(Vector3 target, float furthestDistance)
    {
        float closestDistance = furthestDistance;
        Vector3 backTowardsCamera = -this.transform.forward;

        for (int i = 0; i < SamplingPoints.Count; ++i)
        {
            Vector3 startingPosition = target;
            startingPosition += transform.right * SamplingPoints[i].x;
            startingPosition += transform.up * SamplingPoints[i].y;
            startingPosition += transform.forward * SamplingPoints[i].z;

            float distanceToCheck = closestDistance - SamplingPoints[i].z;

            RaycastHit hit;
            if (CollisionViewRaycast(startingPosition, backTowardsCamera, out hit, distanceToCheck))
            {
                closestDistance = hit.distance;
            }
        }
        return closestDistance;
    }

    /// <summary>
    /// 检测注释点和摄像机之间是否有碰撞体
    /// </summary>
    /// <param name="startingPosition"></param>
    /// <param name="direction"></param>
    /// <param name="hit"></param>
    /// <param name="maxDistance"></param>
    /// <returns></returns>

    private bool CollisionViewRaycast(Vector3 startingPosition, Vector3 direction, out RaycastHit hit, float maxDistance)
    {
        float currentDistance = maxDistance;
        while (currentDistance > 0)
        {
            Debug.DrawLine(startingPosition, startingPosition + direction * maxDistance, Color.yellow);
            if (Physics.Raycast(startingPosition, direction, out hit, maxDistance))
            {
                if (IsCollisionObject(hit.collider.gameObject))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        hit = new RaycastHit();
        return false;
    }

    /// <summary>
    /// 判断物体是否为碰撞体，检测层标签即可。
    /// </summary>
    /// <param name="collisionObject"></param>
    /// <returns></returns>
    public bool IsCollisionObject(GameObject collisionObject)
    {
        if (collisionObject == null)
        {
            return false;
        }

        return (CollisionLayers.value & (1 << collisionObject.layer)) > 0;
    }


    /// <summary>
    /// 计算摄像机距离注视点的距离
    /// </summary>
    /// <param name="currentDistance">当前摄像机和注视点的距离</param>
    /// <param name="calculatedDistance">计算的理想距离</param>
    /// <param name="desiredDistance">在没有碰撞之前的摄像机距离</param>
    /// <returns></returns>
    private float ZoomDistance(float currentDistance, float calculatedDistance, float desiredDistance)
    {
        SetZoomingState(calculatedDistance, currentDistance, desiredDistance);

        float resultDistance = 0;
        if (IsZooming)
        {
            float speed = isZoomingIn ? zoomInSpeed : zoomOutSpeed;
            float t = Mathf.Clamp01((Time.time - zoomStartTime) / speed);
            float lerped = Mathf.Lerp(zoomStartDistance, desiredDistance, t);
            resultDistance = Math.Min(lerped, calculatedDistance);
        }
        else
        {
            resultDistance = Mathf.Min(calculatedDistance, desiredDistance);
        }

        if (resultDistance < minDistance)
        {
            resultDistance = minDistance;
        }

        return resultDistance;
    }


    /// <summary>
    /// 设置当前是否处于缩放，还是检测到注视点和人物之间是否有碰撞体
    /// </summary>
    /// <param name="calculatedDistance"></param>
    /// <param name="currentDistance"></param>
    /// <param name="desiredDistance"></param>
    private void SetZoomingState(float calculatedDistance, float currentDistance, float desiredDistance)
    {
        if (currentDistance - calculatedDistance > FLOAT_TOLERANCE)
        {
            isZoomingIn = false;
            isZoomingOut = false;
        }
        else if ((Math.Abs(calculatedDistance - currentDistance) < FLOAT_TOLERANCE) && (currentDistance - desiredDistance > FLOAT_TOLERANCE))
        {
            if (!isZoomingIn)
            {
                isZoomingIn = true;
                isZoomingOut = false;

                zoomStartTime = Time.time;
                zoomStartDistance = currentDistance;
            }
        }
        else if ((calculatedDistance - currentDistance > FLOAT_TOLERANCE) && (desiredDistance - currentDistance > FLOAT_TOLERANCE))
        {
            if (!isZoomingOut)
            {
                isZoomingIn = false;
                isZoomingOut = true;

                zoomStartTime = Time.time;
                zoomStartDistance = currentDistance;
            }
        }
        else
        {
            isZoomingIn = false;
            isZoomingOut = false;
        }
    }
}
#endif