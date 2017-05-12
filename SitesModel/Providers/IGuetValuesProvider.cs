using SitesModel.Helpers;
using SitesModel.ModelBase;
using System.Collections.Generic;
using System.IO;

namespace SitesModel.Providers
{
    public interface IGuetValuesProvider
    {
        OptionalItem<string>[] GetTerms();
        OptionalItem<string>[] GetYears();
        OptionalItem<string>[] GetCourseProperties();
        OptionalItem<string>[] GetTypeExams();
        OptionalItem<string>[] GetGrades();
        OptionalItem<string>[] GetMajors();
        OptionalItem<string>[] GetSelectTypes();
        Stream LoadSitesXMLStream();
        void SaveDataToLocal(Dictionary<string, SiteModel> datas);
        Dictionary<string, SiteModel> LoadLocalData();
    }
}
