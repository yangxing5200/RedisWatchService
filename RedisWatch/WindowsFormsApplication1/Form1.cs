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
using Common.Logging;
using RedisWatch;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private ILog _log = LogManager.GetLogger("logSample");
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.FileName = @"C:\Users\Jason\Desktop\redis\start.bat";
            //startInfo.Arguments = "";
            //Process.Start(startInfo);

            Watcher a=new Watcher();
            a.Start();
          //  RunCmdWithoutResult(@"C:\Users\Jason\Desktop\redis\start.bat","", true);
            // Process.Start(@"C:\Users\Jason\Desktop\redis\install.vbs");
        }
        public static void RunCmdWithoutResult(string file, string command, bool wait)
        {
            //创建实例
            Process p = new Process();
            //设定调用的程序名，不是系统目录的需要完整路径
            p.StartInfo.FileName = file;
            //传入执行参数
            p.StartInfo.Arguments = " " + command;
            p.StartInfo.UseShellExecute = false;
            //是否重定向标准输入
            p.StartInfo.RedirectStandardInput = false;
            //是否重定向标准转出
            p.StartInfo.RedirectStandardOutput = false;
            //是否重定向错误
            p.StartInfo.RedirectStandardError = false;
            //执行时是不是显示窗口
            p.StartInfo.CreateNoWindow = true;
            //启动
            p.Start();
            if (wait)
            {
                //是不是需要等待结束
                p.WaitForExit();
            }
            p.Close();
        }
    }
}
