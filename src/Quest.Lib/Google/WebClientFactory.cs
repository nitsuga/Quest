namespace Quest.Lib.Google
{
    public class WebClientFactory : IWebClientFactory
    {
        public IWebClient Create() { return new SystemWebClient(); }
    }
}
