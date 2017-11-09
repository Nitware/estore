using System.Collections.Generic;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using System;

namespace SmartStore.Core.Domain.Promotions
{
	/// <summary>
	/// Represents an Promotions
	/// </summary>
	[DataContract]
	public partial class Promotion : BaseEntity, ISoftDeletable
	{
		//title
		[DataMember]
		public string Title { get; set; }
		[DataMember]
		public string TitleFontType { get; set; }
		[DataMember]
		public int TitleFontSize { get; set; }
		[DataMember]
		public string TitleFontColor { get; set; }

		//Subtitle
		[DataMember]
		public string SubTitle { get; set; }
		[DataMember]
		public string SubTitleFontType { get; set; }
		[DataMember]
		public int SubTitleFontSize { get; set; }
		[DataMember]
		public string SubTitleFontColor { get; set; }


		//discounttext
		[DataMember]
		public string DiscountText { get; set; }
		[DataMember]
		public string DiscountTextFontType { get; set; }
		[DataMember]
		public int DiscountTextFontSize { get; set; }
		[DataMember]
		public string DiscountTextFontColor { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public string TextFrameType { get; set; }
		[DataMember]
		public string TextFrameBackground { get; set; }
		[DataMember]
		public int TextFrameHeight { get; set; }
		[DataMember]
		public int TextFrameWidth { get; set; }
		[DataMember]
		public DateTime CreationDate { get; set; }
		[DataMember]
		public string CreatedBy { get; set; }
		[DataMember]
		public Nullable<DateTime> ExpiryDate { get; set; }
		[DataMember]
		public int NoOfColumn { get; set; }

		[DataMember]
		public int DiscountPercentage { get; set; }
		[DataMember]
		public double DiscountAmount { get; set; }

		[DataMember]
		public int PictureId { get; set; }
		[DataMember]
		public string PictureUrl { get; set; }

		[DataMember]
		public string RedirectUrl { get; set; }

		[DataMember]
		public int DisplayOrder { get; set; }

		[DataMember]
		public bool Published { get; set; }
										   /// <summary>
										   /// Gets or sets a value indicating whether the entity has been deleted
										   /// </summary>
		[DataMember]
		public bool Deleted { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is active
		/// </summary>
		[DataMember]
		public bool Active { get; set; }


        [DataMember]
        public string MenuColor { get; set; }


        public virtual Picture Picture { get; set; }
		public virtual IList<PromotionProducts> PromotionProducts { get; set; }
	}
}
