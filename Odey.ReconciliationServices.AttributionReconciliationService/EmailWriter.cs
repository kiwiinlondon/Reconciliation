using HandlebarsDotNet;
using Odey.Framework.Infrastructure.EmailClient;
using Odey.Framework.Keeley.Entities;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
                var warning = (args.Length < 2 || (args[1] is bool && (bool)args[1] == true) ? null : "warning");
                writer.WriteSafeString($"<div class=\"numeric {warning}\">{args[0]:n2}%</div>");
            });

            Handlebars.RegisterHelper("number", (writer, context, args) => {
                var warning = (args.Length < 2 || (args[1] is bool && (bool)args[1] == true) ? null : "warning");
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
            if (IsWithinTolerace(keeleyToReturnComparison)
                && IsWithinTolerace(masterToReturnComparison)
                && IsWithinTolerace(keeleyToAdminComparison)
                && IsWithinTolerace(masterToAdminComparison)
                && !IsWithinTolerace(positionComparison))
            {
                return true;
            }
            return false;
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
            var mtdStatus = AreAllWithinTolerace(keeleyToActualMTD, masterToActualMTD, keeleyToAdminMTD, masterToAdminMTD, positionMTD) ? "OK" : "BROKEN";
            var ytdStatus = AreAllWithinTolerace(keeleyToActualYTD, masterToActualYTD, keeleyToAdminYTD, masterToAdminYTD, positionMTD) ? "OK" : "BROKEN";
            var subject = $"{fund.Name} Attribution Rec {referenceDate:dd-MMM-yyyy}: MTD {mtdStatus} YTD {ytdStatus}";

            var masterToAdminCurrencyDifferences = GetDifferences(masterToAdminMTD.CurrencyDifferences, masterToAdminYTD.CurrencyDifferences);
            var keelyToAdminCurrencyDifferences = GetDifferences(keeleyToAdminMTD.CurrencyDifferences, keeleyToAdminYTD.CurrencyDifferences);
            var masterToAdminInstrumentDifferences = GetDifferences(masterToAdminMTD.InstrumentDifferences, masterToAdminYTD.InstrumentDifferences);
            var keeleyToAdminInstrumentDifferences = GetDifferences(keeleyToAdminMTD.InstrumentDifferences, keeleyToAdminYTD.InstrumentDifferences);

            var message = GenerateEmail(new
            {
                keeleyToActualMTD, keeleyToActualYTD, masterToActualMTD, masterToActualYTD, keeleyToAdminMTD, keeleyToAdminYTD, masterToAdminMTD, masterToAdminYTD, positionMTD, positionYTD,
                masterToAdminCurrencyDifferences, keelyToAdminCurrencyDifferences, masterToAdminInstrumentDifferences, keeleyToAdminInstrumentDifferences
            });
            
            var to = "b.parker@odey.com";
            //var to = "j.meyer@odey.com";
            client.SendAsHtml("AttributionRecs@Odey.com", "Attribution Recs", to, null, null, subject, message, null);
        }

        private IEnumerable<Tuple<SimpleComparison, SimpleComparison>> GetDifferences(IEnumerable<SimpleComparison> mtd, IEnumerable<SimpleComparison> ytd)
        {
            var output = new List<Tuple<SimpleComparison, SimpleComparison>>();
            for (int i = 0; i < Math.Max(mtd.Count(), ytd.Count()); i++)
            {
                SimpleComparison mtdComparison = null;
                if (i < mtd.Count())
                {
                    mtdComparison = mtd.ElementAt(i);
                }

                SimpleComparison ytdComparison = null;
                if (i < ytd.Count())
                {
                    ytdComparison = ytd.ElementAt(i);
                }

                output.Add(new Tuple<SimpleComparison, SimpleComparison>(mtdComparison, ytdComparison));
            }
            return output.ToArray();
        }        
    }
}