using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Session
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 400)]
    public class GlobalGpsManager : MySessionComponentBase
    {
        struct DynamicGpsId
        {
            long playerId;
            string gpsName;

            public DynamicGpsId(long playerId, string gpsName)
            {
                this.playerId = playerId;
                this.gpsName = gpsName;
            }
        }

        private const string FILENAME = "GlobalGpsData.xml";

        public static GlobalGpsManager Static;

        private HashSet<MyGps> Gpss;
        private HashSet<MyGps> ToAddGpss;
        private Dictionary<long, HashSet<int>> AddedGpss;
        private Dictionary<DynamicGpsId, int> DynamicGpss;

        public void AddGps(string name, Color color, Vector3D pos)
        {
            MyGps g = new MyGps()
            {
                Name = name,
                Coords = pos,
                ShowOnHud = true,
                GPSColor = color,
                AlwaysVisible = true,
                DiscardAt = null
            };
            g.UpdateHash();
            ToAddGpss.Add(g);
        }

        public override void LoadData()
        {
            MyObjectBuilder_GpsManager builder;
            if (FileUtils.FileExistsInWorldStorage(FILENAME, typeof(GlobalGpsManager)))
            {
                builder = FileUtils.ReadXmlFileFromWorld<MyObjectBuilder_GpsManager>(FILENAME, typeof(GlobalGpsManager));
            }
            else
            {
                builder = new MyObjectBuilder_GpsManager();
            }

            Gpss = new HashSet<MyGps>();
            ToAddGpss = new HashSet<MyGps>();
            AddedGpss = new Dictionary<long, HashSet<int>>();
            DynamicGpss = new Dictionary<DynamicGpsId, int>();
            foreach(var gps in builder.Gpss)
            {
                AddGps(gps.name, new Color(gps.color), gps.position);
            }

            Static = this;
        }

        public override void SaveData()
        {
            MyObjectBuilder_GpsManager builder = new MyObjectBuilder_GpsManager();
            foreach(var gps in Gpss)
            {
                GpsData data = new GpsData();
                data.color = gps.GPSColor.ToVector3();
                data.name = gps.Name;
                data.position = gps.Coords;

                builder.Gpss.Add(data);
            }

            FileUtils.WriteXmlFileToWorld(builder, FILENAME, typeof(GlobalGpsManager));
        }

        public override void UpdateBeforeSimulation()
        {
            if (SettingsSession.Static.Settings.Enable || true)
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
                    AlwaysVisible = true,
                    DiscardAt = null
                };
                gps.CalculateHash();
                gps.UpdateHash();
                MySession.Static.Gpss.SendAddGps(playerId, ref gps, playSoundOnCreation: false);
                DynamicGpss.Add(id, gps.Hash);
            }
            else
            {
                gps = new MyGps()
                {
                    Name = gpsName,
                    Coords = position,
                    ShowOnHud = true,
                    GPSColor = color,
                    AlwaysVisible = true,
                    DiscardAt = null
                };
                gps.CalculateHash();
                gps.UpdateHash();
                MySession.Static.Gpss.SendDelete(playerId, DynamicGpss[id]);
                MySession.Static.Gpss.SendAddGps(playerId, ref gps, playSoundOnCreation: false);
                DynamicGpss[id] = gps.Hash;
            }   
        }

        public void RemoveDynamicGps(string gpsName, long playerId)
        {
            DynamicGpsId id = new DynamicGpsId(playerId, gpsName);
            if (DynamicGpss.ContainsKey(id))
            {
                MyLog.Default.WriteLine("GPS Exists");
                MySession.Static.Gpss.SendDelete(playerId, DynamicGpss[id]);
                DynamicGpss.Remove(id);
            }
        }

        protected override void UnloadData()
        {
            Gpss.Clear();
            ToAddGpss.Clear();
            Static = null;
        }
    }
}
