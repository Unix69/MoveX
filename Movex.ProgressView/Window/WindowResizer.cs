using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Movex.ProgressView
{
    /// <summary>
    /// The dock position of the window
    /// </summary>
    public enum WindowDockPosition
    {
        /// <summary>
        /// Not docked
        /// </summary>
        Undocked,
        /// <summary>
        /// Docked to the left of the screen
        /// </summary>
        Left,
        /// <summary>
        /// Docked to the right of the screen
        /// </summary>
        Right,
    }


    /// <summary>
    /// Fixes the issue with Windows of Style <see cref="WindowStyle.None"/> covering the taskbar
    /// </summary>
    public class WindowResizer
    {
        #region Private Members

        /// <summary>
        /// The window to handle the resizing for
        /// </summary>
        private Window mWindow;

        /// <summary>
        /// The last calculated available screen size
        /// </summary>
        private Rect mScreenSize = new Rect();

        /// <summary>
        /// How close to the edge the window has to be to be detected as at the edge of the screen
        /// </summary>
        private int mEdgeTolerance = 8;

        /// <summary>
        /// The transform matrix used to convert WPF sizes to screen pixels
        /// </summary>
        private DpiScale? mMonitorDpi;

        /// <summary>
        /// The last screen the window was on
        /// </summary>
        private IntPtr mLastScreen;

        /// <summary>
        /// The last known dock position
        /// </summary>
        private WindowDockPosition mLastDock = WindowDockPosition.Undocked;

        #endregion

        #region DLL Imports

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);

        #endregion

        #region Public Events

        /// <summary>
        /// Called when the window dock position changes
        /// </summary>
        public event Action<WindowDockPosition> WindowDockChanged = (dock) => { };

        #endregion

        #region Public Properties

        /// <summary>
        /// The size and position of the current monitor the window is on
        /// </summary>
        public Rectangle CurrentMonitorSize { get; set; } = new Rectangle();

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="window">The window to monitor and correctly maximize</param>
        /// <param name="adjustSize">The callback for the host to adjust the maximum available size if needed</param>
        public WindowResizer(Window window)
        {
            mWindow = window;

            // Listen out for source initialized to setup
            mWindow.SourceInitialized += Window_SourceInitialized;

            // Monitor for edge docking
            mWindow.SizeChanged += Window_SizeChanged;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize and hook into the windows message pump
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SourceInitialized(object sender, System.EventArgs e)
        {
            // Get the handle of this window
            var handle = (new WindowInteropHelper(mWindow)).Handle;
            var handleSource = HwndSource.FromHwnd(handle);

            // If not found, end
            if (handleSource == null)
                return;

            // Hook into it's Windows messages
            handleSource.AddHook(WindowProc);
        }

        #endregion

        #region Edge Docking

        /// <summary>
        /// Monitors for size changes and detects if the window has been docked (Aero snap) to an edge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Cannot calculate size until we know monitor scale
            if (mMonitorDpi == null)
                return;

            // Get the WPF size
            var size = e.NewSize;

            // Get window rectangle
            var top = mWindow.Top;
            var left = mWindow.Left;
            var bottom = top + size.Height;
            var right = left + mWindow.Width;

            // Get window position/size in device pixels
            var windowTopLeft = new Point(left * mMonitorDpi.Value.DpiScaleX, top * mMonitorDpi.Value.DpiScaleX);
            var windowBottomRight = new Point(right * mMonitorDpi.Value.DpiScaleX, bottom * mMonitorDpi.Value.DpiScaleX);

            // Check for edges docked
            var edgedTop = windowTopLeft.Y <= (mScreenSize.Top + mEdgeTolerance);
            var edgedLeft = windowTopLeft.X <= (mScreenSize.Left + mEdgeTolerance);
            var edgedBottom = windowBottomRight.Y >= (mScreenSize.Bottom - mEdgeTolerance);
            var edgedRight = windowBottomRight.X >= (mScreenSize.Right - mEdgeTolerance);

            // Get docked position
            var dock = WindowDockPosition.Undocked;

            // Left docking
            if (edgedTop && edgedBottom && edgedLeft)
                dock = WindowDockPosition.Left;
            else if (edgedTop && edgedBottom && edgedRight)
                dock = WindowDockPosition.Right;
            // None
            else
                dock = WindowDockPosition.Undocked;

            // If dock has changed
            if (dock != mLastDock)
                // Inform listeners
                WindowDockChanged(dock);

            // Save last dock position
            mLastDock = dock;
        }

        #endregion

        #region Windows Message Pump

        /// <summary>
        /// Listens out for all windows messages for this window
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                // Handle the GetMinMaxInfo of the Window
                case 0x0024:/* WM_GETMINMAXINFO */
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }

        #endregion

        /// <summary>
        /// Get the min/max window size for this window
        /// Correctly accounting for the taskbar size and position
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        private void WmGetMinMaxInfo(System.IntPtr hwnd, System.IntPtr lParam)
        {
            // Get the point position to determine what screen we are on
            GetCursorPos(out POINT lMousePosition);
            
            // Now get the current screen
            var lCurrentScreen = MonitorFromPoint(lMousePosition, MonitorOptions.MONITOR_DEFAULTTONEAREST);

            // Try and get the current screen information
            var lCurrentScreenInfo = new MONITORINFO();
            if (GetMonitorInfo(lCurrentScreen, lCurrentScreenInfo) == false)
                return;

            // If this has changed from the last one, update the transform
            if (lCurrentScreen != mLastScreen || mMonitorDpi == null)
                mMonitorDpi = VisualTreeHelper.GetDpi(mWindow);

            // Store last know screen
            mLastScreen = lCurrentScreen;

            // Get min/max structure to fill with information
            var lMmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Size size limits, relative to 0,0 being the current screens top-left corner
            lMmi.mPtMaxPosition.mX = 0;
            lMmi.mPtMaxPosition.mY = 0;
            lMmi.mPtMaxSize.mX = lCurrentScreenInfo.mRcWork.mRight - lCurrentScreenInfo.mRcWork.mLeft;
            lMmi.mPtMaxSize.mY = lCurrentScreenInfo.mRcWork.mBottom - lCurrentScreenInfo.mRcWork.mTop;

            // Set monitor size
            CurrentMonitorSize = new Rectangle(lMmi.mPtMaxPosition.mX, lMmi.mPtMaxPosition.mY, lMmi.mPtMaxSize.mX + lMmi.mPtMaxPosition.mX, lMmi.mPtMaxSize.mY + lMmi.mPtMaxPosition.mY);

            // Set min size
            var minSize = new Point(mWindow.MinWidth * mMonitorDpi.Value.DpiScaleX, mWindow.MinHeight * mMonitorDpi.Value.DpiScaleX);
            lMmi.mPtMinTrackSize.mX = (int)minSize.X;
            lMmi.mPtMinTrackSize.mY = (int)minSize.Y;

            // Store new size
            mScreenSize = new Rect(lMmi.mPtMaxPosition.mX, lMmi.mPtMaxPosition.mY, lMmi.mPtMaxSize.mX, lMmi.mPtMaxSize.mY);

            // Now we have the max size, allow the host to tweak as needed
            Marshal.StructureToPtr(lMmi, lParam, true);
        }
    }

    #region DLL Helper Structures

    enum MonitorOptions : uint
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO
    {
        public int mCbSize = Marshal.SizeOf(typeof(MONITORINFO));
        public Rectangle mRcMonitor = new Rectangle();
        public Rectangle mRcWork = new Rectangle();
        public int mDwFlags = 0;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct Rectangle
    {
        public int mLeft, mTop, mRight, mBottom;

        public Rectangle(int left, int top, int right, int bottom)
        {
            mLeft = left;
            mTop = top;
            mRight = right;
            mBottom = bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT mPtReserved;
        public POINT mPtMaxSize;
        public POINT mPtMaxPosition;
        public POINT mPtMinTrackSize;
        public POINT mPtMaxTrackSize;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        /// <summary>
        /// x coordinate of point.
        /// </summary>
        public int mX;
        /// <summary>
        /// y coordinate of point.
        /// </summary>
        public int mY;

        /// <summary>
        /// Construct a point of coordinates (x,y).
        /// </summary>
        public POINT(int x, int y)
        {
            mX = x;
            mY = y;
        }
    }

    #endregion
}