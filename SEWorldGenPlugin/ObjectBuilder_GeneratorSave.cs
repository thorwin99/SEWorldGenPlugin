using System.Collections.Generic;
using System.Xml.Serialization;

namespace SEWorldGenPlugin
{
    public partial class ObjectBuilder_GeneratorSave
    {
        [XmlElement("StarSystem", typeof(SEWorldGenPlugin.SaveItems.StarSystemItem))]
        public List<Ob_GeneratorSaveItem> Components = new List<Ob_GeneratorSaveItem>();
    }
}
