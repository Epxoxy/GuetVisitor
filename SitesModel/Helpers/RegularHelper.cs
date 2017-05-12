using System.Data;
using System.Text.RegularExpressions;

namespace SitesModel.Helpers
{
    public static class RegularHelper
    {

        public static string clearHTMLHeadBody(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            string result = string.Empty;
            result = Regex.Replace(input, @"<![\s\S]*?-->", string.Empty);
            result = Regex.Replace(result, @"<script[\s\S]*?script>", string.Empty);
            result = Regex.Replace(result, @"<html[\s\S]*?<body>", string.Empty);
            result = Regex.Replace(result, @">\s+<", "><");
            return result;
        }

        public static string clearHTMLHead(this string input)
        {
            string result = string.Empty;
            result = Regex.Replace(input, @"<![\s\S]*?-->", string.Empty);
            result = Regex.Replace(result, @"<script[\s\S]*?</script>", string.Empty);
            result = Regex.Replace(result, @">\s+<", "><");
            return result;
        }

        public static string clearEmptyLine(this string input)
        {
            return Regex.Replace(input, "\n+\n", " ");
        }

        public static string clearGtLt(this string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            string result = string.Empty;
            result = Regex.Replace(input, @"<.+?>", "\n");
            result = Regex.Replace(result, @"&nbsp;", string.Empty);
            return result;
        }

        public static MatchCollection Matches(this string input, string pattern)
        {
            Regex r = new Regex(pattern);
            return r.Matches(input);
        }

        public static DataTable RegexToTable(this string input, string tableName, string pattern, string[] headers, int maxMatches)
        {
            //Validate data
            if (string.IsNullOrEmpty(input)) return null;
            if (string.IsNullOrEmpty(pattern)) return null;
            //Match
            MatchCollection matches = input.clearHTMLHeadBody().Matches(pattern);
            if (matches != null && matches.Count > 0 && matches.Count < maxMatches)
            {
                DataTable dt = new DataTable(tableName);
                //Add headers to datatable
                if (headers != null)
                {
                    for (int m = 0; m < headers.Length; m++)
                    {
                        dt.Columns.Add(headers[m], typeof(string));
                    }
                }
                //Add matches data to datatable
                for (int i = 0; i < matches.Count; i++)
                {
                    GroupCollection group = matches[i].Groups;
                    dt.Rows.Add();
                    for (int j = 1; j < group.Count; j++)
                    {
                        string value = group[j].Value.clearGtLt();
                        dt.Rows[i][j - 1] = value;
                    }
                }
                //Make every column readOnly
                for (int m = 0; m < dt.Columns.Count; m++)
                {
                    dt.Columns[m].ReadOnly = true;
                }
                return dt;
            }
            else
            {
                return null;
            }
        }
    }
}
