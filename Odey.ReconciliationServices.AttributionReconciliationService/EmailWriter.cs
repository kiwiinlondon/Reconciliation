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
                var value = (args.Length > 0 ? (decimal?)args[0] : null);
                var withinTolerance = (args.Length > 1 ? (bool)args[1] : true);
                writer.WriteSafeString($"<div class=\"numeric\" {(!withinTolerance ? "style=\"color: red\"" : null)}>{value:p}</div>");
            });
            Handlebars.RegisterHelper("number", (writer, context, args) => {
                var value = (args.Length > 0 ? (decimal?)args[0] : null);
                var withinTolerance = (args.Length > 1 ? (bool)args[1] : true);
                writer.WriteSafeString($"<div class=\"numeric\" {(!withinTolerance ? "style=\"color: red\"" : null)}>{value:n0}</div>");
            });

            var templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplate.html");
            var templateText = File.ReadAllText(templateFile);
            GenerateEmail = Handlebars.Compile(templateText);
        }

        private bool IsWithinTolerace(ReturnComparison rc)
        {
            return rc == null || (rc.ReturnWithinTolerance && rc.ValueWithinTolerance);
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
            var mtdStatus = (new ReturnComparison[] { keeleyToActualMTD, masterToActualMTD, keeleyToAdminMTD, masterToAdminMTD, positionMTD }.All(IsWithinTolerace) ? "OK" : "BROKEN");
            var ytdStatus = (new ReturnComparison[] { keeleyToActualYTD, masterToActualYTD, keeleyToAdminYTD, masterToAdminYTD, positionMTD }.All(IsWithinTolerace) ? "OK" : "BROKEN");
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