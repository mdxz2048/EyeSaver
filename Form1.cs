﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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


        public Form1()
        {
            InitializeComponent();
            InitializeTrayIcon();
            this.FormClosing += Form1_FormClosing;

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
                Icon = SystemIcons.Application, // Use a temporary default icon
                Visible = true
            };

            // 添加退出事件处理
            trayIcon.DoubleClick += OnTrayIconDoubleClick; // 双击托盘图标的事件处理
        }

        private void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        // 事件处理：用户选择退出时
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 确保托盘图标在程序退出时被移除
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
            }
        }

    }
}