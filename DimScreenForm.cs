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
        private Timer autoFadeOutTimer; // 自动淡出定时器
        private string[] restPrompts = new string[]
        {
            // 鲁迅风格
            "“喧嚣中，片刻的宁静是金。\n那就休息一下吧。”",
            "“天地间，何处无诗意？让眼睛去寻找。\n那就休息一下吧。”",
            "“故纸堆中，也许有生命。\n那就休息一下吧。”",
            "“狂人日记里，记得要休息。\n那就休息一下吧。”",
            "“呐喊吧，不为别的，为了眼睛的清明。\n那就休息一下吧。”",


            // 余华风格
            "“活着，就得喘口气。\n那就休息一下吧。”",
            "“生如夏花，歇如秋叶。\n那就休息一下吧。”",
            "“看太多，不如闭目思。\n那就休息一下吧。”",
            "“生活的烟火里，别忘了歇息。\n那就休息一下吧。”",
            "“当屏幕变成日历，是时候停一停了。\n那就休息一下吧。”",

            // 崔永元风格
            "“屏幕太小，宇宙太大，别总盯着屏幕。\n那就休息一下吧。”",
            "“外面的世界很精彩，外面的世界也很无奈。\n那就休息一下吧。”",
            "“快餐时代，慢活生活。\n那就休息一下吧。”",
            "“别让键盘上的灰尘，埋没了眼里的星辰。\n那就休息一下吧。”",
            "“天下没有不散的宴席，也没有不歇的工作。\n那就休息一下吧。”"
        };


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
                Font = new Font("Microsoft Sans Serif", 24F, FontStyle.Regular, GraphicsUnit.Point, 0), // 设置字体大小和样式
                Visible = false // 开始时不可见
            };
            // 设置提示信息为随机的提示语
            SetRandomRestPrompt();
            this.Controls.Add(lblMessage); // 将 lblMessage 添加到窗体的控件集合中
            synthesizer = new SpeechSynthesizer();

            // 设置鼠标和键盘的钩子监听
            globalHook = Hook.GlobalEvents();
            globalHook.KeyDown += GlobalHook_KeyDown;
           // globalHook.MouseMove += GlobalHook_MouseMove;

            // 初始化淡入定时器
            fadeTimer = new Timer();
            fadeTimer.Interval   = 50; //50毫秒
            fadeTimer.Tick += new EventHandler(fadeTimer_Tick);
            fadeTimer.Start();

            // 初始化自动淡出定时器
            autoFadeOutTimer = new Timer();
            autoFadeOutTimer.Interval = 10000; // 10秒无操作后开始淡出
            autoFadeOutTimer.Tick += new EventHandler(autoFadeOutTimer_Tick);

        }
        private void SetRandomRestPrompt()
        {
            Random random = new Random();
            int index = random.Next(restPrompts.Length);
            lblMessage.Text = restPrompts[index];
        }

        private void fadeTimer_Tick(object sender, EventArgs e)
        {
            if (!fadingOut)
            {
                // 淡入效果
                if (currentOpacity < 0.7) // 设置最大不透明度值为0.7, 0.01 * 70
                {
                    currentOpacity += step;
                    this.Opacity = currentOpacity;
                    if (lblMessage.Visible == false)
                    {
                        lblMessage.Visible = true;
                        //synthesizer.SpeakAsync("您需要远眺1分钟"); // 语音提示
                    }
                    lblMessage.ForeColor = Color.FromArgb((int)(255 * currentOpacity), 255, 255, 255); // 字体颜色淡入
                }
                else
                {
                    fadeTimer.Stop(); // 达到最大不透明度后停止计时器
                    if(userActivityCount <= 0)
                    {
                        autoFadeOutTimer.Start(); // 开始自动淡出计时

                    }
                }
            }
            else
            {
                // 淡出效果
                if (this.Opacity > 0f) // 检查不透明度是否大于0
                {
                    this.Opacity -= step; // 减少不透明度
                    lblMessage.ForeColor = Color.FromArgb((int)(255 * this.Opacity), 255, 255, 255); // 字体颜色随着淡出而变化
                }
                else
                {
                    fadeTimer.Stop(); // 达到完全透明后停止计时器
                    fadingOut = false;
                    this.Close(); // 可以选择关闭窗体
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

        private void autoFadeOutTimer_Tick(object sender, EventArgs e)
        {
            // 如果当前不处于淡出状态且不透明度仍在最大值，则开始淡出
            if (!fadingOut)
            {
                StartFadingOut();
            }
            autoFadeOutTimer.Stop(); // 不论如何，触发一次后停止计时器
        }

        private void StartFadingOut()
        {
            if (userActivityCount >= 3)
            {
                // 用户活动达到5次后，关闭窗体
                this.Close();
            }
            else
            {
                // 根据用户活动计数器设置不透明度
                //currentOpacity = 0.7f - (userActivityCount * 0.14f); // 每次减少大约0.14的不透明度
                //this.Opacity = currentOpacity;
                //lblMessage.ForeColor = Color.FromArgb((int)(255 * currentOpacity), 255, 255, 255);
                if(!fadingOut)
                {
                    fadingOut = true;
                    fadeTimer.Start();
                }

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
