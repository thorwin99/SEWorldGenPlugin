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
        struct MyGpsData
        {
            public string Name;

            public Vector3 Color;

            public Vector3D Position;

            public Guid Id;

            public bool Hidden;

            public HashSet<long> Players;

            public MyGpsData(string name, Vector3 color, Vector3D position, Guid id, bool hidden = false)
            {
                Name = name;
                Color = color;
                Position = position;
                Id = id;
                Hidden = hidden;
                Players = new HashSet<long>();
            }

            public MyGpsData(string name, Vector3 color, Vector3D position, Guid id, HashSet<long> players, bool hidden)
            {
                Name = name;
                Color = color;
                Position = position;
                Id = id;
                Hidden = hidden;
                Players = players;
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
        /// A map of all global gps ids and data
        /// </summary>
        private Dictionary<Guid, MyGpsData> m_globalGpss;

        /// <summary>
        /// A map of all dynamic gpss and their corresponding player ids to the hash of the gps,
        /// to allow modification of it.
        /// </summary>
        private Dictionary<Tuple<Guid, long>, int> m_dynamicGpss;

        /// <summary>
        /// Adds a new gps persistent gps to all players
        /// </summary>
        /// <param name="name">Name of the gps</param>
        /// <param name="color">Color of the gps</param>
        /// <param name="pos">Position of the gps</param>
        /// <param name="id">The id of the gps</param>
        /// <param name="hidden">If the gps is hidden</param>
        public void AddPersistentGps(string name, Color color, Vector3D pos, Guid id, bool hidden)
        {
            MyGpsData data = new MyGpsData()
            {
                Name = name,
                Color = color,
                Position = pos,
                Id = id,
                Hidden = hidden,
                Players = new HashSet<long>()
            };

            if (!m_globalGpss.ContainsKey(id))
            {
                m_globalGpss[id] = data;
            }
        }

        /// <summary>
        /// Checks if a given persistent gps already exists
        /// </summary>
        /// <param name="id">The id of the gps</param>
        /// <returns>True if the persistent gps exists</returns>
        public bool PersistenGpsExists(Guid id)
        {
            return m_globalGpss.ContainsKey(id);
        }

        /// <summary>
        /// Removes the given persistent gps if it exists
        /// </summary>
        /// <param name="id">Id of persistent gps</param>
        public void RemovePersistentGps(Guid id)
        {
            if (PersistenGpsExists(id))
            {
                MyGps gps = new MyGps
                {
                    Name = m_globalGpss[id].Name,
                    Coords = m_globalGpss[id].Position,
                    GPSColor = m_globalGpss[id].Color,
                    ShowOnHud = true,
                    AlwaysVisible = false,
                    DiscardAt = null
                };

                foreach (var playerId in m_globalGpss[id].Players)
                {
                    MySession.Static.Gpss.SendDelete(playerId, gps.CalculateHash());
                }

                m_globalGpss.Remove(id);
            }
        }

        /// <summary>
        /// Adds a new Dynamic gps to the player
        /// </summary>
        /// <param name="name">Name of the gps</param>
        /// <param name="color">Color of the gps</param>
        /// <param name="pos">Position of the gps</param>
        /// <param name="playerId">Player, the gps belongs to</param>
        /// <param name="id">The id of the gps</param>
        /// <returns>False, if the gps is already added, else true</returns>
        public bool AddDynamicGps(string name, Color color, Vector3D pos, long playerId, Guid id)
        {
            Tuple<Guid, long> key = new Tuple<Guid, long>(id, playerId);

            if (m_dynamicGpss.ContainsKey(key))
            {
                RemoveDynamicGps(playerId, id);
            }
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
        /// <param name="playerId"></param>
        /// <param name="id">The id of the gps</param>
        /// <returns></returns>
        public bool ModifyDynamicGps(string name, Color color, Vector3D pos, long playerId, Guid id)
        {
            Tuple<Guid, long> key = new Tuple<Guid, long>(id, playerId);

            if (m_dynamicGpss.ContainsKey(key))
            {
                MyGps gps;
                MySession.Static.Gpss[playerId].TryGetValue(m_dynamicGpss[key], out gps);

                if (gps == null) return false;

                gps.Coords = pos;
                gps.Name = name;
                gps.GPSColor = color;

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
        /// <param name="id">The gps id</param>
        /// <param name="playerId">The player id of the player this gps belongs to</param>
        /// <returns></returns>
        public bool RemoveDynamicGps(long playerId, Guid id)
        {
            Tuple<Guid, long> key = new Tuple<Guid, long>(id, playerId);

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
        /// <param name="id">The gps id</param>
        /// <param name="playerId">The player id of the player this gps belongs to</param>
        /// <returns></returns>
        public bool DynamicGpsExists(Guid id, long playerId)
        {
            Tuple<Guid, long> key = new Tuple<Guid, long>(id, playerId);

            if (m_dynamicGpss.ContainsKey(key) && MySession.Static.Gpss.ExistsForPlayer(playerId))
            {
                return MySession.Static.Gpss[playerId].ContainsKey(m_dynamicGpss[key]);
            }

            return false;
        }

        /// <summary>
        /// Updates before simulation. Will add all persistent gpss to the players,
        /// that dont have them yet.
        /// </summary>
        public override void UpdateBeforeSimulation()
        {
            if (MySettingsSession.Static.IsEnabled())
            {
                foreach (var entry in m_globalGpss.Keys)
                {
                    foreach (var p in MySession.Static.Players.GetOnlinePlayers())
                    {
                        if (m_globalGpss[entry].Players.Contains(p.Identity.IdentityId)) continue;

                        MyGps gps = new MyGps
                        {
                            Name = m_globalGpss[entry].Name,
                            Coords = m_globalGpss[entry].Position,
                            GPSColor = m_globalGpss[entry].Color,
                            ShowOnHud = !m_globalGpss[entry].Hidden,
                            AlwaysVisible = false,
                            DiscardAt = null
                        };

                        gps.CalculateHash();

                        MySession.Static.Gpss.SendAddGps(p.Identity.IdentityId, ref gps, playSoundOnCreation: false);
                        m_globalGpss[entry].Players.Add(p.Identity.IdentityId);
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
            if (MyFileUtils.FileExistsInWorldStorage(FILENAME))
            {
                ob = MyFileUtils.ReadXmlFileFromWorld<MyObjectBuilder_WorldGpsData>(FILENAME);
            }
            else
            {
                ob = new MyObjectBuilder_WorldGpsData();
            }

            m_globalGpss = new Dictionary<Guid, MyGpsData>();
            m_dynamicGpss = new Dictionary<Tuple<Guid, long>, int>();

            foreach(var item in ob.PersistentGpss)
            {
                var data = new MyGpsData(item.Name, item.Color, item.Position, item.Id, item.PlayerIds, item.Hidden);
                m_globalGpss[item.Id] = data;
            }

            MySession.Static.OnSavingCheckpoint += delegate
            {
                foreach (var key in m_dynamicGpss.Keys)
                {
                    MySession.Static.Gpss.SendDelete(key.Item2, m_dynamicGpss[key]);
                };
                m_dynamicGpss.Clear();
            };

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
            foreach(var entry in m_globalGpss)
            {
                PersistentGpsData item = new PersistentGpsData();
                item.Name = entry.Value.Name;
                item.Color = entry.Value.Color;
                item.Position = entry.Value.Position;
                item.PlayerIds = entry.Value.Players;
                item.Id = entry.Key;
                item.Hidden = entry.Value.Hidden;

                ob.PersistentGpss.Add(item);
            }

            MyFileUtils.WriteXmlFileToWorld(ob, FILENAME);

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

            m_dynamicGpss.Clear();
            m_dynamicGpss = null;

            MyPluginLog.Log("Unloading GPS manager data completed");
        }
    }
}
