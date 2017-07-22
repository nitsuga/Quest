using System.Net;
using System.Threading.Tasks;

namespace Quest.Lib.Google
{
    public class SystemWebClient : WebClient, IWebClient
    {
        public async Task<byte[]> DownloadDataAsync(string address) { return await DownloadDataTaskAsync(address); }
    }
}
