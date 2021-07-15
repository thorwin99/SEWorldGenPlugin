using Sandbox.Game.Multiplayer;
using SEWorldGenPlugin.http.Responses;
using SEWorldGenPlugin.Networking;
using SEWorldGenPlugin.Networking.Attributes;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace SEWorldGenPlugin.http
{
    /// <summary>
    /// Class to request the latest plugin version from github and checking if the current
    /// version is the latest version. Is a singleton class
    /// </summary>
    [EventOwner]
    public class VersionCheck
    {
        public static VersionCheck Static;

        private readonly string m_version;
        private string m_latestBuild;
        private string m_latestPage;

        private Dictionary<uint, Action<bool>> m_versionCheckCallbacks;
        private uint m_currentCallbackIndex = 0;

        /// <summary>
        /// Creates the instance for this class and fetches the latest
        /// version from github
        /// </summary>
        public VersionCheck()
        {
            Static = this;
            m_version = typeof(Startup).Assembly.GetName().Version.ToString();
            m_versionCheckCallbacks = new Dictionary<uint, Action<bool>>();
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
        /// Returns the page of the latest version of the plugin
        /// </summary>
        /// <returns>The page of the latest version as a url string</returns>
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
            int vres = CompareVersions(m_version, m_latestBuild);

            return vres >= 0;
        }

        /// <summary>
        /// Compares 2 version strings
        /// </summary>
        /// <param name="v1">Version string 1</param>
        /// <param name="v2">Version string 2</param>
        /// <returns>-1 if v1 < v2, 0 if v1 = v2, 1 if v1 > v2</returns>
        private int CompareVersions(string v1, string v2)
        {
            Regex versionRegex = new Regex(@"(?<vn>\d+)+|pre(?<pre>\d+)?");

            var v1matches = versionRegex.Matches(v1);
            var v2matches = versionRegex.Matches(v2);

            for (int i = 0; i < Math.Max(v1matches.Count, v2matches.Count); i++)
            {
                if (v1matches.Count <= i)
                {
                    
                    if (v2matches[i].Groups["vn"].Value != string.Empty && int.Parse(v2matches[i].Groups["vn"].Value) != 0) return -1;
                    if (v2matches[i].Groups["pre"].Value != string.Empty && int.Parse(v2matches[i].Groups["pre"].Value) != 0) return 1;
                    continue;
                }
                if (v2matches.Count <= i)
                {
                    if (v1matches[i].Groups["vn"].Value != string.Empty && int.Parse(v1matches[i].Groups["vn"].Value) != 0) return 1;
                    if (v1matches[i].Groups["pre"].Value != string.Empty && int.Parse(v1matches[i].Groups["pre"].Value) != 0) return -1;

                    continue;
                }

                string v1n = v1matches[i].Groups["vn"].Value;
                string v2n = v2matches[i].Groups["vn"].Value;
                string v1p = v1matches[i].Groups["pre"].Value;
                string v2p = v2matches[i].Groups["pre"].Value;

                if (v1n != string.Empty && v2n != string.Empty)
                {
                    if (int.Parse(v1n) > int.Parse(v2n)) return 1;
                    if (int.Parse(v1n) < int.Parse(v2n)) return -1;

                    int pre1 = v1p == string.Empty ? 0 : int.Parse(v1p);
                    int pre2 = v2p == string.Empty ? 0 : int.Parse(v2p);

                    if (pre1 < pre2) return -1;
                    if (pre1 > pre2) return 1;
                }
                else
                {
                    if (v1p != string.Empty && v2n != string.Empty) return -1;
                    if (v2p != string.Empty && v1n != string.Empty) return 1;
                    if (v1p != string.Empty && v2p != string.Empty)
                    {
                        int pre1 = int.Parse(v1p);
                        int pre2 = int.Parse(v2p);

                        if (pre1 < pre2) return -1;
                        if (pre1 > pre2) return 1;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Compares the current client version with the server version and calls the callback <paramref name="callback"/> with the result,
        /// if both versions are the same.
        /// </summary>
        /// <param name="callback">Callback to check server version.</param>
        public void CompareVersionWithServer(Action<bool> callback)
        {
            //Add timeout
            m_versionCheckCallbacks.Add(++m_currentCallbackIndex, callback);
            PluginEventHandler.Static.RaiseStaticEvent(GetServerVersion, Sync.MyId, m_currentCallbackIndex);

            Timer timeout = null;
            timeout = new Timer((key) => 
            {
                if (!m_versionCheckCallbacks.ContainsKey((uint)key))
                {
                    timeout?.Dispose();
                    return;
                }

                callback(false);
                m_versionCheckCallbacks.Remove((uint)key);

                timeout?.Dispose();
            }, m_currentCallbackIndex, 1000, 1000);
        }

        [Event(1)]
        [Server]
        private static void GetServerVersion(ulong clientId, uint callbackId)
        {
            PluginEventHandler.Static.RaiseStaticEvent(GetServerVersionClient, Static.GetVersion(), callbackId, clientId);
        }

        [Event(2)]
        [Client]
        private static void GetServerVersionClient(string versionString, uint callbackId)
        {
            if (Static.m_versionCheckCallbacks.ContainsKey(callbackId))
            {
                Static.m_versionCheckCallbacks[callbackId](Static.CompareVersions(Static.m_version, versionString) == 0);
                Static.m_versionCheckCallbacks.Remove(callbackId);
            }
        }
    }
}
