using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace Overlayer.WebAPI
{
    public class BinaryInputFormatter : InputFormatter
    {
        const string binaryContentType = "application/octet-stream";
        public BinaryInputFormatter() => SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(binaryContentType));
        public async override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await context.HttpContext.Request.Body.CopyToAsync(ms);
                return await InputFormatterResult.SuccessAsync(ms.ToArray());
            }
        }
        protected override bool CanReadType(Type type) => type == typeof(byte[]);
    }
}
