using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Yggdrasil.Logging.Targets
{
	/// <summary>
	/// Logger target that sends messages via SMTP.
	/// </summary>
	public class SmtpTarget : LoggerTarget
	{
		/// <summary>
		/// The from eMail address.
		/// </summary>
		public string From { get; private set; }

		/// <summary>
		/// The to eMail address.
		/// </summary>
		public string To { get; private set; }

		/// <summary>
		/// The eMail address to reply to.
		/// </summary>
		public string ReplyTo { get; private set; }

		/// <summary>
		/// The subject of the eMail.
		/// </summary>
		public string Subject { get; private set; }

		/// <summary>
		/// The SMTP host to use.
		/// </summary>
		public string SmtpHost { get; private set; }

		/// <summary>
		/// The SMTP server's port.
		/// </summary>
		public int SmtpPort { get; private set; }

		/// <summary>
		/// The username to log into the SMTP server with.
		/// </summary>
		public string Username { get; private set; }

		/// <summary>
		/// The password to log into the SMTP server with.
		/// </summary>
		public string Password { get; private set; }

		/// <summary>
		/// Whether SSL is enabled or not.
		/// </summary>
		public bool Ssl { get; private set; }

		/// <summary>
		/// Creates new instance.
		/// </summary>
		/// <param name="from">The from eMail address.</param>
		/// <param name="to">The to eMail address.</param>
		/// <param name="replyTo">The eMail address to reply to.</param>
		/// <param name="subject">The subject of the eMail.</param>
		/// <param name="smtpHost">The SMTP host to use.</param>
		/// <param name="smtpPort">The SMTP server's port.</param>
		/// <param name="username">The username to log into the SMTP server with.</param>
		/// <param name="password">The password to log into the SMTP server with.</param>
		/// <param name="ssl">Whether SSL is enabled or not.</param>
		public SmtpTarget(string from, string to, string replyTo, string subject, string smtpHost, int smtpPort, string username, string password, bool ssl)
		{
			this.From = from;
			this.To = to;
			this.ReplyTo = replyTo;
			this.Subject = subject;
			this.SmtpHost = smtpHost;
			this.SmtpPort = smtpPort;
			this.Username = username;
			this.Password = password;
			this.Ssl = ssl;
		}

		/// <summary>
		/// Sends the clean message via SMTP, prepending it with the time
		/// and day the message was sent at.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="message"></param>
		/// <param name="messageRaw"></param>
		/// <param name="messageClean"></param>
		public override void Write(LogLevel level, string message, string messageRaw, string messageClean)
		{
			messageClean = string.Format("{0:yyyy-MM-dd HH:mm} {1}", DateTime.Now, messageClean);
			this.Send(messageClean);
		}

		/// <summary>
		/// Sends message through the specified SMTP connection.
		/// </summary>
		/// <param name="message"></param>
		private void Send(string message)
		{
			var client = new SmtpClient(this.SmtpHost, this.SmtpPort);
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.EnableSsl = this.Ssl;
			client.UseDefaultCredentials = false;
			client.Credentials = new NetworkCredential(this.Username, this.Password);

			using (var mail = new MailMessage(this.From, this.To))
			{
				mail.Body = message;
				mail.BodyEncoding = Encoding.UTF8;
				mail.ReplyTo = new MailAddress(this.ReplyTo);
				mail.Subject = this.Subject;
				mail.SubjectEncoding = Encoding.UTF8;
				mail.Priority = MailPriority.Normal;

				client.Send(mail);
			}
		}

		/// <summary>
		/// Returns raw message format.
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public override string GetFormat(LogLevel level)
		{
			return "[{0}] - {1}";
		}
	}
}
