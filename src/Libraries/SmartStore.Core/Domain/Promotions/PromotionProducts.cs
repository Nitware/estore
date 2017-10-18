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
	/// Represents an PromotionProducts
	/// </summary>
	[DataContract]
	public partial class PromotionProducts : BaseEntity, ISoftDeletable
	{

		[DataMember]
		public int ProductId { get; set; }

		[DataMember]
		public int? CategoryId { get; set; }

		[DataMember]
		public int PromotionId { get; set; }

		[DataMember]
		public bool Deleted { get; set; }

		public virtual Product Product { get; set; }
		public virtual Category Category { get; set; }		
		public virtual Promotion Promotion { get; set; }
	}
}
