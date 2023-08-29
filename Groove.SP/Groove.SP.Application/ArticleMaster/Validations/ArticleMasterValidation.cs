using FluentValidation;
using Groove.SP.Application.ArticleMaster.ViewModels;
using Groove.SP.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Application.ArticleMaster.Validations;

public class ArticleMasterValidation : BaseValidation<ArticleMasterViewModel>
{
    public ArticleMasterValidation(bool isUpdating = false)
    {
        if (isUpdating)
        {
            ValidateUpdate();
        }
        else
        {
            ValidateAdd();
        }

        RuleFor(x => x.CompanyType).MaximumLength(1).When(x => !string.IsNullOrEmpty(x.CompanyType));
    }

    private void ValidateAdd()
    {
        RuleFor(x => x.CompanyCode).NotNull();
        RuleFor(x => x.CompanyType).NotNull();
        RuleFor(x => x.ItemNo).NotNull();
        RuleFor(x => x.POSeq).NotNull();

        // To accept PONo, ShipmentNo, DestCode is blank/empty value ""
        //RuleFor(x => x.PONo).NotNull();
        //RuleFor(x => x.ShipmentNo).NotNull();
        //RuleFor(x => x.DestCode).NotNull();

    }
    private void ValidateUpdate()
    {

    }
}

