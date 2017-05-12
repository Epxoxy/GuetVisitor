using SitesModel.Providers;
using SitesModel.Helpers;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using SitesModel.ModelBase;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace GuetSample
{
    public class SitesDataProvider : IGuetValuesProvider
    {
        private bool isLocalFolderExist;
        private string localDirectory;
        private string localBinPath;
        private string localXmlPath;
        private string asmResDir;
        //Basic datas
        private OptionalItem<string>[] term;
        private OptionalItem<string>[] year;
        private OptionalItem<string>[] grade;
        private OptionalItem<string>[] majorList;
        private OptionalItem<string>[] courseProperties;
        private static OptionalItem<string>[] type_examplan = new OptionalItem<string>[] {
            new OptionalItem<string>("Normal", "0"),
            new OptionalItem<string>("Retake", "1")  };
        private static OptionalItem<string>[] selectTyle = new OptionalItem<string>[] {
            new OptionalItem<string>("Normal", "%d5%fd%b3%a3"),
            new OptionalItem<string>("Retake", "%d6%d8%d0%de")  };

        public SitesDataProvider(string asmResDir)
        {
            this.asmResDir = asmResDir;
            this.localDirectory = AppDomain.CurrentDomain.BaseDirectory + "StoredData";
            this.isLocalFolderExist = ensureLocalFolderExists();
            if (this.isLocalFolderExist)
            {
                this.localBinPath = localDirectory + "/SitesModel.bin";
                this.localXmlPath = localDirectory + "/SitesModel.xml";
            }
        }

        private bool ensureLocalFolderExists()
        {
            if (!Directory.Exists(localDirectory))
            {
                try
                {
                    Directory.CreateDirectory(localDirectory);
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                return false;
            }
            else return true;
        }

        private OptionalItem<string>[] loadTerm(bool createEmpty, int beginYear, int endYear)
        {
            List<OptionalItem<string>> xTerm = new List<OptionalItem<string>>();
            if (createEmpty) xTerm.Add(new OptionalItem<string>("Empty", ""));
            for (int i = endYear; i >= beginYear; i--)
            {
                string s = i + "-" + (i + 1);
                xTerm.Add(new OptionalItem<string>(s + "Second Term", s + "_2"));
                xTerm.Add(new OptionalItem<string>(s + "First Term", s + "_1"));
            }
            return xTerm.ToArray();
        }

        private OptionalItem<string>[] loadYear(bool createEmpty, int beginYear, int endYear)
        {
            List<OptionalItem<string>> xYear = new List<OptionalItem<string>>();
            if (createEmpty) xYear.Add(new OptionalItem<string>("Empty", ""));
            for (int i = endYear; i >= beginYear; i--)
            {
                string s = i + "-" + (i + 1);
                xYear.Add(new OptionalItem<string>(s + "Year", s));
            }
            return xYear.ToArray();
        }

        private OptionalItem<string>[] loadGrades(bool createEmpty, int fromYear, int toYear)
        {
            List<OptionalItem<string>> grades = new List<OptionalItem<string>>();
            if (createEmpty) grades.Add(new OptionalItem<string>("Empty", ""));
            for (int i = toYear; i >= fromYear; i--)
            {
                string year = i.ToString();
                grades.Add(new OptionalItem<string>(year, year));
            }
            return grades.ToArray();
        }

        private OptionalItem<string>[] loadOptionals(string resourcesName, string path)
        {
            string content = string.Empty;
            if (!File.Exists(path))
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                using (Stream stream = asm.GetManifestResourceStream(resourcesName))
                {
                    using (Stream target = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        if (target != null) stream.CopyTo(target);
                    }
                }
            }
            using (FileStream stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }
            }
            List<OptionalItem<string>> xList = new List<OptionalItem<string>>();
            MatchCollection matches = Regex.Matches(content, "<option value=\"(.+?)\">(.+?)</option>");
            foreach (Match match in matches)
            {
                GroupCollection group = match.Groups;
                if (group.Count > 2)
                    xList.Add(new OptionalItem<string>(group[2].Value, group[1].Value));
            }
            if (xList.Count > 0) return xList.ToArray();
            return null;
        }
        

        #region Implements

        public OptionalItem<string>[] GetCourseProperties()
        {
            if (courseProperties == null)
                courseProperties = loadOptionals(asmResDir + ".CourseProperties.txt", localDirectory + "/CourseProperties.txt");
            return courseProperties;
        }

        public OptionalItem<string>[] GetGrades()
        {
            if(grade == null)
                grade = loadGrades(true, 2012, DateTime.Now.Year);
            return grade;
        }

        public OptionalItem<string>[] GetMajors()
        {
            if (majorList == null)
                majorList = loadOptionals(asmResDir + ".MajorList.txt", localDirectory + "/MajorList.txt");
            return majorList;
        }

        public OptionalItem<string>[] GetSelectTypes()
        {
            return selectTyle;
        }

        public OptionalItem<string>[] GetTerms()
        {
            if (term == null)
                term = loadTerm(true, 2012, DateTime.Now.Year + 1);
            return type_examplan;
        }

        public OptionalItem<string>[] GetTypeExams()
        {
            return type_examplan;
        }

        public OptionalItem<string>[] GetYears()
        {
            if (year == null)
                year = loadYear(true, 2012, DateTime.Now.Year + 1);
            return year;
        }

        public Stream LoadSitesXMLStream()
        {
            if (isLocalFolderExist)
            {
                //Get local stored data
                FileInfo file = new FileInfo(localXmlPath);
                if (file.Exists)
                {
                    return file.Open(FileMode.Open);
                }
            }
            //If local not exist, create from asm resources
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            Stream stream = asm.GetManifestResourceStream(asmResDir + ".SitesModel.xml");
            if (isLocalFolderExist)
            {
                StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                byte[] bytes = UTF8Encoding.UTF8.GetBytes(sr.ReadToEnd());
                using (FileStream fs = new FileStream(localXmlPath, FileMode.OpenOrCreate))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
                stream.Position = 0;
            }
            return stream;
        }

        public Dictionary<string, SiteModel> LoadLocalData()
        {
            return null;
            /*
            if (!isLocalFolderExist) return null;
            FileInfo file = new FileInfo(localBinPath);
            if (file.Exists)
            {
                Dictionary<string, SiteModel> localdata = null;
                using (Stream stream = file.Open(FileMode.Open))
                {
                    if (stream != null)
                    {
                        BinaryFormatter bFormat = new BinaryFormatter();
                        localdata = (Dictionary<string, SiteModel>)bFormat.Deserialize(stream);
                    }
                }
                return localdata;
            }
            return null;*/
        }

        public void SaveDataToLocal(Dictionary<string, SiteModel> datas)
        {
            /*
            if (isLocalFolderExist)
            {
                using (FileStream stream = new FileStream(localBinPath, FileMode.OpenOrCreate))
                {
                    BinaryFormatter bFormat = new BinaryFormatter();
                    bFormat.Serialize(stream, datas);
                }
            }*/
        }

        #endregion
    }
}
