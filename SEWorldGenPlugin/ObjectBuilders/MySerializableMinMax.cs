using ProtoBuf;
using System;

namespace SEWorldGenPlugin.ObjectBuilders
{
    /// <summary>
    /// Serializable struct used to represent a min max pair of values.
    /// The values are automatically sorted in the constructor, so
    /// min is always min and max is always max.
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class MySerializableMinMax : MyAbstractConfigObjectBuilder
    {
        [ProtoMember(1)]
        public int Min;

        [ProtoMember(2)]
        public int Max;

        public MySerializableMinMax(int v1, int v2)
        {
            Min = Math.Min(v1, v2);
            Max = Math.Max(v1, v2);
        }

        public override MyAbstractConfigObjectBuilder copy()
        {
            return new MySerializableMinMax(Min, Max);
        }

        public override void Verify()
        {
            if (Min > Max)
            {
                int t = Max;
                Max = Min;
                Min = t;
            }
        }
    }
}
