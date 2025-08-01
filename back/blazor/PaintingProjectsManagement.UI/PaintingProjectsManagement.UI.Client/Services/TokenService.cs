using System;

namespace PaintingProjectsManagement.UI.Client.Services
{
    public class TokenService
    {
        private string? _token;

        public string? GetToken()
        {
            return _token;
        }

        public void SetToken(string token, bool persist = false)
        {
            _token = token;
            if (persist)
            {
                localStorage.setItem("authToken", token);
            }
        }

        public void ClearToken()
        {
            _token = null;
            localStorage.removeItem("authToken");
        }

        public void InitializeTokenFromStorage()
        {
            _token = localStorage.getItem("authToken");
        }
    }
}