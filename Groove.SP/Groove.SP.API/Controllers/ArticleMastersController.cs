using Groove.SP.Application.ArticleMaster.Services.Interfaces;
using Groove.SP.Application.ArticleMaster.ViewModels;
using Groove.SP.Application.Authorization;
using Groove.SP.Application.Exceptions;
using Groove.SP.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Groove.SP.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AppAuthorize(Policy = AppPolicies.ImportClientOnly)]
public class ArticleMastersController : ControllerBase
{
    public readonly IArticleMasterService _articleMasterService;

    public ArticleMastersController(IArticleMasterService articleMasterService)
    {
        _articleMasterService = articleMasterService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateArticleMasterViewModel model)
    {
        if (!model.IsPropertyDirty(nameof(model.CreatedOn)))
        {
            model.CreatedOn = DateTime.UtcNow;
            model.FieldStatus[nameof(model.CreatedOn)] = FieldDeserializationStatus.HasValue;
        }
        if (!model.IsPropertyDirty(nameof(model.UpdatedOn)))
        {
            model.UpdatedOn = model.CreatedOn;
            model.FieldStatus[nameof(model.UpdatedOn)] = FieldDeserializationStatus.HasValue;
        }

        model.AuditForAPI(CurrentUser.Username, false);
        model.FieldStatus[nameof(model.CreatedBy)] = FieldDeserializationStatus.HasValue;
        model.FieldStatus[nameof(model.UpdatedBy)] = FieldDeserializationStatus.HasValue;

        // To accept PONo, ShipmentNo, DestCode is blank/empty value ""
        model.PONo ??= string.Empty;
        model.ShipmentNo ??= string.Empty;
        model.DestCode ??= string.Empty;

        var result = await _articleMasterService.CreateAsync(model);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] ArticleMasterViewModel model)
    {
        if (!model.IsPropertyDirty(nameof(model.UpdatedOn)))
        {
            model.UpdatedOn = DateTime.UtcNow;
            model.FieldStatus[nameof(model.UpdatedOn)] = FieldDeserializationStatus.HasValue;
        }

        try
        {
            model.CompanyCode ??= string.Empty;
            model.CompanyType ??= string.Empty;
            model.PONo ??= string.Empty;
            model.ItemNo ??= string.Empty;
            model.ShipmentNo ??= string.Empty;
            model.DestCode ??= string.Empty;

            object[] keys = {
                model.CompanyCode,
                model.CompanyType,
                model.PONo,
                model.ItemNo,
                model.ShipmentNo,
                model.POSeq,
                model.DestCode
            };
            model.AuditForAPI(CurrentUser.Username, true);
            model.FieldStatus[nameof(model.UpdatedBy)] = FieldDeserializationStatus.HasValue;

            var result = await _articleMasterService.UpdateAsync(model, keys);
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found!"))
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }
            throw;
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAsync([FromBody] ArticleMasterViewModel model)
    {
        try
        {
            model.CompanyCode ??= string.Empty;
            model.CompanyType ??= string.Empty;
            model.PONo ??= string.Empty;
            model.ItemNo ??= string.Empty;
            model.ShipmentNo ??= string.Empty;
            model.DestCode ??= string.Empty;

            object[] keys = {
                model.CompanyCode,
                model.CompanyType,
                model.PONo,
                model.ItemNo,
                model.ShipmentNo,
                model.POSeq,
                model.DestCode
            };
            var result = await _articleMasterService.DeleteByKeysAsync(keys);
            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("not found!"))
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }

            throw;
        }
    }
}