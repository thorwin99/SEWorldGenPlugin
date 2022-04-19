namespace SEWorldGenPluginMod.Data.Scripts
{
    /// <summary>
    /// Class representing 2 item tuple
    /// </summary>
    /// <typeparam name="T1">First item type</typeparam>
    /// <typeparam name="T2">Second item type</typeparam>
    public struct MyTuple<T1, T2>
    {
        /// <summary>
        /// First item of tuple
        /// </summary>
        public T1 Item1;

        /// <summary>
        /// Second item of tuple
        /// </summary>
        public T2 Item2;

        public MyTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
