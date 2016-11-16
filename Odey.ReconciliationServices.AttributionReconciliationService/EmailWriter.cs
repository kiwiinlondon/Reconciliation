using HandlebarsDotNet;
using Odey.Framework.Infrastructure.EmailClient;
using Odey.Framework.Keeley.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class EmailWriter
    {
        private Func<object, string> GenerateEmail;

        public EmailWriter()
        {
            Handlebars.RegisterHelper("percent", (writer, context, args) => {
                var warning = (args.Length < 2 || (bool)args[1] == true ? null : "warning");
                writer.WriteSafeString($"<div class=\"numeric {warning}\">{args[0]:n2}%</div>");
            });
            Handlebars.RegisterHelper("number", (writer, context, args) => {
                var warning = (args.Length < 2 || (bool)args[1] == true ? null : "warning");
                writer.WriteSafeString($"<div class=\"numeric {warning}\">{args[0]:n0}</div>");
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
            var message = GenerateEmail(new {
                keeleyToActualMTD,
                keeleyToActualYTD,
                masterToActualMTD,
                masterToActualYTD,
                keeleyToAdminMTD,
                keeleyToAdminYTD,
                keeleyToAdminTopInstruments = MergeListsForTemplate(keeleyToAdminMTD.InstrumentDifferences, keeleyToAdminYTD.InstrumentDifferences),
                keeleyToAdminTopCurrency = MergeListsForTemplate(keeleyToAdminMTD.CurrencyDifferences, keeleyToAdminYTD.CurrencyDifferences),
                masterToAdminMTD,
                masterToAdminYTD,
                masterToAdminTopInstruments = MergeListsForTemplate(masterToAdminMTD.InstrumentDifferences, masterToAdminYTD.InstrumentDifferences),
                masterToAdminTopCurrency = MergeListsForTemplate(masterToAdminMTD.CurrencyDifferences, masterToAdminYTD.CurrencyDifferences),
                positionMTD,
                positionYTD
            });
            
            var to = "g.poore@odey.com";
            client.SendAsHtml("AttributionRecs@Odey.com", "Attribution Recs", to, null, null, subject, message, null);
        }

        private List<Tuple<T, T>> MergeListsForTemplate<T>(List<T> mtd, List<T> ytd)
        {
            return ytd.Zip(mtd, (a, b) => Tuple.Create(a, b)).ToList();
        }
    }
}