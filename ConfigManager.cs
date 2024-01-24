// config.cs
using System;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace EyeSaver
{
    public class Config
    {
        public string UpdateTime { get; set; }
        public bool AutoStartAtBootIsEnable { get; set; }
        public EyeSaverConfig EyeSaverConfig { get; set; }
    }
    public class EyeSaverConfig
    {
        public bool IsEnable { get; set; }
        public int ReminderIntervalMinutes { get; set; }

        [JsonConverter(typeof(StringEnumConverter))] // 这使得枚举值在JSON中以字符串形式存储
        public ReminderMethod ReminderMethod { get; set; }
    }

    public enum ReminderMethod
    {
        ScreenDim, // 屏幕变暗
        PopupReminder // 弹窗提醒
    }

    public class ConfigManager
    {
        // 定义一个委托和事件
        public delegate void ConfigChangedHandler(Config newConfig);
        public event ConfigChangedHandler ConfigChanged;

        private string configFilePath = "config.json";
        private Config appConfig = null;

        public Config AppConfig
        {
            get { return appConfig; }
            set
            {
                if (appConfig != value)
                {
                    appConfig = value;
                    OnConfigChanged(appConfig);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void InitializeConfig()
        {
            // 如果配置文件不存在，则创建并写入默认配置
            if (!File.Exists(configFilePath))
            {
                appConfig = new Config
                {
                    UpdateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss"),
                    AutoStartAtBootIsEnable = false,
                    EyeSaverConfig = new EyeSaverConfig
                    {
                        IsEnable = true,
                        ReminderIntervalMinutes = 20,
                        ReminderMethod = ReminderMethod.ScreenDim // 默认方法
                    }
                };
                File.WriteAllText(configFilePath, JsonConvert.SerializeObject(appConfig, Formatting.Indented));
            }
            else
            {
                // 读取现有的配置文件
                appConfig = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFilePath));
            }
        }

        protected virtual void OnConfigChanged(Config newConfig)
        {
            // 如果有对象订阅了事件，则通知它们
            ConfigChanged?.Invoke(newConfig);
        }
        public void SaveConfig()
        {
            appConfig.UpdateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss");
            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(appConfig, Formatting.Indented));
            OnConfigChanged(appConfig); // 在保存后触发事件
        }
    }


}
