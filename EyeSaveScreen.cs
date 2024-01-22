using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

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

        public void Deinitialize()
        {
            if (reminderTimer != null)
            {
                reminderTimer.Stop();
                reminderTimer.Dispose();
                reminderTimer = null;
            }
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
