using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace RedisWatch
{
    public partial class RedisWatch : ServiceBase
    {
        private ILog _log = LogManager.GetLogger("logSample");
        private Timer timer;
        public RedisWatch()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            TimerCallback tc = new Watcher().Run;
            timer = new Timer(tc);

            timer.Change( 1000 * 6 * 1,Timeout.Infinite);
           
        }

        protected override void OnStop()
        {
            _log.Info("停止Redis Watch 服务...");
            if (timer != null)
            {
                timer.Dispose();
            }

        }
    }
}
