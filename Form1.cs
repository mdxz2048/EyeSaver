using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EyeSaver
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu = new ContextMenuStrip();
        private MenuManager menuManager;
        private EyeSaveScreen eyeSaveScreen; // EyeSaveScreen 实例



        public Form1()
        {
            Debug.WriteLine($"{DateTime.Now} " +"开始运行");
            InitializeComponent();
            InitializeTrayIcon();
            this.FormClosing += Form1_FormClosing;

            // 注册配置改变事件
            menuManager.configManger.ConfigChanged += ConfigManger_ConfigChanged;
            eyeSaveScreen = new EyeSaveScreen(menuManager.configManger.AppConfig);

        }

        private void InitializeTrayIcon()
        {
            if (this.components == null)
            {
                this.components = new System.ComponentModel.Container();
            }
            // 创建托盘菜单
            trayMenu = new ContextMenuStrip();
            menuManager = new MenuManager(trayMenu); // MenuManager将负责填充菜单项

            // 创建并设置托盘图标
            trayIcon = new NotifyIcon(this.components)
            {
                ContextMenuStrip = trayMenu,
                Icon = Properties.Resources.NotifyIconTrayIcon,
                Visible = true
            };

            // 添加退出事件处理
            //trayIcon.DoubleClick += OnTrayIconDoubleClick; // 双击托盘图标的事件处理
        }

        private void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        // 事件处理：用户选择退出时
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 取消注册配置改变事件
            menuManager.configManger.ConfigChanged -= ConfigManger_ConfigChanged;

            // 确保托盘图标在程序退出时被移除
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
            }
        }

        private void ConfigManger_ConfigChanged(Config newConfig)
        {
            // 配置改变时的操作
            Debug.WriteLine($"{DateTime.Now} " + "配置发生改变");
            eyeSaveScreen.Deinitialize(); // 停止当前的定时器和屏幕变暗逻辑
            eyeSaveScreen.Initialize(newConfig); // 用新的配置重新初始化
        }

    }
}