using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xNet;

namespace YourNameSpace
{
    public class RequestXNet
    {
        private readonly HttpRequest _request;

        public RequestXNet(string cookie = "", string userAgent = "", string proxy = "", int typeProxy = 0)
        {
            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36";
            }
            this._request = new HttpRequest
            {
                KeepAlive = true,
                AllowAutoRedirect = true,
                Cookies = new CookieDictionary(false),
                UserAgent = userAgent
            };
            this._request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            this._request.AddHeader("Accept-Language", "en-US,en;q=0.9");
            if (!string.IsNullOrEmpty(cookie))
            {
                this.AddCookie(cookie);
            }
            if (proxy != "")
            {
                string[] proxyValues = proxy.Split(':').Select(p => p.Trim()).ToArray();
                switch (proxyValues.Length)
                {
                    case 1:
                        this._request.Proxy = Socks5ProxyClient.Parse("127.0.0.1:" + proxy);
                        break;
                    case 2 when typeProxy == 0:
                        this._request.Proxy = HttpProxyClient.Parse(proxy);
                        break;
                    case 2 when typeProxy != 0:
                        this._request.Proxy = Socks5ProxyClient.Parse(proxy);
                        break;
                    case 4 when typeProxy == 0:
                        this._request.Proxy = new HttpProxyClient(proxyValues[0],
                                Convert.ToInt32(proxyValues[1]),
                                proxyValues[2],
                                proxyValues[3]);
                        break;
                    case 4 when typeProxy != 0:
                        this._request.Proxy = new Socks5ProxyClient(proxyValues[0],
                                Convert.ToInt32(proxyValues[1]),
                                proxyValues[2],
                                proxyValues[3]);
                        break;
                }
            }
        }
        public string RequestPost(string url, string data = "", string contentType = "application/x-www-form-urlencoded")
        {
            try
            {
                var response = string.IsNullOrEmpty(data) || string.IsNullOrEmpty(contentType)
                    ? this._request.Post(url) : this._request.Post(url, data, contentType);

                return response?.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string UpLoad(string url, MultipartContent data = null) => this._request.Post(url, data)?.ToString();

        public void AddCookie(string cookie)
        {
            var cookieArray = cookie.Split(';')
                .Select(x => x.Trim())
                .Select(x => x.Split('='))
                .Where(x => x.Length == 2)
                .ToList();

            foreach (var cookiePair in cookieArray)
            {
                try
                {
                    this._request.Cookies.Add(cookiePair[0], cookiePair[1]);
                }
                catch
                {
                    // Handle exception
                }
            }
        }

        public bool DownLoad(string url, string path)
        {
            try
            {
                this._request.Get(url).ToFile(path);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public string RequestGet(string url) => this._request.Get(url)?.ToString();
        public string RequestGet(string url, string username, string password) => this._request.Get($"{url}?user={username}&password={password}")?.ToString();

        public byte[] GetBytes(string url) => this._request.Get(url, null).ToBytes();

        public string GetCookie() => _request.Cookies.ToString();

        public string Uri() => _request.Address.AbsoluteUri;

        public void AddParam(string name, string value) => _request.AddParam(name, value);

        public void AddHeader(string name, string value) => _request.AddHeader(name, value);

        public void UserAgent(string useragent) => _request.UserAgent = useragent;

        public string RequestPut(string url, string data = "")
        {
            var datatoSend = string.IsNullOrEmpty(data) ? null : new StringContent(data);
            return this._request.Raw(HttpMethod.PUT, url, datatoSend)?.ToString();
        }

    }
}
