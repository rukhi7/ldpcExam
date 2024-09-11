using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace dipLdpc
{
    public class ConfigObj
    {
        [Category("Configs")]
        public int countNoise { get; set; }
        [Category("Configs")]
        public int countRealization { get; set; }

        [Category("Configs")]
        [Browsable(true)]
        [TypeConverter(typeof(MatrixConverter))]
        public string LdpcMatrix { get; set; }


        [DisplayName("Count Of Iterations")]
        [Category("Configs")]
        [Description("Count Of Iterations")]
        public int CountOfIterations { get; set; }
        public double fld1
        { get; set; }
        public int[] fld2
        { get; set; }


        internal static ConfigObj allAppOpts = new ConfigObj();
        internal static ConfigObj CurrentConfig() => allAppOpts;
        public static ConfigObj loadOptsFromFile()
        {
            string defaultPath = Directory.GetCurrentDirectory();
            try
            {
                string fileName = "msgBodySaveOpts.xml";
                if (!File.Exists(fileName))
                {
                    string resourceName = "msgBodySaveOpts";
                    var prop = (string)Properties.Resources.ResourceManager.GetObject(resourceName);
                    File.WriteAllText(fileName, prop);
                }
                //if (OptsEnable.Checked)
                {
                    using (FileStream fs = new FileStream("msgBodySaveOpts.xml", FileMode.OpenOrCreate))
                    {
                        //               XmlReader reader = new XmlTextReader(fs);
                        // передаем в конструктор тип класса
                        XmlSerializer formatter = new XmlSerializer(typeof(ConfigObj));

                        //                 if (formatter.CanDeserialize(reader))
                        allAppOpts = (ConfigObj)formatter.Deserialize(fs);
                        //                    else
                        //                   { outLine.AppendText("I can't Deserialize XML file "); }
                        //outLine.AppendText("XML opts:" + fs.Name + Environment.NewLine);]
                    }
                }
                //else openSvOptsBtn.Enabled = false;
            }
            catch (Exception)
            {
                //                outLine.AppendText("error while XML file open" + Environment.NewLine + exep.ToString());
            }
            //            if (OptsEnable.Checked)
            {

                if (allAppOpts == null)
                {
                    allAppOpts = new ConfigObj();
                }

                return allAppOpts;
            }
        }

        public static void saveAllToFile()
        {
            XmlSerializer formatter_m = new XmlSerializer(typeof(ConfigObj));
            FileStream fs_m = new FileStream("msgBodySaveOpts.xml", FileMode.OpenOrCreate, FileAccess.Write);

            fs_m.SetLength(0);
            fs_m.Seek(0, SeekOrigin.Begin);
            formatter_m.Serialize(fs_m, allAppOpts);
            fs_m.Close();
        }
    }
}
