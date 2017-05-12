using System.Collections.Generic;

namespace SitesModel.ModelBase
{
    public class SiteModel
    {
        public string SiteName { get; set; }
        public string EncodingName { get; set; }
        public WebPageModel LoginModel { get; set; }
        public WebPageModel LogoutModel { get; set; }

        public string LoginUrl => LoginModel == null ? null : LoginModel.Url;
        public string LogoutUrl => LogoutModel == null ? null : LogoutModel.Url;
        
        public Dictionary<string, string> Patterns { get; set; }
        public Dictionary<string, WebPageModel> WebPageModels { get; set; }
        //
        public List<string> MenuKeys { get; internal set; }
        public List<string> PostKeys { get; internal set; }

        public WebPageModel getWebPageModel(string key)
        {
            if (WebPageModels == null || !WebPageModels.ContainsKey(key)) return null;
            return WebPageModels[key];
        }

        public string getPattern(string key)
        {
            if (Patterns == null || !Patterns.ContainsKey(key)) return null;
            return Patterns[key];
        }
    }
}
