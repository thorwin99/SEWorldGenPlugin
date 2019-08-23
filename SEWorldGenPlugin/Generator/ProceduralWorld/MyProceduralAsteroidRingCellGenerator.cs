using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Utils;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using SEWorldGenPlugin.Generator.Asteroids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Library.Utils;
using VRage.Network;
using VRage.Noise;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralWorld
{
    public class MyProceduralAsteroidRingCellGenerator : MyProceduralWorldModule
    {
        private MyAsteroidGeneratorDefinition m_data;

        private double m_seedTypeProbabilitySum;

        private double m_seedClusterTypeProbabilitySum;

        private bool m_isClosingEntities;

        private List<MyVoxelBase> m_tmpVoxelMapsList = new List<MyVoxelBase>();

        private List<BoundingBoxD> m_tmpClusterBoxes;

        private bool m_enabled;

        public MyProceduralAsteroidRingCellGenerator(int seed, double density, MyProceduralWorldModule parent = null)
            : base(GetSubCellInfo(), 1, seed, density, parent)
        {
            m_enabled = MyFakes.ENABLE_ASTEROIDS;
            m_data = GetData();
            AddDensityFunctionFilled(new MyInfiniteDensityFunction(MyRandom.Instance, 0.003));
            m_seedTypeProbabilitySum = 0.0;
            foreach (double value in m_data.SeedTypeProbability.Values)
            {
                m_seedTypeProbabilitySum += value;
            }
            m_seedClusterTypeProbabilitySum = 0.0;
            foreach (double value2 in m_data.SeedClusterTypeProbability.Values)
            {
                m_seedClusterTypeProbabilitySum += value2;
            }
        }

        private void AddDensityFunctionFilled(MyInfiniteDensityFunction myInfiniteDensityFunction)
        {
            throw new NotImplementedException();
        }

        public static long GetAsteroidEntityId(string storageName)
        {
            return (storageName.GetHashCode64() & 0xFFFFFFFFFFFFFF) | 0x600000000000000;
        }

        protected override MyProceduralCell GenerateProceduralCell(ref Vector3I cellId)
        {
            MyProceduralCell myProceduralCell = new MyProceduralCell(cellId, CELL_SIZE);
            IMyModule cellDensityFunctionFilled = GetCellDensityFunctionFilled(myProceduralCell.BoundingVolume);
            if (cellDensityFunctionFilled == null)
            {
                return null;
            }
            IMyModule cellDensityFunctionRemoved = GetCellDensityFunctionRemoved(myProceduralCell.BoundingVolume);
            int cellSeed = GetCellSeed(ref cellId);
            MyRandom instance = MyRandom.Instance;
            using (instance.PushSeed(cellSeed))
            {
                int index = 0;
                Vector3I next = Vector3I.Zero;
                Vector3I end = new Vector3I(m_data.SubCells - 1);
                for (Vector3I_RangeIterator vector3I_RangeIterator = new Vector3I_RangeIterator(ref Vector3I.Zero, ref end); vector3I_RangeIterator.IsValid(); vector3I_RangeIterator.GetNext(out next))
                {
                    Vector3D value = new Vector3D(instance.NextDouble(), instance.NextDouble(), instance.NextDouble());
                    value += (Vector3D)next / (double)m_data.SubcellSize;
                    value += cellId;
                    value *= CELL_SIZE;
                    if (MyEntities.IsInsideWorld(value))
                    {
                        double num = -1.0;
                        if (cellDensityFunctionRemoved != null)
                        {
                            num = cellDensityFunctionRemoved.GetValue(value.X, value.Y, value.Z);
                            if (num <= -1.0)
                            {
                                continue;
                            }
                        }
                        double value2 = cellDensityFunctionFilled.GetValue(value.X, value.Y, value.Z);
                        if ((cellDensityFunctionRemoved == null || !(num < value2)) && value2 < m_objectDensity)
                        {
                            MyObjectSeed myObjectSeed = new MyObjectSeed(myProceduralCell, value, GetObjectSize(instance.NextDouble()));
                            myObjectSeed.Params.Type = GetSeedType(instance.NextDouble());
                            myObjectSeed.Params.Seed = instance.Next();
                            myObjectSeed.Params.Index = index++;
                            GenerateObject(myProceduralCell, myObjectSeed, ref index, instance, cellDensityFunctionFilled, cellDensityFunctionRemoved);
                        }
                    }
                }
                return myProceduralCell;
            }
        }

        public override void GenerateObjects(List<MyObjectSeed> objectsList, HashSet<MyObjectSeedParams> existingObjectsSeeds)
        {
            if (m_enabled)
            {
                foreach (MyObjectSeed objects in objectsList)
                {
                    if (!objects.Params.Generated && !existingObjectsSeeds.Contains(objects.Params))
                    {
                        objects.Params.Generated = true;
                        using (MyRandom.Instance.PushSeed(GetObjectIdSeed(objects)))
                        {
                            switch (objects.Params.Type)
                            {
                                case MyObjectSeedType.Empty:
                                    break;
                                case MyObjectSeedType.Asteroid:
                                    {
                                        BoundingBoxD box = objects.BoundingVolume;
                                        MyGamePruningStructure.GetAllVoxelMapsInBox(ref box, m_tmpVoxelMapsList);
                                        string text = $"Asteroid_{objects.CellId.X}_{objects.CellId.Y}_{objects.CellId.Z}_{objects.Params.Index}_{objects.Params.Seed}";
                                        bool flag = false;
                                        foreach (MyVoxelBase tmpVoxelMaps in m_tmpVoxelMapsList)
                                        {
                                            if (tmpVoxelMaps.StorageName == text)
                                            {
                                                existingObjectsSeeds.Add(objects.Params);
                                                flag = true;
                                                break;
                                            }
                                        }
                                        if (!flag)
                                        {
                                            MyStorageBase myStorageBase = new MyOctreeStorage(MyCompositeShapeProvider.CreateAsteroidShape(objects.Params.Seed, objects.Size, m_data.UseGeneratorSeed ? objects.Params.GeneratorSeed : 0), GetAsteroidVoxelSize(objects.Size));
                                            Vector3D vector3D = objects.BoundingVolume.Center - MathHelper.GetNearestBiggerPowerOfTwo(objects.Size) / 2;
                                            MyVoxelMap voxelMap;
                                            if (m_data.RotateAsteroids)
                                            {
                                                using (MyRandom.Instance.PushSeed(objects.Params.Seed))
                                                {
                                                    MatrixD worldMatrix = CreateAsteroidRotation(MyRandom.Instance, vector3D, myStorageBase.Size);
                                                    voxelMap = MyWorldGenerator.AddVoxelMap(text, myStorageBase, worldMatrix, GetAsteroidEntityId(text));
                                                }
                                            }
                                            else
                                            {
                                                voxelMap = MyWorldGenerator.AddVoxelMap(text, myStorageBase, vector3D, GetAsteroidEntityId(text));
                                            }
                                            if (voxelMap != null)
                                            {
                                                voxelMap.Save = false;
                                                MyVoxelBase.StorageChanged OnStorageRangeChanged = null;
                                                OnStorageRangeChanged = delegate
                                                {
                                                    voxelMap.Save = true;
                                                    voxelMap.RangeChanged -= OnStorageRangeChanged;
                                                };
                                                voxelMap.RangeChanged += OnStorageRangeChanged;
                                                MyObjectSeedParams voxelParams = objects.Params;
                                                if (Sync.IsServer)
                                                {
                                                    voxelMap.OnMarkForClose += delegate
                                                    {
                                                        if (!m_isClosingEntities)
                                                        {
                                                            MyMultiplayer.RaiseStaticEvent((IMyEventOwner x) => MyProceduralWorldGenerator.AddExistingObjectsSeed, voxelParams);
                                                        }
                                                    };
                                                }
                                            }
                                        }
                                        m_tmpVoxelMapsList.Clear();
                                        break;
                                    }
                                case MyObjectSeedType.EncounterAlone:
                                case MyObjectSeedType.EncounterSingle:
                                    break;
                                default:
                                    throw new InvalidBranchException();
                            }
                        }
                    }
                }
            }
        }

        private void GenerateObject(MyProceduralCell cell, MyObjectSeed objectSeed, ref int index, MyRandom random, IMyModule densityFunctionFilled, IMyModule densityFunctionRemoved)
        {
            if (m_data.UseGeneratorSeed && objectSeed.Params.GeneratorSeed == 0)
            {
                objectSeed.Params.GeneratorSeed = random.Next();
            }
            if (m_data.UseClusterDefAsAsteroid)
            {
                cell.AddObject(objectSeed);
            }
            switch (objectSeed.Params.Type)
            {
                case MyObjectSeedType.Empty:
                    break;
                case MyObjectSeedType.Asteroid:
                case MyObjectSeedType.EncounterAlone:
                case MyObjectSeedType.EncounterSingle:
                    if (!m_data.UseClusterDefAsAsteroid)
                    {
                        cell.AddObject(objectSeed);
                    }
                    break;
                case MyObjectSeedType.AsteroidCluster:
                    if (m_data.UseClusterDefAsAsteroid)
                    {
                        objectSeed.Params.Type = MyObjectSeedType.Asteroid;
                    }
                    using (MyUtils.ReuseCollection(ref m_tmpClusterBoxes))
                    {
                        int generatorSeed = m_data.UseGeneratorSeed ? random.Next() : 0;
                        double value = m_data.ObjectMaxDistanceInClusterMin;
                        if (m_data.UseClusterVariableSize)
                        {
                            value = MathHelper.Lerp(m_data.ObjectMaxDistanceInClusterMin, m_data.ObjectMaxDistanceInClusterMax, random.NextDouble());
                        }
                        for (int i = 0; i < m_data.ObjectMaxInCluster; i++)
                        {
                            Vector3D randomDirection = MyProceduralWorldGenerator.GetRandomDirection(random);
                            double clusterObjectSize = GetClusterObjectSize(random.NextDouble());
                            double num = MathHelper.Lerp(m_data.ObjectMinDistanceInCluster, value, random.NextDouble());
                            double scaleFactor = m_data.ClusterDispersionAbsolute ? num : (clusterObjectSize + objectSeed.BoundingVolume.HalfExtents.Length() * 2.0 + num);
                            Vector3D vector3D = objectSeed.BoundingVolume.Center + randomDirection * scaleFactor;
                            double num2 = -1.0;
                            if (densityFunctionRemoved != null)
                            {
                                num2 = densityFunctionRemoved.GetValue(vector3D.X, vector3D.Y, vector3D.Z);
                                if (num2 <= -1.0)
                                {
                                    continue;
                                }
                            }
                            double value2 = densityFunctionFilled.GetValue(vector3D.X, vector3D.Y, vector3D.Z);
                            if ((densityFunctionRemoved == null || !(num2 < value2)) && value2 < m_data.ObjectDensityCluster)
                            {
                                MyObjectSeed myObjectSeed = new MyObjectSeed(cell, vector3D, clusterObjectSize);
                                myObjectSeed.Params.Seed = random.Next();
                                myObjectSeed.Params.Index = index++;
                                myObjectSeed.Params.Type = GetClusterSeedType(random.NextDouble());
                                myObjectSeed.Params.GeneratorSeed = generatorSeed;
                                BoundingBoxD hitBox = myObjectSeed.BoundingVolume;
                                if (m_data.AllowPartialClusterObjectOverlap)
                                {
                                    Vector3D center = hitBox.Center;
                                    Vector3D halfExtents = hitBox.HalfExtents;
                                    hitBox = new BoundingBoxD(center - halfExtents * 0.30000001192092896, center + halfExtents * 0.30000001192092896);
                                }
                                if (m_tmpClusterBoxes.All((BoundingBoxD box) => !hitBox.Intersects(box)))
                                {
                                    m_tmpClusterBoxes.Add(hitBox);
                                    GenerateObject(cell, myObjectSeed, ref index, random, densityFunctionFilled, densityFunctionRemoved);
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new InvalidBranchException();
            }
        }

        private MyObjectSeedType GetSeedType(double d)
        {
            d *= m_seedTypeProbabilitySum;
            foreach (KeyValuePair<MyObjectSeedType, double> item in m_data.SeedTypeProbability)
            {
                if (item.Value >= d)
                {
                    return item.Key;
                }
                d -= item.Value;
            }
            return MyObjectSeedType.Asteroid;
        }

        private MyObjectSeedType GetClusterSeedType(double d)
        {
            d *= m_seedClusterTypeProbabilitySum;
            foreach (KeyValuePair<MyObjectSeedType, double> item in m_data.SeedClusterTypeProbability)
            {
                if (item.Value >= d)
                {
                    return item.Key;
                }
                d -= item.Value;
            }
            return MyObjectSeedType.Asteroid;
        }

        private double GetObjectSize(double noise)
        {
            if (m_data.UseLinearPowOfTwoSizeDistribution)
            {
                double value = Math.Log(MathHelper.GetNearestBiggerPowerOfTwo(m_data.ObjectSizeMin), 2.0);
                double value2 = Math.Log(MathHelper.GetNearestBiggerPowerOfTwo(m_data.ObjectSizeMax), 2.0);
                int num = (int)Math.Round(MathHelper.Lerp(value, value2, noise));
                return 1 << num;
            }
            return (double)m_data.ObjectSizeMin + noise * noise * (double)(m_data.ObjectSizeMax - m_data.ObjectSizeMin);
        }

        private double GetClusterObjectSize(double noise)
        {
            if (m_data.UseLinearPowOfTwoSizeDistribution)
            {
                double value = Math.Log(MathHelper.GetNearestBiggerPowerOfTwo(m_data.ObjectSizeMinCluster), 2.0);
                double value2 = Math.Log(MathHelper.GetNearestBiggerPowerOfTwo(m_data.ObjectSizeMaxCluster), 2.0);
                int num = (int)Math.Round(MathHelper.Lerp(value, value2, noise));
                return 1 << num;
            }
            return (double)m_data.ObjectSizeMinCluster + noise * (double)(m_data.ObjectSizeMaxCluster - m_data.ObjectSizeMinCluster);
        }

        private static Vector3I GetAsteroidVoxelSize(double asteroidRadius)
        {
            return new Vector3I(Math.Max(64, (int)Math.Ceiling(asteroidRadius)));
        }

        protected override void CloseObjectSeed(MyObjectSeed objectSeed)
        {
            switch (objectSeed.Params.Type)
            {
                case MyObjectSeedType.Empty:
                    break;
                case MyObjectSeedType.Asteroid:
                case MyObjectSeedType.AsteroidCluster:
                    {
                        BoundingBoxD box = objectSeed.BoundingVolume;
                        MyGamePruningStructure.GetAllVoxelMapsInBox(ref box, m_tmpVoxelMapsList);
                        string b = $"Asteroid_{objectSeed.CellId.X}_{objectSeed.CellId.Y}_{objectSeed.CellId.Z}_{objectSeed.Params.Index}_{objectSeed.Params.Seed}";
                        foreach (MyVoxelBase tmpVoxelMaps in m_tmpVoxelMapsList)
                        {
                            if (tmpVoxelMaps.StorageName == b)
                            {
                                if (!tmpVoxelMaps.Save)
                                {
                                    m_isClosingEntities = true;
                                    tmpVoxelMaps.Close();
                                    m_isClosingEntities = false;
                                }
                                break;
                            }
                        }
                        m_tmpVoxelMapsList.Clear();
                        break;
                    }
                case MyObjectSeedType.EncounterAlone:
                case MyObjectSeedType.EncounterSingle:
                    break;
                default:
                    throw new InvalidBranchException();
            }
        }

        private static double GetSubCellInfo()
        {
            MyAsteroidGeneratorDefinition data = GetData();
            return data.SubCells * data.SubcellSize;
        }

        private static MatrixD CreateAsteroidRotation(MyRandom random, Vector3D offset, Vector3I storageSize)
        {
            MatrixD matrix = MatrixD.CreateTranslation(offset + storageSize / 2);
            Matrix matrix2 = Matrix.CreateRotationZ((float)((double)random.NextFloat() * Math.PI * 2.0)) * Matrix.CreateRotationX((float)((double)random.NextFloat() * Math.PI * 2.0)) * Matrix.CreateRotationY((float)((double)random.NextFloat() * Math.PI * 2.0));
            return MatrixD.CreateTranslation(new Vector3(storageSize / 2)) * matrix2 * matrix;
        }

        private static MyAsteroidGeneratorDefinition GetData()
        {
            MyAsteroidGeneratorDefinition myAsteroidGeneratorDefinition = null;
            int voxelGeneratorVersion = MySession.Static.Settings.VoxelGeneratorVersion;
            foreach (MyAsteroidGeneratorDefinition value in MyDefinitionManager.Static.GetAsteroidGeneratorDefinitions().Values)
            {
                if (value.Version == voxelGeneratorVersion)
                {
                    myAsteroidGeneratorDefinition = value;
                    break;
                }
            }
            if (myAsteroidGeneratorDefinition == null)
            {
                MyLog.Default.WriteLine("Generator of version " + voxelGeneratorVersion + "not found!");
                {
                    foreach (MyAsteroidGeneratorDefinition value2 in MyDefinitionManager.Static.GetAsteroidGeneratorDefinitions().Values)
                    {
                        if (myAsteroidGeneratorDefinition == null || (value2.Version > voxelGeneratorVersion && (myAsteroidGeneratorDefinition.Version < voxelGeneratorVersion || value2.Version < myAsteroidGeneratorDefinition.Version)))
                        {
                            myAsteroidGeneratorDefinition = value2;
                        }
                    }
                    return myAsteroidGeneratorDefinition;
                }
            }
            return myAsteroidGeneratorDefinition;
        }
    }
}
