using System;
using System.Collections.Generic;
using System.Configuration;
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
        private ILog _log = LogManager.GetLogger("logSample");
        private string _redisStart = ConfigurationManager.AppSettings["RedisStart"];
        private string _redisConf = ConfigurationManager.AppSettings["RedisConf"];
        Timer timer;
        public void Run(object o)
        {
            var key = "redis__alive__key";
            var value = "Redis alive";

            int error = 0;
            _log.Info("Redis 监听服务正在启动...");
            while (true)
            {
                try
                {
                    CacheHelper.Item_Set(key, value);
                    CacheHelper.Item_Remove(key);
                }
                catch
                {
                    error++;
                    if (error > 3)
                    {
                        _log.Info("Redis 三次重启失败！");
                        Thread.Sleep(1000 * 60 * 10);
                        break;
                    }
                    _log.Info(string.Format("Redis 第{0}次启动...", error));
                  
                    Task.Factory.StartNew(() =>
                    {
                        RunCmdWithoutResult(_redisStart, _redisConf, true);
                    });

                    _log.Info(string.Format("尝试写入信息，{0}：{1}", key, value));
                    CacheHelper.Item_Set(key, value);
                    _log.Info(string.Format("尝试读取信息，{0}", key));
                    var v = CacheHelper.Item_Get<object>(key);
                    _log.Info(string.Format("读取完成，{0}：{1}", key, v));
                    _log.Info("Redis 正常工作。");

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
