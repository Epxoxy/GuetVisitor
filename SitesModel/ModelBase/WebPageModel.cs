using SitesModel.Helpers;

namespace SitesModel.ModelBase
{
    public class WebPageModel
    {
        public string Key { get; private set; }
        public string Url { get; private set; }
        public bool IsPost { get; private set; }
        public string EncodingName { get; private set; }
        public string RegexPattern { get; private set; }
        public string PostDataFormat { get; private set; }
        public string[] DataHeaders { get; private set; }
        public Optional<string>[] PostOptionals { get; set; }

        public WebPageModel(string key, string url, bool isPost, string encodingName, string regexPattern, string postDataFormat, string[] dataHeaders)
        {
            this.Key = key;
            this.Url = url;
            this.IsPost = isPost;
            this.EncodingName = encodingName;
            this.RegexPattern = regexPattern;
            this.PostDataFormat = postDataFormat;
            this.DataHeaders = dataHeaders;
        }
    }
}
