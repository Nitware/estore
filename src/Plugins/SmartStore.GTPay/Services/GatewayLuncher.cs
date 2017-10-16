using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using SmartStore.GTPay.Interfaces;
using SmartStore.Services.Payments;
using System.Web.Mvc;
using System.Text;
using System.Security.Cryptography;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Orders;
using SmartStore.GTPay.Domain;

namespace SmartStore.GTPay.Services
{
    public class GatewayLuncher : IGatewayLuncher
    {
        private int noOfTrial;
        private readonly ITransactionLogService _transactionLogService;
        
        private string _hash = "D3D1D05AFE42AD50818167EAC73C109168A0F108F32645C8B59E897FA930DA44F9230910DAC9E20641823799A107A02068F7BC0F4CC41D2952E249552255710F";

        public string HashKey { get { return _hash; } }
        public bool IsSuccessful { get; set; }
        public static int OrderId { get; set; }

        private string _gtpay_mert_id = "gtpay_mert_id";
        private string _gtpay_tranx_id = "gtpay_tranx_id";
        private string _gtpay_tranx_amt = "gtpay_tranx_amt";
        private string _gtpay_tranx_curr = "gtpay_tranx_curr";
        private string _gtpay_cust_id = "gtpay_cust_id";
        private string _gtpay_cust_name = "gtpay_cust_name";
        private string _gtpay_tranx_memo = "gtpay_tranx_memo";
        private string _gtpay_no_show_gtbank = "gtpay_no_show_gtbank";
        private string _gtpay_echo_data = "gtpay_echo_data";
        private string _gtpay_gway_first = "gtpay_gway_first";
        private string _gtpay_gway_name = "gtpay_gway_name";
        private string _gtpay_hash = "gtpay_hash";
        private string _gtpay_tranx_noti_url = "gtpay_tranx_noti_url";
        private string _gtpay_tranx_status_code = "gtpay_tranx_status_code";
        private string _gtpay_tranx_status_msg = "gtpay_tranx_status_msg";
        private string _gtpay_tranx_amt_small_denom = "gtpay_tranx_amt_small_denom";
        private string _gatewayMessage = "gatewayMessage";
        private string _transactionRef = "transactionRef";
        private string _isManInTheMiddleAttack = "isManInTheMiddleAttack";
        private string _manInTheMiddleAttackMessage = "One or more transaction parameters have been modified in transit! The system has aborted this transaction to save you from malicious attack. Please click the 'Continue shopping' button below to initiate another transaction, or click <a href=\"{0}\"><i class=\"fa fa-home\"></i><span> here</span></a> to go to the home page. Sorry for the inconveniences this might have caused you.";
        private string _errorOccurred = "errorOccurred";
        private string _errorMessage = "errorMessage";
        private string _gtpay_mert_id_value = "8692";
        private string _gtpay_verification_hash = "gtpay_verification_hash";

        public string GtpayMertId { get { return _gtpay_mert_id; } }
        public string GtpayMertIdValue { get { return _gtpay_mert_id_value; } }
        public string GtpayTranxId { get { return _gtpay_tranx_id; } }
        public string GtpayTranxAmt { get { return _gtpay_tranx_amt; } }
        public string GtpayTranxCurr { get { return _gtpay_tranx_curr; } }
        public string GtpayCustId { get { return _gtpay_cust_id; } }
        public string GtpayCustName { get { return _gtpay_cust_name; } }
        public string GtpayTranxMemo { get { return _gtpay_tranx_memo; } }
        public string GtpayNoShowGtbank { get { return _gtpay_no_show_gtbank; } }
        public string GtpayEchoData { get { return _gtpay_echo_data; } }
        public string GtpayGwayFirst { get { return _gtpay_gway_first; } }
        public string GtpayGwayName { get { return _gtpay_gway_name; } }
        public string GtpayHash { get { return _gtpay_hash; } }
        public string GtpayTranxNotiUrl { get { return _gtpay_tranx_noti_url; } }
        public string GtpayTranxStatusCode { get { return _gtpay_tranx_status_code; } }
        public string GtpayTranxStatusMsg { get { return _gtpay_tranx_status_msg; } }
        public string GatewayMessage { get { return _gatewayMessage; } }
        public string TransactionRef { get { return _transactionRef; } }
        public string GtpayTranxAmtSmallDenom { get { return _gtpay_tranx_amt_small_denom; } }
        public string ManInTheMiddleAttackMessage { get { return _manInTheMiddleAttackMessage; } }
        public string IsManInTheMiddleAttack { get { return _isManInTheMiddleAttack; } }
        public string ErrorOccurred { get { return _errorOccurred; } }
        public string ErrorMessage { get { return _errorMessage; } }
        public string GtpayVerificationHash { get { return _gtpay_verification_hash; } }

        public GatewayLuncher(ITransactionLogService transactionLogService)
        {
            _transactionLogService = transactionLogService;
        }

        public void Lunch(PostProcessPaymentRequest postProcessPaymentRequest, GTPaySupportedCurrency currency, HttpContextBase httpContext)
        {
            OrderId = postProcessPaymentRequest.Order.Id;
            Tuple<string, string> nameAndEmail = GetNameAndEmail(postProcessPaymentRequest.Order.BillingAddress);

            decimal orderTotal = Math.Truncate(postProcessPaymentRequest.Order.OrderTotal * 100);
            string gtpay_tranx_memo = GetOrderSummary(nameAndEmail, postProcessPaymentRequest.Order);
            string gtpay_tranx_id = httpContext.Session[TransactionRef] as string;
            string gtpay_mert_id = GtpayMertIdValue;
            string gtpay_tranx_amt = orderTotal.ToString();
            string gtpay_tranx_curr = currency.Code.ToString();
            string gtpay_cust_id = postProcessPaymentRequest.Order.Customer.Id.ToString();
            string gtpay_cust_name = nameAndEmail.Item1;
            string gtpay_tranx_noti_url = GetRedirectUrl(httpContext.Request, "Details", "Order", OrderId);
            string hash = HashKey;
            string parameters_to_hash = gtpay_mert_id + gtpay_tranx_id + gtpay_tranx_amt + gtpay_tranx_curr + gtpay_cust_id + gtpay_tranx_noti_url + hash;
            string gtpay_echo_data = gtpay_mert_id + ";" + hash + ";" + nameAndEmail.Item1 + ";" + nameAndEmail.Item2;
            string gtpay_hash = GenerateSHA512String(parameters_to_hash);
            string url = "http://gtweb2.gtbank.com/orangelocker/gtpaym/tranx.aspx";
            string gtpay_gway_name = currency.Gateway;

            //string gtpay_gway_name = gtpay_tranx_curr == "566" ? "webpay" : "migs";
            //string gtpay_tranx_noti_url = GetRedirectUrl(httpContext.Request, "Completed", "Checkout");

            HttpResponseBase response = httpContext.Response;
            response.Clear();

            StringBuilder form = new StringBuilder();
            form.Append("<html>");
            form.AppendFormat("<body onload='document.forms[0].submit()'>");
            form.AppendFormat("<form action='{0}' method='post'>", url);
            form.AppendFormat("<input type='hidden' name='" + GtpayMertId + "' value='{0}'>", GtpayMertIdValue);
            form.AppendFormat("<input type='hidden' name='" + GtpayTranxId + "' value='{0}'>", gtpay_tranx_id);
            form.AppendFormat("<input type='hidden' name='" + GtpayTranxAmt + "' value='{0}'>", gtpay_tranx_amt);
            form.AppendFormat("<input type='hidden' name='" + GtpayTranxCurr + "' value='{0}'>", gtpay_tranx_curr);
            form.AppendFormat("<input type='hidden' name='" + GtpayCustId + "' value='{0}'>", gtpay_cust_id);
            form.AppendFormat("<input type='hidden' name='" + GtpayCustName + "' value='{0}'>", gtpay_cust_name);
            form.AppendFormat("<input type='hidden' name='" + GtpayTranxMemo + "' value='{0}'>", gtpay_tranx_memo);
            form.AppendFormat("<input type='hidden' name='" + GtpayNoShowGtbank + "' value='{0}'>", "yes");
            form.AppendFormat("<input type='hidden' name='" + GtpayEchoData + "' value='{0}'>", gtpay_echo_data);
            form.AppendFormat("<input type='hidden' name='" + GtpayGwayFirst + "' value='{0}'>", "yes");
            form.AppendFormat("<input type='hidden' name='" + GtpayGwayName + "' value='{0}'>", gtpay_gway_name);
            form.AppendFormat("<input type='hidden' name='" + GtpayHash + "' value='{0}'>", gtpay_hash);
            form.AppendFormat("<input type='hidden' name='" + GtpayTranxNotiUrl + "' value='{0}'>", gtpay_tranx_noti_url);
            form.Append("</form>");
            form.Append("</body>");
            form.Append("</html>");

            response.Write(form.ToString());
            response.End();
        }

        private string GetOrderSummary(Tuple<string, string> nameAndEmail, Order order)
        {
            if (order == null || order.OrderItems == null)
            {
                return null;
            }

            string orderSummary = null;
            string item = order.OrderItems.Count > 1 ? " items" : " item";

            if (nameAndEmail != null && nameAndEmail.Item1.HasValue())
            {
                orderSummary = nameAndEmail.Item1 + " (" + order.Customer.Id + ")" + " ordered " + order.OrderItems.Count + item + " that cost " + order.OrderTotal.ToString("n") + " with order ID of " + order.Id;
            }
            else
            {
                orderSummary = "Customer with ID of '" + order.Customer.Id + "'" + " ordered " + order.OrderItems.Count + item + " that cost " + order.OrderTotal.ToString("n") + " with order ID of " + order.Id;
            }

            return orderSummary;
        }

        private Tuple<string, string> GetNameAndEmail(Address customerAddress)
        {
            string name = null;
            string email = null;

            if (customerAddress != null)
            {
                name = string.Format("{0} {1}", customerAddress.FirstName, customerAddress.LastName);
                name = name.Trim();
                email = customerAddress.Email;
            }

            return new Tuple<string, string>(name, email);
        }

        public string GetRedirectUrl(HttpRequestBase request, string action, string controller, int id = 0)
        {
            string url = null;
            if (request == null)
            {
                return url;
            }

            UrlHelper urlHelper = new UrlHelper(request.RequestContext);
            if (id > 0)
            {
                url = urlHelper.Action(action, controller, new { Area = "", id = id }, request.Url.Scheme);
            }
            else
            {
                url = urlHelper.Action(action, controller, new { Area = "" }, request.Url.Scheme);
            }

            return url;
        }

        public string GenerateSHA512String(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        private string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

       
        public string CreateTransactionRef()
        {
            int maximumTrial = 20;
            string transactionRefNo = null;
            
            while (noOfTrial <= maximumTrial)
            {
                ++noOfTrial;
                string tranxRefNo = CreateTransactionRefHelper();
                if (_transactionLogService.TransactionReferenceExist(tranxRefNo))
                {
                    CreateTransactionRef();
                }
                else
                {
                    transactionRefNo = tranxRefNo;
                    break;
                }
            }
          
            return transactionRefNo;
        }

        private string CreateTransactionRefHelper()
        {
            Random rng = new Random();
            StringBuilder builder = new StringBuilder();
            while (builder.Length < 16)
            {
                builder.Append(rng.Next(10).ToString());
            }

            return "SRI" + builder.ToString();

            //return "SRI5535739914645655";
        }

        //private int GenerateSeed()
        //{
        //    int seed = 0;
        //    string guid = Guid.NewGuid().ToString("N");

        //    foreach (char character in guid)
        //    {
        //        seed += character.ToInt();
        //    }

        //    return seed;
        //}






    }
}