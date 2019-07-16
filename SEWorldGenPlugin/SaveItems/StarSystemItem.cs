using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SEWorldGenPlugin.SaveItems
{
    public class StarSystemItem : Ob_GeneratorSaveItem
    {
        [XmlArray("Planets")]
        [XmlArrayItem("Planet")]
        public List<PlanetItem> Planets = new List<PlanetItem>();
    }
}
