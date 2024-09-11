using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace dipLdpc
{
    class LdpcMatrixDefs
    {
        //matrixF1Speed1
        static public LdpcMatrixObj matrix1 = new LdpcMatrixObj("0 - - - 0 0 - - 0 - - 0 1 0 - - - - - - - - - -\n" +
"22 0 - - 17 - 0 0 12 - - - - 0 0 - - - - - - - - -\n" +
"6 - 0 - 10 - - - 24 - 0 - - - 0 0 - - - - - - - -\n" +
"2 - - 0 20 - - - 25 0 - - - - - 0 0 - - - - - - -\n" +
"23 - - - 3 - - - 0 - 9 11 - - - - 0 0 - - - - - -\n" +
"24 - 23 1 17 - 3 - 10 - - - - - - - - 0 0 - - - - -\n" +
"25 - - - 8 - - - 7 18 - - 0 - - - - - 0 0 - - - -\n" +
"13 24 - - 0 - 8 - 6 - - - - - - - - - - 0 0 - - -\n" +
"7 20 - 16 22 10 - - 23 - - - - - - - - - - - 0 0 - -\n" +
"11 - - - 19 - - - 13 - 3 17 - - - - - - - - - 0 0 -\n" +
"25 - 8 - 23 18 - 14 9 - - - - - - - - - - - - - 0 0\n" +
"3 - - - 16 - - 2 25 5 - - 1 - - - - - - - - - - 0", 27);

        //matrixF2Speed1
        static public LdpcMatrixObj matrix2 = new LdpcMatrixObj("40 - - - 22 - 49 23 43 - - - 1 0 - - - - - - - - - -\n" +
                        "50 1 - - 48 35 - - 13 - 30 - - 0 0 - - - - - - - - -\n" +
                        "39 50 - - 4 - 2 - - - - 49 - - 0 0 - - - - - - - -\n" +
                        "33 - - 38 37 - - 4 1 - - - - - - 0 0 - - - - - - -\n" +
                        "45 - - - 0 22 - - 20 42 - - - - - - 0 0 - - - - - -\n" +
                        "51 - - 48 35 - - - 44 - 18 - - - - - - 0 0 - - - - -\n" +
                        "47 11 - - - 17 - - 51 - - - 0 - - - - - 0 0 - - - -\n" +
                        "5 - 25 - 6 - 45 - 13 40 - - - - - - - - - 0 0 - - -\n" +
                        "33 - - 34 24 - - - 23 - - 46 - - - - - - - - 0 0 - -\n" +
                        "1 - 27 - 1 - - - 38 - 44 - - - - - - - - - - 0 0 -\n" +
                        "- 18 - - 23 - - 8 0 35 - - - - - - - - - - - - 0 0\n" +
                        "49 - 17 - 30 - - - 34 - - 19 1 - - - - - - - - - - 0", 54);

        //matrixF2Speed2
        static public LdpcMatrixObj matrix3 = new LdpcMatrixObj("17 13 8 21 9 3 18 12 10 0 4 15 19 2 5 10 26 19 13 13 1 0 - -\n" +
                                "3 12 11 14 11 25 5 18 0 9 2 26 26 10 24 7 14 20 4 2 - 0 0 -\n" +
                                "22 16 4 3 10 21 12 5 21 14 19 5 - 8 5 18 11 5 5 15 0 - 0 0\n" +
                                "7 7 14 14 4 16 16 24 24 10 1 7 15 6 10 26 8 18 21 14 1 - - 0", 54);

    }
    public class LdpcMatrixObj
    {
        internal int[][] standartMatrix;
        internal int blovkSize;
        //        [XmlText] 
        internal string id { get; set; }
        public LdpcMatrixObj(string tbl, int blkSz)
        {
            blovkSize = blkSz;
            standartMatrix = BeProData.convertStandartMatrix(tbl, blkSz);
            height = standartMatrix.Length;
            width = standartMatrix[0].Length;
            id = height + "x" + width + "x" + blkSz;
        }
        public LdpcMatrixObj() { }
        public override string ToString()
        {
            return id;
        }
        int height;
        int width;
        internal int Strings()
        {
            return height;
        }
        internal int Columns()
        {
            return width;
        }
    }
    public class MatrixConverter : StringConverter
    {
        private static Dictionary<string, LdpcMatrixObj> values;
        public MatrixConverter()
        {
            // Initializes the standard values list with defaults.
            values = new Dictionary<string, LdpcMatrixObj>();
            values.Add(LdpcMatrixDefs.matrix1.id, LdpcMatrixDefs.matrix1);
            values.Add(LdpcMatrixDefs.matrix2.id, LdpcMatrixDefs.matrix2);
            values.Add(LdpcMatrixDefs.matrix3.id, LdpcMatrixDefs.matrix3);
        }

        internal static LdpcMatrixObj getMatrixFromConfig(string keyStr)
        {
            return values[keyStr];
        }
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //true means show a combobox
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //true will limit to list. false will show the list, 
            //but allow free-form entry
            return true;
        }

        public override System.ComponentModel.TypeConverter.StandardValuesCollection
               GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(values.Keys);
        }
    }

}
