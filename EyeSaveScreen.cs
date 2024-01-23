using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;

namespace EyeSaver
{
    internal class EyeSaveScreen
    {
        private Config config;
        private Timer reminderTimer;

        // Windows API，用于屏幕变暗
        [DllImport("user32.dll")]
        static extern bool SetScreenBrightness(byte bV);

        public EyeSaveScreen(Config config)
        {
            Initialize(config);


        }

        public void Initialize(Config config)
        {

            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            SystemEvents.PowerModeChanged += SystemEvents_OnPowerModeChanged;

            this.config = config;
            // 初始化定时器
            if (reminderTimer != null)
            {
                reminderTimer.Stop();
                reminderTimer.Dispose();
            }


            reminderTimer = new Timer();
            reminderTimer.Interval = config.EyeSaverConfig.ReminderIntervalMinutes * 60 * 1000; // 将分钟转换为毫秒
            reminderTimer.Tick += ReminderTimer_Tick;
            reminderTimer.Start();
        }

        //会话被锁定，关闭定时器
        //会话解锁，打开定时器
        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                // 电脑被锁定，停止计时器
                reminderTimer.Stop();
                Debug.WriteLine($"{DateTime.Now} " + "SessionLock事件触发，reminderTimer计时器停止");

            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                // 电脑被解锁，启动计时器
                reminderTimer.Start();
                Debug.WriteLine($"{DateTime.Now} " + "SessionUnlock事件触发,reminderTimer计时器停止计时器开始");
            }
        }
        //系统电源模式改变，关闭定时器
        private void SystemEvents_OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    // 系统挂起操作，比如休眠或睡眠
                    reminderTimer.Stop();

                    break;
                case PowerModes.Resume:
                    // 系统从挂起状态恢复，比如从休眠或睡眠中唤醒
                    reminderTimer.Start();
                    break;
            }
        }
        public void Deinitialize()
        {
            if (reminderTimer != null)
            {
                reminderTimer.Stop();
                reminderTimer.Dispose();
                reminderTimer = null;
            }
            SystemEvents.SessionSwitch -= new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            SystemEvents.PowerModeChanged -= SystemEvents_OnPowerModeChanged;


        }

        private void ReminderTimer_Tick(object sender, EventArgs e)
        {
            if (config.EyeSaverConfig.IsEnable)
            {
                switch (config.EyeSaverConfig.ReminderMethod)
                {
                    case ReminderMethod.ScreenDim:
                        DimScreen();
                        break;
                    case ReminderMethod.PopupReminder:
                        ShowPopupReminder();
                        break;
                }
            }
        }

        public void DimScreen()
        {
            // 调用 Windows API 函数设置屏幕亮度
            DimScreenForm dimForm = new DimScreenForm();
            dimForm.ShowDialog(); // 显示为对话框形式
        }

         public void ShowPopupReminder()
        {
            MessageBox.Show("是时候休息一下眼睛了！", "护眼提醒");
        }

        // 扩展接口：重置屏幕亮度
        public void ResetScreenBrightness()
        {
            // 假设 100 为正常亮度值
            SetScreenBrightness(100);
        }
    }


}
