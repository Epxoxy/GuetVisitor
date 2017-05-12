using SitesModel.ModelBase;
using System.Collections.Generic;

namespace SitesModel.Providers
{
    public class SitesProvider
    {
        private Dictionary<string, SiteModel> sitesModel = null;

        private SitesProvider() { }

        public SitesProvider(IGuetValuesProvider provider)
        {
            var data = provider.LoadLocalData();
            if(data == null)
            {
                var loader = new SitesXmlLoader(provider);
                loader.load(provider.LoadSitesXMLStream(), ref sitesModel);
                if (sitesModel != null)
                    provider.SaveDataToLocal(sitesModel);
            }
        }

        public void reload(IGuetValuesProvider provider)
        {
            var loader = new SitesXmlLoader(provider);
            loader.load(provider.LoadSitesXMLStream(), ref sitesModel);
            if (sitesModel != null)
                provider.SaveDataToLocal(sitesModel);
        }

        public SiteModel getSiteModel(string key)
        {
            if (sitesModel == null || !sitesModel.ContainsKey(key)) return null;
            return sitesModel[key];
        }
    }
}
