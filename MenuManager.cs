using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace EyeSaver
{
    public class MenuManager
    {
        private ContextMenuStrip trayMenu;
        private Config appConfig;
        public ConfigManager configManger;

        // 菜单项作为类的成员变量
        private ToolStripMenuItem enableFeatureItem;
        private ToolStripMenuItem screenDimItem;
        private ToolStripMenuItem popupReminderItem;
        private ToolStripMenuItem reminderIntervalItem;
        private ToolStripMenuItem reminderMethodItem;
        private ToolStripMenuItem eyeSaverSettingsItem;
        private ToolStripMenuItem helpItem;
        private ToolStripMenuItem aboutItem;
        private ToolStripMenuItem exitItem;
        private ToolStripMenuItem autoStartItem;


        public MenuManager(ContextMenuStrip menu)
        {
            trayMenu = menu;
            configManger = new ConfigManager();
            configManger.InitializeConfig();
            appConfig = configManger.AppConfig;

            CreateContextMenu();
        }


        private void CreateContextMenu()
        {

            // 开机自启动菜单项
            autoStartItem = new ToolStripMenuItem("开机自启动");

            // EyeSaver settings menu
            eyeSaverSettingsItem = new ToolStripMenuItem("护眼提醒设置");
            enableFeatureItem = new ToolStripMenuItem("开启功能");
            reminderIntervalItem = new ToolStripMenuItem("提醒周期(分钟)");
            reminderMethodItem = new ToolStripMenuItem("提醒方式");

            // Submenu for reminder method
            screenDimItem = new ToolStripMenuItem("屏幕变暗");
            popupReminderItem = new ToolStripMenuItem("弹窗提醒");
            reminderMethodItem.DropDownItems.Add(screenDimItem);
            reminderMethodItem.DropDownItems.Add(popupReminderItem);
            // 提醒周期菜单项
            reminderIntervalItem = new ToolStripMenuItem("提醒周期(分钟)");

            // 添加几个固定的周期选项
            var intervals = new[] { 1, 20, 30, 40, 60 };
            foreach (var interval in intervals)
            {
                var intervalItem = new ToolStripMenuItem(interval.ToString());
                intervalItem.Click += (sender, e) => ChangeReminderInterval(interval);
                reminderIntervalItem.DropDownItems.Add(intervalItem);
            }

       

            eyeSaverSettingsItem.DropDownItems.Add(enableFeatureItem);
            eyeSaverSettingsItem.DropDownItems.Add(reminderIntervalItem);
            eyeSaverSettingsItem.DropDownItems.Add(reminderMethodItem);

            // Help menu item
            helpItem = new ToolStripMenuItem("帮助");

            // About menu item
            aboutItem = new ToolStripMenuItem("关于");
            aboutItem.Click += OnAboutClick; // Add event handler for the About click


            // Exit menu item
            exitItem = new ToolStripMenuItem("退出");

            // Add items to the main context menu
            trayMenu.Items.Add(autoStartItem);
            trayMenu.Items.Add(eyeSaverSettingsItem);
            trayMenu.Items.Add(helpItem);
            trayMenu.Items.Add(aboutItem);
            trayMenu.Items.Add(exitItem);

            // 初始化菜单项的状态
            UpdateContextMenu(appConfig);

            // 为菜单项添加事件处理器
            autoStartItem.Click += AutoStartItem_Click; // 添加事件处理器
            enableFeatureItem.Click += (sender, e) => ToggleFeature(enableFeatureItem);
            screenDimItem.Click += (sender, e) => ChangeReminderMethod(ReminderMethod.ScreenDim);
            popupReminderItem.Click += (sender, e) => ChangeReminderMethod(ReminderMethod.PopupReminder);
            exitItem.Click += OnExit; // 添加点击事件处理器

        }

        private void AutoStartItem_Click(object sender, EventArgs e)
        {
            // 切换开机自启动的配置
            appConfig.AutoStartAtBootIsEnable = !appConfig.AutoStartAtBootIsEnable;
            ((ToolStripMenuItem)sender).Checked = appConfig.AutoStartAtBootIsEnable; // 更新菜单项的选中状态

            // 实现开机自启动的逻辑
            ToggleAutoStartAtBoot(appConfig.AutoStartAtBootIsEnable);

            // 保存配置更改
            configManger.SaveConfig();
        }

        private void ToggleAutoStartAtBoot(bool enable)
        {
            // 获取启动文件夹路径
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            // 快捷方式的完整路径
            string shortcutPath = Path.Combine(startupFolderPath, "EyeSaver.lnk");

            if (enable)
            {
                if (!System.IO.File.Exists(shortcutPath))
                {
                    // 创建快捷方式
                    WshShell shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                    shortcut.TargetPath = Application.ExecutablePath; // 快捷方式指向的路径
                    shortcut.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath); // 快捷方式的工作目录
                    shortcut.Description = "EyeSaver Application"; // 快捷方式的描述
                    shortcut.IconLocation = Application.ExecutablePath; // 快捷方式的图标路径
                    shortcut.Save(); // 保存快捷方式
                    MessageBox.Show("已将快捷方式添加到启动文件夹: " + shortcutPath);
                }
            }
            else
            {
                if (System.IO.File.Exists(shortcutPath))
                {
                    // 删除快捷方式
                    System.IO.File.Delete(shortcutPath);
                    MessageBox.Show("已从启动文件夹删除快捷方式: " + shortcutPath);
                }
            }
        }




        // 更新上下文菜单状态
        private void UpdateContextMenu(Config config)
        {
            autoStartItem.Checked = config.AutoStartAtBootIsEnable;
            enableFeatureItem.Checked = config.EyeSaverConfig.IsEnable;
            screenDimItem.Checked = config.EyeSaverConfig.ReminderMethod == ReminderMethod.ScreenDim;
            popupReminderItem.Checked = config.EyeSaverConfig.ReminderMethod == ReminderMethod.PopupReminder;

            // 将所有提醒周期项的Checked属性设置为false
            foreach (ToolStripItem item in reminderIntervalItem.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.Checked = false; // 先全部设为未选中
                }
            }

            // 根据appConfig设置正确的选中状态
            foreach (ToolStripItem item in reminderIntervalItem.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem && int.TryParse(menuItem.Text, out int interval))
                {
                    // 检查menuItem的Text是否与appConfig中的值匹配
                    if (interval == config.EyeSaverConfig.ReminderIntervalMinutes)
                    {
                        menuItem.Checked = true;
                        break; // 找到匹配项后跳出循环
                    }
                }
            }
            // ... 更新其他菜单项的状态 ...
        }

        // 切换护眼提醒功能的开关
        private void ToggleFeature(ToolStripMenuItem item)
        {
            appConfig.EyeSaverConfig.IsEnable = !appConfig.EyeSaverConfig.IsEnable;
            UpdateContextMenu(appConfig); // 更新菜单状态
            configManger.SaveConfig(); // 保存配置
        }

        // 更改提醒方式
        private void ChangeReminderMethod(ReminderMethod method)
        {
            appConfig.EyeSaverConfig.ReminderMethod = method;
            UpdateContextMenu(appConfig); // 更新菜单状态
            configManger.SaveConfig(); // 保存配置
        }

        private void CustomIntervalItem_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 检查按键是否为回车键
            if (e.KeyChar == (char)Keys.Return)
            {
                var textBox = sender as ToolStripTextBox;
                if (textBox != null && int.TryParse(textBox.Text, out var customInterval))
                {
                    ChangeReminderInterval(customInterval);
                }
            }
        }
        // 更新提醒周期
        private void ChangeReminderInterval(int minutes)
        {
            appConfig.EyeSaverConfig.ReminderIntervalMinutes = minutes;
            configManger.SaveConfig(); // 保存配置
            UpdateContextMenu(appConfig); // 更新菜单显示
        }

        // ... 其他与菜单相关的方法 ...
        // 添加一个方法来处理点击退出的事件
        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit(); // 这将关闭应用程序
        }

        private void OnAboutClick(object sender, EventArgs e)
        {
            // Create a custom form for the About dialog
            Form aboutForm = new Form()
            {
                Text = "关于 拯救近视人",
                ClientSize = new Size(300, 200), // Set the size you want
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };

            // Add labels to the form
            Label nameLabel = new Label() { Text = "软件名称：拯救近视人", AutoSize = true, Location = new Point(10, 20) };
            Label versionLabel = new Label() { Text = "当前版本：V1.0.0", AutoSize = true, Location = new Point(10, 50) };
            Label authorLabel = new Label() { Text = "作者：MD小智", AutoSize = true, Location = new Point(10, 80) };
            Label websiteLabel = new Label() { Text = "网站：mddxz.top", AutoSize = true, Location = new Point(10, 110) };

            aboutForm.Controls.Add(nameLabel);
            aboutForm.Controls.Add(versionLabel);
            aboutForm.Controls.Add(authorLabel);
            aboutForm.Controls.Add(websiteLabel);

            // Show the form as a dialog
            aboutForm.ShowDialog();
        }
    }
}
