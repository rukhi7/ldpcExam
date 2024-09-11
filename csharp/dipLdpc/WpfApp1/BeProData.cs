using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace dipLdpc
{
    class BeProData
    {
        public static int ZERO_MATRIX = -1;
        public static void Init()
        {

        }
        public static int[][] convertStandartMatrix(String str, int BLOCK_SIZE)
        {
            WpfApp1.MainWindow.printOutput($"convertStandartMatrix Start!");
            String[] lines = str.Split("\n");
            int columns = lines[0].Split(" ").Length;
            //            int columns = codewordSize / blockSize;
            int strings = lines.Length;// columns - messageSize / blockSize;
            int[][] matrix = new int[strings][];
            for (int i = 0; i < strings; i++)
                matrix[i] = new int[columns];

            int len = lines.Length;
            if (len != strings)
            {
                WpfApp1.MainWindow.printOutput($"Lines count:{len} != {strings} error!");
                return null;
            }
            WpfApp1.MainWindow.printOutput($"It is {len}-square or {len * BLOCK_SIZE} rows Matrix! ");

            for (int i = 0; i < strings; i++)
            {
                String[] massString = lines[i].Split(" ");
                len = massString.Length;
                if (len != columns)
                {
                    WpfApp1.MainWindow.printOutput($"Line{i} len:{len} != {columns} error!");
                    return null;
                }

                for (int j = 0; j < massString.Length; j++)
                {
                    if (massString[j].Equals("-"))
                    {
                        matrix[i][j] = ZERO_MATRIX;
                    }
                    else
                    {
                        matrix[i][j] = int.Parse(massString[j]);
                    }
                }
                WpfApp1.MainWindow.printOutput($"Line{i} is OK!");
            }
            WpfApp1.MainWindow.printOutput($"And It is {columns}-square or {columns * BLOCK_SIZE} columns Matrix! ");
            return matrix;
        }

        public int[] decode(List<List<SquareBlock>> shiftBlocks)
        {
            LoadWordToTable(shiftBlocks);
            int iter = 0;
            int[] correctedCodeWord = null;
//            List<Integer> compareAfter = new ArrayList<>();

            while (iter < iterations)
            {

                List<List<SquareBlock>> newLLR = iterationOfBeliefPropagationClassic(shiftBlocks);

                double[] summNewOldLLRs = summLLRsForOneVariable(newLLR);

                //int[] sindrom1 = calculateSindromTEST(shiftBlocks, newLLR);
                correctedCodeWord = Array.ConvertAll<double, int>(summNewOldLLRs, x => x > 0 ? 0 : 1);
                int sindromCount = calculateSindrom(shiftBlocks, correctedCodeWord);
                //System.out.println("Sindrom: " + checkToZero(sindrom));
                if (sindromCount == 0)
                {
//                    WpfApp1.MainWindow.printOutput($"Zero-sindrom! {iter+1 } iters completed from {iterations}!");
                    return correctedCodeWord;
                }
                //            break;
                iter++;
                shiftBlocks = newLLR;
            }
//            WpfApp1.MainWindow.printOutput($"No zero-sindrom! All {iter+1 } iters completed!");

            return correctedCodeWord;

        }

        LdpcMatrixObj ldpcMa;
        public BeProData(LdpcMatrixObj matrix, int iterations)
        {
            ldpcMa = matrix;
            this.iterations = iterations;
        }
        internal void StartWithLLR(double[] message)
        {
            this.message = message;
        }
        private double[] message;
        private int columns { get { return ldpcMa.Columns(); } }
        private int tstring { get { return ldpcMa.Strings(); } }
        private int iterations;
        private int blockSize { get { return ldpcMa.blovkSize; } }
        private List<List<SquareBlock>> LoadWordToTable(List<List<SquareBlock>> shiftBlocks)
        {

            for (int i = 0; i < shiftBlocks.Count; i++)
            {
                List<SquareBlock> line = shiftBlocks[i];
                int j = 0;

                for (int k = 0; k < blockSize; k++)
                {
                    line[1].LoadElement(k, message);
                    for (int l = 2; l < line.Count; l++)
                    {
                        line[l].LoadElement(k, message);
                    }
                }
                for (j = 1; j < line.Count; j++)
                {
                    for (int k = 0; k < blockSize; k++)
                    {
                        line[0].LoadElement(k, message);

                        for (int l = 1; l < j; l++)
                        {
                            line[l].LoadElement(k, message);
                        }

                        for (int l = j + 1; l < line.Count; l++)
                        {
                            line[l].LoadElement(k, message);
                        }
                    }
                }
            }
            return shiftBlocks;
        }
        private List<List<SquareBlock>> iterationOfBeliefPropagationClassic(List<List<SquareBlock>> shiftBlocks)
        {

            for (int i = 0; i < shiftBlocks.Count; i++)
            {
                int j = 0;
                List<SquareBlock> line = shiftBlocks[i];
                double[] element = line[j].GetArr();

                for (int k = 0; k < blockSize; k++)
                {
                    double newElementLLR = line[1].element(k, message);
                    newElementLLR = coreOfIterationSign(line, k, 2, line.Count, newElementLLR);
                    //newStringLLR.add(newElementLLR);
                    element[k] = newElementLLR;
                }
                for (j = 1; j < line.Count; j++)
                {
                    //List<Double[]> newStringLLR = new ArrayList<>();
                    element = line[j].GetArr();

                    for (int k = 0; k < blockSize; k++)
                    {
                        double newElementLLR = line[0].element(k, message);
                        newElementLLR = coreOfIterationSign(line, k, 1, j, newElementLLR);
                        /*                    for (int l = 1; l < j; l++) {
                                                double yy = line.get(l).element(k, message);
                                                newElementLLR = Math.log((1 + Math.exp(newElementLLR + yy))
                                                        / (Math.exp(newElementLLR) + Math.exp(yy)));
                                            }*/

                        newElementLLR = coreOfIterationSign(line, k, j + 1, line.Count, newElementLLR);
                        /*                   for (int l = j + 1; l < line.size(); l++) {
                                               double yy = line.get(l).element(k, message);
                                               newElementLLR = Math.log((1 + Math.exp(newElementLLR + yy))
                                                       / (Math.exp(newElementLLR) + Math.exp(yy)));
                                           }*/
                        //newStringLLR.add(newElementLLR);
                        element[k] = newElementLLR;
                    }
                }
            }
            return shiftBlocks;
        }
        private double coreOfIterationSign(List<SquareBlock> line, int k, int startIndex, int endIndx, double newElementLLR)
        {
            for (int l = startIndex; l < endIndx; l++)
            {
                double yy = line[l].element(k, message);
/*                if (newElementLLR > 300) newElementLLR = 300;
                else if (newElementLLR < -300) newElementLLR = -300;
                if (yy > 300) yy = 300;
                else if (yy < -300) yy = -300; */
                double tmp = Math.Exp(newElementLLR) + Math.Exp(yy);
                Debug.Assert(tmp != 0);
                newElementLLR = Math.Log((1 + Math.Exp(newElementLLR + yy)) / tmp);
                if (newElementLLR > 100) newElementLLR = 100;
                else if (newElementLLR < -100) newElementLLR = -100;

                Debug.Assert(newElementLLR != double.NaN);
                //newElementLLR = Math.signum(newElementLLR) * Math.signum(yy) * Math.min(Math.abs(newElementLLR), Math.abs(yy));
                //+ Math.log((1 + Math.exp(newElementLLR + yy))) - Math.log((Math.exp(newElementLLR) + Math.exp(yy)));
                /*newElementLLR = Math.max(0, newElementLLR + yy) - Math.max(newElementLLR, yy)
                        + QuantElement.myLog(Math.abs(newElementLLR + yy)) - QuantElement.myLog(Math.abs(newElementLLR - yy));*/
            }
            return newElementLLR;
        }
        private double[] summLLRsForOneVariable(List<List<SquareBlock>> newLLR)
        {

            double[] summ = new double[message.Length];
            int[] everyLinePos = new int[newLLR.Count];
            Debug.Assert(newLLR.Count == tstring);
            for (int i = 0; i < columns; i++)
            {

                for (int j = 0; j < blockSize; j++)
                {
                    double xx = 0;
                    for (int k = 0; k < newLLR.Count; k++)
                    {
                        int inLinePos = everyLinePos[k];
                        if (newLLR[k].Count > inLinePos)
                        {
                            if (newLLR[k][inLinePos].column == i)
                            {
                                xx += newLLR[k][inLinePos].GetRightVal(j); //GetArr()[j];

                            }
                        }
                    }

                    xx += message[j + i * blockSize];
                    summ[j + i * blockSize] = xx;
                    for (int k = 0; k < newLLR.Count; k++)
                    {
                        int inLinePos = everyLinePos[k];
                        if (newLLR[k].Count > inLinePos)
                        {
                            if (newLLR[k][inLinePos].column == i)
                            {
                                newLLR[k][inLinePos].UpdateRightVal(j, xx); //GetArr()[j];

                            }
                        }
                    }
                }

                for (int k = 0; k < newLLR.Count; k++)
                {
                    if (newLLR[k].Count > everyLinePos[k])
                        if (newLLR[k][everyLinePos[k]].column == i)
                        {
                            everyLinePos[k]++;
                        }
                }
            }
            /*Double[] vektor = newLLR.get(0).get(0).GetLoadedArr();
            for (int l = 0; l < blockSize; l++) {
                double dbg = vektor[l];
                System.out.println("vSum+: " + dbg);
            }*/
            return summ;
        }
        private double[] threshold(double[] l)
        {
            for (int i = 0; i < l.Length; i++)
            {
                if (l[i] > 0)
                {
                    l[i] = 0;
                }
                else
                {
                    l[i] = 1;
                }
            }
            return l;
        }

        public int calculateSindrom(List<List<SquareBlock>> shiftBlocks, int[] codeWord)
        {
            int res = 0;
            //            double[] correctedCodeWord = codeWord;
            int[] sindrom = new int[tstring * blockSize];
            for (int i = 0; i < shiftBlocks.Count; i++)
            {

                for (int k = 0; k < blockSize; k++)
                {
                    int ssum = 0;
                    double mass;
                    for (int j = 0; j < shiftBlocks[i].Count; j++)
                    {
                        SquareBlock blk = shiftBlocks[i][j];
                        int shft = (blk.GetShift() + k) % blockSize;
                        int col = blk.GetCol();
                        mass = codeWord[shft + col * blockSize];
                        if (mass > 0)
                        {
                            ssum ^= 1;
                        }
                        else
                        {
                            ssum ^= 0;
                        }
                    }
                    sindrom[k + i * blockSize] = ssum;
                    res += ssum;
                }
            }

            return res;
        }
    }//class BeProData
}
