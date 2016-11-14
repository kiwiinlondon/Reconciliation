using Odey.Framework.Infrastructure.EmailClient;
using Odey.Framework.Keeley.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class EmailWriter
    {
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
            ReturnComparison keeleyToMTDReturnComparison,
            ReturnComparison keeleyToYTDReturnComparison,
            ReturnComparison masterToMTDReturnComparison,
            ReturnComparison masterToYTDReturnComparison,
            ReturnComparison keeleyToAdminMTDComparison,
            ReturnComparison keeleyToAdminYTDComparison,
            ReturnComparison masterToAdminMTDComparison,
            ReturnComparison masterToAdminYTDComparison,
            ReturnComparison positionMTDComparison,
            ReturnComparison positionYTDComparison)
        {
            EmailClient client = new EmailClient();

           

            //decimal keeleyMTDDiff = mtdReturn - mtdKeeleyTotalContribution;
            //decimal keeleyYTDDiff = ytdReturn - ytdKeeleyTotalContribution;


            //decimal? keeleyMasterMTDDiff = 0;
            //decimal? masterMTDDiff = 0;

            //bool mtdBreak = Math.Abs(keeleyMTDDiff) > mtdTolerance;

            //if (mtdMasterTotalContribution.HasValue)
            //{
            //    keeleyMasterMTDDiff = mtdKeeleyTotalContribution - mtdMasterTotalContribution.Value;
            //    masterMTDDiff = mtdMasterTotalContribution - mtdReturn;

            //    mtdBreak = mtdBreak || Math.Abs(keeleyMasterMTDDiff.Value) > mtdTolerance || Math.Abs(masterMTDDiff.Value) > mtdTolerance;
            //}

            //bool ytdBreak = Math.Abs(keeleyYTDDiff) > ytdTolerance;
            //decimal? keeleyMasterYTDDiff = 0;
            //decimal? masterYTDDiff = 0;
            //if (ytdMasterTotalContribution.HasValue)
            //{
            //    keeleyMasterYTDDiff = ytdKeeleyTotalContribution - ytdMasterTotalContribution.Value;
            //    masterYTDDiff = ytdMasterTotalContribution - ytdReturn;

            //    ytdBreak = ytdBreak || Math.Abs(keeleyMasterYTDDiff.Value) > ytdTolerance || Math.Abs(masterYTDDiff.Value) > ytdTolerance;

            //}


            string mtdStatus = AreAllWithinTolerace(keeleyToMTDReturnComparison, masterToMTDReturnComparison, keeleyToAdminMTDComparison, masterToAdminMTDComparison, positionMTDComparison) ? "BROKEN" : "OK";
            string ytdStatus = AreAllWithinTolerace(keeleyToYTDReturnComparison, masterToYTDReturnComparison, keeleyToAdminYTDComparison, masterToAdminYTDComparison, positionMTDComparison) ? "BROKEN" : "OK";

            string subject = $"{fund.Name} Attribution Rec {referenceDate:dd-MMM-yyyy}: MTD {mtdStatus} YTD {ytdStatus}";

            client.SendAsHtml("AttributionRecs@Odey.com", "Attribution Recs", "g.poore@odey.com", null, null, subject, null, null);

            //StringBuilder bodyBuilder = new StringBuilder();

            //bodyBuilder.AppendLine("MTD");
            //bodyBuilder.AppendLine("===");
            //bodyBuilder.AppendLine();
            //bodyBuilder.AppendLine("Actual Return vs Keeley Contribution ");
            //bodyBuilder.AppendLine($"{Math.Round(mtdReturn,2)} - {Math.Round(mtdKeeleyTotalContribution,2)} = {Math.Round(keeleyMTDDiff, 2)}" );
            //bodyBuilder.AppendLine();
            //if (keeleyMasterMTDDiff.HasValue)
            //{
            //    bodyBuilder.AppendLine("Master Contribution vs Keeley Contribution");
            //}

            //if (masterMTDDiff.HasValue)
            //{
            //    bodyBuilder.AppendLine("Master Contribution vs Actual Return");
            //}

            ////string body = 
        }


    }
}