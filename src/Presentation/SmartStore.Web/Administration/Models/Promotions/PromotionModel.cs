using System;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Validators.Promotions;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Admin.Models.Stores;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Promotions;

namespace SmartStore.Admin.Models.Promotions
{
	[Validator(typeof(PromotionValidator))]
	public class PromotionModel : EntityModelBase
	{
		public PromotionModel()
		{
			PromotionProducts = new List<PromotionProductsModel>();
		}

		[SmartResourceDisplayName("Admin.Promotions.Promotion.ID")]
		public override int Id { get; set; }

		[SmartResourceDisplayName("Admin.Promotions.Promotion.Active")]
		public bool Active { get; set; }

		
		//title
		[SmartResourceDisplayName("Admin.Promotions.Promotion.Title")]
		public string Title { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.TitleFontType")]
		public string TitleFontType { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.TitleFontSize")]
		public int TitleFontSize { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.TitleFontColor")]
		public string TitleFontColor { get; set; }

		//subtitle
		[SmartResourceDisplayName("Admin.Promotions.Promotion.SubTitle")]
		public string SubTitle { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.SubTitleFontType")]
		public string SubTitleFontType { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.SubTitleFontSize")]
		public int SubTitleFontSize { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.SubTitleFontColor")]
		public string SubTitleFontColor { get; set; }

		//DiscountText 
		[SmartResourceDisplayName("Admin.Promotions.Promotion.DiscountText")]
		public string DiscountText { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.DiscountTextFontType")]
		public string DiscountTextFontType { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.DiscountTextFontSize")]
		public int DiscountTextFontSize { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.DiscountTextFontColor")]
		public string DiscountTextFontColor { get; set; }

		[SmartResourceDisplayName("Admin.Promotions.Promotion.Description")]
		public string Description { get; set; }

		//textframe
		[SmartResourceDisplayName("Admin.Promotions.Promotion.TextFrame")]
		public string TextFrame { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.TextFrameType")]
		public string TextFrameType { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.TextFrameBackground")]
		public string TextFrameBackground { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.TextFrameHeight")]
		public int TextFrameHeight { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.TextFrameWidth")]
		public int TextFrameWidth { get; set; }

		[SmartResourceDisplayName("Admin.Promotions.Promotion.ExpiryDate")]
		public DateTime? ExpiryDate { get; set; }

		[SmartResourceDisplayName("Admin.Promotions.Promotion.NoOfColumn")]
		public int NoOfColumn { get; set; }

		//[SmartResourceDisplayName("Admin.Promotions.Fields.Picture")]		
		[SmartResourceDisplayName("Admin.Promotions.Promotion.Picture")]
		public int PictureId { get; set; }
		public string PictureUrl { get; set; }

		[SmartResourceDisplayName("Admin.Promotions.Promotion.DiscountPercentage")]
		public int DiscountPercentage { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.DiscountAmount")]
		public double DiscountAmount { get; set; }

		[SmartResourceDisplayName("Admin.Promotions.Promotion.Published")]
		public bool Published { get; set; }

		public int DisplayOrder { get; set; }
		public List<SelectListItem> AvailableCategories { get; set; }
		public List<SelectListItem> AvailableProducts { get; set; }

		public IList<PromotionProductsModel> PromotionProducts { get; set; }
		

		public bool Deleted { get; set; }

		public int GridPageSize { get; set; }
		public bool UsernamesEnabled { get; set; }

		public PromotionModel ToModel(Promotion model)
		{
			return new PromotionModel
			{
				Active = model.Active,
				
				//title
				Title = model.Title,
				TitleFontType = model.TitleFontType,
				TitleFontSize = model.TitleFontSize,
				TitleFontColor = model.TitleFontColor,

				//subtitle
				SubTitle = model.SubTitle,
				SubTitleFontType = model.SubTitleFontType,
				SubTitleFontSize = model.SubTitleFontSize,
				SubTitleFontColor = model.SubTitleFontColor,

				//discountext
				DiscountText = model.DiscountText,
				DiscountTextFontType = model.DiscountTextFontType,
				DiscountTextFontSize = model.DiscountTextFontSize,
				DiscountTextFontColor = model.DiscountTextFontColor,

				Description = model.Description,

				//textframe
				//TextFrame= model.TextFrameType,
				TextFrameType = model.TextFrameType,
				TextFrameBackground = model.TextFrameBackground,
				TextFrameHeight = model.TextFrameHeight,
				TextFrameWidth = model.TextFrameWidth,

				PictureId = model.PictureId,
				PictureUrl = model.PictureUrl,


				DiscountAmount = model.DiscountAmount,
				DiscountPercentage = model.DiscountPercentage,

				ExpiryDate = model.ExpiryDate,
				NoOfColumn = model.NoOfColumn,
				Published = model.Published,
				DisplayOrder = model.DisplayOrder,
				Id = model.Id,
				Deleted = model.Deleted				

			};
		}

		public Promotion ToEntity()
		{
			return new Promotion
			{
				Active = this.Active,
				
				//title
				Title = this.Title,
				TitleFontType = this.TitleFontType,
				TitleFontSize = this.TitleFontSize,
				TitleFontColor = this.TitleFontColor,

				//subtitle
				SubTitle = this.SubTitle,
				SubTitleFontType = this.SubTitleFontType,
				SubTitleFontSize = this.SubTitleFontSize,
				SubTitleFontColor = this.SubTitleFontColor,

				//discountext
				DiscountText = this.DiscountText,
				DiscountTextFontType = this.DiscountTextFontType,
				DiscountTextFontSize = this.DiscountTextFontSize,
				DiscountTextFontColor = this.DiscountTextFontColor,

				Description = this.Description,

				//textframe
				//TextFrame= model.TextFrameType,
				TextFrameType = this.TextFrameType,
				TextFrameBackground = this.TextFrameBackground,
				TextFrameHeight = this.TextFrameHeight,
				TextFrameWidth = this.TextFrameWidth,

				PictureId = this.PictureId,
				PictureUrl = this.PictureUrl,


				DiscountAmount = this.DiscountAmount,
				DiscountPercentage = this.DiscountPercentage,

				ExpiryDate = this.ExpiryDate,
				NoOfColumn = this.NoOfColumn,
				Published = this.Published,
				DisplayOrder = this.DisplayOrder,
				Id = this.Id,
				Deleted = this.Deleted

			};
		}
	}
}