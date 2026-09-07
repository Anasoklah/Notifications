using System;

namespace NotificationService.Core.ValueObjects;

public class LocalizedContent
{
    public string  EN { get; set; } = default!;

    public string  AR { get; set; } = default!;



    public string Resolve(string locale)
{
    return locale switch
    {
        "ar" => AR,
        _ => EN // Default to English if locale is not recognized
    };
}

}

