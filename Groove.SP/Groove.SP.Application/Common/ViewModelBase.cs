//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="ViewModelBase.cs" company="Groove Technology">
//    Copyright (c) Groove Technology. All rights reserved.
//  </copyright>
//  <summary>
//    Base ViewModel Class
//  </summary>
//  --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;

using Groove.SP.Application.Utilities;
using Groove.SP.Core;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.Common
{
    /// <summary>
    /// ViewModel Base Class
    /// </summary>
    public abstract class ViewModelBase<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// Gets or sets the row version.
        /// </summary>
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// Gets or sets the created user.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the updated user.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the updated date.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        protected ViewModelBase()
        {}

        protected ViewModelBase(Entity entity)
        {
            RowVersion = entity.RowVersion;
            CreatedBy = entity.CreatedBy;
            CreatedDate = entity.CreatedDate;
            UpdatedBy = entity.UpdatedBy;
            UpdatedDate = entity.UpdatedDate;
        }
        
        /// <summary>
        /// Audits user.
        /// </summary>
        /// <param name="user">The username</param>
        public virtual void Audit(string user = AppConstant.SYSTEM_USERNAME)
        {
            var utcNow = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(this.CreatedBy))
            {
                this.CreatedBy = user;
                this.CreatedDate = utcNow;
            }

            this.UpdatedBy = user;
            this.UpdatedDate = utcNow;

            this.AuditChildren(user);
        }

        /// <summary>
        /// Audit manually from API
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isUpdating"></param>
        public virtual void AuditForAPI(string user, bool isUpdating)
        {
            if (!isUpdating)
            {
                if (string.IsNullOrWhiteSpace(this.CreatedBy))
                {
                    this.CreatedBy = user;
                }
                if (this.CreatedDate == DateTime.MinValue || this.CreatedDate == null)
                {
                    this.CreatedDate = DateTime.UtcNow;
                }
            }
            if (string.IsNullOrWhiteSpace(this.UpdatedBy))
            {
                this.UpdatedBy = isUpdating ? user : this.CreatedBy;
            }
            if (this.UpdatedDate == DateTime.MinValue || this.UpdatedDate == null)
            {
                this.UpdatedDate = isUpdating ? DateTime.UtcNow : this.CreatedDate;
            }
            this.AuditChildren(user);
        }

        public virtual void Audit(object id, string username)
        {
            var utcNow = DateTime.UtcNow;
            if (id == null)
            {
                CreatedDate = utcNow;
                CreatedBy = username;
            }

            this.UpdatedBy = username;
            this.UpdatedDate = utcNow;

            this.AuditChildren(username);
        }

        /// <summary>
        /// Audits children entities.
        /// </summary>
        /// <param name="user">The username</param>
        protected virtual void AuditChildren(string user)
        {
            return;
        }

        /// <summary>
        /// Validates the view model and throws.
        /// </summary>
        public abstract void ValidateAndThrow(bool isUpdating = false);
    }
}