namespace Overlayer.WebAPI.Core.Utils
{
    public static class HttpContextAccessorExtensions
    {
        public static string? GetIpAddress(this HttpContext context)
        {
            if (!string.IsNullOrEmpty(context.Request.Headers["CF-CONNECTING-IP"]))
                return context.Request.Headers["CF-CONNECTING-IP"];
            var ipAddress = context.GetServerVariable("HTTP_X_FORWARDED_FOR");
            if (!string.IsNullOrEmpty(ipAddress))
            {
                var addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                    return addresses.Last();
            }
            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
