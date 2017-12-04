using System;
using System.Configuration;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Xml.Linq;

namespace LoggerNET
{
    /// <summary>
    /// Enum with types of log message
    /// </summary>
    public enum TypeOfLog {Debug, Error, Info, Warning}
    /// <summary>
    /// Interface with overload log method
    /// </summary>
    public interface ILoggable
    {
        void Log(TypeOfLog type, DateTime date, Type module, string logString);
        void Log(DateTime date, Type module, string logString);
        void Log(Type module, string logString);
    }
    /// <summary>
    /// Class with fields which contain data of log
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]//hide class from intellisense
    [Serializable]
    public class LoggerAssist : IDisposable
    {
        public TypeOfLog Type;
        public DateTime Date;
        public string Module;
        public string LogString;
        /// <summary>
        /// Default constructor for serializable
        /// </summary>
        public LoggerAssist() { }
        /// <summary>
        /// Constructor with next parameters: type, date, module, logString
        /// </summary>
        /// <param name="type">Type of log message</param>
        /// <param name="date">Date of log message</param>
        /// <param name="module">Module where log was recorded</param>
        /// <param name="logString">Text of message</param>
        public LoggerAssist(TypeOfLog type, DateTime date, Type module, string logString)
        {
            Type = type;
            Date = date;
            Module = module.ToString();
            LogString = logString;
        }
        /// <summary>
        /// Constructor with next parameters: date, module, logString
        /// </summary>
        /// <param name="date">Date of log message</param>
        /// <param name="module">Module where log was recorded</param>
        /// <param name="logString">Text of message</param>
        public LoggerAssist(DateTime date, Type module, string logString)
        {
            Date = date;
            Module = module.ToString();
            LogString = logString;
        }
        /// <summary>
        /// Constructor with next parameters: module, logString
        /// </summary>
        /// <param name="module">Module where log was recorded</param>
        /// <param name="logString">Text of message</param>
        public LoggerAssist(Type module, string logString)
        {
            Module = module.ToString();
            LogString = logString;
        }

        protected virtual void Dispose(bool disposing)
        {
            //Console.WriteLine($"Dispose with param {disposing}");
            if (disposing)
            {
                //Dispose unmanaged resources which I haven't
            }
        }

        ~LoggerAssist()
        {
            //Console.WriteLine("Finalizing");
            Dispose(false);
        }

        public void Dispose()
        {
            //Console.WriteLine("Finalizing");
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
    /// <summary>
    /// Class which realizes ILoggable interface. All overloads of Log method intended for write logs in files and console.
    /// </summary>
    public class Logger : ILoggable
    {
        /// <summary>
        /// Make log with next parameters: type, date, module, logString
        /// </summary>
        /// <param name="type">Type of log message</param>
        /// <param name="date">Date of log message</param>
        /// <param name="module">Module where log was recorded</param>
        /// <param name="logString">Text of message</param>
        public void Log(TypeOfLog type, DateTime date, Type module, string logString)
        {
            LoggerAssist ls = new LoggerAssist(type, date, module, logString);
            Console.WriteLine($"{type}\t{date}\t{module}\t{logString}");
            string[] arr = new string[1] { String.Concat(type.ToString(), "\t", date.ToString(), "\t", module.ToString(), "\t", logString) };
            Serialize serialize = new Serialize();
            serialize.StartSerialize(ls, arr);
        }
        /// <summary>
        /// Make log with next parameters: date, module, logString
        /// </summary>
        /// <param name="date">Date of log message</param>
        /// <param name="module">Module where log was recorded</param>
        /// <param name="logString">Text of message</param>
        public void Log(DateTime date, Type module, string logString)
        {
            LoggerAssist ls = new LoggerAssist(date, module, logString);
            Console.WriteLine($"\t{date}\t{module}\t{logString}");
            string[] arr = new string[1] { String.Concat("\t", date.ToString(), "\t", module.ToString(), "\t", logString) };
            Serialize serialize = new Serialize();
            serialize.StartSerialize(ls, arr);
        }
        /// <summary>
        /// Make log with next parameters: module, logString
        /// </summary>
        /// <param name="module">Module where log was recorded</param>
        /// <param name="logString">Text of message</param>
        public void Log(Type module, string logString)
        {
            LoggerAssist ls = new LoggerAssist(module, logString);
            Console.WriteLine($"\t\t\t\t{module}\t{logString}");
            string[] arr = new string[1] { String.Concat("\t\t\t\t", module.ToString(), "\t", logString) };
            Serialize serialize = new Serialize();
            serialize.StartSerialize(ls, arr);
        }
    }
    /// <summary>
    /// This class is for serialization of LoggerAssist type
    /// </summary>
    class Serialize : IDisposable
    {
        /// <summary>
        /// Method which reads app settings from config file and make log in specified format
        /// </summary>
        /// <param name="log">Object which we need serialize</param>
        /// <param name="arr">Text which we need to write in file if we don't need serialization</param>
        public void StartSerialize(LoggerAssist log, string[] arr)
        {
            switch (ConfigurationManager.AppSettings["format"])
            {
                case "xml":
                    XmlSerialize(log);
                    break;
                case "json":
                    JsonSerialize(log);
                    break;
                case "txt":
                    PlainText(arr);
                    break;
                default:
                    Console.WriteLine("Unexpected format");
                    break;
            }
        }
        /// <summary>
        /// Serialize object in xml format
        /// </summary>
        /// <param name="log">Object which we need to serialize</param>
        public void XmlSerialize(LoggerAssist log)
        {
            try
            {
                if (!File.Exists($@"{ConfigurationManager.AppSettings["path"]}\Log.{ConfigurationManager.AppSettings["format"]}"))
                {
                    LoggerAssist[] logs = new LoggerAssist[] { log };
                    XmlSerializer xmlFormatter = new XmlSerializer(typeof(LoggerAssist[]));
                    using (var fStream = new FileStream($@"{ConfigurationManager.AppSettings["path"]}\Log.{ConfigurationManager.AppSettings["format"]}", FileMode.Create, FileAccess.Write))
                    {
                        xmlFormatter.Serialize(fStream, logs);
                    }
                }
                else
                {
                    XDocument doc = XDocument.Load($@"{ConfigurationManager.AppSettings["path"]}\Log.{ConfigurationManager.AppSettings["format"]}");
                    XElement school = doc.Element("ArrayOfLoggerAssist");
                    school.Add(new XElement("LoggerAssist",
                               new XElement("Type", log.Type),
                               new XElement("Date", log.Date),
                               new XElement("Module", log.Module),
                               new XElement("LogString", log.LogString)));
                    doc.Save($@"{ConfigurationManager.AppSettings["path"]}\Log.{ConfigurationManager.AppSettings["format"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("-> Saved log in XML format.");
        }
        /// <summary>
        /// Write text of log in txt format
        /// </summary>
        /// <param name="arr">Text of log</param>
        public void PlainText(string[] arr)
        {
            try
            {
                File.AppendAllLines($@"{ConfigurationManager.AppSettings["path"]}\Log.{ConfigurationManager.AppSettings["format"]}", arr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("-> Saved log in txt format.");
        }
        /// <summary>
        /// Serialize object in xml format
        /// </summary>
        /// <param name="log">Object which we need to serialize</param>
        public void JsonSerialize(LoggerAssist log)
        {
            string[] jsonData = new string[] { JsonConvert.SerializeObject(log) };
            try
            {
                File.AppendAllLines($@"{ConfigurationManager.AppSettings["path"]}\Log.{ConfigurationManager.AppSettings["format"]}", jsonData);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            Console.WriteLine("-> Saved log in Json format.");
        }

        protected virtual void Dispose(bool disposing)
        {
            //Console.WriteLine($"Dispose with param {disposing}");
            if (disposing)
            {
                //Dispose unmanaged resources which I haven't
            }
        }

        ~Serialize()
        {
            //Console.WriteLine("Finalizing");
            Dispose(false);
        }

        public void Dispose()
        {
            //Console.WriteLine("Finalizing");
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}