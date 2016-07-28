using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ClipBoard
{
    public partial class Form1 : Form
    {
        //定义数据存放的目录
        string Dir = "ClipBoard";

        /****************引入系统相关API******************/
        [DllImport("user32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private static int WM_CLIPBOARDUPDATE = 0x031D;

        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                try
                {
                    if (listBox1.Items.Count > 100) {
                        listBox1.Items.RemoveAt(0);
                    }

                    //根据.net自带Clipboard类，获取裁切板中的数据
                    if (Clipboard.ContainsText())
                    {
                        //将数据存放到本地
                        string fileName = Dir + "/" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".txt";
                        listBox1.Items.Add(fileName);
                        File.AppendAllText(fileName, Clipboard.GetText(), Encoding.UTF8);
                    }
                    else if (Clipboard.ContainsFileDropList())
                    {
                        //Clipboard.GetFileDropList();
                    }
                    else if (Clipboard.ContainsImage())
                    {
                        //Clipboard.GetImage();
                    }
                    else if (Clipboard.ContainsAudio())
                    {
                        //Clipboard.GetAudioStream();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        public Form1()
        {
            InitializeComponent();
            this.FormClosed += Form1_FormClosed;         
            this.SizeChanged += Form1_SizeChanged;
            this.listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;            
        }

        void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                string value = listBox1.SelectedItem.ToString();
                if (File.Exists(value))
                {
                    richTextBox1.Text = File.ReadAllText(value, Encoding.UTF8);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //添加启动
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                key.SetValue("ClipBoard", Application.ExecutablePath.ToString());
            }
            catch (Exception ex) {
                MessageBox.Show("请使用管理员方式运行！");
            }
            
            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }

            foreach (string info in Directory.GetFiles(Dir))
            {
                listBox1.Items.Add(info);
            }

            //添加裁切板监听
            AddClipboardFormatListener(this.Handle);
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //移除裁切板监听
            RemoveClipboardFormatListener(this.Handle);
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = listBox1.SelectedItem.ToString();
                if (File.Exists(path))
                {
                    File.Delete(path);
                    listBox1.Items.Remove(listBox1.SelectedItem);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void 清空ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (string file in Directory.GetFiles(Dir))
            {
                File.Delete(file);
            }
            listBox1.Items.Clear();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }
    }
}
