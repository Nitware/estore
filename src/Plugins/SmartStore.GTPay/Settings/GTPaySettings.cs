using SmartStore.Core.Configuration;

namespace SmartStore.GTPay.Settings
{
    public abstract class GTPayPaymentSettingsBase : ISettings
    {
        public string DescriptionText { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeePercentage { get; set; }
    }

    //public class GTPayCashOnDeliveryPaymentSettings : GTPayPaymentSettingsBase, ISettings
    //{
    //}
    
    public class GTPayATMCardPaymentSettings : GTPayPaymentSettingsBase, ISettings
    {
        public TransactionStatus TransactionStatus { get; set; }
        public string ExcludedCards { get; set; }
    }

    //public class GTPayPayInStorePaymentSettings : GTPayPaymentSettingsBase, ISettings
    //{
    //}

    //public class GTPayPrepaymentPaymentSettings : GTPayPaymentSettingsBase, ISettings
    //{
    //}

    /// <summary>
    /// Represents card payment processor transaction status
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Successful
        /// </summary>
        Successful = 1,

        /// <summary>
        /// Failed
        /// </summary>
        Failed = 2
    }



}