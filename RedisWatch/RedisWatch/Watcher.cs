using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace RedisWatch
{
   public class Watcher
   {
       private ILog _log = LogManager.GetLogger("system");

       public void Run()
       {
           int error = 0;
           _log.Info("Redis 监听服务正在启动...");
           while (true)
           {
               try
               {
                   CacheHelper.Item_Set("redis__alive__key", "Redis alive");
                   CacheHelper.Item_Remove("redis__alive__key");
               }
               catch
               {
                   error++;
                   _log.Info(string.Format("Redis 第{0}次重启...",error));
                   if (error > 3)
                   {
                       _log.Info("Redis 三次重启失败！");
                       break;
                       
                   }
               }
               Thread.Sleep(5000);
           }
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
