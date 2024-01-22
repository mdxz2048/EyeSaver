using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Gma.System.MouseKeyHook;
namespace EyeSaver
{
    internal class DimScreenForm : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        private IKeyboardMouseEvents globalHook; // 用于全局钩子
        private Timer fadeTimer;
        private float step = 0.01f; // 变暗的步长
        private float currentOpacity = 0; // 当前不透明度

        public DimScreenForm()
        {
            this.BackColor = Color.Black;
            this.Opacity = currentOpacity;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;

            // 设置鼠标和键盘的钩子监听，这里需要你自己实现相关的钩子逻辑
            globalHook = Hook.GlobalEvents();
            globalHook.KeyDown += GlobalHook_KeyDown;
            globalHook.MouseMove += GlobalHook_MouseMove;

            // 初始化淡入定时器
            fadeTimer = new Timer();
            fadeTimer.Interval = 50; // 间隔时间可以自己调整
            fadeTimer.Tick += new EventHandler(fadeTimer_Tick);
            fadeTimer.Start();
        }

        private void fadeTimer_Tick(object sender, EventArgs e)
        {
            if (currentOpacity < 0.7) // 设置最大不透明度值为0.7
            {
                currentOpacity += step;
                this.Opacity = currentOpacity;
            }
            else
            {
                fadeTimer.Stop(); // 达到最大不透明度后停止计时器
            }
        }

        private void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            // 恢复屏幕亮度并关闭覆盖层
            this.Close();
        }

        private void GlobalHook_MouseMove(object sender, MouseEventArgs e)
        {
            // 如果鼠标移动，则取消暗化效果并关闭Form
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            // 取消订阅事件并释放全局钩子
            globalHook.KeyDown -= GlobalHook_KeyDown;
            globalHook.MouseMove -= GlobalHook_MouseMove;
            globalHook.Dispose();
        }
    }
}
