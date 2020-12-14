using ProtoBuf;
using System;

namespace SEWorldGenPlugin.ObjectBuilders
{
    [Serializable]
    [ProtoContract]
    public class MyObjectBuilder_GlobalSettings : MyAbstractConfigObjectBuilder
    {
        public MyObjectBuilder_GlobalSettings() : base()
        {

        }

        public override MyAbstractConfigObjectBuilder copy()
        {
            return new MyObjectBuilder_GlobalSettings();
        }

        public override void Verify()
        {
        }
    }
}
