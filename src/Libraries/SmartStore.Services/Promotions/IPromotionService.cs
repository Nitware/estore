using System.Collections.Generic;
using SmartStore.Core.Domain.Promotions;

namespace SmartStore.Services.Promotions
{
	/// <summary>
	/// promotion service interface
	/// </summary>
	public partial interface IPromotionService
	{
		/// <summary>
		/// Gets an promotion by promotion identifier
		/// </summary>
		/// <param name="PromotionId">promotion identifier</param>
		/// <returns>Affiliate</returns>
		Promotion GetPromotionById(int PromotionId);

		/// <summary>
		/// Marks affiliate as deleted 
		/// </summary>
		/// <param name="Promotion">promotion</param>
		void DeletePromotion(Promotion promotion);

		/// <summary>
		/// Gets all affiliates
		/// </summary>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>promotion collection</returns>
		IList<Promotion> GetAllPromotions(bool showHidden = false);

		IList<Promotion> GetAllDisplayPromotions(bool showHidden = false);
		/// <summary>
		/// Inserts an promotion
		/// </summary>
		/// <param name="promotion">promotion</param>
		void InsertPromotion(Promotion promotion);

		/// <summary>
		/// Updates the promotion
		/// </summary>
		/// <param name="promotion">promotion</param>
		void UpdatePromotion(Promotion promotion);
        
    }
}