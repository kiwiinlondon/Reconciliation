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
                var value = (args.Length > 0 ? (decimal?)args[0] : null);
                var withinTolerance = (args.Length > 1 ? (bool)args[1] : true);
                writer.WriteSafeString($"<div class=\"numeric\" {(!withinTolerance ? "style=\"color: red\"" : null)}>{value:n2}%</div>");
            });

            Handlebars.RegisterHelper("number", (writer, context, args) => {
                var value = (args.Length > 0 ? (decimal?)args[0] : null);
                var withinTolerance = (args.Length > 1 ? (bool)args[1] : true);
                writer.WriteSafeString($"<div class=\"numeric\" {(!withinTolerance ? "style=\"color: red\"" : null)}>{value:n0}</div>");
            });

            //_generateEmail = CompileTemplate("ReconciliationEmailTemplate.html");
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
            return returnComparison == null || (returnComparison.ReturnWithinTolerance && returnComparison.ValueWithinTolerance);
        }

        public void SendEmail(Fund fund, DateTime referenceDate,
            ReturnComparison keeleyToActualMTD,
            ReturnComparison keeleyToActualYTD,
            ReturnComparison keeleyToActualDay)
        {
            var client = new EmailClient();
            var dayStatus = (new ReturnComparison[] {keeleyToActualDay }.All(IsWithinTolerace)) ? "OK" : "BROKEN";
            var mtdStatus = (new ReturnComparison[] {keeleyToActualMTD}.All(IsWithinTolerace)) ? "OK" : "BROKEN";
            var ytdStatus = (new ReturnComparison[] {keeleyToActualYTD}.All(IsWithinTolerace)) ? "OK" : "BROKEN";
            var subject = $"{fund.Name} Attribution Rec {referenceDate:dd-MMM-yyyy}: Day {dayStatus} MTD {mtdStatus} YTD {ytdStatus}";

            var message = _generateSummaryEmail(new
            {
                keeleyToActualMTD,
                keeleyToActualYTD,
                keeleyToActualDay
            });

            //var to = "programmers@odey.com";
            var to = "brad.parker@odey.com";
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