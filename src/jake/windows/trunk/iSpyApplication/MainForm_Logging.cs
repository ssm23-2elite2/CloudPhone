﻿using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace iSpyApplication
{
    public partial class MainForm
    {
        private static readonly StringBuilder LogFile = new StringBuilder(100000);
        private static readonly StringBuilder PluginLogFile = new StringBuilder(100000);

        internal static void LogExceptionToFile(Exception ex, string info)
        {
            ex.HelpLink = info + ": " + ex.Message;
            LogExceptionToFile(ex);
        }


        internal static void LogExceptionToFile(Exception ex)
        {
            if (!_logging)
                return;

            try
            {
                string em = ex.HelpLink + "<br/>" + ex.Message + "<br/>" + ex.Source + "<br/>" + ex.StackTrace +
                            "<br/>" + ex.InnerException + "<br/>" + ex.Data;
                LogFile.Append("<tr><td style=\"color:red\" valign=\"top\">Exception:</td><td valign=\"top\">" +
                               DateTime.Now.ToLongTimeString() + "</td><td valign=\"top\">" + em + "</td></tr>");
            }
            catch
            {

            }
        }
        internal static void LogMessageToFile(String message, string e)
        {
            LogMessageToFile(String.Format(message, e));
        }
        internal static void LogMessageToFile(String message)
        {
            if (!_logging)
                return;

            try
            {
                LogFile.Append("<tr><td style=\"color:green\" valign=\"top\">Message</td><td valign=\"top\">" +
                               DateTime.Now.ToLongTimeString() + "</td><td valign=\"top\">" + message + "</td></tr>");
            }
            catch
            {
                //do nothing
            }
        }

        internal static void LogPluginToFile(string name, int id, string action, string detail)
        {
            DateTime dt = Helper.Now;
            PluginLogFile.Append("<message name=\"" + name + "\" id=\"" + id + "\" action=\"" + action + "\" timestamp=\"" + dt.Ticks+"\">" + detail.Replace("&", "&amp;") + "</message>");
        }

        internal static void LogErrorToFile(String message)
        {
            if (!_logging)
                return;

            try
            {
                LogFile.Append("<tr><td style=\"color:red\" valign=\"top\">Error</td><td valign=\"top\">" +
                               DateTime.Now.ToLongTimeString() + "</td><td valign=\"top\">" + message + "</td></tr>");
            }
            catch
            {
                //do nothing
            }
        }

        internal static void LogWarningToFile(String message)
        {
            if (!_logging)
                return;

            try
            {
                LogFile.Append("<tr><td style=\"color:orange\" valign=\"top\">Warning</td><td valign=\"top\">" +
                               DateTime.Now.ToLongTimeString() + "</td><td valign=\"top\">" + message + "</td></tr>");
            }
            catch
            {
                //do nothing
            }
        }

        private void WriteLogs()
        {
            if (_logging)
            {
                try
                {
                    if (LogFile.Length > Conf.LogFileSizeKB * 1024)
                    {
                        LogFile.Append(
                            "<tr><td style=\"color:red\" valign=\"top\">Logging Exiting</td><td valign=\"top\">" +
                            DateTime.Now.ToLongTimeString() +
                            "</td><td valign=\"top\">Logging is being disabled as it has reached the maximum size (" +
                            Conf.LogFileSizeKB + "kb).</td></tr>");
                        _logging = false;
                    }
                    if (_lastlog.Length != LogFile.Length)
                    {
                        string fc = LogTemplate.Replace("<!--CONTENT-->", LogFile.ToString()).Replace("<!--VERSION-->",
                                                                                                      Application.
                                                                                                          ProductVersion + " Platform: " + Program.Platform);
                        File.WriteAllText(Program.AppDataPath + @"log_" + NextLog + ".htm", fc);
                        _lastlog = LogFile.ToString();
                    }
                }
                catch (Exception)
                {
                    _logging = false;
                }
            }

            try
            {
                if (_lastPluginLog.Length != PluginLogFile.Length)
                {
                    string fc = PluginLogTemplate.Replace("<!--CONTENT-->", PluginLogFile.ToString());
                    File.WriteAllText(Program.AppDataPath + @"plugin_log_" + NextLog + ".xml", fc);
                    _lastPluginLog = PluginLogFile.ToString();
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
