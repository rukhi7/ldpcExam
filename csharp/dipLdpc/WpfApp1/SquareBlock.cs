namespace dipLdpc
{
    internal class SquareBlock
    {
        public static int blkSz;
        double[] LoadedMessage;
        int shft;
        int str;
        internal int column;
        public SquareBlock(int shft, int str, int column)
        {
            //        this.message = message;
            this.shft = shft;
            this.str = str;
            this.column = column;
            LoadedMessage = new double[blkSz];
        }
        public int GetShift()
        {
            return shft;
        }
        public int GetCol()
        {
            return column;
        }
        public void LoadElement(int k, double[] message)
        {
            int indx = (shft + k) % blkSz;
            LoadedMessage[k] = message[column * blkSz + indx];
        }
        double[] DiagValues;
        public double[] GetArr()
        {
            if (DiagValues == null)
            {
                DiagValues = new double[blkSz];
            }
            else
            {
                //            System.out.println("Error of SquareBlock value access!!!");
            }
            return DiagValues;
        }
        public double element(int k, double[] message)
        {
            return LoadedMessage[k];
        }
        public double GetRightVal(int j)
        {
            int indx = ((blkSz - shft) + j) % blkSz;
            return DiagValues[indx];
        }
        public void UpdateRightVal(int j, double xx)
        {
            int indx = ((blkSz - shft) + j) % blkSz;
            LoadedMessage[indx] = xx - DiagValues[indx];
        }
    }
}