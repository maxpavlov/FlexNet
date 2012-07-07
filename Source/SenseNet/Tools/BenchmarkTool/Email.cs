//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Configuration;
//using System.Linq;
//using System.Net;
//using System.Net.Mail;
//using System.Text;
//using System.Threading;

//namespace SenseNet.Benchmarking
//{
//    class Email
//    {
//        private static System.Timers.Timer _emailTimer;
//        private const string EMAIL_BODY = @"<html><body>
//<h2>Benchmark status ({0})</h2>
//<p>
//Avg CPS: <span style='color:Red'>{1}</span> <br/><br/>
//Elapsed time: {2} <br/>
//Folders: {3} <br/>
//Files: {4} <br/>
//Queue length: {5} <br/>
//</p>
//</body></html>
//";

//        private const string FROM_ADDRESS = "benchmark@sensenet.com";
//        private const string FROM_NAME = "SenseNet Benchmark tool";

//        public static void InitEmail()
//        {
//            if (Configuration.AdminEmails.Length == 0 || Configuration.EmailFrequency <= 0)
//                return;

//            _emailTimer = new System.Timers.Timer(Configuration.EmailFrequency * 60 * 1000.0);
//            _emailTimer.Elapsed += EmailTimer_Elapsed;
//            _emailTimer.Start();
//        }

//        private static void EmailTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
//        {
//            try
//            {
//                SendMail();
//            }
//            catch (Exception ex)
//            {
//                Logger.WriteErrorToLog("Email sender error", ex);
//                _emailTimer.Stop();
//            }
//        }

//        private static void SendMail()
//        {
//            var toAddress = string.Join(";", Configuration.AdminEmails);
//            var subject = string.Format("Benchmark status ({0}) - Avg CPS: {1}", Configuration.SessionName, Program._cpsAvgPerfCounter.RawValue);
//            var bodyHtml = string.Format(EMAIL_BODY, 
//                        Configuration.SessionName,
//                        Program._cpsAvgPerfCounter.RawValue,
//                        TimeSpan.FromSeconds(Logger.Elapsed).ToString("g"),
//                        Program._folders,
//                        Program._files,
//                        Program.TaskQueueCount);

//            if (!string.IsNullOrEmpty(Configuration.ElasticEmailUserName))
//                SendElasticEmail(toAddress, subject, bodyHtml, FROM_ADDRESS, FROM_NAME);
//            else
//                SendWithSmtp(subject, bodyHtml, FROM_ADDRESS, FROM_NAME);
//        }

//        private static string SendElasticEmail(string to, string subject, string bodyHtml, string from, string fromName)
//        {
//            var wp = string.IsNullOrEmpty(Configuration.ProxyAddress)
//                         ? null
//                         : new WebProxy(Configuration.ProxyAddress)
//                               {
//                                   Credentials = new NetworkCredential(Configuration.ProxyUserName, Configuration.ProxyPassword)
//                               };
//            var client = wp == null ? new WebClient() : new WebClient {Proxy = wp};
//            var values = new NameValueCollection
//                             {
//                                 {"username", Configuration.ElasticEmailUserName},
//                                 {"api_key", Configuration.ElasticEmailApiKey},
//                                 {"from", from},
//                                 {"from_name", fromName},
//                                 {"subject", subject}
//                             };

//            if (bodyHtml != null)
//                values.Add("body_html", bodyHtml);

//            values.Add("to", to);

//            var response = client.UploadValues("https://api.elasticemail.com/mailer/send", values);
//            return Encoding.UTF8.GetString(response);
//        }

//        private static void SendWithSmtp(string subject, string bodyHtml, string fromAddress, string fromName)
//        {
//            using (var serv = new SmtpClient())
//            {
//                using (var msg = new MailMessage())
//                {
//                    msg.From = new MailAddress(fromAddress, fromName);

//                    foreach (var adminEmail in Configuration.AdminEmails)
//                    {
//                        msg.To.Add(adminEmail);
//                    }

//                    msg.Subject = subject;
//                    msg.Body = bodyHtml;
//                    msg.BodyEncoding = Encoding.ASCII;
//                    msg.IsBodyHtml = true;

//                    serv.Send(msg);
//                }
//            }
//        }
//    }
//}
