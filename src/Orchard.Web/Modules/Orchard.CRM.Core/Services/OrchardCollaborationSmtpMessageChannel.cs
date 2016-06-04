/// Orchard Collaboration is a series of plugins for Orchard CMS that provides an integrated ticketing system and collaboration framework on top of it.
/// Copyright (C) 2014-2016  Siyamand Ayubi
///
/// This file is part of Orchard Collaboration.
///
///    Orchard Collaboration is free software: you can redistribute it and/or modify
///    it under the terms of the GNU General Public License as published by
///    the Free Software Foundation, either version 3 of the License, or
///    (at your option) any later version.
///
///    Orchard Collaboration is distributed in the hope that it will be useful,
///    but WITHOUT ANY WARRANTY; without even the implied warranty of
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
///    GNU General Public License for more details.
///
///    You should have received a copy of the GNU General Public License
///    along with Orchard Collaboration.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Logging;
using Orchard.Email.Models;
using Orchard.Email.Services;
using Orchard.Messaging.Services;

namespace Orchard.CRM.Core.Services
{

    public class OrchardCollaborationEmailMessageChannelSelector : Component, IMessageChannelSelector
    {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly SmtpSettingsPart _smtpSettings;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly IOrchardServices _orchardServices;

        public OrchardCollaborationEmailMessageChannelSelector(
            IWorkContextAccessor workContextAccessor,
            IOrchardServices orchardServices,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay)
        {
            _orchardServices = orchardServices;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;

        }

        public MessageChannelSelectorResult GetChannel(string messageType, object payload)
        {
            if (messageType == CRMHelper.OrchardCollaborationEmailMessageType ||
                messageType == CRMHelper.OrcharCollaborationDefinitiveEmailMessageType)
            {
                var workContext = _workContextAccessor.GetContext();
                var messageChannel = new OrchardCollaborationSmtpMessageChannel(_orchardServices, _shapeFactory, _shapeDisplay);

                if (messageType == CRMHelper.OrcharCollaborationDefinitiveEmailMessageType)
                {
                    messageChannel.RaiseExceptionForFailedMessages = true;
                }

                return new MessageChannelSelectorResult
                {
                    Priority = 50,
                    MessageChannel = () => messageChannel
                };
            }

            return null;
        }
    }

    public class OrchardCollaborationSmtpMessageChannel : Component, ISmtpChannel, IDisposable
    {
        private readonly SmtpSettingsPart _smtpSettings;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeDisplay _shapeDisplay;
        private readonly Lazy<SmtpClient> _smtpClientField;

        public bool RaiseExceptionForFailedMessages = false;

        public OrchardCollaborationSmtpMessageChannel(
            IOrchardServices orchardServices,
            IShapeFactory shapeFactory,
            IShapeDisplay shapeDisplay)
        {

            _shapeFactory = shapeFactory;
            _shapeDisplay = shapeDisplay;

            _smtpSettings = orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            _smtpClientField = new Lazy<SmtpClient>(CreateSmtpClient);
        }

        public void Dispose()
        {
            if (!_smtpClientField.IsValueCreated)
            {
                return;
            }

            _smtpClientField.Value.Dispose();
        }

        public void Process(IDictionary<string, object> parameters)
        {

            if (!_smtpSettings.IsValid())
            {
                return;
            }

            var emailMessage = new EmailMessage
            {
                Body = Read(parameters, "Body"),
                Subject = Read(parameters, "Subject"),
                Recipients = Read(parameters, "Recipients"),
                ReplyTo = Read(parameters, "ReplyTo"),
                From = Read(parameters, "From"),
                Bcc = Read(parameters, "Bcc"),
                Cc = Read(parameters, "CC")
            };

            if (emailMessage.Recipients.Length == 0)
            {
                Logger.Error("Email message doesn't have any recipient");
                return;
            }

            var mailMessage = new MailMessage
            {
                Subject = emailMessage.Subject,
                Body = System.Web.HttpUtility.HtmlDecode(_shapeDisplay.Display(emailMessage.Body)),
                IsBodyHtml = true
            };

            if (parameters.ContainsKey("Message"))
            {
                // A full message object is provided by the sender.

                var oldMessage = mailMessage;
                mailMessage = (MailMessage)parameters["Message"];

                if (String.IsNullOrWhiteSpace(mailMessage.Subject))
                    mailMessage.Subject = oldMessage.Subject;

                if (String.IsNullOrWhiteSpace(mailMessage.Body))
                {
                    mailMessage.Body = oldMessage.Body;
                    mailMessage.IsBodyHtml = oldMessage.IsBodyHtml;
                }
            }

            try
            {

                foreach (var recipient in ParseRecipients(emailMessage.Recipients))
                {
                    mailMessage.To.Add(new MailAddress(recipient));
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.Cc))
                {
                    foreach (var recipient in ParseRecipients(emailMessage.Cc))
                    {
                        mailMessage.CC.Add(new MailAddress(recipient));
                    }
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.Bcc))
                {
                    foreach (var recipient in ParseRecipients(emailMessage.Bcc))
                    {
                        mailMessage.Bcc.Add(new MailAddress(recipient));
                    }
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.From))
                {
                    mailMessage.From = new MailAddress(emailMessage.From);
                }
                else
                {
                    // Take 'From' address from site settings or web.config.
                    mailMessage.From = !String.IsNullOrWhiteSpace(_smtpSettings.Address)
                        ? new MailAddress(_smtpSettings.Address)
                        : new MailAddress(((SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp")).From);
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.ReplyTo))
                {
                    foreach (var recipient in ParseRecipients(emailMessage.ReplyTo))
                    {
                        mailMessage.ReplyToList.Add(new MailAddress(recipient));
                    }
                }

                _smtpClientField.Value.Send(mailMessage);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not send email");

                if (RaiseExceptionForFailedMessages)
                {
                    parameters.Add("ERROR", e);
                    throw new OrchardException(T("Error in sending email"), e);
                }
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            // If no properties are set in the dashboard, use the web.config value.
            if (String.IsNullOrWhiteSpace(_smtpSettings.Host))
            {
                return new SmtpClient();
            }

            var smtpClient = new SmtpClient
            {
                UseDefaultCredentials = !_smtpSettings.RequireCredentials,
            };

            if (!smtpClient.UseDefaultCredentials && !String.IsNullOrWhiteSpace(_smtpSettings.UserName))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password);
            }

            if (_smtpSettings.Host != null)
            {
                smtpClient.Host = _smtpSettings.Host;
            }

            smtpClient.Port = _smtpSettings.Port;
            smtpClient.EnableSsl = _smtpSettings.EnableSsl;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            return smtpClient;
        }

        private string Read(IDictionary<string, object> dictionary, string key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] as string : null;
        }

        private IEnumerable<string> ParseRecipients(string recipients)
        {
            return recipients.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}