using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common.Logging;
using Quartz;

namespace QuartzJobService.Core
{
    public class JobTest : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            JobHelper.Log.Info("I am alive!");
        }
    }
}
