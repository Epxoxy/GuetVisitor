using SitesModel.Helpers;
using SitesModel.ModelBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace SitesModel.Providers
{
    public class SitesXmlLoader
    {
        private IGuetValuesProvider dataProvider = null;

        public SitesXmlLoader(IGuetValuesProvider provider)
        {
            this.dataProvider = provider;
        }

        #region Assignment optionals

        /// <summary>
        /// Assignment the optional array from optionalStringValue
        /// </summary>
        /// <param name="optionalValue">Value the get optional array</param>
        /// <param name="cSplit">Char value to splite string to array</param>
        private Optional<string>[] linkToOptionals(string optionalValue, char cSplit)
        {
            if (string.IsNullOrEmpty(optionalValue)) return null;
            if (dataProvider == null) return null;
            var optionalItems = new List<Optional<string>>();
            string[] array = optionalValue.Split(cSplit);
            foreach (var item in array)
            {
                Optional<string> optionals = new Optional<string>();
                optionals.Items = linkToOptional(item);
                optionalItems.Add(optionals);
            }
            return optionalItems.ToArray();
        }

        /// <summary>
        /// Assignment the optional array from optionalStringValue
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private OptionalItem<string>[] linkToOptional(string param)
        {
            if (string.IsNullOrEmpty(param)) return null;
            if (dataProvider == null) return null;
            switch (param)
            {
                case "term": return dataProvider.GetTerms();
                case "year": return dataProvider.GetYears();
                case "courseProperties": return dataProvider.GetCourseProperties();
                case "type_exam": return dataProvider.GetTypeExams();
                case "grade": return dataProvider.GetGrades();
                case "majorList": return dataProvider.GetMajors();
                case "selecttype": return dataProvider.GetSelectTypes();
                default: return null;
            }
        }

        #endregion

        /// <summary>
        ///  Read test only
        /// </summary>
        public bool load(Stream xmlStream, ref Dictionary<string, SiteModel> models)
        {
            Dictionary<string, SiteModel> cacheSiteModels = new Dictionary<string, SiteModel>();
            #region Read xml and initalize dictionary
            try
            {
                #region Reading
                using (xmlStream)
                {
                    XElement rootNode = XElement.Load(xmlStream);
                    foreach (var childElement in rootNode.Elements())
                    {
                        List<string> cachePostKeys = new List<string>();
                        List<string> cacheMenuKeys = new List<string>();
                        //Get basic attribute of site name and encoding name
                        var siteNameAttribute = childElement.Attribute(Struct.siteNameAttr);
                        var encodingAttribute = childElement.Attribute(Struct.encodingAttr);
                        string siteName = siteNameAttribute == null ? "Common" : siteNameAttribute.Value;
                        string encodingName = encodingAttribute == null ? "utf-8" : encodingAttribute.Value;
                        //Initilize request dictionary
                        Dictionary<string, WebPageModel> webpageModels = new Dictionary<string, WebPageModel>();
                        Dictionary<string, string> patternDictionary = new Dictionary<string, string>();
                        WebPageModel loginModel = null;
                        WebPageModel logoutModel = null;
                        foreach (var element in childElement.Elements())
                        {
                            if (element.Name.LocalName == Struct.commentMark) continue;
                            string key = string.Empty;
                            string pattern = string.Empty;
                            if (element.Name.LocalName == Struct.patternNode)
                            {
                                foreach (var attribute in element.Attributes())
                                {
                                    string value = attribute.Value;
                                    switch (attribute.Name.LocalName)
                                    {
                                        case Struct.keyAttr: { key = value; } break;
                                        case Struct.patternAttr: { pattern = value; } break;
                                    }
                                }
                                patternDictionary.Add(key, pattern);
                                continue;
                            }
                            //init
                            string itemName = element.Name.LocalName;
                            string type = string.Empty;
                            string url = string.Empty;
                            string handHeader = string.Empty;
                            string getHandRule = string.Empty;//TODO for page of second action type
                            string postdataFormat = string.Empty;
                            string nextKey = string.Empty;
                            string nomenu = string.Empty;
                            string headersValue = string.Empty;
                            string optionalValue = string.Empty;
                            ////Get and set basic attributes value
                            foreach (var attribute in element.Attributes())
                            {
                                string value = attribute.Value;
                                switch (attribute.Name.LocalName)
                                {
                                    case Struct.keyAttr: { key = value; } break;
                                    case Struct.typeAttr: { type = value; } break;
                                    case Struct.patternAttr: { pattern = value; } break;
                                    case Struct.urlAttr: { url = value; } break;
                                    case Struct.nextKeyAttr: { nextKey = value; } break;
                                    case Struct.nomenuAttr: { nomenu = value; } break;
                                    case Struct.postdataFormatAttr: { postdataFormat = value; } break;
                                    case Struct.handHeaderAttr: { handHeader = value; } break;
                                    case Struct.getHandRuleAttr: { getHandRule = value; } break;
                                    case Struct.headersAttr: { headersValue = value; } break;
                                    case Struct.optionalAttr: { optionalValue = value; } break;
                                }
                            }
                            System.Diagnostics.Debug.WriteLine(siteName + " -->" + key);
                            //Parse id
                            bool isPost = type == Struct.typePost;
                            //Get headers
                            string[] headers = null;
                            if (!string.IsNullOrEmpty(headersValue)) headers = headersValue.Split('|');
                            ///Create object
                            ///From parameters
                            WebPageModel page = new WebPageModel(key, url, isPost, encodingName, pattern, postdataFormat, headers);
                            //Get if optionals
                            if (!string.IsNullOrEmpty(optionalValue))
                            {
                                page.PostOptionals = linkToOptionals(optionalValue, '|');
                            }
                            if (!string.IsNullOrEmpty(nextKey))
                            {

                            }
                            //Check for login/logout item
                            if (key == Struct.loginKey)
                            {
                                loginModel = page;
                            }
                            else if (key == Struct.logoutKey)
                            {
                                logoutModel = page;
                            }
                            //Add to dictionary
                            webpageModels.Add(key, page);
                            //Cache post/menu item
                            if (isPost) cachePostKeys.Add(key);
                            if (!string.IsNullOrEmpty(nomenu)) nomenu = string.Empty;
                            else cacheMenuKeys.Add(key);
                        }
                        //Initilize siteModel
                        SiteModel siteModel = new SiteModel()
                        {
                            SiteName = siteName,
                            LoginModel = loginModel,
                            LogoutModel = logoutModel,
                            EncodingName = encodingName,
                            WebPageModels = webpageModels,
                            PostKeys = cachePostKeys,
                            MenuKeys = cacheMenuKeys
                        };
                        if (patternDictionary.Count > 0)
                        {
                            siteModel.Patterns = patternDictionary;
                        }
                        //Add model to dictionary
                        cacheSiteModels.Add(siteName, siteModel);
                    }
                }
                #endregion

                models = cacheSiteModels;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Initialize fail with error (XmlModelLoader.cs, Line:315)\n " + e.Message + e.StackTrace);
                return false;
            }
            return true;
            #endregion
        }

        /// <summary>
        /// Output dictionary data to console
        /// </summary>
        /// <param name="dict"></param>
        public static string debugOutput(Dictionary<string, SiteModel> models)
        {
            StringBuilder sb = new StringBuilder("ModelDictionary count " + models.Count);

            foreach (var pair in models)
            {
                Dictionary<string, WebPageModel> requestDict = pair.Value.WebPageModels;
                sb.Append(
                    "\nRequestDict count " + requestDict.Count
                    + "\nSite -> " + pair.Key
                    + "\n  Site Name -> " + pair.Value.SiteName
                    + "\n  Encoding  -> " + pair.Value.EncodingName);
                foreach (var item in requestDict)
                {
                    WebPageModel rbm = item.Value;
                    sb.Append(
                          "\n    key -> " + item.Key
                        + "\n      Url -> " + rbm.Url
                        + "\n      RegexPattern -> " + rbm.RegexPattern);

                    string[] headers;
                    if ((headers = rbm.DataHeaders) != null)
                    {
                        sb.Append("\n      DataHeaders");
                        foreach (var header in headers)
                        {
                            sb.Append("\n        > " + header);
                        }
                    }
                    if (!string.IsNullOrEmpty(item.Value.PostDataFormat))
                    {
                        sb.Append("\n      PostDataFormat");
                        sb.Append("\n        > " + item.Value.PostDataFormat);
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine(sb.ToString());
            return sb.ToString();
        }

        class Struct
        {
            public const string commentMark = "#comment";
            public const string modelsNode = "models";
            public const string siteNode = "site";
            public const string pageNode = "page";
            public const string patternNode = "pattern";
            public const string siteNameAttr = "siteName";
            public const string encodingAttr = "encoding";
            public const string keyAttr = "key";
            public const string idAttr = "id";
            public const string patternAttr = "pattern";
            public const string typeAttr = "type";
            public const string headersAttr = "headers";
            public const string handHeaderAttr = "handHeader";
            public const string urlAttr = "url";
            public const string nomenuAttr = "nomenu";
            public const string postdataFormatAttr = "postdataFormat";
            public const string nextKeyAttr = "nextKey";
            public const string getHandRuleAttr = "getHandRule";
            public const string optionalAttr = "optional";
            public const string typePost = "post";
            public const string loginKey = "Login";
            public const string logoutKey = "Logout";
        }
    }
}
