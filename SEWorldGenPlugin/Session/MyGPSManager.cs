using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VRage.Game;
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
        /// Struct that contains data for gps creation
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
        /// Struct used to identify dynamic Gpss
        /// </summary>
        struct MyDynamicGpsId
        {
            public Guid GpsId;

            public long PlayerId;

            public MyDynamicGpsId(Guid gpsId, long playerId)
            {
                GpsId = gpsId;
                PlayerId = playerId;
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
        /// A map of all dynamic gpss and their corresponding player ids to the gps,
        /// to allow modification of it.
        /// </summary>
        private Dictionary<MyDynamicGpsId, MyGps> m_dynamicGpss;

        /// <summary>
        /// Dictionary of all dynamic gpss queued to be added
        /// </summary>
        private ConcurrentDictionary<MyDynamicGpsId, MyGps> m_newDynamicGpss;

        /// <summary>
        /// Bag of all dynamic gpss queued to be removed
        /// </summary>
        private List<MyDynamicGpsId> m_toDeleteDynamicGpss;

        /// <summary>
        /// Reference to read save data of this component after loading it
        /// </summary>
        private MyObjectBuilder_WorldGpsData m_loadedData;

        /// <summary>
        /// Whether dynamic gps data was loaded from file yet or not. Needs to be done in the first simulation loop
        /// </summary>
        private bool loadedDynamicGpss = false;

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
                    MySession.Static.Gpss.SendDeleteGpsRequest(playerId, gps.CalculateHash());
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
            MyDynamicGpsId key = new MyDynamicGpsId(id, playerId);

            if (m_dynamicGpss.ContainsKey(key))
            {
                RemoveDynamicGps(playerId, id);

                return false;
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

            m_newDynamicGpss.TryAdd(key, gps);

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
            MyDynamicGpsId key = new MyDynamicGpsId(id, playerId);

            if (m_dynamicGpss.ContainsKey(key))
            {
                MyGps gps;
                MySession.Static.Gpss[playerId].TryGetValue(m_dynamicGpss[key].Hash, out gps);

                if (gps == null) return false;

                gps.Coords = pos;
                gps.Name = name;
                gps.GPSColor = color;
                m_newDynamicGpss[key] = gps;

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
            MyDynamicGpsId key = new MyDynamicGpsId(id, playerId);

            if (m_dynamicGpss.ContainsKey(key))
            {
                lock (m_toDeleteDynamicGpss)
                {
                    m_toDeleteDynamicGpss.Add(key);
                }
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
            MyDynamicGpsId key = new MyDynamicGpsId(id, playerId);

            lock (m_dynamicGpss)
            {
                if (m_dynamicGpss.ContainsKey(key) && MySession.Static.Gpss.ExistsForPlayer(playerId))
                {
                    return MySession.Static.Gpss[playerId].ContainsKey(m_dynamicGpss[key].Hash);
                }
            }
            

            return false;
        }

        /// <summary>
        /// Updates before simulation. Will add all persistent gpss to the players,
        /// that dont have them yet.
        /// Will update all queued dynamic gpss
        /// </summary>
        public override void UpdateBeforeSimulation()
        {
            if (!loadedDynamicGpss)
            {
                foreach (var player in m_loadedData.DynamicGpss)
                {
                    foreach (var gps in player.DynamicGpss)
                    {
                        var collection = MySession.Static.Gpss[player.PlayerId];
                        var mgps = collection[gps.Hash];

                        if (mgps == null) continue;

                        m_dynamicGpss.Add(new MyDynamicGpsId(gps.ID, player.PlayerId), mgps);
                    }
                }
                loadedDynamicGpss = true;
            }

            if (MySettingsSession.Static.IsEnabled() && Sync.IsServer)
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
                        MySession.Static.Gpss.SendAddGpsRequest(p.Identity.IdentityId, ref gps, playSoundOnCreation: false);
                        m_globalGpss[entry].Players.Add(p.Identity.IdentityId);
                    }
                }

                foreach(var entry in m_newDynamicGpss)
                {
                    if (m_dynamicGpss.ContainsKey(entry.Key))
                    {
                        MyGps gps = entry.Value;
                        MySession.Static.Gpss.SendModifyGpsRequest(entry.Key.PlayerId, gps);
                        gps.UpdateHash();
                        m_dynamicGpss[entry.Key] = gps;
                    }
                    else
                    {
                        MyGps gps = entry.Value;
                        MySession.Static.Gpss.SendAddGpsRequest(entry.Key.PlayerId, ref gps, playSoundOnCreation: false);
                        m_dynamicGpss.Add(entry.Key, entry.Value);
                    }
                    m_newDynamicGpss.Remove(entry.Key);
                }

                lock (m_toDeleteDynamicGpss) lock(m_dynamicGpss)
                {
                    foreach (var entry in m_toDeleteDynamicGpss)
                    {
                        if (m_dynamicGpss.ContainsKey(entry))
                        {
                            MySession.Static.Gpss.SendDeleteGpsRequest(entry.PlayerId, m_dynamicGpss[entry].Hash);
                            m_dynamicGpss.Remove(entry);
                        }
                    }
                    m_toDeleteDynamicGpss.Clear();
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

            if (MyFileUtils.FileExistsInWorldStorage(FILENAME))
            {
                m_loadedData = MyFileUtils.ReadXmlFileFromWorld<MyObjectBuilder_WorldGpsData>(FILENAME);
            }
            else
            {
                m_loadedData = new MyObjectBuilder_WorldGpsData();
            }

            m_globalGpss = new Dictionary<Guid, MyGpsData>();
            m_dynamicGpss = new Dictionary<MyDynamicGpsId, MyGps>();
            m_newDynamicGpss = new ConcurrentDictionary<MyDynamicGpsId, MyGps>();
            m_toDeleteDynamicGpss = new List<MyDynamicGpsId>();

            foreach (var item in m_loadedData.PersistentGpss)
            {
                var data = new MyGpsData(item.Name, item.Color, item.Position, item.Id, item.PlayerIds, item.Hidden);
                m_globalGpss[item.Id] = data;
            }

            MyPluginLog.Log("Loading GPS manager data completed");
        }

        /// <summary>
        /// Saves the gpss to the file
        /// </summary>
        public override void SaveData()
        {
            if (!MySettingsSession.Static.IsEnabled() || !Sync.IsServer) return;

            MyPluginLog.Log("Saving GPS manager data");

            m_loadedData = new MyObjectBuilder_WorldGpsData();
            foreach(var entry in m_globalGpss)
            {
                PersistentGpsData item = new PersistentGpsData();
                item.Name = entry.Value.Name;
                item.Color = entry.Value.Color;
                item.Position = entry.Value.Position;
                item.PlayerIds = entry.Value.Players;
                item.Id = entry.Key;
                item.Hidden = entry.Value.Hidden;

                m_loadedData.PersistentGpss.Add(item);
            }

            Dictionary<long, DynamicGpsData> buffer = new Dictionary<long, DynamicGpsData>();

            foreach(var entry in m_dynamicGpss)
            {
                if (!buffer.ContainsKey(entry.Key.PlayerId))
                {
                    buffer.Add(entry.Key.PlayerId, new DynamicGpsData());
                    buffer[entry.Key.PlayerId].PlayerId = entry.Key.PlayerId;
                }

                buffer[entry.Key.PlayerId].DynamicGpss.Add(new DynamicGpsId(entry.Key.GpsId, entry.Value.Hash));
            }

            foreach(var data in buffer.Values)
            {
                m_loadedData.DynamicGpss.Add(data);
            }

            MyFileUtils.WriteXmlFileToWorld(m_loadedData, FILENAME);

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
