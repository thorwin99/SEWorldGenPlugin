using Sandbox.Definitions;
using Sandbox.Engine.Voxels;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VRage;
using VRage.Collections;
using VRage.FileSystem;
using VRage.Game;
using VRage.Library.Utils;
using VRage.Noise;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.Composites
{
    internal struct MySEWGCompositeShapes
    {
        private List<MyVoxelMaterialDefinition> m_coreMaterials;

        private List<MyVoxelMaterialDefinition> m_surfaceMaterials;

        private List<MyVoxelMaterialDefinition> m_depositMaterials;

        public static readonly MyCompositeShapeGeneratorDelegate[] AsteroidGenerators;

        private List<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>> m_primarySelections;

        private List<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>> m_secondarySelections;

        static MySEWGCompositeShapes()
        {
            int[] source = new int[3]
            {
                0,
                1,
                2
            };
            int[] source2 = new int[2]
            {
                3,
                4
            };
            AsteroidGenerators = (from x in source
                select MyTuple.Create(x, arg2: false)).Concat(from x in source2
            select MyTuple.Create(x, arg2: true)).Select((Func<MyTuple<int, bool>, MyCompositeShapeGeneratorDelegate>)delegate (MyTuple<int, bool> info)
            {
                int version = info.Item1;
                bool combined = info.Item2;
                return delegate (int generatorSeed, int seed, float size)
                {
                    if (size == 0f)
                    {
                        size = MyUtils.GetRandomFloat(128f, 512f);
                    }
                    MySEWGCompositeShapes myCompositeShapes = new MySEWGCompositeShapes(generatorSeed, seed, version);
                    using (MyRandom.Instance.PushSeed(seed))
                    {
                        if (combined)
                        {
                            return myCompositeShapes.CombinedGenerator(version, seed, size);
                        }
                        return myCompositeShapes.ProceduralGenerator(version, seed, size);
                    }
                };
            }).ToArray();
        }

        private MySEWGCompositeShapes(int generatorSeed, int asteroidSeed, int version)
        {
            this = default(MySEWGCompositeShapes);
            if (version > 2)
            {
                m_coreMaterials = new List<MyVoxelMaterialDefinition>();
                m_depositMaterials = new List<MyVoxelMaterialDefinition>();
                m_surfaceMaterials = new List<MyVoxelMaterialDefinition>();
                using (MyRandom.Instance.PushSeed(generatorSeed))
                {
                    MyRandom instance = MyRandom.Instance;
                    FillMaterials(version);
                    FilterKindDuplicates(m_coreMaterials, instance);
                    FilterKindDuplicates(m_depositMaterials, instance);
                    FilterKindDuplicates(m_surfaceMaterials, instance);
                    ProcessMaterialSpawnProbabilities(m_coreMaterials);
                    ProcessMaterialSpawnProbabilities(m_depositMaterials);
                    ProcessMaterialSpawnProbabilities(m_surfaceMaterials);
                    if (instance.Next(100) < 1)
                    {
                        MakeIceAsteroid(version, instance);
                    }
                    else if (version >= 4)
                    {
                        int maxCount = (instance.NextDouble() > 0.800000011920929) ? 4 : 2;
                        int maxCount2 = (!(instance.NextDouble() > 0.40000000596046448)) ? 1 : 2;
                        LimitMaterials(m_coreMaterials, maxCount, instance);
                        LimitMaterials(m_depositMaterials, maxCount, instance);
                        using (MyRandom.Instance.PushSeed(asteroidSeed))
                        {
                            LimitMaterials(m_coreMaterials, maxCount2, instance);
                            LimitMaterials(m_depositMaterials, maxCount2, instance);
                        }
                    }
                }
            }
        }

        private IMySEWGCompositionInfoProvider CombinedGenerator(int version, int seed, float size)
        {
            int num = 0;
            MyRandom instance = MyRandom.Instance;
            MySEWGCompositeShapeProvider.MyProceduralCompositeInfoProvider.ConstructionData data = default(MySEWGCompositeShapeProvider.MyProceduralCompositeInfoProvider.ConstructionData);
            data.DefaultMaterial = null;
            data.Deposits = Array.Empty<MySEWGCompositeShapeOreDeposit>();
            data.FilledShapes = new MySEWGCsgShapeBase[num];
            IMyCompositeShape[] array = new IMyCompositeShape[6];
            FillSpan(instance, size, new Span<IMyCompositeShape>(array, 0, 1), MyDefinitionManager.Static.GetVoxelMapStorageDefinitionsForProceduralPrimaryAdditions(), prefferOnlyBestFittingSize: true);
            size = ((MyOctreeStorage)array[0]).Size.AbsMax();
            float idealSize = size / 2f;
            float idealSize2 = size / 2f;
            int num2 = 5 / ((size > 200f) ? 1 : 2);
            int num3 = 1;
            if (size <= 64f)
            {
                num2 = 0;
                num3 = 0;
            }
            IMyCompositeShape[] array2 = new IMyCompositeShape[num2];
            data.RemovedShapes = new MySEWGCsgShapeBase[num3];
            FillSpan(instance, idealSize2, array, MyDefinitionManager.Static.GetVoxelMapStorageDefinitionsForProceduralAdditions());
            FillSpan(instance, idealSize, array2, MyDefinitionManager.Static.GetVoxelMapStorageDefinitionsForProceduralRemovals());
            TranslateShapes(array2, size, instance);
            TranslateShapes(new Span<IMyCompositeShape>(array, 1, array.Length - 1), size, instance);
            if (size > 512f)
            {
                size /= 2f;
            }
            float num4 = size * 0.5f;
            float storageOffset = (float)MathHelper.GetNearestBiggerPowerOfTwo(size) * 0.5f - num4;
            GetProceduralModules(seed, size, instance, out data.MacroModule, out data.DetailModule);
            GenerateProceduralAdditions(version, size, data.FilledShapes, instance, storageOffset);
            GenerateProceduralRemovals(version, size, data.RemovedShapes, instance, storageOffset);
            MySEWGCompositeShapeProvider.MyCombinedCompositeInfoProvider myCombinedCompositeInfoProvider = new MySEWGCompositeShapeProvider.MyCombinedCompositeInfoProvider(ref data, array, array2);
            GenerateMaterials(version, size, instance, data.FilledShapes, storageOffset, out MyVoxelMaterialDefinition defaultMaterial, out MySEWGCompositeShapeOreDeposit[] deposits, myCombinedCompositeInfoProvider);
            myCombinedCompositeInfoProvider.UpdateMaterials(defaultMaterial, deposits);
            return myCombinedCompositeInfoProvider;
        }

        private void TranslateShapes(Span<IMyCompositeShape> array, float size, MyRandom random)
        {
            for (int i = 0; i < array.Length; i++)
            {
                int num = 0;
                MyStorageBase myStorageBase = array[i] as MyStorageBase;
                if (myStorageBase != null)
                {
                    num = myStorageBase.Size.AbsMax();
                }
                array[i] = new MyCompositeTranslateShape(array[i], CreateRandomPointInBox(random, size - (float)num));
            }
        }

        private void FillSpan(MyRandom random, float idealSize, Span<IMyCompositeShape> shapes, ListReader<MyVoxelMapStorageDefinition> voxelMaps, bool prefferOnlyBestFittingSize = false)
        {
            bool flag = false;
            for (int i = 0; i < shapes.Length; i++)
            {
                if (shapes[i] == null)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                using (MyUtils.ReuseCollection(ref m_primarySelections))
                {
                    using (MyUtils.ReuseCollection(ref m_secondarySelections))
                    {
                        m_primarySelections.EnsureCapacity(voxelMaps.Count);
                        m_secondarySelections.EnsureCapacity(voxelMaps.Count);
                        int num = int.MinValue;
                        int num2 = int.MaxValue;
                        foreach (MyVoxelMapStorageDefinition item in voxelMaps)
                        {
                            MyOctreeStorage myOctreeStorage = CreateAsteroidStorage(item);
                            int num3 = myOctreeStorage.Size.AbsMax();
                            if ((float)num3 > idealSize)
                            {
                                if (num3 <= num2)
                                {
                                    if (num3 < num2)
                                    {
                                        num2 = num3;
                                        m_secondarySelections.Clear();
                                    }
                                    m_secondarySelections.Add(MyTuple.Create(item, myOctreeStorage));
                                }
                            }
                            else
                            {
                                if (prefferOnlyBestFittingSize)
                                {
                                    if (num3 < num)
                                    {
                                        continue;
                                    }
                                    if (num3 > num)
                                    {
                                        num = num3;
                                        m_primarySelections.Clear();
                                    }
                                }
                                m_primarySelections.Add(MyTuple.Create(item, myOctreeStorage));
                            }
                        }
                        List<MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage>> list = (m_primarySelections.Count > 0) ? m_primarySelections : m_secondarySelections;
                        float num4 = list.Sum((MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage> x) => x.Item1.SpawnProbability);
                        for (int j = 0; j < shapes.Length; j++)
                        {
                            if (shapes[j] == null)
                            {
                                float num5 = num4 * random.NextFloat();
                                foreach (MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage> item2 in list)
                                {
                                    float spawnProbability = item2.Item1.SpawnProbability;
                                    if (!(num5 < spawnProbability))
                                    {
                                        num5 -= spawnProbability;
                                        continue;
                                    }
                                    shapes[j] = item2.Item2;
                                    goto IL_0221;
                                }
                                shapes[j] = list.MaxBy((MyTuple<MyVoxelMapStorageDefinition, MyOctreeStorage> x) => x.Item1.SpawnProbability).Item2;
                            }
                        IL_0221:;
                        }
                    }
                }
            }
        }

        public static MyOctreeStorage CreateAsteroidStorage(MyVoxelMapStorageDefinition definition)
        {
            return (MyOctreeStorage)MyStorageBase.LoadFromFile(Path.Combine(definition.Context.IsBaseGame ? MyFileSystem.ContentPath : definition.Context.ModPath, definition.StorageFile));
        }

        private IMySEWGCompositionInfoProvider ProceduralGenerator(int version, int seed, float size)
        {
            MyRandom instance = MyRandom.Instance;
            MySEWGCompositeShapeProvider.MyProceduralCompositeInfoProvider.ConstructionData data = default(MySEWGCompositeShapeProvider.MyProceduralCompositeInfoProvider.ConstructionData);
            data.FilledShapes = new MySEWGCsgShapeBase[2];
            data.RemovedShapes = new MySEWGCsgShapeBase[2];
            GetProceduralModules(seed, size, instance, out data.MacroModule, out data.DetailModule);
            float num = size * 0.5f;
            float num2 = (float)MathHelper.GetNearestBiggerPowerOfTwo(size) * 0.5f;
            float storageOffset = num2 - num;
            MySEWGCsgShapeBase myCsgShapeBase;
            switch (instance.Next() % 3)
            {
                case 0:
                    {
                        float num3 = (instance.NextFloat() * 0.05f + 0.1f) * size;
                        myCsgShapeBase = new MySEWGCsgTorus(new Vector3(num2), CreateRandomRotation(instance), (instance.NextFloat() * 0.1f + 0.2f) * size, num3, (instance.NextFloat() * 0.4f + 0.4f) * num3, instance.NextFloat() * 0.8f + 0.2f, instance.NextFloat() * 0.6f + 0.4f);
                        break;
                    }
                default:
                    myCsgShapeBase = new MySEWGCsgSphere(new Vector3(num2), (instance.NextFloat() * 0.1f + 0.35f) * size * ((version > 2) ? 0.8f : 1f), (instance.NextFloat() * 0.05f + 0.05f) * size + 1f, instance.NextFloat() * 0.8f + 0.2f, instance.NextFloat() * 0.6f + 0.4f);
                    break;
            }
            data.FilledShapes[0] = myCsgShapeBase;
            GenerateProceduralAdditions(version, size, data.FilledShapes, instance, storageOffset);
            GenerateProceduralRemovals(version, size, data.RemovedShapes, instance, storageOffset);
            GenerateMaterials(version, size, instance, data.FilledShapes, storageOffset, out data.DefaultMaterial, out data.Deposits);
            return new MySEWGCompositeShapeProvider.MyProceduralCompositeInfoProvider(ref data);
        }

        private static void GetProceduralModules(int seed, float size, MyRandom random, out IMyModule macroModule, out IMyModule detailModule)
        {
            macroModule = new MySimplexFast(seed, 7f / size);
            switch (random.Next() & 1)
            {
                case 0:
                    detailModule = new MyRidgedMultifractalFast(MyNoiseQuality.Low, 1, seed, random.NextFloat() * 0.09f + 0.11f);
                    break;
                default:
                    detailModule = new MyBillowFast(MyNoiseQuality.Low, 1, seed, random.NextFloat() * 0.07f + 0.13f);
                    break;
            }
        }

        private static void GenerateProceduralAdditions(int version, float size, MySEWGCsgShapeBase[] filledShapes, MyRandom random, float storageOffset)
        {
            bool flag = version > 2;
            for (int i = 0; i < filledShapes.Length; i++)
            {
                if (filledShapes[i] != null)
                {
                    continue;
                }
                float num = size * (random.NextFloat() * 0.2f + 0.1f) + 2f;
                float num2 = 2f * num;
                float boxSize = size - num2;
                switch (random.Next() % (flag ? 2 : 3))
                {
                    case 0:
                        {
                            Vector3 value2 = CreateRandomPointOnBox(random, boxSize, version) + num;
                            float num6 = num * (random.NextFloat() * 0.4f + 0.35f) * (flag ? 0.8f : 1f);
                            MySEWGCsgSphere myCsgSphere = (MySEWGCsgSphere)(filledShapes[i] = new MySEWGCsgSphere(value2 + storageOffset, num6, num6 * (random.NextFloat() * 0.1f + 0.1f), random.NextFloat() * 0.8f + 0.2f, random.NextFloat() * 0.6f + 0.4f));
                            break;
                        }
                    case 1:
                        {
                            Vector3 vector2 = CreateRandomPointOnBox(random, boxSize, version) + num;
                            Vector3 value = new Vector3(size) - vector2;
                            if (random.Next() % 2 == 0)
                            {
                                MyUtils.Swap(ref vector2.X, ref value.X);
                            }
                            if (random.Next() % 2 == 0)
                            {
                                MyUtils.Swap(ref vector2.Y, ref value.Y);
                            }
                            if (random.Next() % 2 == 0)
                            {
                                MyUtils.Swap(ref vector2.Z, ref value.Z);
                            }
                            float num5 = (random.NextFloat() * 0.25f + 0.5f) * num * (flag ? 0.5f : 1f);
                            MySEWGCsgCapsule myCsgCapsule = (MySEWGCsgCapsule)(filledShapes[i] = new MySEWGCsgCapsule(vector2 + storageOffset, value + storageOffset, num5, (random.NextFloat() * 0.25f + 0.5f) * (flag ? 1f : num5), random.NextFloat() * 0.4f + 0.4f, random.NextFloat() * 0.6f + 0.4f));
                            break;
                        }
                    case 2:
                        {
                            Vector3 vector = CreateRandomPointInBox(random, boxSize) + num;
                            Quaternion invRotation = CreateRandomRotation(random);
                            float num3 = ComputeBoxSideDistance(vector, size);
                            float num4 = (random.NextFloat() * 0.15f + 0.1f) * num3;
                            MySEWGCsgTorus myCsgTorus = (MySEWGCsgTorus)(filledShapes[i] = new MySEWGCsgTorus(vector + storageOffset, invRotation, (random.NextFloat() * 0.2f + 0.5f) * num3, num4, (random.NextFloat() * 0.25f + 0.2f) * num4, random.NextFloat() * 0.8f + 0.2f, random.NextFloat() * 0.6f + 0.4f));
                            break;
                        }
                }
            }
        }

        private static void GenerateProceduralRemovals(int version, float size, MySEWGCsgShapeBase[] removedShapes, MyRandom random, float storageOffset)
        {
            bool flag = version > 2;
            for (int i = 0; i < removedShapes.Length; i++)
            {
                if (removedShapes[i] != null)
                {
                    continue;
                }
                float num = size * (random.NextFloat() * 0.2f + 0.1f) + 2f;
                float num2 = 2f * num;
                float boxSize = size - num2;
                switch (random.Next() % 7)
                {
                    case 0:
                        {
                            Vector3 vector = CreateRandomPointInBox(random, boxSize) + num;
                            float num3 = ComputeBoxSideDistance(vector, size);
                            float num4 = (random.NextFloat() * (flag ? 0.3f : 0.4f) + (flag ? 0.1f : 0.3f)) * num3;
                            MySEWGCsgSphere myCsgSphere = (MySEWGCsgSphere)(removedShapes[i] = new MySEWGCsgSphere(vector + storageOffset, num4, (random.NextFloat() * (flag ? 0.2f : 0.3f) + (flag ? 0.45f : 0.35f)) * num4, random.NextFloat() * (flag ? 0.2f : 0.8f) + (flag ? 1f : 0.2f), random.NextFloat() * (flag ? 0.1f : 0.6f) + 0.4f));
                            continue;
                        }
                    case 1:
                    case 2:
                    case 3:
                        {
                            Vector3 vector2 = CreateRandomPointInBox(random, boxSize) + num;
                            Quaternion invRotation = CreateRandomRotation(random);
                            float num5 = ComputeBoxSideDistance(vector2, size);
                            float num6 = (random.NextFloat() * (flag ? 0.1f : 0.15f) + (flag ? 0.2f : 0.1f)) * num5;
                            MySEWGCsgTorus myCsgTorus = (MySEWGCsgTorus)(removedShapes[i] = new MySEWGCsgTorus(vector2 + storageOffset, invRotation, (random.NextFloat() * 0.2f + (flag ? 0.3f : 0.5f)) * num5, num6, (random.NextFloat() * (flag ? 0.2f : 0.25f) + (flag ? 1f : 0.2f)) * num6, random.NextFloat() * (flag ? 0.2f : 0.8f) + (flag ? 1f : 0.2f), random.NextFloat() * (flag ? 0.2f : 0.6f) + 0.4f));
                            continue;
                        }
                }
                Vector3 vector3 = CreateRandomPointOnBox(random, boxSize, version) + num;
                Vector3 value = new Vector3(size) - vector3;
                if (random.Next() % 2 == 0)
                {
                    MyUtils.Swap(ref vector3.X, ref value.X);
                }
                if (random.Next() % 2 == 0)
                {
                    MyUtils.Swap(ref vector3.Y, ref value.Y);
                }
                if (random.Next() % 2 == 0)
                {
                    MyUtils.Swap(ref vector3.Z, ref value.Z);
                }
                float num7 = (random.NextFloat() * (flag ? 0.3f : 0.25f) + (flag ? 0.1f : 0.5f)) * num;
                MySEWGCsgCapsule myCsgCapsule = (MySEWGCsgCapsule)(removedShapes[i] = new MySEWGCsgCapsule(vector3 + storageOffset, value + storageOffset, num7, (random.NextFloat() * (flag ? 0.5f : 0.25f) + (flag ? 1f : 0.5f)) * (flag ? 1f : num7), random.NextFloat() * (flag ? 0.5f : 0.4f) + (flag ? 1f : 0.4f), random.NextFloat() * (flag ? 0.2f : 0.6f) + 0.4f));
            }
        }

        private void GenerateMaterials(int version, float size, MyRandom random, MySEWGCsgShapeBase[] filledShapes, float storageOffset, out MyVoxelMaterialDefinition defaultMaterial, out MySEWGCompositeShapeOreDeposit[] deposits, MySEWGCompositeShapeProvider.MyCombinedCompositeInfoProvider shapeInfo = null)
        {
            bool flag = version > 2;
            if (m_coreMaterials == null)
            {
                m_coreMaterials = new List<MyVoxelMaterialDefinition>();
                m_depositMaterials = new List<MyVoxelMaterialDefinition>();
                m_surfaceMaterials = new List<MyVoxelMaterialDefinition>();
                FillMaterials(version);
            }
            Action<List<MyVoxelMaterialDefinition>> action = delegate (List<MyVoxelMaterialDefinition> list)
            {
                int num10 = list.Count;
                while (num10 > 1)
                {
                    int index = random.Next() % num10;
                    num10--;
                    MyVoxelMaterialDefinition value = list[index];
                    list[index] = list[num10];
                    list[num10] = value;
                }
            };
            action(m_depositMaterials);
            if (m_surfaceMaterials.Count == 0)
            {
                if (m_depositMaterials.Count == 0)
                {
                    defaultMaterial = m_coreMaterials[random.Next() % m_coreMaterials.Count];
                }
                else
                {
                    defaultMaterial = m_depositMaterials[random.Next() % m_depositMaterials.Count];
                }
            }
            else
            {
                defaultMaterial = m_surfaceMaterials[random.Next() % m_surfaceMaterials.Count];
            }
            int val;
            if (flag)
            {
                int num = 0;
                num = ((size <= 64f) ? 1 : ((size <= 128f) ? 2 : ((size <= 256f) ? 3 : ((!(size <= 512f)) ? 5 : 4))));
                val = (int)(MySession.Static.Settings.DepositsCountCoefficient * (float)num);
                if (m_depositMaterials.Count == 0)
                {
                    val = 0;
                }
            }
            else
            {
                val = (int)Math.Log(size);
            }
            val = Math.Max(val, filledShapes.Length);
            deposits = new MySEWGCompositeShapeOreDeposit[val];
            float depositSizeDenominator = MySession.Static.Settings.DepositSizeDenominator;
            float num2 = (!flag || !(depositSizeDenominator > 0f)) ? (size / 10f) : (size / depositSizeDenominator + 8f);
            MyVoxelMaterialDefinition material = defaultMaterial;
            int num3 = 0;
            for (int i = 0; i < filledShapes.Length; i++)
            {
                if (i == 0)
                {
                    if (m_coreMaterials.Count == 0)
                    {
                        if (m_depositMaterials.Count == 0)
                        {
                            if (m_surfaceMaterials.Count != 0)
                            {
                                material = m_surfaceMaterials[random.Next() % m_surfaceMaterials.Count];
                            }
                        }
                        else
                        {
                            material = m_depositMaterials[num3++];
                        }
                    }
                    else
                    {
                        material = m_coreMaterials[random.Next() % m_coreMaterials.Count];
                    }
                }
                else if (m_depositMaterials.Count == 0)
                {
                    if (m_surfaceMaterials.Count != 0)
                    {
                        material = m_surfaceMaterials[random.Next() % m_surfaceMaterials.Count];
                    }
                }
                else
                {
                    material = m_depositMaterials[num3++];
                }
                deposits[i] = new MySEWGCompositeShapeOreDeposit(filledShapes[i].DeepCopy(), material);
                deposits[i].Shape.ShrinkTo(random.NextFloat() * (flag ? 0.6f : 0.15f) + (flag ? 0.1f : 0.6f));
                if (num3 == m_depositMaterials.Count)
                {
                    num3 = 0;
                    action(m_depositMaterials);
                }
            }
            for (int j = filledShapes.Length; j < val; j++)
            {
                float num6 = 0f;
                Vector3 vector = Vector3.Zero;
                for (int k = 0; k < 10; k++)
                {
                    vector = CreateRandomPointInBox(random, size * (flag ? 0.6f : 0.7f)) + storageOffset + size * 0.15f;
                    num6 = random.NextFloat() * num2 + (flag ? 5f : 8f);
                    if (shapeInfo == null)
                    {
                        break;
                    }
                    double num7 = Math.Sqrt(num6 * num6 / 2f) * 0.5;
                    Vector3I b = new Vector3I((int)num7);
                    BoundingBoxI box = new BoundingBoxI((Vector3I)vector - b, (Vector3I)vector + b);
                    if (MySEWGCompositeShapeProvider.Intersect(shapeInfo, box, 0) != 0)
                    {
                        break;
                    }
                }
                random.NextFloat();
                random.NextFloat();
                MySEWGCsgShapeBase shape = new MySEWGCsgSphere(vector, num6);
                material = ((m_depositMaterials.Count != 0) ? m_depositMaterials[num3++] : m_surfaceMaterials[num3++]);
                deposits[j] = new MySEWGCompositeShapeOreDeposit(shape, material);
                if (m_depositMaterials.Count == 0)
                {
                    if (num3 == m_surfaceMaterials.Count)
                    {
                        num3 = 0;
                        action(m_surfaceMaterials);
                    }
                }
                else if (num3 == m_depositMaterials.Count)
                {
                    num3 = 0;
                    action(m_depositMaterials);
                }
            }
        }

        private void FillMaterials(int version)
        {
            foreach (MyVoxelMaterialDefinition voxelMaterialDefinition in MyDefinitionManager.Static.GetVoxelMaterialDefinitions())
            {
                if (IsAcceptedAsteroidMaterial(voxelMaterialDefinition, version))
                {
                    if (version > 2)
                    {
                        if (voxelMaterialDefinition.MinedOre == "Stone")
                        {
                            m_surfaceMaterials.Add(voxelMaterialDefinition);
                        }
                        else
                        {
                            m_depositMaterials.Add(voxelMaterialDefinition);
                        }
                    }
                    else if (voxelMaterialDefinition.MinedOre == "Stone")
                    {
                        m_surfaceMaterials.Add(voxelMaterialDefinition);
                    }
                    else if (voxelMaterialDefinition.MinedOre == "Iron")
                    {
                        m_coreMaterials.Add(voxelMaterialDefinition);
                    }
                    else if (voxelMaterialDefinition.MinedOre == "Uranium")
                    {
                        m_depositMaterials.Add(voxelMaterialDefinition);
                        m_depositMaterials.Add(voxelMaterialDefinition);
                    }
                    else if (voxelMaterialDefinition.MinedOre == "Ice")
                    {
                        m_depositMaterials.Add(voxelMaterialDefinition);
                        m_depositMaterials.Add(voxelMaterialDefinition);
                    }
                    else
                    {
                        m_depositMaterials.Add(voxelMaterialDefinition);
                    }
                }
            }
            if (m_surfaceMaterials.Count == 0 && m_depositMaterials.Count == 0)
            {
                throw new Exception("There are no voxel materials allowed to spawn in asteroids!");
            }
        }

        private static Vector3 CreateRandomPointInBox(MyRandom random, float boxSize)
        {
            return new Vector3(random.NextFloat() * boxSize, random.NextFloat() * boxSize, random.NextFloat() * boxSize);
        }

        private static Vector3 CreateRandomPointOnBox(MyRandom random, float boxSize, int version)
        {
            Vector3 value = Vector3.Zero;
            if (version <= 2)
            {
                switch (random.Next() & 6)
                {
                    case 0:
                        return new Vector3(0f, random.NextFloat(), random.NextFloat());
                    case 1:
                        return new Vector3(1f, random.NextFloat(), random.NextFloat());
                    case 2:
                        return new Vector3(random.NextFloat(), 0f, random.NextFloat());
                    case 3:
                        return new Vector3(random.NextFloat(), 1f, random.NextFloat());
                    case 4:
                        return new Vector3(random.NextFloat(), random.NextFloat(), 0f);
                    case 5:
                        return new Vector3(random.NextFloat(), random.NextFloat(), 1f);
                }
            }
            else
            {
                float num = random.NextFloat();
                float num2 = random.NextFloat();
                switch (random.Next() % 6)
                {
                    case 0:
                        value = new Vector3(0f, num, num2);
                        break;
                    case 1:
                        value = new Vector3(1f, num, num2);
                        break;
                    case 2:
                        value = new Vector3(num, 0f, num2);
                        break;
                    case 3:
                        value = new Vector3(num, 1f, num2);
                        break;
                    case 4:
                        value = new Vector3(num, num2, 0f);
                        break;
                    case 5:
                        value = new Vector3(num, num2, 1f);
                        break;
                }
            }
            return value * boxSize;
        }

        private static Quaternion CreateRandomRotation(MyRandom self)
        {
            Quaternion result = new Quaternion(self.NextFloat() * 2f - 1f, self.NextFloat() * 2f - 1f, self.NextFloat() * 2f - 1f, self.NextFloat() * 2f - 1f);
            result.Normalize();
            return result;
        }

        private static float ComputeBoxSideDistance(Vector3 point, float boxSize)
        {
            return Vector3.Min(point, new Vector3(boxSize) - point).Min();
        }

        private static void FilterKindDuplicates(List<MyVoxelMaterialDefinition> materials, MyRandom random)
        {
            materials.SortNoAlloc((MyVoxelMaterialDefinition x, MyVoxelMaterialDefinition y) => string.Compare(x.MinedOre, y.MinedOre, StringComparison.InvariantCultureIgnoreCase));
            int num = 0;
            for (int i = 1; i <= materials.Count; i++)
            {
                if (i != materials.Count && !(materials[i].MinedOre != materials[num].MinedOre))
                {
                    continue;
                }
                int num2 = random.Next(num, i);
                for (int num3 = i - 1; num3 >= num; num3--)
                {
                    if (num3 != num2)
                    {
                        materials.RemoveAt(num3);
                    }
                }
                num++;
                i = num;
            }
        }

        private static void LimitMaterials(List<MyVoxelMaterialDefinition> materials, int maxCount, MyRandom random)
        {
            while (materials.Count > maxCount)
            {
                materials.RemoveAt(random.Next(materials.Count));
            }
        }

        private static void ProcessMaterialSpawnProbabilities(List<MyVoxelMaterialDefinition> materials)
        {
            int count = materials.Count;
            for (int i = 0; i < count; i++)
            {
                MyVoxelMaterialDefinition myVoxelMaterialDefinition = materials[i];
                int num = myVoxelMaterialDefinition.AsteroidGeneratorSpawnProbabilityMultiplier - 1;
                for (int j = 0; j < num; j++)
                {
                    materials.Add(myVoxelMaterialDefinition);
                }
            }
        }

        private void MakeIceAsteroid(int version, MyRandom random)
        {
            List<MyVoxelMaterialDefinition> list = new List<MyVoxelMaterialDefinition>();
            foreach (MyVoxelMaterialDefinition voxelMaterialDefinition in MyDefinitionManager.Static.GetVoxelMaterialDefinitions())
            {
                if (IsAcceptedAsteroidMaterial(voxelMaterialDefinition, version) && voxelMaterialDefinition.MinedOre == "Ice")
                {
                    list.Add(voxelMaterialDefinition);
                }
            }
            if (list.Count == 0)
            {
                MyLog.Default.Log(MyLogSeverity.Error, "No ice material suitable for ice cluster. Ice cluster will not be generated!");
                return;
            }
            m_coreMaterials.Clear();
            m_depositMaterials.Clear();
            m_surfaceMaterials = list;
            FilterKindDuplicates(m_surfaceMaterials, random);
        }

        private static bool IsAcceptedAsteroidMaterial(MyVoxelMaterialDefinition material, int version)
        {
            if (!material.SpawnsInAsteroids)
            {
                return false;
            }
            if (material.MinVersion > version || material.MaxVersion < version)
            {
                return false;
            }
            return true;
        }
    }
}
