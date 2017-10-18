using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Promotions;
using SmartStore.Core;
using SmartStore.Core.Domain.Promotions;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Promotions;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;
using SmartStore.Services.Stores;
using SmartStore.Services.Media;
using SmartStore.Services.Configuration;
using SmartStore.Core.Domain.Cms;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public partial class PromotionController : AdminControllerBase
	{
		#region Fields

		private readonly IWorkContext _workContext;
		private readonly ISettingService _settingService;
		private readonly ILocalizationService _localizationService;
		private readonly IPermissionService _permissionService;
		private readonly ILocalizedEntityService _localizedEntityService;
		private readonly ILanguageService _languageService;
		private readonly IPictureService _pictureService;
		private readonly ContentSliderSettings _contentSliderSettings;
		private readonly IStoreService _storeService;
		private readonly ICategoryService _categoryService;
		private readonly IProductService _productService;
		private readonly IPromotionService _promotionService;
		private readonly IPromotionProductsService _promotionProductsService;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly CustomerSettings _customerSettings;
		#endregion

		#region Constructors

		public PromotionController(ISettingService settingService,
			ILocalizationService localizationService,
			IPermissionService permissionService,
			ILocalizedEntityService localizedEntityService,
			ILanguageService languageService,
			IPictureService pictureService,
			ContentSliderSettings contentSliderSettings,
			IStoreService storeService,
			IWorkContext workContext,
			ICategoryService categoryService,
		IProductService productService,
		AdminAreaSettings adminAreaSettings,
		CustomerSettings customerSettings,
		IPromotionService promotionService,
		IPromotionProductsService promotionProductService)
		{
			this._settingService = settingService;
			this._localizationService = localizationService;
			this._permissionService = permissionService;
			this._localizedEntityService = localizedEntityService;
			this._languageService = languageService;
			this._pictureService = pictureService;
			this._contentSliderSettings = contentSliderSettings;
			this._storeService = storeService;
			this._workContext = workContext;
			this._adminAreaSettings = adminAreaSettings;
			this._customerSettings = customerSettings;
			this._categoryService = categoryService;
			this._productService = productService;
			this._promotionService = promotionService;
			this._promotionProductsService = promotionProductService;
		}

		#endregion

		#region Utilities

		[NonAction]
		protected PromotionModel PreparePromotionModel(Promotion promotion, bool excludeProperties, PromotionModel model = null)
		{
			if (model == null)
				model = new PromotionModel();

			if (promotion != null)
			{
				model = model.ToModel(promotion);
				if (!excludeProperties)
				{
					model.Active = promotion.Active;
				}
			}

			model.GridPageSize = _adminAreaSettings.GridPageSize;
			model.UsernamesEnabled = _customerSettings.UsernamesEnabled;

			//address
			//model.Address.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
			return model;
		}

		private void InitCreateEdit()
		{
			var allCategories = _categoryService.GetAllCategories();
			IList<SelectListItem> categories = new List<SelectListItem>();
			foreach (var s in allCategories)
			{
				categories.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
			}

			var allProducts = _productService.GetAllProductsDisplayedOnHomePage();
			IList<SelectListItem> products = new List<SelectListItem>();
			foreach (var s in allProducts)
			{
				products.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
			}

			IList<SelectListItem> fontColors = new List<SelectListItem>();
			fontColors.Add(new SelectListItem { Value = "0", Text = "Default" });
			fontColors.Add(new SelectListItem { Value = "1", Text = "Primary" });
			fontColors.Add(new SelectListItem { Value = "2", Text = "Success" });
			fontColors.Add(new SelectListItem { Value = "3", Text = "Info" });
			fontColors.Add(new SelectListItem { Value = "4", Text = "Warning" });
			fontColors.Add(new SelectListItem { Value = "5", Text = "Danger" });


			IList<SelectListItem> fontTypes = new List<SelectListItem>();
			fontTypes.Add(new SelectListItem { Value = "'AvenirHeavy', arial, sans-serif", Text = "'AvenirHeavy', arial, sans-serif" });
			fontTypes.Add(new SelectListItem { Value = "Arial", Text = "Arial" });
			fontTypes.Add(new SelectListItem { Value = "Times New Roman", Text = "Times New Roman" });
			fontTypes.Add(new SelectListItem { Value = "Courier New, Courier, monospace", Text = "Courier New, Courier, monospace" });


			IList<SelectListItem> frameTypes = new List<SelectListItem>();
			frameTypes.Add(new SelectListItem { Value = "circle", Text = "Circle" });
			frameTypes.Add(new SelectListItem { Value = "oval", Text = "Oval" });
			frameTypes.Add(new SelectListItem { Value = "square", Text = "Square" });
			frameTypes.Add(new SelectListItem { Value = "rectangle", Text = "Rectangle" });


			ViewBag.AllProducts = products;
			ViewBag.AllCategories = categories;
			ViewBag.fontColors = fontColors;
			ViewBag.frameTypes = frameTypes;
			ViewBag.fontTypes = fontTypes;
		}
		#endregion

		#region Methods

		//list
		public ActionResult Index()
		{
			return RedirectToAction("List");
		}

		public ActionResult List()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePromotions))
				return AccessDeniedView();

			var gridModel = new GridModel<PromotionModel>();
			return View(gridModel);
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
		{
			var model = new GridModel<PromotionModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManagePromotions))
			{
				var Promotions = _promotionService.GetAllPromotions(true);


				model.Data = Promotions.PagedForCommand(command).Select(x =>
				{
					var m = PreparePromotionModel(x, false);

					return m;
				});

				model.Total = Promotions.Count;
			}
			else
			{
				model.Data = Enumerable.Empty<PromotionModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
			{
				Data = model
			};
		}

		//create

		public ActionResult Create()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePromotions))
				return AccessDeniedView();

			this.InitCreateEdit();
			var model = new PromotionModel();
			PreparePromotionModel(null, false, model);
			return View(model);
		}

		[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		[FormValueRequired("save", "save-continue")]
		public ActionResult Create(PromotionModel model, bool continueEditing)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePromotions))
				return AccessDeniedView();

			if (model.PictureId == 0)
				ModelState.AddModelError("PictureId", "Picture is required");

			if (ModelState.IsValid)
			{
				model.PictureUrl = _pictureService.GetPictureUrl(model.PictureId);
				//model.LanguageName = _languageService.GetLanguageByCulture(model.LanguageCulture).Name;
				MediaHelper.UpdatePictureTransientState(0, model.PictureId, true);
				model.Active = model.Published;
				Promotion promo = model.ToEntity();
				promo.CreationDate = DateTime.Now;
				promo.CreatedBy = User.Identity.Name;
				_promotionService.InsertPromotion(promo);

				NotifySuccess(_localizationService.GetResource("Admin.Promotions.Added"));
				return continueEditing ? RedirectToAction("Edit", new { id = promo.Id }) : RedirectToAction("List");
			}
			this.InitCreateEdit();
			//If we got this far, something failed, redisplay form
			PreparePromotionModel(null, true, model);
			return View(model);

		}


		//edit
		public ActionResult Edit(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePromotions))
				return AccessDeniedView();

			this.InitCreateEdit();
			var Promotion = _promotionService.GetPromotionById(id);
			if (Promotion == null || Promotion.Deleted)
				//No Promotion found with the specified id
				return RedirectToAction("List");

			var model = PreparePromotionModel(Promotion, false);
			return View(model);
		}

		[HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		public ActionResult Edit(PromotionModel model, bool continueEditing)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePromotions))
				return AccessDeniedView();

			var promotion = _promotionService.GetPromotionById(model.Id);
			if (promotion == null || promotion.Deleted)
				//No Promotion found with the specified id
				return RedirectToAction("List");

			var index = Request.QueryString["index"].ToInt();
			if (model.PictureId == 0)
				ModelState.AddModelError("PictureId", "Picture is required");

			if (ModelState.IsValid)
			{
				promotion.PictureUrl = _pictureService.GetPictureUrl(model.PictureId);

				// delete an old picture (if deleted or updated)
				int prevPictureId = promotion.PictureId;
				//MediaHelper.UpdatePictureTransientState(prevPictureId, model.PictureId, true);

				promotion.PictureId = model.PictureId;
				//promotion.PictureUrl = model.PictureUrl;

				promotion.Active = model.Published;

				//title
				promotion.Title = model.Title;
				promotion.TitleFontType = model.TitleFontType;
				promotion.TitleFontSize = model.TitleFontSize;
				promotion.TitleFontColor = model.TitleFontColor;

				//subtitle
				promotion.SubTitle = model.SubTitle;
				promotion.SubTitleFontType = model.SubTitleFontType;
				promotion.SubTitleFontSize = model.SubTitleFontSize;
				promotion.SubTitleFontColor = model.SubTitleFontColor;

				//discountext
				promotion.DiscountText = model.DiscountText;
				promotion.DiscountTextFontType = model.DiscountTextFontType;
				promotion.DiscountTextFontSize = model.DiscountTextFontSize;
				promotion.DiscountTextFontColor = model.DiscountTextFontColor;

				promotion.Description = model.Description;

				//textframe
				//TextFrame= model.TextFrameType,
				promotion.TextFrameType = model.TextFrameType;
				promotion.TextFrameBackground = model.TextFrameBackground;
				promotion.TextFrameHeight = model.TextFrameHeight;
				promotion.TextFrameWidth = model.TextFrameWidth;

				promotion.DiscountAmount = model.DiscountAmount;
				promotion.DiscountPercentage = model.DiscountPercentage;

				promotion.ExpiryDate = model.ExpiryDate;
				promotion.NoOfColumn = model.NoOfColumn;
				promotion.Published = model.Published;
				promotion.DisplayOrder = model.DisplayOrder;
				if (prevPictureId != model.PictureId)
					promotion.Picture = null;

				_promotionService.UpdatePromotion(promotion);

				NotifySuccess(_localizationService.GetResource("Admin.Promotions.Updated"));
				return continueEditing ? RedirectToAction("Edit", promotion.Id) : RedirectToAction("List");
			}
			this.InitCreateEdit();
			//If we got this far, something failed, redisplay form
			PreparePromotionModel(promotion, true, model);
			return View(model);
		}

		//delete
		[HttpPost]
		public ActionResult Delete(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePromotions))
				return AccessDeniedView();

			var Promotion = _promotionService.GetPromotionById(id);
			if (Promotion == null)
				//No Promotion found with the specified id
				return RedirectToAction("List");

			_promotionService.DeletePromotion(Promotion);
			NotifySuccess(_localizationService.GetResource("Admin.Promotions.Deleted"));
			return RedirectToAction("List");
		}



		#endregion

		#region Products

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult ProductList(GridCommand command, int promoId)
		{
			var model = new GridModel<PromotionProductsModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var promoProducts = _promotionProductsService.GetProductsByPromoId(promoId);

				var productIds = promoProducts.Select(x => x.ProductId).ToArray();
				var products = _productService.GetProductsByIds(productIds);

				model.Data = promoProducts
					.Select(x =>
					{
						var product = products.FirstOrDefault(y => y.Id == x.ProductId);

						return new PromotionProductsModel
						{
							Id = x.Id,
							ProductId = x.ProductId,
							ProductName = product.Name
						};
					});

				model.Total = promoProducts.Count();
			}
			else
			{
				model.Data = Enumerable.Empty<PromotionProductsModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
			{
				Data = model
			};
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult ProductUpdate(GridCommand command, PromotionProductsModel model)
		{
			var product = _promotionProductsService.GetPromotionById(model.Id);

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				product.ProductId = model.ProductId;
				_promotionProductsService.UpdatePromotion(product);
			}

			return ProductList(command, model.ProductId);
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult ProductDelete(int id, GridCommand command)
		{
			var product = _promotionProductsService.GetPromotionById(id);
			var productId = product.ProductId;

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				_promotionProductsService.DeletePromotion(product);
			}

			return ProductList(command, product.PromotionId);
		}

		[HttpPost]
		public ActionResult ProductAdd(int promoId, string selectedProductIds)
		{
			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var productIds = selectedProductIds.SplitSafe(",").Select(x => x.ToInt()).ToArray();
				var products = _productService.GetProductsByIds(productIds);
				PromotionProducts productManu = null;
				var maxDisplayOrder = -1;

				foreach (var product in products)
				{
					var existingProductManus = _promotionProductsService.GetProductsByPromoId(promoId);

					if (!existingProductManus.Any(x => x.ProductId == product.Id && x.PromotionId == promoId))
					{
						_promotionProductsService.InsertPromotion(new PromotionProducts
						{
							Deleted = false,
							PromotionId = promoId,
							ProductId = product.Id
						});
					}
				}
			}
			else
			{
				NotifyAccessDenied();
			}

			return new EmptyResult();
		}

		#endregion
	}
}
