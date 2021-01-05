using SEWorldGenPlugin.http.Responses;
using SEWorldGenPlugin.Utilities;
using System;
using System.Net;

namespace SEWorldGenPlugin.http
{
    /// <summary>
    /// Class to request the latest plugin version from github and checking if the current
    /// version is the latest version. Is a singleton class
    /// </summary>
    public class VersionCheck
    {
        public static VersionCheck Static;

        string m_version;
        string m_latestBuild;
        string m_latestPage;

        /// <summary>
        /// Creates the instance for this class and fetches the latest
        /// version from github
        /// </summary>
        public VersionCheck()
        {
            Static = this;
            m_version = typeof(Startup).Assembly.GetName().Version.ToString();
            GetNewestVersion();
        }

        /// <summary>
        /// Returns the current installed version
        /// </summary>
        /// <returns>The current installed version of this plugin</returns>
        public string GetVersion()
        {
            return m_version;
        }

        /// <summary>
        /// Fetches the latest plugin version from github and processes the result.
        /// </summary>
        /// <returns>The latest version</returns>
        public string GetNewestVersion()
        {
            HttpWebRequest request = WebRequest.Create("https://api.github.com/repos/thorwin99/SEWorldGenPlugin/releases/latest") as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = "SEWorldGenPlugin";
            try
            {
                WebResponse response = request.GetResponse();

                JsonRelease latest = GitHubVersionResponse.Deserialize(response.GetResponseStream());

                m_latestBuild = latest.tag_name.Trim().Substring(1) + ".0";
                m_latestPage = latest.html_url;
                return m_latestBuild;
            }catch(Exception e)
            {
                MyPluginLog.Log(e.Message, LogLevel.ERROR);
                m_latestBuild = m_version;
                return m_version;
            }
            
        }

        /// <summary>
        /// Returns the latest version of the plugin
        /// </summary>
        /// <returns>The latest version as a string</returns>
        public string GetLatestVersionPage()
        {
            return m_latestPage;
        }

        /// <summary>
        /// Checks whether the installed plugin version is the latest
        /// </summary>
        /// <returns>True, if it is the latest version</returns>
        public bool IsNewest()
        {
            if (m_latestBuild.Contains("pre"))
            {
                m_latestBuild.Remove(m_latestBuild.IndexOf("p"));
            }
            string[] vc = m_version.Split('.');
            string[] vl = m_latestBuild.Split('.');

            for(int i = 0; i < Math.Min(vc.Length, vl.Length); i++)
            {
                if (int.Parse(vc[i]) > int.Parse(vl[i])) return true;
                if (int.Parse(vc[i]) < int.Parse(vl[i])) return false;
            }

            return true;
        }

    }
}
