using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace TaskManager.Messages.Constants
{
    public class SupportedCultureConstants
    {
        public static IEnumerable<SelectListItem> GetSupportedUICulture(IStringLocalizer localizer)
        {
            return new List<SelectListItem>
            {
                new() { Value = "es", Text = localizer["SpanishLanguage"] },
                new() { Value = "en", Text = localizer["EnglishLanguage"] }
            };
        }
    }
}
