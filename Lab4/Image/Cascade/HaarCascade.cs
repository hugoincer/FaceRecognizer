using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Lab4.Image.Cascade
{
    
        public class HaarCascade : ICloneable
        {

            public int Width { get; protected set; }

            public int Height { get; protected set; }

            public HaarCascadeStage[] Stages { get; protected set; }
             
            public bool HasTiltedFeatures { get; protected set; }

             
            public HaarCascade(int baseWidth, int baseHeight, HaarCascadeStage[] stages)
            {
                Width = baseWidth;
                Height = baseHeight;
                Stages = stages;

                HasTiltedFeatures = checkTiltedFeatures(stages);
            }

            protected HaarCascade(int baseWidth, int baseHeight)
            {
                Width = baseWidth;
                Height = baseHeight;
            }

            private static bool checkTiltedFeatures(HaarCascadeStage[] stages)
            {
                foreach (var stage in stages)
                    foreach (var tree in stage.Trees)
                        foreach (var node in tree)
                            if (node.Feature.Tilted == true)
                                return true;
                return false;
            }

            /// <summary>
            ///   Creates a new object that is a copy of the current instance.
            /// </summary>
            /// 
            /// <returns>
            ///   A new object that is a copy of this instance.
            /// </returns>
            /// 
            public object Clone()
            {
                HaarCascadeStage[] newStages = new HaarCascadeStage[Stages.Length];
                for (int i = 0; i < newStages.Length; i++)
                    newStages[i] = (HaarCascadeStage)Stages[i].Clone();

                HaarCascade r = new HaarCascade(Width, Height);
                r.HasTiltedFeatures = this.HasTiltedFeatures;
                r.Stages = newStages;

                return r;
            }


            /// <summary>
            ///   Loads a HaarCascade from a OpenCV-compatible XML file.
            /// </summary>
            /// 
            /// <param name="stream">
            ///    A <see cref="Stream"/> containing the file stream
            ///    for the xml definition of the classifier to be loaded.</param>
            ///    
            /// <returns>The HaarCascadeClassifier loaded from the file.</returns>
            /// 
            public static HaarCascade FromXml(Stream stream)
            {
                return FromXml(new StreamReader(stream));
            }

            /// <summary>
            ///   Loads a HaarCascade from a OpenCV-compatible XML file.
            /// </summary>
            /// 
            /// <param name="path">
            ///    The file path for the xml definition of the classifier to be loaded.</param>
            ///    
            /// <returns>The HaarCascadeClassifier loaded from the file.</returns>
            /// 
            public static HaarCascade FromXml(string path)
            {
                return FromXml(new StreamReader(path));
            }

            public static HaarCascade FromXml(TextReader stringReader)
            {
                XmlTextReader xmlReader = new XmlTextReader(stringReader);

                // Gathers the base window size
                xmlReader.ReadToFollowing("size");
                string size = xmlReader.ReadElementContentAsString();

                // Proceeds to load the cascade stages
                xmlReader.ReadToFollowing("stages");
                XmlSerializer serializer = new XmlSerializer(typeof(HaarCascadeSerializationObject));
                var stages = (HaarCascadeSerializationObject)serializer.Deserialize(xmlReader);

                // Process base window size
                string[] s = size.Trim().Split(' ');
                int baseWidth = int.Parse(s[0], CultureInfo.InvariantCulture);
                int baseHeight = int.Parse(s[1], CultureInfo.InvariantCulture);

                return new HaarCascade(baseWidth, baseHeight, stages.Stages);
            }
        }
}
