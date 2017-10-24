using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Promotions;
using SmartStore.Core.Events;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Promotions
{
    /// <summary>
    /// Promotion service
    /// </summary>
    public partial class PromotionProductsService : IPromotionProductsService
	{
        #region Fields

        private readonly IRepository<PromotionProducts> _PromotionRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="PromotionRepository">Promotion repository</param>
        /// <param name="eventPublisher">Event published</param>
        public PromotionProductsService(IRepository<PromotionProducts> PromotionRepository,
            IEventPublisher eventPublisher)
        {
            _PromotionRepository = PromotionRepository;
            _eventPublisher = eventPublisher;
        }

		#endregion

		#region Methods


		public virtual IList<Product> GetProductsPromotionById(int PromotionId)
		{
			if (PromotionId == 0)
				return null;

			var query = _PromotionRepository.Table;
			query = query.Where(a => !a.Deleted);
			query = query.Where(a => a.PromotionId==PromotionId);
			query = query.OrderBy(a => a.Id);
		
			var Promotions = query.ToList();

			return Promotions.Select(d=>d.Product).ToList();
		}


		/// <summary>
		/// Gets an Promotion by Promotion identifier
		/// </summary>
		/// <param name="PromotionId">Promotion identifier</param>
		/// <returns>Promotion</returns>
		public virtual PromotionProducts GetPromotionById(int PromotionId)
        {
            if (PromotionId == 0)
                return null;
            
            return _PromotionRepository.GetById(PromotionId);
        }

		public virtual PromotionProducts FindProductsByPromoId(int promotionId,int productId)
		{
			if (promotionId == 0 || productId==0)
				return null;

			var query = _PromotionRepository.Table;
			query = query.Where(a => !a.Deleted);
			query = query.Where(a => a.PromotionId == promotionId && a.ProductId==productId);
			query = query.OrderByDescending(d => d.Id);
			var Promotions = query.FirstOrDefault();
			return Promotions;
		}

		public virtual IList<PromotionProducts> GetProductsByPromoId(int promotionId)
		{
			if (promotionId == 0)
				return null;

			var query = _PromotionRepository.Table;
			query = query.Where(a => !a.Deleted);
			query = query.Where(a => a.PromotionId==promotionId);
			query = query.OrderByDescending(d => d.Id);
			var Promotions = query.ToList();
			return Promotions;
		}
		/// <summary>
		/// Marks Promotion as deleted 
		/// </summary>
		/// <param name="Promotion">Promotion</param>
		public virtual void DeletePromotion(PromotionProducts Promotion)
        {
            if (Promotion == null)
                throw new ArgumentNullException("Promotion");

            Promotion.Deleted = true;
            UpdatePromotion(Promotion);
        }

        /// <summary>
        /// Gets all Promotions
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Promotion collection</returns>
        public virtual IList<PromotionProducts> GetAllPromotions(bool showHidden = false)
        {
            var query = _PromotionRepository.Table;            
            query = query.Where(a => !a.Deleted);
            query = query.OrderBy(a => a.Id);
            var Promotions = query.ToList();
            return Promotions;
        }

		/// <summary>
		/// Gets all Promotions
		/// </summary>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>Promotion collection</returns>
		public virtual IList<PromotionProducts> GetAllDisplayPromotions(bool showHidden = false)
		{
			var query = _PromotionRepository.Table;
			
			query = query.Where(a => !a.Deleted);			
			query = query.OrderBy(a => a.Id);			
			var Promotions = query.ToList();
			return Promotions;
		}

		/// <summary>
		/// Inserts an Promotion
		/// </summary>
		/// <param name="Promotion">Promotion</param>
		public virtual void InsertPromotion(PromotionProducts Promotion)
        {
            if (Promotion == null)
                throw new ArgumentNullException("Promotion");

            _PromotionRepository.Insert(Promotion);

            //event notification
            _eventPublisher.EntityInserted(Promotion);
        }

        /// <summary>
        /// Updates the Promotion
        /// </summary>
        /// <param name="Promotion">Promotion</param>
        public virtual void UpdatePromotion(PromotionProducts Promotion)
        {
            if (Promotion == null)
                throw new ArgumentNullException("Promotion");

            _PromotionRepository.Update(Promotion);

            //event notification
            _eventPublisher.EntityUpdated(Promotion);
        }

        #endregion
        
    }
}