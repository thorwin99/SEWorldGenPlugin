using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System.Collections.Generic;
using VRage.Game.Components;
using VRageMath;

namespace SEWorldGenPlugin.Session
{
    /// <summary>
    /// Sessioncomponent that manages global gpss. Global gpss are gpss that should be displayed
    /// to all players on the server and be permanent. It also manages dynamic gpss which are only displayed to
    /// one player and can be dynamically updated.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 400)]
    public class GlobalGpsManager : MySessionComponentBase
    {
        /// <summary>
        /// Struct that represents the id for a dynamic gps. The ID of a dynamic gps is
        /// made up of the id of the player that the gps is displayed to, and the name of the gps.
        /// </summary>
        struct DynamicGpsId
        {
            public long playerId;
            public string gpsName;

            public DynamicGpsId(long playerId, string gpsName)
            {
                this.playerId = playerId;
                this.gpsName = gpsName;
            }
        }

        /// <summary>
        /// File name for the save file containing the global gps info
        /// </summary>
        private const string FILENAME = "GlobalGpsData.xml";

        /// <summary>
        /// Singleton GlobalGpsManager instance
        /// </summary>
        public static GlobalGpsManager Static;

        /// <summary>
        /// All currently managed global gpss
        /// </summary>
        private HashSet<MyGps> Gpss;

        /// <summary>
        /// All gpss that have to be added
        /// </summary>
        private HashSet<MyGps> ToAddGpss;

        /// <summary>
        /// Dictionary contains all global gpss that are currently displayed to a player.
        /// Key is the player id and the value is a hashset containing all gps hashes of the global
        /// gpss currently shown to the player
        /// </summary>
        private Dictionary<long, HashSet<int>> AddedGpss;

        /// <summary>
        /// Dictionary that contains all dynamic gpss.
        /// The key is the dynamic gps id, while the value is the hash of the gps used
        /// to modify it.
        /// </summary>
        private Dictionary<DynamicGpsId, int> DynamicGpss;

        /// <summary>
        /// Adds a new gps to the global gps to be added list.
        /// Will be added in the next update of the simulation.
        /// </summary>
        /// <param name="name">Name of the gps</param>
        /// <param name="color">Color of the gps</param>
        /// <param name="pos">Position of the gps</param>
        public void AddGps(string name, Color color, Vector3D pos)
        {
            MyGps g = new MyGps()
            {
                Name = name,
                Coords = pos,
                ShowOnHud = true,
                GPSColor = color,
                AlwaysVisible = false,
                DiscardAt = null
            };
            g.UpdateHash();
            ToAddGpss.Add(g);
        }

        /// <summary>
        /// Loads all existend global and dynamic gpss from the save file
        /// </summary>
        public override void LoadData()
        {
            LegacyMyObjectBuilder_GpsManager builder;
            if (MyFileUtils.FileExistsInWorldStorage(FILENAME, typeof(GlobalGpsManager)))
            {
                builder = MyFileUtils.ReadXmlFileFromWorld<LegacyMyObjectBuilder_GpsManager>(FILENAME, typeof(GlobalGpsManager));
            }
            else
            {
                builder = new LegacyMyObjectBuilder_GpsManager();
            }

            Gpss = new HashSet<MyGps>();
            ToAddGpss = new HashSet<MyGps>();
            AddedGpss = new Dictionary<long, HashSet<int>>();
            DynamicGpss = new Dictionary<DynamicGpsId, int>();
            foreach(var gps in builder.Gpss)
            {
                AddGps(gps.name, new Color(gps.color), gps.position);
            }

            foreach(var gps in builder.DynamicGpss)
            {
                DynamicGpss.Add(new DynamicGpsId(gps.playerId, gps.gpsName), gps.gpsHash);
            }

            Static = this;
        }

        /// <summary>
        /// Saves all dynamic and global gpss to the save data.
        /// </summary>
        public override void SaveData()
        {
            LegacyMyObjectBuilder_GpsManager builder = new LegacyMyObjectBuilder_GpsManager();
            foreach(var gps in Gpss)
            {
                GpsData data = new GpsData();
                data.color = gps.GPSColor.ToVector3();
                data.name = gps.Name;
                data.position = gps.Coords;

                builder.Gpss.Add(data);
            }

            foreach(var gps in DynamicGpss)
            {
                DynamicGpsData data = new DynamicGpsData();
                data.playerId = gps.Key.playerId;
                data.gpsName = gps.Key.gpsName;
                data.gpsHash = gps.Value;

                builder.DynamicGpss.Add(data);
            }

            MyFileUtils.WriteXmlFileToWorld(builder, FILENAME, typeof(GlobalGpsManager));
        }

        /// <summary>
        /// Run before the simulation update.
        /// Adds all to be added gpss to the managed gps list and
        /// updates all players gpss with this new list.
        /// </summary>
        public override void UpdateBeforeSimulation()
        {
            if (MySettingsSession.Static.Settings.Enable || true)
            {
                foreach(var g in ToAddGpss)
                {
                    Gpss.Add(g);
                }
                ToAddGpss.Clear();
                foreach (var gps in Gpss)
                {
                    foreach (var p in MySession.Static.Players.GetOnlinePlayers())
                    {
                        MyGps g = gps;
                        List<MyGps> gpses = new List<MyGps>();

                        if(!(AddedGpss.TryGetValue(p.Identity.IdentityId, out HashSet<int> value) && value.Contains(gps.Hash)))
                        {
                            MySession.Static.Gpss.SendAddGps(p.Identity.IdentityId, ref g, playSoundOnCreation: false);
                            if (!AddedGpss.ContainsKey(p.Identity.IdentityId))
                                AddedGpss.Add(p.Identity.IdentityId, new HashSet<int>());
                            AddedGpss[p.Identity.IdentityId].Add(gps.Hash);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds or updates a dynamic gps. If the dynamic gps with the given name currently is not
        /// added to the player with playerId, it will be added as a new one. If it already exists, it will
        /// be updated.
        /// </summary>
        /// <param name="gpsName">Name of the gps</param>
        /// <param name="playerId">Id of the player</param>
        /// <param name="position">Position of the gps</param>
        /// <param name="color">Color of the gps</param>
        public void AddOrUpdateDynamicGps(string gpsName, long playerId, Vector3D position, Color color)
        {
            DynamicGpsId id = new DynamicGpsId(playerId, gpsName);

            MyGps gps;

            if(!DynamicGpss.ContainsKey(id))
            {
                gps = new MyGps()
                {
                    Name = gpsName,
                    Coords = position,
                    ShowOnHud = true,
                    GPSColor = color,
                    AlwaysVisible = false,
                    DiscardAt = null
                };
                gps.CalculateHash();
                gps.UpdateHash();
                MySession.Static.Gpss.SendAddGps(playerId, ref gps, playSoundOnCreation: false);
                DynamicGpss.Add(id, gps.Hash);
            }
            else
            {
                MySession.Static.Gpss[playerId].TryGetValue(DynamicGpss[id], out gps);
                if(gps == null)
                {
                    DynamicGpss.Remove(id);
                    AddOrUpdateDynamicGps(gpsName, playerId, position, color);
                    return;
                }

                gps.Coords = position;
                MySession.Static.Gpss.SendModifyGps(playerId, gps);
                gps.UpdateHash();
                DynamicGpss[id] = gps.Hash;
            }   
        }

        /// <summary>
        /// Removes a dynamic gps from a player, if it exists
        /// </summary>
        /// <param name="gpsName">Name of the gps</param>
        /// <param name="playerId">Player id of the player</param>
        public void RemoveDynamicGps(string gpsName, long playerId)
        {
            DynamicGpsId id = new DynamicGpsId(playerId, gpsName);
            if (DynamicGpss.ContainsKey(id))
            {
                MySession.Static.Gpss.SendDelete(playerId, DynamicGpss[id]);
                DynamicGpss.Remove(id);
            }
        }

        /// <summary>
        /// Unloads all data used by this session component
        /// </summary>
        protected override void UnloadData()
        {
            DynamicGpss.Clear();

            Gpss.Clear();
            ToAddGpss.Clear();
            Static = null;
        }
    }
}
