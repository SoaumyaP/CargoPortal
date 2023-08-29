using FluentValidation;
using Groove.SP.Application.Container.Validations;
using System;

namespace Groove.SP.Application.Container.ViewModels
{
    public class UpdateContainerViaUIViewModel
    {
        public long Id { get; set; }

        public string ContainerNo { get; set; }

        public string ContainerType { get; set; }

        public DateTime? LoadingDate { get; set; }

        public string SealNo { get; set; }

        public string SealNo2 { get; set; }

        public string CarrierSONo { get; set; }

        public void ValidateAndThrow()
        {
            new UpdateContainerViaUIValidator().ValidateAndThrow(this);
        }
    }
}