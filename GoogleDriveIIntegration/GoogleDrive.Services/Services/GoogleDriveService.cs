using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using GoogleDrive.Services.Models;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Essentials;

namespace GoogleDrive.Services.Services
{
    public interface IGoogleDriveService
    {
        Task WebLogin();
        Task<string> CreateNewFile();
        Task<GoogleDriveFileModel> ReadFile(string fileId, string folderId = null);
        Task<byte[]> GetByteArrayFromFile(string filename, string folderId);
        Task<bool> SaveFile(string fileId, string filename, byte[] byteArray, string folderId = null);
        Task<string> SaveFile(string filename, byte[] byteArray, string folderId = null);
        Task<List<GoogleDriveFileModel>> GetFiles(string folderId);
        Task<string> GetFileIdByName(string filename, string folderId);
        Task<List<GoogleDriveFileModel>> GetFoldersAsync();
        Task<bool> DeleteFileAsync(string fileId);
        Task<bool> RenameFileAsync(string fileId, string newName);
    }

    public class GoogleDriveService : IGoogleDriveService
    {
        string _clientId = "334177818474-mc4kpcnp9g04ssd2hs13ei6jcvjlfdr1.apps.googleusercontent.com";
        string _redirectUri = "com.googleusercontent.apps.334177818474-mc4kpcnp9g04ssd2hs13ei6jcvjlfdr1:/oauth2redirect";
        string _scope = "https://www.googleapis.com/auth/drive.file";

        IIDentityService identSvc;
        DriveService drvSvc;

        public GoogleDriveService()
        {
            identSvc = new IdentityService();
        }

        #region authentications
        public async Task WebLogin()
        {
            var authenticator = new OAuth2Authenticator(
                clientId: _clientId,
                clientSecret: null,
                scope: _scope,
                authorizeUrl: new Uri("https://accounts.google.com/o/oauth2/auth"),
                redirectUrl: new Uri(_redirectUri),
                accessTokenUrl: new Uri("https://oauth2.googleapis.com/token"),
                getUsernameAsync: null,
                isUsingNativeUI: true
            );

            //authenticator.Completed += OnAuthCompleted;
            //authenticator.Error += OnAuthError;

            AuthenticationState.Authenticator = authenticator;

            var url = await authenticator.GetInitialUrlAsync();
            var authResult = await Xamarin.Essentials.WebAuthenticator.AuthenticateAsync
            (
                url: url,
                callbackUrl: new Uri(_redirectUri)
            );
            var raw = UrlizedResult(authResult);
            var authResponse = new AuthorizeResponse(raw);
            if (!authResponse.IsError)
            {
                identSvc.Setup(
                    "https://oauth2.googleapis.com/token",
                    _clientId,
                    "",
                    _redirectUri
                );

                var authdata = await identSvc.GetTokenAsync(authResponse.Code);

                AuthGoogleDrive(
                    accessToken: authdata.AccessToken,
                    expirationInSeconds: authdata.ExpiresIn,
                    refreshToken: authdata.RefreshToken,
                    scope: authdata.Scope,
                    tokenType: authdata.TokenType
                );

                authdata.Code = authResponse.Code;
            }
        }

        private string UrlizedResult(WebAuthenticatorResult result)
        {
            string code = result?.Properties["code"];
            string idToken = result?.IdToken;
            string scope = result?.Properties["scope"];
            string state = result?.Properties["state"];
            string sessionState = "";

            string redirectUrl = $"{_redirectUri}#code={code}&id_token={idToken}&scope={scope}&state={state}&session_state={sessionState}";

            return redirectUrl;
        }

        private void AuthGoogleDrive(
            string accessToken,
            int expirationInSeconds,
            string refreshToken,
            string scope,
            string tokenType
        )
        {
            try
            {
                var initializer = new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets()
                    {
                        ClientId = _clientId,
                    }
                };
                initializer.Scopes = new[] { scope };
                initializer.DataStore = new FileDataStore("Google.Apis.Auth");
                var flow = new GoogleAuthorizationCodeFlow(initializer);
                var user = "DriveTest";
                var token = new Google.Apis.Auth.OAuth2.Responses.TokenResponse()
                {
                    AccessToken = accessToken,
                    ExpiresInSeconds = expirationInSeconds,
                    RefreshToken = refreshToken,
                    Scope = scope,
                    TokenType = tokenType
                };
                var userCredential = new UserCredential(flow, user, token);
                drvSvc = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = userCredential,
                    ApplicationName = "BackupTest",
                });
                //drvSvcHelper = new DriveServiceHelper(drvSvc);

                //_isGoogleDriveEnabled = true;
            }
            catch (Exception ex)
            {
                //_isGoogleDriveEnabled = false;
            }
        }
        #endregion

        #region drive stuffs
        public async Task<string> CreateNewFile()
        {
            File metadata = new File()
            {
                Parents = new List<string>() { "root" },
                MimeType = "application/octet-stream",
                Name = "Untitled file"
            };

            var newFile = await drvSvc.Files.Create(metadata).ExecuteAsync();

            return newFile != null ? newFile.Id : null;
        }

        public async Task<GoogleDriveFileModel> ReadFile(string fileId, string folderId = null)
        {
            GoogleDriveFileModel data = new GoogleDriveFileModel();

            var metadata = await drvSvc.Files.Get(fileId).ExecuteAsync();
            if (!string.IsNullOrEmpty(folderId))
                metadata.Parents = new List<string> { folderId };

            data.Id = metadata.Id;
            data.Name = metadata.Name;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                await drvSvc.Files.Get(fileId).DownloadAsync(ms);
                ms.Position = 0;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(ms, Encoding.Default))
                {
                    var content = await sr.ReadToEndAsync();
                    data.Content = content;
                }
            }

            return data;
        }

        public async Task<byte[]> GetByteArrayFromFile(string filename, string folderId)
        {
            byte[] b;

            string fileId = await GetFileIdByName(filename, folderId);

            var metadata = await drvSvc.Files.Get(fileId).ExecuteAsync();
            if (!string.IsNullOrEmpty(folderId))
                metadata.Parents = new List<string> { folderId };

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                await drvSvc.Files.Get(fileId).DownloadAsync(ms);
                ms.Position = 0;
                b = ms.ToArray();
            }

            return b;
        }

        public async Task<bool> SaveFile(string fileId, string filename, byte[] byteArray, string folderId = null)
        {
            File metadata = new File()
            {
                Name = filename,
            };

            if (!string.IsNullOrEmpty(folderId))
                metadata.Parents = new List<string> { folderId };

            var progress = await drvSvc.Files.Update(
                metadata,
                fileId,
                new System.IO.MemoryStream(byteArray), "application/octet-stream"
            ).UploadAsync();

            return progress.Status == Google.Apis.Upload.UploadStatus.Completed;
        }

        public async Task<string> SaveFile(string filename, byte[] byteArray, string folderId = null)
        {
            string fileId = null;

            File metadata = new File()
            {
                Name = filename,
                MimeType = "application/octet-stream"
            };

            if (!string.IsNullOrEmpty(folderId))
                metadata.Parents = new List<string>() { folderId };

            var newFile = await this.drvSvc.Files.Create(metadata).ExecuteAsync();
            var progress = await this.drvSvc.Files.Update(
                new File() { Name = filename },
                newFile.Id,
                new System.IO.MemoryStream(byteArray),
                "application/octet-stream").UploadAsync();

            fileId = progress.Status == Google.Apis.Upload.UploadStatus.Completed ? newFile.Id : null;

            return fileId;
        }

        public async Task<List<GoogleDriveFileModel>> GetFiles(string folderId)
        {
            List<GoogleDriveFileModel> ret = new List<GoogleDriveFileModel>();
            FilesResource.ListRequest listRequest = drvSvc.Files.List();
            listRequest.Q = $"'{folderId}' in parents";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            foreach (var file in files)
            {
                ret.Add(new GoogleDriveFileModel()
                {
                    Id = file.Id,
                    Name = file.Name
                });
            }

            await Task.CompletedTask;

            return ret;
        }

        public async Task<string> GetFileIdByName(string filename, string folderId)
        {
            string fileId = string.Empty;

            FilesResource.ListRequest listRequest = drvSvc.Files.List();
            // '18wZ2W3bkwUTxuUvNXVadSJi8nDbSP4rb' in parents and fullText contains 'quick_info'
            listRequest.Q = $"'{folderId}' in parents and trashed=false";

            // List files
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            var thefile = files.SingleOrDefault(x => x.Name == filename);

            await Task.CompletedTask;

            return thefile != null ? thefile.Id : null;
        }

        public async Task<List<GoogleDriveFileModel>> GetFoldersAsync()
        {
            List<GoogleDriveFileModel> ret = new List<GoogleDriveFileModel>();
            FilesResource.ListRequest listRequest = drvSvc.Files.List();
            //listRequest.Q = "mimeType='application/vnd.google-apps.folder' and fullText contains 'Home App' and explicitlyTrashed=false";

            // return folders in root excluding the ones in Trash
            listRequest.Q = "mimeType='application/vnd.google-apps.folder' and trashed=false";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            foreach (var file in files)
            {
                ret.Add(new GoogleDriveFileModel()
                {
                    Id = file.Id,
                    Kind = file.Kind,
                    MimeType = file.MimeType,
                    Name = file.Name
                });
            }

            await Task.CompletedTask;

            return ret;
        }

        public async Task<string> GetFolderIdByNameAsync(string folderName)
        {
            var folderId = string.Empty;

            var folders = await GetFoldersAsync();
            if (folders.Any() && folders.FirstOrDefault(x => x.Name.ToLower() == folderName.ToLower()) != null)
            {
                folderId = folders.FirstOrDefault(x => x.Name.ToLower() == folderName.ToLower()).Id;
            }

            return folderId;
        }

        public async Task<string> CreateFolderAsync(string folderName)
        {
            string folderId = string.Empty;

            File metaData = new File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            File file = drvSvc
                .Files
                .Create(metaData)
                .Execute();

            await Task.CompletedTask;

            return file.Id;
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            bool ret = false;
            try
            {
                var result = drvSvc.Files.Delete(fileId).Execute();
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
            }

            await Task.CompletedTask;

            return ret;
        }

        public async Task<bool> RenameFileAsync(string fileId, string newName)
        {
            bool ret = false;

            try
            {
                await drvSvc.Files.Update(new File() { Name = newName }, fileId).ExecuteAsync();
                ret = true;
            }
            catch (Exception ex)
            {
                ret = false;
            }

            await Task.CompletedTask;

            return ret;
        }
        #endregion
    }

    public class AuthenticationState
    {
        public static OAuth2Authenticator Authenticator;
    }
}
