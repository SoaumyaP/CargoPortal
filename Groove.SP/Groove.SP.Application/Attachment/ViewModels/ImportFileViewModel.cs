using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Groove.SP.Application.POFulfillment.ViewModels
{
    public class ImportFileViewModel
    {
        [Required]
        public IFormFile Files { get; set; }
    }
}
