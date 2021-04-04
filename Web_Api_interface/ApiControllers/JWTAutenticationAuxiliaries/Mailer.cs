using Flight_Center_Project_FinalExam_DAL;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using Web_Api_interface.Models;

namespace Web_Api_interface.ApiControllers.JWTAutenticationAuxiliaries
{
    public class Mailer
    {

        const string SENDER_HOST_DOMAIN = "demo.com";
        const string SENDER_MAIL = "user1@demo.com";
        const string SENDER_PASSWORD = "12345";

        private string _userEmail;

        //from secrets vault, "pathToRegexpressionForEmailEvaluationFile" is secret name
        private string _pathToRegExTextFile;

        private Admin_ValuesDAOMSSQL<Admin_Value> _adminValuesDAO = new Admin_ValuesDAOMSSQL<Admin_Value>();
        

        public Mailer(string userEmail) : this()
        {
            _userEmail = userEmail;
        }

        public Mailer()
        {
            _pathToRegExTextFile = ConfigurationManager.AppSettings["pathToRegexpressionForEmailEvaluationFile"];
        }

        public BooleanFunctionAnswer<string> SendEmailToUser()
        {
            return SendEmailToUser(_userEmail);
        }

        public BooleanFunctionAnswer<string> SendEmailToUser(string userEmail)
        {
            _userEmail = userEmail;

            string messageSubject = "Flights Management System - Unregistereg Goggle user";
            string messageBody = "You authorized by Google, but unauthorized by Flight Management System, so we're sent you a confirmation Email.";

            Mailer mailer = new Mailer(userEmail);
            BooleanFunctionAnswer<string> answer = mailer.SendMail(messageSubject, messageBody);

            answer.OptionalProps.Add("messageSubject", messageSubject);
            answer.OptionalProps.Add("messageBody", messageBody);

            return answer;
        }

        private BooleanFunctionAnswer<string> SendMail(string messageSubject, string messageBody)
        {
            if(ValidateEmail(_userEmail) == true)
            {
                return SendMailInternal(_userEmail, SENDER_MAIL, SENDER_PASSWORD, SENDER_HOST_DOMAIN, messageSubject, messageBody);
            }
            else
            {
                return new BooleanFunctionAnswer<string>(false, $"Mail message WASN'T sent to the user because the user email \"{_userEmail}\" is NOT valid.");
            }
        }


        private bool ValidateEmail(string email)
        {
            string expression = File.ReadAllText(_pathToRegExTextFile);
            Regex rg = new Regex(expression);

            Match match = rg.Match(email);
            return match.Success;
        }


        private BooleanFunctionAnswer<string> SendMailInternal(string userMail, string senderMail, string senderPassword, string senderHost, string subject, string body)
        {

            List<Admin_Value> mailAddesses = _adminValuesDAO.GetAll();

            BooleanFunctionAnswer<string> result = new BooleanFunctionAnswer<string>(true, "Mail message was sent sucsessful to the user, no exceptions was thrown.");
            string senderID = senderMail; //"user1@demo.com"
            string senderPasswordInternal = senderPassword; //"12345"
            try
            {
                SmtpClient smtp = new SmtpClient
                {
                    Host = senderHost, //"demo.com"
                    Port = 25,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new System.Net.NetworkCredential(senderID, senderPasswordInternal),
                    Timeout = 10000,
                    EnableSsl = false,
                };

                MailMessage message = new MailMessage(senderID, userMail, subject, body);
                for(int i = 0; i < mailAddesses.Count; i++)
                {
                    message.To.Add(mailAddesses[i].adminMail);
                }
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.GetType().Name}\n\n{ex.Message}\n\n\n{ex.StackTrace}");
                string exceptionMessages = $"Exception type: {ex.GetType().Name}\n\nExceprion message:\n{ex.Message}\n\n\nException stack trace:\n{ex.StackTrace}";

                result = new BooleanFunctionAnswer<string>(false, "The mailmessage WASN'T sent to the user, exception was thrown:\n" + exceptionMessages);
            }
            return result;
        }

    }
}