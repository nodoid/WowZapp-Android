using System.Text.RegularExpressions;

namespace LOLApp_Common.UrlHelper
{
    public class UrlParse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="querystring">Query string or full url. </param>
        /// <param name="parameter">The parameter to be parsed out</param>
        /// <returns>Parameter value or null if not found, empty if no value but parameter present</returns>
        public static string ParseGetParameter(string querystring, string parameter)
        {

            int indx = querystring.IndexOf(parameter + "=");
            if (indx < 0)
                return null;
            indx += parameter.Length + 1;

            int end = querystring.IndexOf("&", indx);
            end = end < 0 ? querystring.Length - indx : end -indx;
            if (end == 0)
                return string.Empty;
            return querystring.Substring(indx, end);
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="querystring">Query string must not include anything before ? </param>
        /// <param name="parameter">The parameter to be parsed out</param>
        /// <returns>Parameter value or null if not found</returns>
        public static string ParseGetParameterWithRegex(string querystring, string parameter)
        {
            parameter = Regex.Escape(parameter);
            Regex r = new Regex("&?" + parameter + "=([^&]+)");
            Match m;
            if ((m = r.Match(querystring)).Success)
                return m.Groups[1].Value;
            return null;


        }
        
    }
}