
using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;
using System.Net;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    public class MoveEmailToFolderRules
    {
        public List<EmailDetails> lstEmailDetailsMoveCriteria = new List<EmailDetails>();

        public string? FolderName { get; set; }

        public SearchQuery? query = null;
    }

    public class EmailDetails
    {
        public int EmailDetailsID { get; set; }
        public string? Source { get; set; }
        public string? SubfolderName { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime InputDate { get; set; }
        public string? Htmlbody { get; set; }
        public DateTime MailDate { get; set; }
        public string? From { get; set; }
        public string? MessageId { get; set; }
        public string? Subject { get; set; }
        public string? To { get; set; }
        public string? TextBody { get; set; }
        public string? DeletedByCode { get; set; }
    }

public class EmailService
{
    public void SendEmail(string from, string to, string mailbody, string subject)
    {
        //search for app password in user setting
        from = "nagendra.uvce@gmail.com";
        using (MailMessage mail = new MailMessage())
        {
            mail.From = new MailAddress(from);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = mailbody;
            mail.IsBodyHtml = true;

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate("your_email@gmail.com", "your_password");
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sender Name", from));
                message.To.Add(new MailboxAddress("Recipient Name", to));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = mailbody };
                client.Send(message);
                client.Disconnect(true);
            }
            //mail.Attachments.Add(new Attachment("D:\\TestFile.txt"));//--Uncomment this to send any attachment  
            using (System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
            {
                //smtp.Credentials = new NetworkCredential(from, "Xeqf iuac ryld ldyc");
                smtp.Credentials = new NetworkCredential(from, "utgv uihb vfow wtfi");

                smtp.EnableSsl = true; ;
                smtp.Send(mail);
            }
        }
    }



}

 public class MailKitClass
    {
    public List<EmailDetails> MailKitInitial(DateTime dtInputDate)
    {
        List<EmailDetails> lstEmailDetails = new List<EmailDetails>();
        using (var client = new ImapClient())
        {
            client.Connect("imap.gmail.com", 993, true);

            client.Authenticate("nagendra.uvce@gmail.com", "utgv uihb vfow wtfi");

            // The Inbox folder is always available on all IMAP servers...
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadWrite);

            Console.WriteLine("Total messages: {0}", inbox.Count);
            Console.WriteLine("Recent messages: {0}", inbox.Recent);


            var ns = client.GetFolder(client.PersonalNamespaces[0]);
            var GmailRetainCode = ns.GetSubfolder("GmailRetainCode");
            IMailFolder? drafts = null;
            // inbox.AddFlags(uniqueId, MessageFlags.Deleted, true);
            if ((client.Capabilities & (ImapCapabilities.SpecialUse | ImapCapabilities.XList)) != 0)
            {
                drafts = client.GetFolder(SpecialFolder.Trash);
                //inbox.Expunge();
            }
            else
            {
                // maybe check the user's preferences for the Drafts folder?
            }

            List<MoveEmailToFolderRules> moveEmailToFolderRulesList = new List<MoveEmailToFolderRules>();
            moveEmailToFolderRulesList.Add(ToBeDeletedInputCriteria());
            moveEmailToFolderRulesList.Add(ToBeSavedToImportantInputCriteria());
            foreach (var moveFolderRules in moveEmailToFolderRulesList)
            {

                var results2 = inbox.Search(SearchOptions.All, moveFolderRules.query);
                foreach (var uniqueId in results2.UniqueIds)
                {

                    var message = inbox.GetMessage(uniqueId);
                    if (moveFolderRules.FolderName == "GmailRetainCode")
                    {
                        inbox.MoveTo(uniqueId, GmailRetainCode);
                    }
                    if (moveFolderRules.FolderName == "Trash")
                    {
                        inbox.MoveTo(uniqueId, drafts);
                    }
                }
            }
            client.Disconnect(true);
        }
        return lstEmailDetails;
    }

    public async Task UpdateDBWithLatestGmail()
        {

        }
        public bool DoesEmailMatchCriteria(MimeKit.MimeMessage inputMessage, List<EmailDetails> lstCriteriaToBeDeletedLocal)
        {
            bool emailMatchCriteria = false;
            bool subjectMatches = false;
            string from = inputMessage.From.Count > 0 ? inputMessage.From[0].Name : "NoFrom";
            foreach (var tbd in lstCriteriaToBeDeletedLocal)
            {
                if (tbd != null && tbd.Subject != null && inputMessage.Subject.ToLowerInvariant().Contains(tbd.Subject.ToLowerInvariant()))
                {
                    subjectMatches = true;
                }
                if(tbd.Subject.Trim().Length <= 0)
                {
                    subjectMatches = true;
                }
                if (subjectMatches
                    && 
                    from.ToLowerInvariant().Contains(tbd.From.ToLowerInvariant())
                    )
                {
                    emailMatchCriteria = true;
                    return emailMatchCriteria;
                }
            }
            return emailMatchCriteria;
        }
        public MoveEmailToFolderRules ToBeDeletedInputCriteria()
        {
            MoveEmailToFolderRules move = new MoveEmailToFolderRules();
            List<EmailDetails> lstCriteriaToBeDeletedLocal =null;
            lstCriteriaToBeDeletedLocal = new List<EmailDetails>();
            lstCriteriaToBeDeletedLocal.Add(new EmailDetails { From = "citi", Subject = "offer" });
            lstCriteriaToBeDeletedLocal.Add(new EmailDetails { From = "zee5", Subject="" });
            lstCriteriaToBeDeletedLocal.Add(new EmailDetails { From = "Fitbit", Subject = "Your Blaze battery level is low" });
            lstCriteriaToBeDeletedLocal.Add(new EmailDetails { From = "FS (Farnam Street)", Subject = "" });
            lstCriteriaToBeDeletedLocal.Add(new EmailDetails { From = "ET Markets", Subject = "" });
            move.lstEmailDetailsMoveCriteria = lstCriteriaToBeDeletedLocal;

            // let's search for all messages received after Jan 12, 2013 with "MailKit" in the subject...
            SearchQuery query = null;

            query = SearchQuery.SubjectContains("Your Blaze battery level is low")
                .Or(SearchQuery.FromContains("ET Markets"))
                .Or(SearchQuery.FromContains("FS (Farnam Street)"))
                .Or(SearchQuery.FromContains("Soumi_Sannamoth"))
                .Or(SearchQuery.FromContains("ET Markets"))
                .Or(SearchQuery.SubjectContains("Fortnightly Portfolio Disclosure"))
                .Or(SearchQuery.FromContains("zee5"))
                .Or(SearchQuery.FromContains("Udemy Instructor"));

            query = query.Or(SearchQuery.FromContains("citi").And(SearchQuery.SubjectContains("offer")));
            query = query.Or(SearchQuery.FromContains("Fitbit").And(SearchQuery.SubjectContains("Your Blaze battery level is low")));
            query = query.Or(SearchQuery.FromContains("The Economic Times"));
            move.query = query;
            move.FolderName = "Trash";
            return move;
        }
        public MoveEmailToFolderRules ToBeSavedToImportantInputCriteria()
        {
            MoveEmailToFolderRules move = new MoveEmailToFolderRules();
            // let's search for all messages received after Jan 12, 2013 with "MailKit" in the subject...
            SearchQuery query = null;

            query = SearchQuery.SubjectContains("Your NSDL CAS - ")
                .Or(SearchQuery.SubjectContains("Transactions In Your Demat Account"))
                .Or(SearchQuery.SubjectContains("ride to"))
            .Or(SearchQuery.SubjectContains("Balance in your account"))
            .Or(SearchQuery.SubjectContains("Folio 8762379 - Processing of systematic Purchase in ICICI Prudential Mutual Fund"))
            .Or(SearchQuery.SubjectContains("Statement of account for ICICI Prudential Mutual Fund"))
            .Or(SearchQuery.SubjectContains("Citibank Account Statement"))
            .Or(SearchQuery.SubjectContains("CitiAlert: NEFT Fund Transfer acknowledgement"))
            .Or(SearchQuery.SubjectContains("CitiAlert - Confirmation of transfer via NEFT / RTGS"))
            .Or(SearchQuery.SubjectContains("ICICI Bank Statement from"))
            .Or(SearchQuery.SubjectContains("Debit alert for your installment"))
            .Or(SearchQuery.SubjectContains("Monthly Demat Transaction with Holding Statement"));

            query = query.Or(SearchQuery.FromContains("Fitbit").And(SearchQuery.SubjectContains("Your weekly progress report from Fitbit")));
            query = query.Or(SearchQuery.FromContains("Enetadvicemailing").And(SearchQuery.SubjectContains("Payment from")));
            query = query.Or(SearchQuery.FromContains("Paytm ").And(SearchQuery.SubjectContains("Paytm Wallet Statement")));
            query = query.Or(SearchQuery.FromContains("HDFC Mutual Fund").And(SearchQuery.SubjectContains("Smart Account Statement")));
            query = query.Or(SearchQuery.FromContains("donotreply_bescom").And(SearchQuery.SubjectContains("BESCOM ")));
            query = query.Or(SearchQuery.FromContains("icici").And(SearchQuery.SubjectContains("Transaction alert ")));
            query = query.Or(SearchQuery.FromContains("BookMyShow").And(SearchQuery.SubjectContains("Your Tickets")));
            query = query.Or(SearchQuery.FromContains("Morgan Stanley").And(SearchQuery.SubjectContains("eDelivery Notification")));
            query = query.Or(SearchQuery.FromContains("Zerodha Broking Ltd"));
            query = query.Or(SearchQuery.FromContains("ticketadmin"));
            query = query.Or(SearchQuery.FromContains("nse_alerts").And(SearchQuery.SubjectContains("Balance")));
            move.query = query;
            move.FolderName = "GmailRetainCode";
            return move;
        }

        public void DBCode()
        {

            /* string dbConnectionstring
                   = CloudConfigurationManager.GetSetting("dbConnectionString");
             string query = "SELECT * FROM EmailDetailsDelete";
             DBUtilities.ConnectionString = dbConnectionstring;
             DataSet dsSeries = DBUtilities.ExecuteDataSet(query, "EmailDetailsDelete");
             List<EmailDetails> lstCriteriaToBeDeletedLocal = DataSetUtilities.BindList<EmailDetails>(dsSeries.Tables[0]);*/


            /*lstEmailDetails.Add(new EmailDetails
            {
                Source = "gmail",
                UserName = "Nagendra Gmail",
                CreatedDate = DateTime.Now,
                InputDate = dtInputDate,
                MailDate = message.Date.DateTime,
                Subject = message.Subject,
                TextBody = message.TextBody,
                From = message.From.Count > 0 ? message.From[0].Name : "NoFrom",
                Htmlbody = message.HtmlBody,
                MessageId = message.MessageId,
                To = message.To.Count > 0 ? message.To[0].Name : "NoTo",
                DeletedByCode ="Deleted"
            });*/
        }
    }
