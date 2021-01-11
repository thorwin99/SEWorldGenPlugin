using Sandbox.Game.World.Generator;
using System;
using System.Collections.Generic;
using VRage.Collections;
using VRage.Game;
using VRageMath;

namespace SEWorldGenPlugin.Generator.ProceduralGeneration
{
    /// <summary>
    /// Cell based generator module. Will only generate objects of loaded cells
    /// and will handle unloading of said cells.
    /// </summary>
    public abstract class MyAbstractProceduralCellModule
    {
        /// <summary>
        /// Seed used in this module
        /// </summary>
        protected int m_seed;

        /// <summary>
        /// All currently loaded cells
        /// </summary>
        protected Dictionary<Vector3I, MyProceduralCell> m_loadedCells = new Dictionary<Vector3I, MyProceduralCell>();

        /// <summary>
        /// Tree of cell positions, to get cells based on bounding boxes, spheres etc...
        /// </summary>
        protected MyDynamicAABBTreeD m_cellsTree = new MyDynamicAABBTreeD(Vector3D.Zero);

        /// <summary>
        /// All cells, that should get unloaded, if no entity is in range of it.
        /// </summary>
        protected CachingHashSet<MyProceduralCell> m_toUnloadCells = new CachingHashSet<MyProceduralCell>();

        /// <summary>
        /// All currently existing object seeds.
        /// </summary>
        protected HashSet<MyObjectSeedParams> m_existingObjectSeeds = new HashSet<MyObjectSeedParams>();

        /// <summary>
        /// Size of the cells of this module
        /// </summary>
        protected double m_cellSize;

        /// <summary>
        /// Creates a new cell module with given seed and cell size.
        /// If the Cellsize is larger than 25000, it wont be used and 25000 will be used instead
        /// </summary>
        /// <param name="seed">Seed of the module</param>
        /// <param name="cellSize">Size of the indevidual cells</param>
        public MyAbstractProceduralCellModule(int seed, double cellSize)
        {
            m_seed = seed;
            m_cellSize = Math.Min(cellSize, 25000);
        }

        /// <summary>
        /// Generates a new Procedural cell and generates its seeds inside it.
        /// Does not generate the objects of the seeds itself
        /// </summary>
        /// <param name="cellId">The cell id of the cell</param>
        /// <returns>The new Procedural cell</returns>
        public abstract MyProceduralCell GenerateCellSeeds(Vector3I cellId);

        /// <summary>
        /// Generates all objects inside the currently loaded cells, if they are not
        /// already generated.
        /// </summary>
        public abstract void GenerateLoadedCellObjects();

        /// <summary>
        /// Closes the specified object. This should remove it from the world and memory.
        /// </summary>
        /// <param name="seed">The MyObjectSeed of the object that should be removed.</param>
        public abstract void CloseObject(MyObjectSeed seed);
    }
}
