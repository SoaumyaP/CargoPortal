// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Entity.cs" company="Groove Technology">
//   Copyright (c) Groove Technology. All rights reserved.
// </copyright>
// <summary>
//   Entity Class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Groove.SP.Core.Models;
using MediatR;

namespace Groove.SP.Core
{
    /// <summary>
    /// Entity Class Base
    /// </summary>
    public abstract class Entity
    {
        private List<INotification> _domainEvents;      

        #region Properties

        [Timestamp]
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// Gets or sets the created user.
        /// </summary>
        [MaxLength(128)]
        [Column(TypeName = "NVARCHAR(256)")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the updated user.
        /// </summary>
        [MaxLength(128)]
        [Column(TypeName = "NVARCHAR(256)")]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the updated date.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents?.AsReadOnly();

        #endregion

        #region Methods
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
        /// Audits children entities.
        /// </summary>
        /// <param name="user">The username</param>
        protected virtual void AuditChildren(string user)
        {
            return;
        }

        public void AddDomainEvent(INotification eventItem)
        {
            _domainEvents = _domainEvents ?? new List<INotification>();
            _domainEvents.Add(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        #endregion
    }
}
