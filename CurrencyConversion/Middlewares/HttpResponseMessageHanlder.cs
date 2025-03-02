namespace CurrencyConversion.Middlewares
{
    public class HttpResponseMessageHanlder : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (System.Exception ex)
            {
                _ = ex;
                throw;
            }
        }
    }
}
