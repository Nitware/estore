using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System.Web.Mvc;
using System.IO;
using SmartStore.GTPay.Domain;

namespace SmartStore.GTPay.Models
{
    public abstract class GTPayInfoModelBase : ModelBase
    {
        public string DescriptionText { get; set; }
    }

    public class GTPayGatewayResponse
    {
        public decimal Amount { get; set; }
        public string MerchantReference { get; set; }
        public string MertID { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
    }

    public class GTPayCardPaymentInfoModel : GTPayInfoModelBase
    {
        public string IconUrl { get; set; }
        public List<string> CardIconUrls { get; set; }
        public List<GTPaySupportedCurrency> SupportedCurrencies { get; set; }
        public GTPaySupportedCurrency SupportedCurrency { get; set; }

        [SmartResourceDisplayName("Payment.SelectCard")]
        [AllowHtml]
        public string CardType { get; set; }

        [SmartResourceDisplayName("Payment.SelectCard")]
        public IList<SelectListItem> CardTypes { get; set; }

        private const string CARD_FOLDER = "~/Plugins/SmartStore.GTPay/Content/card";

        public GTPayCardPaymentInfoModel(string logoPhysicalPath)
        {
            CardTypes = new List<SelectListItem>();
            CardIconUrls = GetAllCardIconUrl(logoPhysicalPath);
        }

        public List<string> GetAllCardIconUrl(string logoPhysicalPath)
        {
            List<string> cardIcons = null;
            string folderPath = string.Format(@"{0}\Content\card", logoPhysicalPath);

            //string folderPath = @"C:\Users\Dan\Documents\Repositories\eStore\src\Plugins\SmartStore.GTPay\Content\card"; // HttpContext.Current.Server.MapPath(CARD_FOLDER);

            if (!Directory.Exists(folderPath))
            {
                return cardIcons;
            }

            IEnumerable<string> filePaths = Directory.EnumerateFiles(folderPath);
            if (filePaths != null && filePaths.Count() > 0)
            {
                cardIcons = new List<string>();

                filePaths = filePaths.OrderBy(x => x);
                foreach (string filePath in filePaths)
                {
                    FileInfo file = new FileInfo(filePath);
                    string fileVirtualPath = string.Format(CARD_FOLDER + "/{0}", file.Name);
                    cardIcons.Add(fileVirtualPath);
                }
            }

            return cardIcons;
        }


    }

}