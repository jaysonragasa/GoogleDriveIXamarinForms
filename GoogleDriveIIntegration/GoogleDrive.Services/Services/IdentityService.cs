using GoogleDrive.Services.Models;
using GoogleDrive.Services.Providers;
using System.Net;
using System.Threading.Tasks;

namespace GoogleDrive.Services.Services
{
    public interface IIDentityService
    {
        void Setup(string tokenUrl, string clientId, string clientSecret, string redirectUrl);
        Task<AuthModel> GetTokenAsync(string code);
        Task<AuthModel> GetNewTokenAsync(string refresh_token);
        Task<string> GetAsync(string uri, string accessToken);
    }

    public class IdentityService : IIDentityService
    {
        readonly IHttpRequestProvider _requestProvider;

        string _tokenUrl = string.Empty;
        string _clientId = string.Empty;
        string _clientSecret = string.Empty;
        string _redirectUrl = string.Empty;

        public IdentityService()
        {
            this._requestProvider = new HttpRequestProvider();
        }

        public void Setup(
            string tokenUrl,
            string clientId,
            string clientSecret,
            string redirectUrl
        )
        {
            _tokenUrl = tokenUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUrl = redirectUrl;
        }

        public string CreateAuthorizationRequest()
        {
            return null;
        }

        public async Task<AuthModel> GetTokenAsync(string code)
        {
            string data = $"grant_type=authorization_code" +
                $"&code={code}" +
                $"&redirect_uri={WebUtility.UrlEncode(_redirectUrl)}" +
                $"&client_id={_clientId}";
            var token = await _requestProvider.PostAsync<AuthModel>(_tokenUrl, data, _clientId, _clientSecret);
            return token;
        }

        public async Task<AuthModel> GetNewTokenAsync(string refresh_token)
        {
            string data = $"grant_type=refresh_token" +
                $"&refresh_token={refresh_token}" +
                $"&redirect_uri={WebUtility.UrlEncode(_redirectUrl)}" +
                $"&client_id={_clientId}";

            var token = await _requestProvider.PostAsync<AuthModel>(_tokenUrl, data, _clientId, _clientSecret);
            return token;
        }

        public async Task<string> GetAsync(string uri, string accessToken)
        {
            var response = await _requestProvider.GetAsync(uri, accessToken);
            return response;
        }
    }
}