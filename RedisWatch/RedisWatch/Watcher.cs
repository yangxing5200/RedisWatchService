using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace RedisWatch
{
    public class Watcher
    {
        private ILog _log = LogManager.GetLogger("logSample");
  
        Timer timer;



        public void Start()
        {
            TimerCallback tc = Run;
            timer = new Timer(tc);
            timer.Change(1000 * 2, 1000 * 6);
        }
        public void Run(object o)
        {
            timer.Change(Timeout.Infinite, 1000 * 30);
            const string key = "redis__alive__key";
            const string value = "Redis alive";

            int error = 0;
            _log.Info("Redis Listener Service Starting...");
            while (true)
            {

                try
                {
                    object taskWatchValue = null;
                    //预防 redis 卡死
                    var taskWatch = Task.Factory.StartNew(() => Thread.Sleep(3000));
                    var taskRedis = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            CacheHelper.Item_Set(key, value);
                            taskWatchValue = CacheHelper.Item_Get<object>(key);
                            _log.Info(string.Format("Content: {0}", taskWatchValue));
                          
                        }
                        catch
                        {
                        }
                    });
                    //等待两个任务执行完毕
                    Task.WaitAny(taskRedis, taskWatch);
                    if (taskWatchValue == null)
                    {
                        _log.Info(string.Format("Redis connection timeout"));
                        var procs = Process.GetProcessesByName("redis-server");
                        if (procs.Length > 0)
                        {
                            _log.Info(string.Format("Kill process..."));
                            procs[0].Kill();
                        }
                        _log.Info(string.Format("Redis restart..."));
                        _log.Info(AppDomain.CurrentDomain.BaseDirectory + "redis\\redis-server.exe" + "       " + AppDomain.CurrentDomain.BaseDirectory + "redis\\redis.conf");
                        RunCmdWithoutResult(AppDomain.CurrentDomain.BaseDirectory + "redis\\redis-server.exe", AppDomain.CurrentDomain.BaseDirectory + "redis\\redis.conf", false);
                        Thread.Sleep(1000);
                        _log.Info(string.Format("Redis restart success"));
                    }
                  
                    error = 0;
                }
                catch (Exception ex)
                {
                  
                }
                Thread.Sleep(10 * 1000);
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
