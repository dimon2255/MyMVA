using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MyMVA.TagHelpers
{
    public class RepeatTagHelper : TagHelper
    {
        public int Count { get; set; }

        /// <summary>
        /// Process Tag
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            for (int i = 0; i < Count; i++)
            {
                output.Content.AppendHtml(await output.GetChildContentAsync(useCachedResult:false));
            }
        }
    }
}
