using System;
using System.Threading.Tasks;

namespace Quest.Lib.Google
{
    public interface IWebClient : IDisposable
    {
        byte[] DownloadData(string address);
        string DownloadString(string address);

        Task<byte[]> DownloadDataAsync(string address);
    }
}
