using System.IO;

namespace SitesModel.Providers
{
    public interface IStorageProvider
    {
        Stream openLocal(string fileName);
        Stream openAssets(string fileName);
        bool rewriteLocal(string assetFileName, string localFileName);
        object objectFromLocal(string fileName);
        bool saveObjectToLocal(object obj, string fileName);
    }
}
