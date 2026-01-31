using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Utility
{
    public class LogService
    {
        private readonly ILogger<LogService> _logger;

        public LogService(ILogger<LogService> logger)
        {
            _logger = logger;

        }

        public void LogExceptionError(Exception ex)
        {
            var logDirectory = "EmpoweRxLogs";

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var logFile = logDirectory + "/" + "Log-" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";

            if (!File.Exists(logFile))
            {
                File.Create(logFile).Dispose();
            }

            using (var txtWriter = new StreamWriter(logFile, true))
            {
                Log(ex, txtWriter);
            }

        }

        public void Log(Exception ex, TextWriter txtWriter)
        {
            txtWriter.Write("\r\nLog Entry : ");
            txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            txtWriter.WriteLine("  :");
            txtWriter.WriteLine("  :{0}", $"[Error]: {ex.Message}");
            var stackTrace = ex.StackTrace?.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                                   .Take(1);
            txtWriter.WriteLine("  :{0}", $"[StackTrace]: {string.Join(Environment.NewLine, stackTrace!)}");
            txtWriter.WriteLine("---------------------------------------------------------------------------------------------");
        }

        public void LogCustom(string message, string logfile)
        {
            var logDirectory = "EmpoweRxLogs";

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var logFile = logDirectory + "/" + logfile + "-" + DateTime.Now.ToString("MM-dd-yyyy") + ".txt";

            if (!File.Exists(logFile))
            {
                File.Create(logFile).Dispose();
            }

            using (var txtWriter = new StreamWriter(logFile, true))
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", message);
                txtWriter.WriteLine("---------------------------------------------------------------------------------------------");
            }

        }
    }
}
