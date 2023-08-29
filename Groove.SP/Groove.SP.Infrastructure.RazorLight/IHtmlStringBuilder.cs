using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Groove.SP.Infrastructure.RazorLight
{
    public interface IHtmlStringBuilder
    {
        Task<string> CreateHtmlStringAsync<T>(string templateName, T parameter);
    }
}
