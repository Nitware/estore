using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SmartStore.Services.Payments;

namespace SmartStore.GTPay.Services
{
    public class GatewayLuncher : IGatewayLuncher
    {

        public void Lunch(PostProcessPaymentRequest postProcessPaymentRequest, HttpContextBase httpContext)
        {
            string gtpay_mert_id = "8692";
            string gtpay_tranx_id = "SRI1280095759511012";
            string gtpay_tranx_amt = "2350000";
            string gtpay_tranx_curr = "566";
            string gtpay_cust_id = "67";
            //string gtpay_tranx_noti_url = "http://localhost:5569/home/ReponseMessage";

            string gtpay_tranx_noti_url = "http://www.nitware.com.ng";
            string hash = "D3D1D05AFE42AD50818167EAC73C109168A0F108F32645C8B59E897FA930DA44F9230910DAC9E20641823799A107A02068F7BC0F4CC41D2952E249552255710F";
            string parameters_to_hash = gtpay_mert_id + gtpay_tranx_id + gtpay_tranx_amt + gtpay_tranx_curr + gtpay_cust_id + gtpay_tranx_noti_url + hash;
            string gtpay_hash = GenerateSHA512String(parameters_to_hash);
            var url = "http://gtweb2.gtbank.com/orangelocker/gtpaym/tranx.aspx";

            //HttpResponse res = new HttpResponse(); //  Response;

            //System.Net.WebClient client = new System.Net.WebClient();
            //client.UploadValues(url, new System.Collections.Specialized.NameValueCollection());

            //Response.Clear();

            //HttpResponse response = HttpContext; //.Response;


            

            HttpResponseBase response = httpContext.Response;
            response.Clear();

            var sb = new System.Text.StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat("<body onload='document.forms[0].submit()'>");
            sb.AppendFormat("<form action='{0}' method='post'>", url);

            sb.AppendFormat("<input type='hidden' name='gtpay_mert_id' value='{0}'>", gtpay_mert_id);
            sb.AppendFormat("<input type='hidden' name='gtpay_tranx_id' value='{0}'>", gtpay_tranx_id);
            sb.AppendFormat("<input type='hidden' name='gtpay_tranx_amt' value='{0}'>", gtpay_tranx_amt);
            sb.AppendFormat("<input type='hidden' name='gtpay_tranx_curr' value='{0}'>", gtpay_tranx_curr);
            sb.AppendFormat("<input type='hidden' name='gtpay_cust_id' value='{0}'>", gtpay_cust_id);
            sb.AppendFormat("<input type='hidden' name='gtpay_cust_name' value='{0}'>", "Test Customer");
            sb.AppendFormat("<input type='hidden' name='gtpay_tranx_memo' value='{0}'>", "Aba Rose (67) : Purchase of reading table");
            sb.AppendFormat("<input type='hidden' name='gtpay_no_show_gtbank' value='{0}'>", "yes");
            sb.AppendFormat("<input type='hidden' name='gtpay_echo_data' value='{0}'>", "Aba Rose (67): loyal customer");
            sb.AppendFormat("<input type='hidden' name='gtpay_gway_name' value='{0}'>", "");
            sb.AppendFormat("<input type='hidden' name='gtpay_hash' value='{0}'>", gtpay_hash);
            sb.AppendFormat("<input type='hidden' name='gtpay_tranx_noti_url' value='{0}'>", gtpay_tranx_noti_url);

            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            response.Write(sb.ToString());
            response.End();

            //Response.Write(sb.ToString());
            //Response.End();
        }

        private static string GenerateSHA512String(string inputString)
        {
            System.Security.Cryptography.SHA512 sha512 = System.Security.Cryptography.SHA512Managed.Create();
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        private static string GetStringFromHash(byte[] hash)
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        //public ActionResult ReponseMessage()
        //{
        //    System.Collections.Specialized.NameValueCollection responseData = Request.Params;

        //    ViewBag.gtpay_tranx_id = responseData["gtpay_tranx_id"];
        //    ViewBag.gtpay_tranx_status_code = responseData["gtpay_tranx_status_code"];
        //    ViewBag.gtpay_tranx_status_msg = responseData["gtpay_tranx_status_msg"];
        //    ViewBag.gtpay_tranx_amt = responseData["gtpay_tranx_amt"];
        //    ViewBag.gtpay_tranx_curr = responseData["gtpay_tranx_curr"];
        //    ViewBag.gtpay_cust_id = responseData["gtpay_cust_id"];
        //    ViewBag.gtpay_gway_name = responseData["gtpay_gway_name"];
        //    ViewBag.gtpay_echo_data = responseData["gtpay_echo_data"];
        //    ViewBag.gtpay_tranx_amt_small_denom = responseData["gtpay_tranx_amt_small_denom"];
        //    ViewBag.gtpay_verification_hash = responseData["gtpay_verification_hash"];

        //    return View();
        //}




    }
}