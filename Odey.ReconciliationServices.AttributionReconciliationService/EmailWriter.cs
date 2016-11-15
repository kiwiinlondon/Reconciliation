using HandlebarsDotNet;
using Odey.Framework.Infrastructure.EmailClient;
using Odey.Framework.Keeley.Entities;
using System;
using System.IO;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class EmailWriter
    {
        private Func<object, string> GenerateEmail;

        public EmailWriter()
        {
            Handlebars.RegisterHelper("formatPercent", (writer, context, args) => {
                writer.WriteSafeString($"{args[0]:n2}%");
            });
            Handlebars.RegisterHelper("formatValue", (writer, context, args) => {
                writer.WriteSafeString($"{args[0]:n0}");
            });

            var templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReconciliationEmailTemplate.html");
            var templateText = File.ReadAllText(templateFile);
            GenerateEmail = Handlebars.Compile(templateText);
        }

        private bool IsWithinTolerace(ReturnComparison returnComparison)
        {
            if (returnComparison != null && (!returnComparison.ReturnWithinTolerance || !returnComparison.ValueWithinTolerance))
            {
                return false;
            }
            return true;
        }

        private bool AreAllWithinTolerace(
            ReturnComparison keeleyToReturnComparison,
            ReturnComparison masterToReturnComparison,
            ReturnComparison keeleyToAdminComparison,
            ReturnComparison masterToAdminComparison,
            ReturnComparison positionComparison)
        {
            if (!IsWithinTolerace(keeleyToReturnComparison)) return false;
            if (!IsWithinTolerace(masterToReturnComparison)) return false;
            if (!IsWithinTolerace(keeleyToAdminComparison)) return false;
            if (!IsWithinTolerace(masterToAdminComparison)) return false;
            if (!IsWithinTolerace(positionComparison)) return false;
            return true;
        }

        public void SendEmail(Fund fund, DateTime referenceDate,
            ReturnComparison keeleyToActualMTD,
            ReturnComparison keeleyToActualYTD,
            ReturnComparison masterToActualMTD,
            ReturnComparison masterToActualYTD,
            ReturnComparison keeleyToAdminMTD,
            ReturnComparison keeleyToAdminYTD,
            ReturnComparison masterToAdminMTD,
            ReturnComparison masterToAdminYTD,
            ReturnComparison positionMTD,
            ReturnComparison positionYTD)
        {
            var client = new EmailClient();
            var mtdStatus = AreAllWithinTolerace(keeleyToActualMTD, masterToActualMTD, keeleyToAdminMTD, masterToAdminMTD, positionMTD) ? "BROKEN" : "OK";
            var ytdStatus = AreAllWithinTolerace(keeleyToActualYTD, masterToActualYTD, keeleyToAdminYTD, masterToAdminYTD, positionMTD) ? "BROKEN" : "OK";
            var subject = $"{fund.Name} Attribution Rec {referenceDate:dd-MMM-yyyy}: MTD {mtdStatus} YTD {ytdStatus}";
            var message = GenerateEmail(new { keeleyToActualMTD, keeleyToActualYTD, masterToActualMTD, masterToActualYTD, keeleyToAdminMTD, keeleyToAdminYTD, masterToAdminMTD, masterToAdminYTD, positionMTD, positionYTD });
            
            var to = "g.poore@odey.com";
            //var to = "j.meyer@odey.com";
            client.SendAsHtml("AttributionRecs@Odey.com", "Attribution Recs", to, null, null, subject, message, null);
        }
    }
}