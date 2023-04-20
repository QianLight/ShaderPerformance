#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.CFEventSystems;
using UnityEngine.CFUI;

public class Joystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector2 Output; //输出操纵杆的移动坐标
    [HideInInspector]
    public Transform model;
    public bool isMoving = false;
    public RectTransform fore;
    private Animator animator;
    // private CFSlider walkSpeed;
    // private CFText speedNote;
    //public float defaultSpeed = 0.5f;//1.40625f;
    // public AudioSource audioSource;
    // public AudioClip[] clips;
    // private CFSlider rotateSpeedSlider;
    // private CFText rotateSpeedNote;
    // private CFButton btnReset;
    [HideInInspector]
    public CameraController cameraController;


    void Start()
    {
        fore.gameObject.SetActive(false);
        animator = model.GetComponent<Animator>();     //动画组件
        //walkSpeed.maxValue = 5;
        //walkSpeed.minValue = 0.5f;
        //walkSpeed.value = defaultSpeed;
        //speedNote.text = string.Format("speed:{0}/s", defaultSpeed);
        //walkSpeed.onValueChanged.AddListener((float value) => OnSliderValueChange(value, walkSpeed));
        //rotateSpeedSlider.maxValue = 50;
        //rotateSpeedSlider.minValue = -50f;
        //rotateSpeedSlider.onValueChanged.AddListener((float value) => OnSliderRotateSpeedValueChange(value, walkSpeed));
        //btnReset.onClick.AddListener(ResetCamera);
    }

    public void ResetCamera()
    {
        if (cameraController != null)
        {
            cameraController.ResetInitConfig();
        }
    }

    private void OnSliderValueChange(float value, CFSlider EventSender)
    {
        // speedNote.text = string.Format("speed:{0}/s", value);
    }

    private void OnSliderRotateSpeedValueChange(float value, CFSlider EventSender)
    {
        // rotateSpeedNote.text = string.Format("speed:{0}", value);
    }

    public float Axis2Angle(bool inDegree = true)
    {
        float angle = Mathf.Atan2(Output.x, Output.y);

        if (inDegree)
        {
            return angle * Mathf.Rad2Deg;
        }
        else
        {
            return angle;
        }
    }

    private Vector2 startPoint;
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 dir = eventData.position - startPoint;
        Output = dir.normalized;
        fore.transform.localEulerAngles = new Vector3(0, 0, 90 - Axis2Angle());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CameraController.useJoystick = true;
        startPoint = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CameraController.useJoystick = false;
        fore.transform.localPosition = Vector3.zero;
        Output = Vector3.zero;
    }

    void Update()
    {
        if (CameraController.useJoystick)
        {
            if (Output.magnitude != 0)
            {
                Move();
            }
            else
            {
                Stop();
            }
        }
        else
        {
            Output.x = Input.GetAxis("Horizontal");
            Output.y = Input.GetAxis("Vertical");
            if (Output.magnitude != 0)
            {
                fore.transform.localEulerAngles = new Vector3(0, 0, 90 - Axis2Angle());
                Move();
            }
            else
            {
                Stop();
            }
        }

        if (cameraController != null)
        {
            cameraController.UpdateCamera();
        }

    }
    private float smoothSpeed = 10;

    void Move()
    {
        animator.SetBool("iswalking", true);
        float angle = Axis2Angle(true);
        model.transform.rotation = Quaternion.Lerp(model.transform.rotation, Quaternion.Euler(new Vector3(0, angle + Camera.main.transform.localEulerAngles.y, 0)), Time.deltaTime * smoothSpeed);
        float moveSpeed = cameraController.moveSpeed;
        Vector3 temp = model.transform.forward */* walkSpeed.value*/ moveSpeed * Time.deltaTime;
        model.transform.position += temp;

        if (!fore.gameObject.activeSelf)
        {
            fore.gameObject.SetActive(true);
        }
    }

    void Stop()
    {
        if (animator == null) return;
        animator.SetBool("iswalking", false);
        if (fore.gameObject.activeSelf)
        {
            fore.gameObject.SetActive(false);
        }
    }
}
#endif