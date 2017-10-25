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
	[Validator(typeof(PromotionProductsValidator))]
	public class PromotionProductsModel : EntityModelBase
	{
		public PromotionProductsModel()
		{
			Product = new ProductModel();
			Category = new CategoryModel();
		}

		[SmartResourceDisplayName("Admin.Promotions.Promotion.ID")]
		public override int Id { get; set; }

		
		[SmartResourceDisplayName("Admin.Promotions.Promotion.ProductId")]
		public int ProductId { get; set; }
		[SmartResourceDisplayName("Admin.Promotions.Promotion.CategoryId")]
		public int? CategoryId { get; set; }

		

		public List<SelectListItem> AvailableCategories { get; set; }
		public List<SelectListItem> AvailableProducts { get; set; }


		public ProductModel Product { get; set; }
		public CategoryModel Category { get; set; }

		public bool Deleted { get; set; }

		public int GridPageSize { get; set; }
		public bool UsernamesEnabled { get; set; }

		public string ProductName { get; set; }
		public PromotionProductsModel ToModel(PromotionProducts model)
		{
			return new PromotionProductsModel
			{	ProductId = model.ProductId,
				CategoryId = model.CategoryId,

			
				Id = model.Id,
				Deleted = model.Deleted,
				Product = model.Product.ToModel(),
				Category = model.CategoryId > 0 ? model.Category.ToModel() : new CategoryModel()

			};
		}

		public PromotionProducts ToEntity()
		{
			return new PromotionProducts
			{
				ProductId = this.ProductId,
				CategoryId = this.CategoryId,
				
				Id = this.Id,
				Deleted = this.Deleted

			};
		}
	}
}