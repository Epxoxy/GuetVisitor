using GuetSample.Extension;
using SitesModel.Request;

namespace GuetSample
{
    public class Requester : RequestWrapper
    {
        public Requester() : base()
        {
        }

        public Requester(HttpRequestConfig httpConfig) : base(httpConfig)
        {
        }

        protected override HttpRequestTask provideRequestTask(bool isPost)
        {
            if (isPost) return NetRequestProvider.HttpPost;
            return NetRequestProvider.HttpGet;
        }
    }
}
