// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Yggdrasil.Logging.Targets
{
	public class SmtpTarget : LoggerTarget
	{
		public string From { get; private set; }
		public string To { get; private set; }
		public string Subject { get; private set; }
		public string SmtpHost { get; private set; }
		public int SmtpPort { get; private set; }
		public string Username { get; private set; }
		public string Password { get; private set; }
		public bool Ssl { get; private set; }

		public SmtpTarget(string from, string to, string subject, string smtpHost, int smtpPort, string username, string password, bool ssl)
		{
			this.From = from;
			this.To = to;
			this.Subject = subject;
			this.SmtpHost = smtpHost;
			this.SmtpPort = smtpPort;
			this.Username = username;
			this.Password = password;
			this.Ssl = ssl;
		}

		public override void Write(LogLevel level, string message, string messageRaw, string messageClean)
		{
			messageClean = string.Format("{0:yyyy-MM-dd HH:mm} {1}", DateTime.Now, messageClean);
			this.Send(messageClean);
		}

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
				mail.ReplyTo = new MailAddress(this.From);
				mail.Subject = this.Subject;
				mail.SubjectEncoding = Encoding.UTF8;
				mail.Priority = MailPriority.Normal;

				client.Send(mail);
			}
		}

		public override string GetFormat(LogLevel level)
		{
			return "[{0}] - {1}";
		}
	}
}
