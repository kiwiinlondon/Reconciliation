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
        private Func<object, string> _generateEmail;
        private Func<object, string> _generateSummaryEmail;

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

            _generateEmail = CompileTemplate("ReconciliationEmailTemplate.html");
            _generateSummaryEmail = CompileTemplate("ReconciliationEmailSummaryTemplate.html");

        }

        private Func<object, string> CompileTemplate(string templateFileName)
        {
            var templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, templateFileName);
            var templateText = File.ReadAllText(templateFile);
            return Handlebars.Compile(templateText);
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
                && IsWithinTolerace(positionComparison))
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
            ReturnComparison keeleyToMasterMTD,
            ReturnComparison keeleyToMasterYTD)
        {
            var client = new EmailClient();
            var mtdStatus = AreAllWithinTolerace(keeleyToActualMTD, masterToActualMTD, keeleyToAdminMTD, masterToAdminMTD, keeleyToMasterMTD) ? "OK" : "BROKEN";
            var ytdStatus = AreAllWithinTolerace(keeleyToActualYTD, masterToActualYTD, keeleyToAdminYTD, masterToAdminYTD, keeleyToMasterMTD) ? "OK" : "BROKEN";
            var subject = $"{fund.Name} Attribution Rec {referenceDate:dd-MMM-yyyy}: MTD {mtdStatus} YTD {ytdStatus}";

            var masterToAdminCurrencyDifferences = GetDifferences(masterToAdminMTD?.CurrencyDifferences, masterToAdminYTD?.CurrencyDifferences);
            var keelyToAdminCurrencyDifferences = GetDifferences(keeleyToAdminMTD?.CurrencyDifferences, keeleyToAdminYTD?.CurrencyDifferences);
            var keelyToMasterCurrencyDifferences = GetDifferences(keeleyToMasterMTD?.CurrencyDifferences, keeleyToMasterYTD?.CurrencyDifferences);
            var masterToAdminInstrumentDifferences = GetDifferences(masterToAdminMTD?.InstrumentDifferences, masterToAdminYTD?.InstrumentDifferences);
            var keeleyToAdminInstrumentDifferences = GetDifferences(keeleyToAdminMTD?.InstrumentDifferences, keeleyToAdminYTD?.InstrumentDifferences);
            var keeleyToMasterInstrumentDifferences = GetDifferences(keeleyToMasterMTD?.InstrumentDifferences, keeleyToMasterYTD?.InstrumentDifferences);

            string message;
            if (masterToAdminMTD == null && keeleyToAdminMTD == null && keeleyToMasterMTD == null &&
                masterToAdminYTD == null && keeleyToAdminYTD == null && keeleyToMasterYTD == null)
            {
                message = _generateSummaryEmail(new
                {
                    keeleyToActualMTD,
                    keeleyToActualYTD
                });
            }
            else
            {
                message = _generateEmail(new
                {
                    keeleyToActualMTD,
                    keeleyToActualYTD,
                    masterToActualMTD,
                    masterToActualYTD,
                    keeleyToAdminMTD,
                    keeleyToAdminYTD,
                    masterToAdminMTD,
                    masterToAdminYTD,
                    keeleyToMasterMTD,
                    keeleyToMasterYTD,
                    masterToAdminCurrencyDifferences,
                    keelyToAdminCurrencyDifferences,
                    masterToAdminInstrumentDifferences,
                    keeleyToAdminInstrumentDifferences,
                    keelyToMasterCurrencyDifferences,
                    keeleyToMasterInstrumentDifferences
                });
            }

            var to = "programmers@odey.com";
            client.SendAsHtml("AttributionRecs@Odey.com", "Attribution Recs", to, null, null, subject, message, null);
        }

        private IEnumerable<Tuple<SimpleComparison, SimpleComparison>> GetDifferences(IEnumerable<SimpleComparison> mtd, IEnumerable<SimpleComparison> ytd)
        {
            var output = new List<Tuple<SimpleComparison, SimpleComparison>>();
            var mtdCount = mtd?.Count() ?? 0;
            var ytdCount = ytd?.Count() ?? 0;
           
            for (int i = 0; i < Math.Max(mtdCount, ytdCount); i++)
            {
                SimpleComparison mtdComparison = null;
                if (i < mtdCount)
                {
                    mtdComparison = mtd.ElementAt(i);
                }

                SimpleComparison ytdComparison = null;
                if (i < ytdCount)
                {
                    ytdComparison = ytd.ElementAt(i);
                }

                output.Add(new Tuple<SimpleComparison, SimpleComparison>(mtdComparison, ytdComparison));
            }
            return output.ToArray();
        }        
    }
}