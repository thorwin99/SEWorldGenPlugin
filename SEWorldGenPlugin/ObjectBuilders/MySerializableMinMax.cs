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
        /// <summary>
        /// Minimum value
        /// </summary>
        [ProtoMember(1)]
        public int Min { get; private set; }

        /// <summary>
        /// Maximum value
        /// </summary>
        [ProtoMember(2)]
        public int Max { get; private set; }

        /// <summary>
        /// Initializes a new instance, where min and max are equal
        /// </summary>
        /// <param name="value">Value of both min and max</param>
        public MySerializableMinMax(int value)
        {
            Min = value;
            Max = value;
        }

        /// <summary>
        /// Initializes a new min and max, where min(v1, v2) the minimum is, and max(v1, v2) the maximum.
        /// </summary>
        /// <param name="v1">Value 1</param>
        /// <param name="v2">Value 2</param>
        public MySerializableMinMax(int v1, int v2)
        {
            Min = Math.Min(v1, v2);
            Max = Math.Max(v1, v2);
        }

        /// <summary>
        /// Sets a new minimum value. If the new minimum value is larger than the current
        /// maximum one, it will instead be a new maximum.
        /// </summary>
        /// <param name="value">Value to set the minimum to</param>
        public void SetMinimum(int value)
        {
            Min = Math.Min(value, Max);
            Max = Math.Max(value, Max);
        }

        /// <summary>
        /// Returns the median of min and max
        /// </summary>
        /// <returns>The median</returns>
        public int GetMedian()
        {
            return (Min + Max) / 2;
        }

        /// <summary>
        /// Sets a new maximum value. If the new maximum value is less than the current
        /// minimum one, it will instead be a new minimum and the old minimum the maximum.
        /// </summary>
        /// <param name="value">Value to set the maximum to</param>
        public void SetMaximum(int value)
        {
            Min = Math.Min(Min, value);
            Max = Math.Max(Min, value);
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
