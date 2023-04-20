using Devops.Core;
using Devops.Performance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PTestUtils : MonoBehaviour
{


    public static void AddCaseSteps(string key, string des , string tag, bool successed, ScreenType screenType = ScreenType.None)
    {
        PTestDataManager.Instance.AddCaseSteps(key, des, tag, successed, screenType);
    }

    /// <summary>
    /// 获取鼠标停留处UI
    /// </summary>
    /// <param name="canvas"></param>
    /// <returns></returns>
    public static void ClickOverGUI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2
            (
#if UNITY_EDITOR
            Input.mousePosition.x, Input.mousePosition.y
#elif UNITY_ANDROID || UNITY_IOS
           Input.touchCount > 0 ? Input.GetTouch(0).position.x : 0, Input.touchCount > 0 ? Input.GetTouch(0).position.y : 0
#endif 
            );
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach (RaycastResult result in results)
        {
            Debug.Log("TouchUIName: " + result.gameObject.name);
        }

    }

    public static long GetTime()
    {
        //精确到毫秒
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

    }
    public static long GetTimeStamp()
    {
        // 注意, 如果直接使用DateTime.Now, 会有系统时区问题, 导致误差
        TimeSpan timeStamp = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(timeStamp.TotalSeconds);
        //return GetTime();
    }


    public static Snapshot ScreenShot(string des = "失败截图")
    {        
        string sendPath = 
            PTestManager.Instance.versionId + "/job-" + 
            PTestManager.Instance.jobId + "/" + 
            PTestManager.Instance.buildTaskId + "/" + 
            PTestManager.Instance.buildRunId + "/" ;
        string imagename = sendPath + DevopsScreenshot.Instance().GetScreenShot(true, sendPath, null);

        ImageData imagedata = new ImageData();
        imagedata.name =  imagename;

        string result = "Image&&" + JsonUtility.ToJson(imagedata);

        SocketHelper.GetInstance().SendMessage(result);


        Snapshot imageData = new Snapshot
        {
            name = imagename,
            frameCount = Time.frameCount

        };

        //PTestDataManager.Instance.screenshots.Add(PTestData.key, imageData) ;
        return imageData;
    }


    // Import function mouse_event() from WinApi
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dwFlags"></param>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    /// <param name="dwData"></param>
    /// <param name="dwExtraInfo"></param>
    [DllImport("user32.dll")]
    private static extern void mouse_event(MouseFlags dwFlags, int dx, int dy, int dwData, System.UIntPtr dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern int SetCursorPos(int x, int y); //设置光标位置
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(ref int x, ref int y); //获取光标位置

    public static bool MoveTo(float x, float y)
    {
        if (x < 0 || y < 0 || x > UnityEngine.Screen.width || y > UnityEngine.Screen.height)
            return true;

        //if (!UnityEngine.Screen.fullScreen)
        //{
        //    UnityEngine.Debug.LogError("只能在全屏状态下使用！");
        //    return false;
        //}

        SetCursorPos((int)x, (int)(UnityEngine.Screen.height - y));
        return true;
    }

    public static void LeftClick(float x = -1, float y = -1)
    {
        if (MoveTo(x, y))
        {
            mouse_event(MouseFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);
            mouse_event(MouseFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }
    }
    // Flags needed to specify the mouse action 
    [System.Flags]
    private enum MouseFlags
    {
        Move = 0x0001,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        Absolute = 0x8000,
    }

    //    public static int MouseXSpeedCoef = 45000; // Cursor rate in Х direction
    //    public static int MouseYSpeedCoef = 45000; // Cursor rate in Y direction

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner
        public int Top;         // y position of upper-left corner
        public int Right;       // x position of lower-right corner
        public int Bottom;      // y position of lower-right corner
    }

    [DllImport("user32.dll")]
    //[return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();

    enum GetWindow_Cmd : uint
    {
        GW_HWNDFIRST = 0,
        GW_HWNDLAST = 1,
        GW_HWNDNEXT = 2,
        GW_HWNDPREV = 3,
        GW_OWNER = 4,
        GW_CHILD = 5,
        GW_ENABLEDPOPUP = 6
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

    const int MONITOR_DEFAULTTONULL = 0;
    const int MONITOR_DEFAULTTOPRIMARY = 1;
    const int MONITOR_DEFAULTTONEAREST = 2;

    [DllImport("user32.dll")]
    static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [DllImport("user32.dll")]
    static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);


    //	private static int windowX = 0;
    //	private static int windowY = 0;
    //	private static int winSizeX = 0;
    //	private static int winSizeY = 0;

    private static Vector2 monitorSize = Vector2.zero;
    private static MONITORINFO monitorInfo = new MONITORINFO();
    private static bool winRectPrinted = false;


    /// <summary>
    /// 左上角为0，0.单位为像素
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void MouseMove(int x, int y)
    {
        float facX = (float)x / Screen.resolutions[0].width;
        float facY = (float)y / Screen.resolutions[0].height;

        int inputX = (int)(65535 * facX);
        int inputY = (int)(65535 * facY);

        mouse_event(MouseFlags.Absolute | MouseFlags.Move, inputX, inputY, 0, System.UIntPtr.Zero);
    }


    // Public function to emulate a mouse button click (left button)
    public static void MouseClick()
    {
        mouse_event(MouseFlags.LeftDown, 0, 0, 0, System.UIntPtr.Zero);
        mouse_event(MouseFlags.LeftUp, 0, 0, 0, System.UIntPtr.Zero);
    }

    // Public function to emulate a mouse drag event (left button)
    public static void MouseDrag()
    {
        mouse_event(MouseFlags.LeftDown, 0, 0, 0, System.UIntPtr.Zero);
    }

    // Public function to emulate a mouse release event (left button)
    public static void MouseRelease()
    {
        mouse_event(MouseFlags.LeftUp, 0, 0, 0, System.UIntPtr.Zero);
    }

    // Public function to move the mouse cursor to the specified position
    public static void MouseMove(Vector3 screenCoordinates)
    {
        int windowX = 0;
        int windowY = 0;
        int winSizeX = 0;
        int winSizeY = 0;

        bool isConvertToFullScreen = true;//Screen.fullScreen;

        IntPtr hWnd = GetActiveWindow();
        hWnd = GetClosestWindow(hWnd, Screen.width, Screen.height);

        if (hWnd != IntPtr.Zero)
        {
            RECT winRect;

            if (GetWindowRect(hWnd, out winRect))
            {
                winSizeX = winRect.Right - winRect.Left;
                winSizeY = winRect.Bottom - winRect.Top;

                windowX = winRect.Left + (winSizeX - (int)Screen.width) / 2;

                if (!isConvertToFullScreen)
                {
                    windowY = winRect.Top + (winSizeY - (int)Screen.height + 36) / 2;
                }
                else
                {
                    windowY = winRect.Top + (winSizeY - (int)Screen.height) / 2;
                }

                // get display resolution
                if (monitorSize == Vector2.zero)
                {
                    monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);

                    IntPtr hMonitoŕ = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
                    if (!GetMonitorInfo(hMonitoŕ, ref monitorInfo))
                    {
                        monitorInfo.rcMonitor.Left = monitorInfo.rcMonitor.Top = 0;
                        monitorInfo.rcMonitor.Right = Screen.currentResolution.width - 1;
                        monitorInfo.rcMonitor.Bottom = Screen.currentResolution.height - 1;

                        monitorInfo.rcWork.Left = monitorInfo.rcWork.Top = 0;
                        monitorInfo.rcWork.Right = Screen.currentResolution.width - 1;
                        monitorInfo.rcWork.Bottom = Screen.currentResolution.height - 1;
                    }

                    monitorSize.x = monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left + 1;
                    monitorSize.y = monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top + 1;
                }

                if (!winRectPrinted)
                {
                    Debug.Log(string.Format("monSize: ({0}, {1})", monitorSize.x, monitorSize.y));
                    Debug.Log(string.Format("scrSize: ({0}, {1})", Screen.width, Screen.height));
                    Debug.Log(string.Format("winRect: ({0}, {1}, {2}, {3})", winRect.Left, winRect.Top, winRect.Right, winRect.Bottom));
                    Debug.Log(string.Format("winPos: ({0}, {1})", windowX, windowY));
                    winRectPrinted = true;
                }
            }
        }
        else
        {
            if (monitorSize == Vector2.zero)
            {
                monitorSize.x = Screen.currentResolution.width;
                monitorSize.y = Screen.currentResolution.height;
            }
        }

        int mouseX = 0;
        int mouseY = 0;

        if (!isConvertToFullScreen)
        {
            float screenX = windowX + screenCoordinates.x * Screen.width;
            float screenY = windowY + (1f - screenCoordinates.y) * Screen.height;

            float screenRelX = screenX / monitorSize.x;
            float screenRelY = screenY / monitorSize.y;

            //			if(debugText)
            //			{
            //				if(!debugText.text.Contains("ScrPos"))
            //				{
            //					string sDebug = string.Format("\nScrPos: ({0:F0}, {1:F0})", screenX, screenY);
            //					debugText.text += sDebug;
            //					//Debug.Log (sDebug);
            //				}
            //			}

            mouseX = (int)(screenRelX * 65535);
            mouseY = (int)(screenRelY * 65535);
        }
        else
        {
            mouseX = (int)(screenCoordinates.x * 65535);
            mouseY = (int)((1f - screenCoordinates.y) * 65535);
        }

        mouse_event(MouseFlags.Absolute | MouseFlags.Move, mouseX, mouseY, 0, System.UIntPtr.Zero);
    }

    // find the closest matching child window to the screen size
    private static IntPtr GetClosestWindow(IntPtr hWndMain, int scrWidth, int scrHeight)
    {
        if (hWndMain == IntPtr.Zero)
            return hWndMain;

        IntPtr hWnd = hWndMain;
        RECT winRect;

        if (GetWindowRect(hWndMain, out winRect))
        {
            int winSizeX = winRect.Right - winRect.Left;
            int winSizeY = winRect.Bottom - winRect.Top;
            int winDiff = Math.Abs(winSizeX - scrWidth) + Math.Abs(winSizeY - scrHeight);

            IntPtr hWndChild = GetWindow(hWndMain, GetWindow_Cmd.GW_CHILD);
            int winDiffMin = winDiff;

            while (hWndChild != IntPtr.Zero)
            {
                if (GetWindowRect(hWndChild, out winRect))
                {
                    winSizeX = winRect.Right - winRect.Left;
                    winSizeY = winRect.Bottom - winRect.Top;
                    winDiff = Math.Abs(winSizeX - scrWidth) + Math.Abs(winSizeY - scrHeight - 36);

                    if (scrWidth <= winSizeX && scrHeight <= winSizeY && winDiff <= winDiffMin)
                    {
                        hWnd = hWndChild;
                        winDiffMin = winDiff;
                    }
                }

                hWndChild = GetWindow(hWndChild, GetWindow_Cmd.GW_HWNDNEXT);
            }
        }

        return hWnd;
    }
}
