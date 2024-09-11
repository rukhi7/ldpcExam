using System;
using System.Collections.Generic;
using System.Text;

namespace dipLdpc
{
    class Channel
    {
        double zero;
        double one;
        double dispersOnDecoder;
        private double[] codeWord;
        private double standardDeviation;
        private const double maen = 0d;

        static double[] sGauss;
        static double[] sGaussSum;
        public Channel(int[] codeWord, double zero, double standardDeviation)
        {
            this.zero = zero;
            this.one = -zero;
            if(codeWord != null)
                this.codeWord = Array.ConvertAll<int, double>(codeWord, x => ((-x)<<1) + 1);

            this.standardDeviation = standardDeviation;
        }
        public void AddCodeWord(int[] codeWord) => this.codeWord = Array.ConvertAll<int, double>(codeWord, x => (((-x) << 1) + 1)* zero);
        static int shft = 128;
        static double sig = 1.0;// / Math.Sqrt(2);
        static double xStart = 6.5;
        static double stepX = xStart * sig / shft;
        public static double getXtransK() { return xStart / shft; }
        public static int getXzero() { return shft; }
        static Channel()
        {
            double[] arr= new double[shft * 2];
            double[] sumarr = new double[shft * 2];
            double sum = 0;
            for (int i=0;i< arr.Length;i++)
            {
                double earg = stepX*(i-shft + 0.5) / sig;
                double fx = 1d / (sig * Math.Sqrt(2 * Math.PI)) *
                    Math.Exp(-earg * earg/ 2) ;
                arr[i] = fx;
                sum += fx;
                sumarr[i] = sum;
            }
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = (int)(arr[i] / (sum * stepX / 10000));
            }
            sGauss = arr;
            sGaussSum = sumarr;
        }

        internal void SetAmplituda(double currentAmplitude)
        {
            this.zero = currentAmplitude;
            this.one = -currentAmplitude;
        }

        internal double[] convertToLLR()
        {
            double[] LLR = new double[codeWord.Length];
            //ruhi ? why signalE = 4??? 
            double signalE = 2 * Math.Abs(one - zero); // полагаем, что Eb = E0 = E1
            signalAddAWGN();
            double threshold = (one + zero) / 2;
            for (int i = 0; i < LLR.Length; i++)
            {//ruhi from:"633972-1 методДекодирования.pdf" (2.4) page-16
                double tmp = (signalE / (standardDeviation * standardDeviation)) * ((codeWord[i] - threshold) / (zero - one)); // дисперсию брать из расчитанной приемником
                                                                                                                               //LLR[i] = - codeWord[i];
                if (tmp > 100) tmp = 100;
                else if (tmp < -100) tmp = -100;

                LLR[i] = tmp;
            }
            return LLR;
        }

        static double stepY;

        Random rand12 = new Random(0); //reuse this if you are generating many
        double average = 0;
        int averCount = 0;
        double nRand12Old()
        {
            //            Jarrett's suggestion of using a Box-Muller transform is good for a quick-and-dirty solution. A simple implementation:
            const int numb = 24;
            double u1 = 0;
            for (int i = 0; i < numb; i++)
            {
                double tmp = rand12.NextDouble() - 0.5;
                u1 += tmp ; //uniform(0,1] random doubles
                average += tmp;
                averCount++;
            }
//            u1 += numb / 2d;
            return u1;
        }
        private double nextNextGaussian;
        private bool haveNextNextGaussian = false;
        double nRand12()
        {//javaVariant
            if (haveNextNextGaussian)
            {
                haveNextNextGaussian = false;
                return nextNextGaussian;
            }
            else
            {
                double v1, v2, s;
                do
                {
                    v1 = 2 * rand12.NextDouble() - 1; // between -1 and 1
                    v2 = 2 * rand12.NextDouble() - 1; // between -1 and 1
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1 || s == 0);
                double multiplier = Math.Sqrt(-2 * Math.Log(s) / s);
                nextNextGaussian = v2 * multiplier;
                haveNextNextGaussian = true;
                return v1 * multiplier;
            }
        }
        double average2 = 0;
        int averCount2 = 0;

        public int[] test_generateAWGN()
        {
            average2 = 0;
            averCount2 = 0;
            average = 0;
            averCount = 0;
            Random random = new Random();
            int[] gauss = new int[shft*2];
            const int ExprmntCnt = 256000;
            for (int i = 0; i < ExprmntCnt; i++)
            {
                double tmp = maen + nRand12() * standardDeviation;//standardDeviation; // + это смещение матожидания; * это изменение среднеквадратического откланения
                average2 += tmp;
                averCount2++;
                int pos = (int)(tmp / stepX + shft);
                if(pos>0 && pos< shft*2)
                    gauss[pos]++; 
            }

            WpfApp1.MainWindow.printOutput($"Ave={average / averCount}; ");
            WpfApp1.MainWindow.printOutput($"Ave2={average2 / averCount2}; ");

                        for (int i = 0; i < gauss.Length; i++)
                        {
                            gauss[i] = (int)(gauss[i] / stepX / (ExprmntCnt/10000));
                        }
            gauss[128] = 0;
            gauss[128 + 62] = -1100;
            sGauss[128] = 0;
//            gauss = Array.ConvertAll(sGauss, (elm) => (int)(elm));
            return gauss;
        }

        static int[] arr2;
        public static int[] Arr2Func()
        {
            int[] gauss1 = Array.ConvertAll(sGauss, (elm) => (int)(elm ));
            return gauss1;
        }
        public int[] Arr3Func()
        {
            int[] arr2 = new int[shft * 2];
            const int ExprmntCnt = 256000;
            for (int i = 0; i < ExprmntCnt; i++)
            {
                double tmp = maen + nRand12Old() * standardDeviation;//standardDeviation; // + это смещение матожидания; * это изменение среднеквадратического откланения
                average2 += tmp;
                averCount2++;
                int pos = (int)(tmp / stepX + shft);
                if (pos > 0 && pos < shft * 2)
                    arr2[pos]++;
            }

            WpfApp1.MainWindow.printOutput($"Ave={average / averCount}; ");
            WpfApp1.MainWindow.printOutput($"Ave2={average2 / averCount2}; ");

            for (int i = 0; i < arr2.Length; i++)
            {
                arr2[i] = (int)(arr2[i] / stepX / (ExprmntCnt / 10000));
            }
            return arr2;
        }
        public double[] signalAddAWGN() 
        {
            int ExprmntCnt = codeWord.Length;
            dispersOnDecoder = 0;
            for (int i = 0; i < ExprmntCnt; i++)
            {
                double tmp = maen + nRand12() * standardDeviation;//standardDeviation; // + это смещение матожидания; * это изменение среднеквадратического откланения
                codeWord[i] += tmp;
                dispersOnDecoder += tmp * tmp;
            }
            dispersOnDecoder /= ExprmntCnt;
//            WpfApp1.MainWindow.printOutput($"Dispersia={Math.Sqrt(dispersOnDecoder)}; when it was set to:{standardDeviation}");
            return codeWord;
        }
    }
}
