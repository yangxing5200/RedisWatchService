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
        private string _redisStart = ConfigurationManager.AppSettings["RedisStart"];
        private string _redisConf = ConfigurationManager.AppSettings["RedisConf"];
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
            _log.Info("Redis 监听服务正在启动...");
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
                            CacheHelper.Item_Remove(key);
                        }
                        catch
                        {
                        }
                    });
                    //等待两个任务执行完毕
                    Task.WaitAny(taskRedis, taskWatch);
                    if (taskWatchValue == null)
                    {
                        _log.Info(string.Format("Redis 阻塞了，开始杀掉进程..."));
                        var procs = Process.GetProcessesByName("redis-server");
                        if (procs.Length > 0)
                        {
                            procs[0].Kill();
                        }
                        _log.Info(string.Format("Redis 重新启动..."));
                        _log.Info(AppDomain.CurrentDomain.BaseDirectory + "redis\\redis-server.exe" + "       " + AppDomain.CurrentDomain.BaseDirectory + "redis\\redis.conf");
                        RunCmdWithoutResult(AppDomain.CurrentDomain.BaseDirectory + "redis\\redis-server.exe", AppDomain.CurrentDomain.BaseDirectory + "redis\\redis.conf", false);
                        Thread.Sleep(2000);
                    }
                  
                    error = 0;
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                    error++;
                    if (error > 3)
                    {
                        _log.Info("Redis 三次重启失败！");
                        timer.Change(1000 * 2, 1000 * 6);
                        break;
                    }
                    _log.Info(string.Format("Redis 第{0}次启动...{1}", error, ex.Message));

                    _log.Info(string.Format("开始杀掉进程..."));
                    var procs = Process.GetProcessesByName("redis-server");
                    if (procs.Length > 0)
                    {
                        _log.Info("Killing...");
                        procs[0].Kill();
                    }

                    _log.Info(AppDomain.CurrentDomain.BaseDirectory + "redis\\redis-server.exe" + "       " + AppDomain.CurrentDomain.BaseDirectory + "redis\\redis.conf");
                    RunCmdWithoutResult(AppDomain.CurrentDomain.BaseDirectory + "redis\\redis-server.exe", AppDomain.CurrentDomain.BaseDirectory + "redis\\redis.conf", false);
                    _log.Info(string.Format("Redis 已启动"));
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
