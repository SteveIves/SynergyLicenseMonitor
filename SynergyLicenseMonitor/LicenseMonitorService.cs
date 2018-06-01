using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynergyLicenseMonitor
{
    public partial class LicenseMonitorService : ServiceBase
    {
        Thread workerThread;

        public LicenseMonitorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            runMethodInThread(doMonitoring);
        }

        protected override void OnStop()
        {
            workerThread.Abort();
        }

        private void runMethodInThread(Action action)
        {
            workerThread = new Thread(new ThreadStart(action));
            workerThread.Start();
        }

        private void doMonitoring()
        {
            var sleepTime = Properties.Settings.Default.IntervalInSeconds * 1000;

            var logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), String.Format("{0}-SynergyLicenseUsage.csv", DateTime.Now.ToString("yyyy-MM-dd-HH-mm")));

            if (File.Exists(logFile))
                File.Delete(logFile);

            var licenseCodes = Properties.Settings.Default.LicenseCodes;
            var sb = new StringBuilder("Date,Time");

            foreach (var licenseCode in licenseCodes)
            {
                sb.Append(String.Format(",{0}", licenseCode));
            }

            sb.Append("\r\n");

            //Create a new log file
            File.AppendAllText(logFile, sb.ToString());

            while (true)
            {
                sb = new StringBuilder(String.Format("{0},{1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString()));
                foreach (var licenseCode in licenseCodes)
                {
                    sb.Append(String.Format(",{0}", LicensingToolkit.LicenseUsage(licenseCode)));
                }

                sb.Append("\r\n");

                File.AppendAllText(logFile, sb.ToString());

                Thread.Sleep(sleepTime);
            }
        }
    }
}
