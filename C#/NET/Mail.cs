using System;
using System.Net;
using System.Net.Mail;

namespace Snippets
{

    /// <summary>
    ///     Errors that the <see cref="Mail"/> class may return. />
    /// </summary>
    public enum MailError
    {
        /// <summary> No errors, success. </summary>        
        NO_ERROR = 0,
        /// <summary> SMTP not configured. </summary>        
        SMTP_NOT_INITIALIZED = 1,
        /// <summary> Mail could not be set. It may be due to a wrong SMTP configuration </summary>        
        SEND_ERROR = 2
    }

    /// <summary>
    ///     Class to configure a SMTP client and send mail through it. 
    /// </summary>    
    public class Mail
    {
        /// <summary> SMTP Client instance. </summary>
        SmtpClient smtp;

        /// <summary> User registered on the SMTP server.</summary>
        private string username;

        /// <summary> Password for the given user. </summary>
        private string password;

        /// <summary> Whether default credentials will be used for authentication or not. </summary>
        private bool defaultCredentials = false;

        /// <summary>
        ///     Initializes the SMTP client for sending messages. This must be done before sent mail.
        ///     The SMTP server must be initialized before sending emails.
        /// </summary>
        /// <example> 
        ///     MailError err = mail.InicializarSMTP(
        ///         "smtp.mysmtpserver.es",
        ///         587,
        ///         "john@doe.es",
        ///         "mypass",
        ///         true,
        ///         false
        ///         );
        /// </example>
        /// <param name="host"> SMTP server address. </param>
        /// <param name="port"> SMTP port. Commonly used ports are 465 for SSL connection and 587 for TSL.</param>
        /// <param name="username"> Username for the credentials.</param>
        /// <param name="password"> Password of the given username. </param>
        /// <param name="enableSSL"> Allows you to use an SSL encrypted connection. </param>
        /// <param name="defaultCredentials"> Use defailt credentials insetead of user/password. </param>
        /// <returns><see cref="MailError"/> con información del resultado.</returns>
        public MailError InitSMTP(string host, int port, string username, string password, bool enableSSL, bool defaultCredentials)
        {
            try
            {
                smtp = new SmtpClient(host, port)
                {
                    EnableSsl = enableSSL,
                    UseDefaultCredentials = defaultCredentials
                };

                if (!defaultCredentials)
                {
                    smtp.Credentials = new NetworkCredential(username, password);
                }

                this.username = username;
                this.password = password;
                this.defaultCredentials = defaultCredentials;
            }
            catch (Exception)
            {
                return MailError.SMTP_NOT_INITIALIZED;
            }

            return MailError.NO_ERROR;
        }


        /// <summary>
        ///     Send mail through a configured SMTP server. 
        /// </summary>
        /// <example>
        ///     err = correo.EnviarCorreo(
        ///         "john@doe.es;jane@doe.es",
        ///         "david@doe.es",
        ///         "My mail subject",
        ///         "<h1>Title</h1><p>Hello World!</p>",
        ///         MailPriority.Normal,
        ///         true);        
        /// </example>
        /// <param name="t">Receivers of the mail separated by ";".</param>
        /// <param name="from">Email address of the sender.</param>
        /// <param name="subject">Mail subject.</param>
        /// <param name="content">Content of the mail. It may be plain text or HTML if the parameter HTMLConent is true.</param>
        /// <param name="fromName">Name of the sender. For exmple: John Doe (dohn@doe.com) </param>
        /// <param name="priority">Priority of the mail, see <see cref="MailPriority"/></param>
        /// <param name="htmlContent">Establish the format of the content, plain text (false) or HTML (true)/param>
        /// <returns>See <see cref="MailError"/></returns>
        public MailError SendMail(
            string to,
            string from,
            string subject,
            string content,
            string fromName = "",
            MailPriority priority = MailPriority.Normal,
            bool htmlContent = false)
        {

            // Check that smtp is intialized. 
            if (smtp == null)
            {
                return MailError.SMTP_NOT_INITIALIZED;
            }

            if (smtp.Port == 465)
            {
                // This port is used for not safe mail sending
                return SendUnsafeMail(smtp.Host, smtp.Port, smtp.EnableSsl, to, subject, content, htmlContent);
            }
            else
            {
                MailMessage mail = new MailMessage();
                mail.From = (fromName != "") ? new MailAddress(from, fromName) : new MailAddress(from);
                mail.Subject = subject;
                mail.Body = content;
                mail.IsBodyHtml = htmlContent;
                mail.Priority = (MailPriority)(int)priority;
                // Add receivers
                foreach (string receiver in to.Split(';'))
                {
                    if (receiver != "")
                    {
                        mail.To.Add(receiver);
                    }
                }
                // TODO: Add attachments
                // Send mail
                try
                {
                    smtp.Send(mail);
                    mail.Dispose();
                    return MailError.NO_ERROR;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    // Try to send mail using unsafe mode
                    return SendUnsafeMail(smtp.Host, smtp.Port, smtp.EnableSsl, to, subject, content, htmlContent);
                }
            }
        }

        /// <summary>
        /// Send mail using deprecated .NET functionality. This might work for SMTP servers working unider old 465 port.
        /// </summary>
        /// <param name="smtpServer">SMTP server address</param>
        /// <param name="port">SMTP server port</param>
        /// <param name="useSSL_TSL">Wherther SSL or TSL will be used or not.</param>
        /// <param name="receiver">Receiver list separated by ';'.</param>
        /// <param name="subject">Mail suubject.</param>
        /// <param name="content">Mail content in html or plain text</param>
        /// <param name="contentInHtml">Set to true to indicate the content is HTML. False to indicate the content is plain text. </param>        
        /// <returns></returns>
        public MailError SendUnsafeMail(string smtpServer, int port, bool useSSL_TSL, string receiver, string subject, string content, bool contentInHtml)
        {
            try
            {
                System.Web.Mail.MailMessage myMail = new System.Web.Mail.MailMessage();
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserver", smtpServer);
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", port.ToString().Trim());
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusing", "2");

                //smtpauthenticate: Authentication method
                // 0 - cdoAnonymous
                // 1 - cdoBasic
                // 2 - cdoNTLM 
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", defaultCredentials ? "2" : "1");
                //Use 0 for anonymous
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", username);
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", password);
                myMail.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpusessl", useSSL_TSL);
                myMail.From = username;
                myMail.To = receiver;
                myMail.Subject = subject;
                myMail.BodyFormat = contentInHtml ? System.Web.Mail.MailFormat.Html : System.Web.Mail.MailFormat.Text;
                myMail.Body = content;

                //if (attachments.Trim() != "") {
                //    TO DO: attachments
                ////    System.Web.Mail.MailAttachment MyAttachment = new System.Web.Mail.MailAttachment(attachments);
                //    myMail.Attachments.Add(MyAttachment);
                //    myMail.Priority = System.Web.Mail.MailPriority.High;
                //}

                System.Web.Mail.SmtpMail.SmtpServer = String.Format("{0}:{1}", smtpServer, port);
                System.Web.Mail.SmtpMail.Send(myMail);
                return MailError.NO_ERROR;
            }
            catch (Exception ex)
            {
                // Handle exception ... 
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return MailError.SEND_ERROR;
            }
        }
    }
}
