using System.Collections.Generic;
using SmartStore.Core.Domain.Promotions;

namespace SmartStore.Services.Promotions
{
	/// <summary>
	/// promotion service interface
	/// </summary>
	public partial interface IPromotionProductsService
	{
		/// <summary>
		/// Gets an promotion by promotion identifier
		/// </summary>
		/// <param name="PromotionId">promotion identifier</param>
		/// <returns>Affiliate</returns>
		PromotionProducts GetPromotionById(int PromotionId);

		IList<PromotionProducts> GetProductsByPromoId(int promotionId);

		/// <summary>
		/// Marks affiliate as deleted 
		/// </summary>
		/// <param name="Promotion">promotion</param>
		void DeletePromotion(PromotionProducts promotion);

		/// <summary>
		/// Gets all affiliates
		/// </summary>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>promotion collection</returns>
		IList<PromotionProducts> GetAllPromotions(bool showHidden = false);

		IList<PromotionProducts> GetAllDisplayPromotions(bool showHidden = false);
		/// <summary>
		/// Inserts an promotion
		/// </summary>
		/// <param name="promotion">promotion</param>
		void InsertPromotion(PromotionProducts promotion);

		/// <summary>
		/// Updates the promotion
		/// </summary>
		/// <param name="promotion">promotion</param>
		void UpdatePromotion(PromotionProducts promotion);
        
    }
}