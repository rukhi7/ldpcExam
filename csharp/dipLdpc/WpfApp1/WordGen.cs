using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dipLdpc
{
    class WordGen
    {
        private int[][] ldpcMatrix;
        private int bsize;
        private int messageSize;
        readonly private int columns;
        private int strings;
        Random rnd = new Random(3);
        private int[] message;
        private int[] codeWord;
        private int codewordSize;
        public WordGen(int[][] standartMatrix, int bLOCK_SIZE)
        {
            this.ldpcMatrix = standartMatrix;
            this.bsize = bLOCK_SIZE;
            strings = standartMatrix.Length;
            columns = standartMatrix[0].Length;
            messageSize = bLOCK_SIZE * (columns - strings);
            this.codewordSize = bLOCK_SIZE * columns;
        }
        public int[] getCodeWord() { return codeWord; }
        public void generateMessage()
        {
            message = new int[messageSize];
            for (int i = 0; i < messageSize; i++)
            {
                double result = rnd.NextDouble();//i &1;//
                if (result < 0.5)
                {
                    message[i] = 0;
                }
                else
                {
                    message[i] = 1;
                }
            }
        }

        public static int[] calculateVectorShift(int shift, int[] vector)
        {
            if (shift == -1)
            {
                return null;
            }
            if (shift == 0)
            {
                return vector;
            }

            List<int> list = new List<int>(vector);
            vector = list.Skip(shift).Concat(list.Take(shift)).ToArray();
            return vector;
        }
        private int[] summVectorsModul2(int[] result, int[] vector2)
        {
            if(vector2 != null)
            for (int i = 0; i < result.Length; i++)
            {
                result[i] ^= vector2[i];
            }
            return result;
        }
        private int[] calculateParity0()
        {
            int[] resultVector = new int[bsize];

            for (int i = 0; i < strings; i++)
            {
                for (int j = 0; j < columns - strings; j++)
                {
                    int[] splitMessage = new List<int>(message).GetRange(j * bsize,bsize).ToArray();
                    resultVector = summVectorsModul2(resultVector, calculateVectorShift(ldpcMatrix[i][j], splitMessage));
                }
            }
            return resultVector;
        }
        public List<int[]> parities()
        {
            int[] parity0 = calculateParity0();
            int[][] lamdas = calculateLamdas();
            List<int[]> parities = new List<int[]>(new int[strings][]);

            int[] parity1 = summVectorsModul2(lamdas[0], calculateVectorShift(1, parity0));

            int[] parityLast = summVectorsModul2(lamdas[strings - 1], calculateVectorShift(1, parity0));


            parities[0] = (parity0);
            parities[1] = (parity1);
            parities[strings - 1] = parityLast;

            int tmp = strings / 2 - 1;
            for (int i = 1; i < tmp; i++)
            {
                parities[i + 1] = summVectorsModul2(lamdas[i], parities[i]);
            }
            tmp++;
            for (int i = strings - 2; i > tmp; i--)
            {
                parities[i] = summVectorsModul2(lamdas[i], parities[i + 1]);
            }

            parities[tmp] = summVectorsModul2(summVectorsModul2(lamdas[tmp], parities[0]), parities[tmp + 1]);

            convertToCodeWord(parities);
            return parities;
        }
        private int[][] calculateLamdas()
        {
            int[][] lamdas = new int[strings][];

            for (int i = 0; i < strings; i++)
            {
                lamdas[i] = new int[bsize];
            }

            for (int i = 0; i < strings; i++)
            {
                for (int j = 0; j < columns - strings; j++)
                {
                    int[] splitMessage = new List<int>(message).GetRange(j * bsize, bsize).ToArray();
                    lamdas[i] = summVectorsModul2(lamdas[i], calculateVectorShift(ldpcMatrix[i][j], splitMessage));
                }
            }
            return lamdas;
        }
        private void convertToCodeWord(List<int[]> parities)
        {
            codeWord = new int[codewordSize];
            int iterator = -1;
            for (int i = 0; i < codewordSize; i++)
            {
                if (i < messageSize)
                {
                    codeWord[i] = message[i];
                }
                else
                {
                    if (i % bsize == 0)
                    {
                        iterator = iterator + 1;
                    }
                    codeWord[i] = parities[iterator][(i - messageSize) % bsize];
                }
            }
        }
        public int calculateSindrom()
        {
            int res = 0;
//            double[] correctedCodeWord = codeWord;
            int[] sindrom = new int[strings * bsize];
            for (int i = 0; i < shiftBlocks.Count; i++)
            {

                for (int k = 0; k < bsize; k++)
                {
                    int ssum = 0;
                    double mass;
                    for (int j = 0; j < shiftBlocks[i].Count; j++)
                    {
                        SquareBlock blk = shiftBlocks[i][j];
                        int shft = (blk.GetShift() + k) % bsize;
                        int col = blk.GetCol();
                        mass = codeWord[shft + col * bsize];
                        if (mass > 0)
                        {
                            ssum ^= 1;
                        }
                        else
                        {
                            ssum ^= 0;
                        }
                    }
                    sindrom[k + i * bsize] = ssum;
                    res += ssum;
                }
            }

            return res;
        }
        public int calculateSindrom(double[] codeWord)
        {
            int res = 0;
            //            double[] correctedCodeWord = codeWord;
 //           int[] sindrom = new int[strings * bsize];
            for (int i = 0; i < shiftBlocks.Count; i++)
            {

                for (int k = 0; k < bsize; k++)
                {
                    int ssum = 0;
                    double mass;
                    for (int j = 0; j < shiftBlocks[i].Count; j++)
                    {
                        SquareBlock blk = shiftBlocks[i][j];
                        int shft = (blk.GetShift() + k) % bsize;
                        int col = blk.GetCol();
                        mass = codeWord[shft + col * bsize];
                        if (-mass > 0)
                        {
                            ssum ^= 1;
                        }
                        else
                        {
                            ssum ^= 0;
                        }
                    }
  //                  sindrom[k + i * bsize] = ssum;
                    res += ssum;
                }
            }

            return res;
        }
        List<List<SquareBlock>> shiftBlocks;// = parseCodeWordToMatrix();
        public List<List<SquareBlock>> parseCodeWordToMatrix()
        {
            SquareBlock.blkSz = bsize;
            shiftBlocks = new List<List<SquareBlock>>();
            for (int i = 0; i < ldpcMatrix.Length; i++)
            {
                List<SquareBlock> listString = new List<SquareBlock>();
                for (int j = 0; j < ldpcMatrix[0].Length; j++)
                {
                    if (ldpcMatrix[i][j] == -1)
                    {
                        //                   listString.add(null);
                    }
                    else
                    {
                        //                    shiftBlocksInString = valueToShiftBlock(i, j);
                        SquareBlock shiftBlocksInString = new SquareBlock(ldpcMatrix[i][j], i, j);

                        listString.Add(shiftBlocksInString);
                    }
                }
                shiftBlocks.Add(listString);
            }
            return shiftBlocks;
        }
    }
}
