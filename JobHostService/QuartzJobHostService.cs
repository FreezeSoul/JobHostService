using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using QuartzJobService.Core;

namespace JobHostService
{
    public partial class QuartzJobHostService : ServiceBase
    {
        private JobHelper _jobHelper = null;

        public QuartzJobHostService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (null == _jobHelper)
            {
                _jobHelper = new JobHelper();
            }
            _jobHelper.Start();
        }

        protected override void OnStop()
        {
            if (null != _jobHelper)
            {
                _jobHelper.Stop();
            }
        }
    }
}
