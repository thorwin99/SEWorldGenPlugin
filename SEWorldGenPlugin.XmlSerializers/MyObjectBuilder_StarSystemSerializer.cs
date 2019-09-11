using Microsoft.Xml.Serialization.GeneratedAssembly;
using System.Xml;
using System.Xml.Serialization;

namespace SEWorldGenPlugin.XmlSerializers
{
    public sealed class MyObjectBuilder_StarSystemSerializer : XmlSerializer1
    {
        public override bool CanDeserialize(XmlReader xmlReader)
        {
            return xmlReader.IsStartElement("MyObjectBuilder_WorldGenerator", "");
        }

        protected override void Serialize(object objectToSerialize, XmlSerializationWriter writer)
        {
            ((XmlSerializationWriter1)writer).Write5715_MyObjectBuilder_WorldGenerator(objectToSerialize);
        }

        protected override object Deserialize(XmlSerializationReader reader)
        {
            return ((XmlSerializationReader1)reader).Read6196_MyObjectBuilder_WorldGenerator();
        }
    }
}
