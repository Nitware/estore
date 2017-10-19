using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using SmartStore.Core.Caching;

using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Media;
using SmartStore.Services;

using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Search;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Framework.UI;
using SmartStore.Services.Catalog;
using SmartStore.Services.Promotions;
using SmartStore.Web.Models.Promotions;

namespace SmartStore.Web.Controllers
{
	public partial class PromotionController : PublicControllerBase
    {
		private readonly ICommonServices _services;
		private readonly IPromotionService _promotionService;
		private readonly IPromotionProductsService _promotionProductsService;
		private readonly MediaSettings _mediaSettings;
		private readonly IPictureService _pictureService;
		private readonly ICategoryService _categoryService;

		public PromotionController(ICommonServices commonServices,
			IPromotionService promotionService, MediaSettings mediaSettings,
			IPictureService pictureService, ICategoryService categoryService,
			IPromotionProductsService promotionProductsService)
        {
			this._services = commonServices;
			this._promotionService = promotionService;
			this._mediaSettings=mediaSettings;
			this._pictureService = pictureService;
			this._categoryService = categoryService;
			this._promotionProductsService = promotionProductsService;

		}


		[ChildActionOnly]
		public ActionResult HomepagePromotion()
		{
			var promotions = _promotionService.GetAllDisplayPromotions()				
				.ToList();

			var listModel = promotions
				.Select(x =>
				{
					var catModel =new PromotionModel().ToModel(x);
					catModel.Categories = new List<CategoryModel>();
					var promotionsList = this._promotionProductsService.GetProductsByPromoId(x.Id);
					foreach (var item in promotionsList)
					{
						List<CategoryModel> d1 = this._categoryService.GetProductCategoriesByProductId(item.ProductId).Select(d => d.Category.ToModel()).ToList();
						catModel.Categories= catModel.Categories.Concat(d1).ToList();
					}
					catModel.Categories = catModel.Categories.Distinct().ToList();
					//catModel.Categories = this._categoryService.GetProductCategoriesByProductId(x.ProductId).Select(d => d.Category.ToModel()).ToList();
					// Prepare picture model
					int pictureSize = _mediaSettings.CategoryThumbPictureSize;
					var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true,
						_services.WorkContext.WorkingLanguage.Id,
						_services.StoreContext.CurrentStore.Id);

					catModel.PictureModel = _services.Cache.Get(categoryPictureCacheKey, () =>
					{
						var pictureModel = new PictureModel
						{
							PictureId = x.PictureId,
							Size = pictureSize,
							FullSizeImageUrl = _pictureService.GetPictureUrl(x.PictureId),
							ImageUrl = _pictureService.GetPictureUrl(x.PictureId, pictureSize, false),
							Title = string.Format(T("Media.Category.ImageLinkTitleFormat"), catModel.Title),
							AlternateText = string.Format(T("Media.Category.ImageAlternateTextFormat"), catModel.Title)
						};
						return pictureModel;
					}, TimeSpan.FromHours(6));

					return catModel;
				})
				.ToList();

			if (listModel.Count == 0)
				return Content("");

			_services.DisplayControl.AnnounceRange(promotions);

			return PartialView(listModel);
		}
	}
}
