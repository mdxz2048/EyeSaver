using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Gma.System.MouseKeyHook;
using System.Speech.Synthesis;

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
        private int userActivityCount = 0; // 用户活动计数器


        private Label lblMessage;
        private SpeechSynthesizer synthesizer;
        private bool fadingOut = false;

        public DimScreenForm()
        {
            this.BackColor = Color.Black;
            this.Opacity = currentOpacity;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            lblMessage = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Text = "请休息一下眼睛", // 这里设置您想要的文本
                Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Regular, GraphicsUnit.Point, 0), // 设置字体大小和样式
                Visible = false // 开始时不可见
            };
            this.Controls.Add(lblMessage); // 将 lblMessage 添加到窗体的控件集合中

            synthesizer = new SpeechSynthesizer();

            // 设置鼠标和键盘的钩子监听
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
            if (!fadingOut)
            {
                // 淡入效果
                if (currentOpacity < 0.7) // 设置最大不透明度值为0.7
                {
                    currentOpacity += step;
                    this.Opacity = currentOpacity;
                    if (lblMessage.Visible == false)
                    {
                        lblMessage.Visible = true;
                        synthesizer.SpeakAsync("您需要远眺1分钟"); // 语音提示
                    }
                    lblMessage.ForeColor = Color.FromArgb((int)(255 * currentOpacity), 255, 255, 255); // 字体颜色淡入
                }
                else
                {
                    fadeTimer.Stop(); // 达到最大不透明度后停止计时器
                }
            }
            else
            {
                // 淡出效果
                if (this.Opacity < 0.7f)
                {
                    this.Opacity += step; // 增加不透明度
                    lblMessage.ForeColor = Color.FromArgb((int)(255 * this.Opacity), 255, 255, 255); // 字体颜色淡入
                }
                else
                {
                    fadeTimer.Stop(); // 达到原始不透明度后停止计时器
                    fadingOut = false;
                }
            }
        }


        private void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            userActivityCount++; // 增加用户活动计数器
            StartFadingOut();
        }

        private void GlobalHook_MouseMove(object sender, MouseEventArgs e)
        {
            userActivityCount++; // 增加用户活动计数器
            StartFadingOut();
        }

        private void StartFadingOut()
        {
            if (userActivityCount >= 5)
            {
                // 用户活动达到5次后，关闭窗体
                this.Close();
            }
            else
            {
                // 根据用户活动计数器设置不透明度
                currentOpacity = 0.7f - (userActivityCount * 0.14f); // 每次减少大约0.14的不透明度
                this.Opacity = currentOpacity;
                lblMessage.ForeColor = Color.FromArgb((int)(255 * currentOpacity), 255, 255, 255);
                fadingOut = true;
                fadeTimer.Start();
            }
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
