using RazorLight;
using System;
using System.Threading.Tasks;

namespace Groove.SP.Infrastructure.RazorLight
{
    public class HtmlStringBuilder : IHtmlStringBuilder
    {
        private readonly IRazorLightEngine _razorLight;

        public HtmlStringBuilder(IRazorLightEngine razorLight)
        {
            _razorLight = razorLight;
        }

        public async Task<string> CreateHtmlStringAsync<T>(string templateName, T parameter)
        {
            var html = await _razorLight.CompileRenderAsync(templateName, parameter);
            return PreMailer.Net.PreMailer.MoveCssInline(html).Html;
        }
    }
}
