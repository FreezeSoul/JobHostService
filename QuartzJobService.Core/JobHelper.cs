using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Common.Logging;
using Quartz;
using Quartz.Impl;
using System.IO;
using System.Windows.Forms;

namespace QuartzJobService.Core
{
    public class JobHelper
    {
        private static IScheduler _sched = null;
        public static readonly ILog Log = LogManager.GetLogger(typeof(JobHelper));

        public void Start()
        {
            try
            {
                Log.Info(m => m("Scheduler Start"));
                if (null == _sched)
                {
                    ISchedulerFactory sf = new StdSchedulerFactory();
                    _sched = sf.GetScheduler();
                }
                _sched.Start();

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        public void Stop()
        {
            try
            {
                if (null != _sched)
                {
                    Log.Info(m => m("Scheduler Shutdown"));
                    _sched.Shutdown(true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }
    }
}
