using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Manifold.ImageServer.OpenStreetMaps
{


    public class FileLog : ILog
    {

        public static ILog Build()
        {
            //create the log in the current directory by default
            string fileName = string.Format(@"{0}{1}", Path.GetTempPath(), "ISI.log");
            var log = new FileLog(fileName);
            return log;
        }

        private readonly string _fileName;

        public FileLog(string fileName)
        {
            _fileName = fileName;
            FileName = fileName;
        }

        public string FileName { get; set; }

        public void Debug(string message)
        {
            using (StreamWriter sw = File.AppendText(_fileName))
            {
                sw.WriteLine("DEBUG:@{0}: {1}",DateTime.Now,message);
            }	
        }

        public void Error(string message, Exception exception)
        {
            using (StreamWriter sw = File.AppendText(_fileName))
            {
                sw.WriteLine("Error:@{0}: {1}",DateTime.Now,message, exception.StackTrace);
                sw.WriteLine("Error:@{0}: {1}   {2}",DateTime.Now,exception.Message,exception.StackTrace);
            }	
        }

        public void Info(string message)
        {
            using (StreamWriter sw = File.AppendText(_fileName))
            {
                sw.WriteLine("Info:@{0}: {1}",DateTime.Now,message);
            }	
        }
    }

    public interface ILog
    {
        string FileName { get; set; }
        void Debug(string message);
        void Error(string message, Exception exception);
        void Info(string message);
    }
}
