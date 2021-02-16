using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRageMath;

namespace SEWorldGenPlugin.Session
{
    /// <summary>
    /// Manages the gpss of the plugin.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 400)]
    public class MyGPSManager : MySessionComponentBase
    {
        /// <summary>
        /// Struct that identifies a gps
        /// </summary>
        struct MyGpsId
        {
            public string Name;

            public Vector3 Color;

            public Vector3D Position;

            public MyGpsId(string name, Vector3 color, Vector3D position)
            {
                Name = name;
                Color = color;
                Position = position;
            }
        }

        /// <summary>
        /// File name for the save file containing the gps data
        /// </summary>
        private const string FILENAME = "GpsManagerData.xml";

        /// <summary>
        /// Singleton MyGPSManager instance
        /// </summary>
        public static MyGPSManager Static;

        /// <summary>
        /// A map of all global gpss and the players that gps is known to
        /// </summary>
        private Dictionary<MyGpsId, HashSet<long>> m_globalGpss;

        /// <summary>
        /// A map of all dynamic gpss and their corresponding player ids to the hash of the gps,
        /// to allow modification of it.
        /// </summary>
        private Dictionary<Tuple<MyGpsId, long>, int> m_dynamicGpss;

        /// <summary>
        /// Adds a new gps persistent gps to all players
        /// </summary>
        /// <param name="name">Name of the gps</param>
        /// <param name="color">Color of the gps</param>
        /// <param name="pos">Position of the gps</param>
        public void AddPersistentGps(string name, Color color, Vector3D pos)
        {
            MyGpsId id = new MyGpsId(name, color, pos);

            if (!m_globalGpss.ContainsKey(id))
            {
                MyPluginLog.Debug("Adding new persistent gps " + name);
                m_globalGpss[id] = new HashSet<long>();
            }
        }

        /// <summary>
        /// Checks if a given persistent gps already exists
        /// </summary>
        /// <param name="name">Name of the gps</param>
        /// <param name="color">Color of the gps</param>
        /// <param name="pos">Position of the gps</param>
        /// <returns>True if the persistent gps exists</returns>
        public bool PersistenGpsExists(string name, Color color, Vector3D pos)
        {
            MyGpsId id = new MyGpsId(name, color, pos);
            return m_globalGpss.ContainsKey(id);
        }

        /// <summary>
        /// Adds a new Dynamic gps to the player
        /// </summary>
        /// <param name="name">Name of the gps</param>
        /// <param name="color">Color of the gps</param>
        /// <param name="pos">Position of the gps</param>
        /// <param name="player">Player, the gps belongs to</param>
        /// <returns>False, if the gps is already added, else true</returns>
        public bool AddDynamicGps(string name, Color color, Vector3D pos, long playerId)
        {
            MyGpsId id = new MyGpsId(name, color, Vector3D.Zero);
            Tuple<MyGpsId, long> key = new Tuple<MyGpsId, long>(id, playerId);

            if (m_dynamicGpss.ContainsKey(key)) return false;
            MyGps gps = new MyGps
            {
                Name = name,
                Coords = pos,
                GPSColor = color,
                ShowOnHud = true,
                AlwaysVisible = false,
                DiscardAt = null
            };
            gps.CalculateHash();
            gps.UpdateHash();

            MySession.Static.Gpss.SendAddGps(playerId, ref gps, playSoundOnCreation: false);
            m_dynamicGpss.Add(key, gps.Hash);

            return true;
        }

        /// <summary>
        /// Modifies an existing dynamic gps.
        /// </summary>
        /// <param name="name">Name of the existing gps</param>
        /// <param name="color">Color of the gps, needs to be the same as the old one</param>
        /// <param name="pos">New Position of the gps</param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool ModifyDynamicGps(string name, Color color, Vector3D pos, long playerId)
        {
            MyGpsId id = new MyGpsId(name, color, Vector3D.Zero);
            Tuple<MyGpsId, long> key = new Tuple<MyGpsId, long>(id, playerId);

            if (m_dynamicGpss.ContainsKey(key))
            {
                MyGps gps;
                MySession.Static.Gpss[playerId].TryGetValue(m_dynamicGpss[key], out gps);

                if (gps == null) return false;

                gps.Coords = pos;

                MySession.Static.Gpss.SendModifyGps(playerId, gps);

                gps.UpdateHash();
                m_dynamicGpss[key] = gps.Hash;

                return true;

            }
            return false;
        }

        /// <summary>
        /// Tries to remove a dynamic gps marker from the player
        /// </summary>
        /// <param name="name">Name of the gps</param>
        /// <param name="color">Color of the gps</param>
        /// <param name="player">Player, the gps belongs to</param>
        /// <returns></returns>
        public bool RemoveDynamicGps(string name, Color color, long playerId)
        {
            MyGpsId id = new MyGpsId(name, color, Vector3D.Zero);
            Tuple<MyGpsId, long> key = new Tuple<MyGpsId, long>(id, playerId);

            if (m_dynamicGpss.ContainsKey(key))
            {
                MySession.Static.Gpss.SendDelete(key.Item2, m_dynamicGpss[key]);
                m_dynamicGpss.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks, whether or not the gps already exists.
        /// </summary>
        /// <param name="name">Name of the gps</param>
        /// <param name="color">Color of the gps</param>
        /// <param name="player">Player, the gps belongs to</param>
        /// <returns></returns>
        public bool DynamicGpsExists(string name, Color color, long playerId)
        {
            MyGpsId id = new MyGpsId(name, color, Vector3D.Zero);
            Tuple<MyGpsId, long> key = new Tuple<MyGpsId, long>(id, playerId);

            return m_dynamicGpss.ContainsKey(key);
        }

        /// <summary>
        /// Updates before simulation. Will add all persistent gpss to the players,
        /// that dont have them yet.
        /// </summary>
        public override void UpdateBeforeSimulation()
        {
            if (MySettingsSession.Static.Settings.Enabled)
            {
                foreach (var entry in m_globalGpss.Keys)
                {
                    foreach (var p in MySession.Static.Players.GetOnlinePlayers())
                    {
                        if (m_globalGpss[entry].Contains(p.Identity.IdentityId)) continue;

                        MyGps gps = new MyGps
                        {
                            Name = entry.Name,
                            Coords = entry.Position,
                            GPSColor = entry.Color,
                            ShowOnHud = true,
                            AlwaysVisible = false,
                            DiscardAt = null
                        };
                        MySession.Static.Gpss.SendAddGps(p.Identity.IdentityId, ref gps, playSoundOnCreation: false);
                        m_globalGpss[entry].Add(p.Identity.IdentityId);
                    }
                }
            }
        }

        /// <summary>
        /// Loads persistent gps data
        /// </summary>
        public override void LoadData()
        {
            MyPluginLog.Log("Loading GPS manager data");
            Static = this;

            MyObjectBuilder_WorldGpsData ob;
            if (MyFileUtils.FileExistsInWorldStorage(FILENAME, typeof(MyGPSManager)))
            {
                ob = MyFileUtils.ReadXmlFileFromWorld<MyObjectBuilder_WorldGpsData>(FILENAME, typeof(MyGPSManager));
            }
            else
            {
                ob = new MyObjectBuilder_WorldGpsData();
            }

            m_globalGpss = new Dictionary<MyGpsId, HashSet<long>>();
            m_dynamicGpss = new Dictionary<Tuple<MyGpsId, long>, int>();

            foreach(var item in ob.PersistentGpss)
            {
                var id = new MyGpsId(item.Name, item.Color, item.Position);
                m_globalGpss[id] = item.PlayerIds;
            }

            MyPluginLog.Log("Loading GPS manager data completed");
        }

        /// <summary>
        /// Saves the gpss to the file
        /// </summary>
        public override void SaveData()
        {
            if (!MySettingsSession.Static.IsEnabled()) return;

            MyPluginLog.Log("Saving GPS manager data");

            MyObjectBuilder_WorldGpsData ob = new MyObjectBuilder_WorldGpsData();
            foreach(var entry in m_globalGpss.Keys)
            {
                PersistentGpsData item = new PersistentGpsData();
                item.Name = entry.Name;
                item.Color = entry.Color;
                item.Position = entry.Position;
                item.PlayerIds = m_globalGpss[entry];

                ob.PersistentGpss.Add(item);
            }

            MyFileUtils.WriteXmlFileToWorld(ob, FILENAME, typeof(MyGPSManager));

            MyPluginLog.Log("Saving GPS manager data completed");
        }

        /// <summary>
        /// Unloads all data used by this class
        /// </summary>
        protected override void UnloadData()
        {
            MyPluginLog.Log("Unloading GPS manager data");

            base.UnloadData();
            SaveData();

            Static = null;
            m_globalGpss.Clear();
            m_globalGpss = null;

            foreach(var key in m_dynamicGpss.Keys)
            {
                MySession.Static.Gpss.SendDelete(key.Item2, m_dynamicGpss[key]);
            }

            m_dynamicGpss.Clear();
            m_dynamicGpss = null;

            MyPluginLog.Log("Unloading GPS manager data completed");
        }
    }
}
