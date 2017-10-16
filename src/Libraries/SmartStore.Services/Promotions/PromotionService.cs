using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Promotions;
using SmartStore.Core.Events;

namespace SmartStore.Services.Promotions
{
    /// <summary>
    /// Promotion service
    /// </summary>
    public partial class PromotionService : IPromotionService
	{
        #region Fields

        private readonly IRepository<Promotion> _PromotionRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="PromotionRepository">Promotion repository</param>
        /// <param name="eventPublisher">Event published</param>
        public PromotionService(IRepository<Promotion> PromotionRepository,
            IEventPublisher eventPublisher)
        {
            _PromotionRepository = PromotionRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets an Promotion by Promotion identifier
        /// </summary>
        /// <param name="PromotionId">Promotion identifier</param>
        /// <returns>Promotion</returns>
        public virtual Promotion GetPromotionById(int PromotionId)
        {
            if (PromotionId == 0)
                return null;
            
            return _PromotionRepository.GetById(PromotionId);
        }

        /// <summary>
        /// Marks Promotion as deleted 
        /// </summary>
        /// <param name="Promotion">Promotion</param>
        public virtual void DeletePromotion(Promotion Promotion)
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
        public virtual IList<Promotion> GetAllPromotions(bool showHidden = false)
        {
            var query = _PromotionRepository.Table;
            if (!showHidden)
                query = query.Where(a => a.Active);
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
		public virtual IList<Promotion> GetAllDisplayPromotions(bool showHidden = false)
		{
			var query = _PromotionRepository.Table;
			if (!showHidden)
				query = query.Where(a => a.Active);
			query = query.Where(a => !a.Deleted);
			query = query.Where(a => a.ExpiryDate >= DateTime.Now);
			query = query.OrderBy(a => a.Id);			
			var Promotions = query.ToList();
			return Promotions;
		}

		/// <summary>
		/// Inserts an Promotion
		/// </summary>
		/// <param name="Promotion">Promotion</param>
		public virtual void InsertPromotion(Promotion Promotion)
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
        public virtual void UpdatePromotion(Promotion Promotion)
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