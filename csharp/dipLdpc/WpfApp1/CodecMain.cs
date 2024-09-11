using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace dipLdpc
{
    class CodecMain
    {
        BackgroundWorker worker = new BackgroundWorker();
        internal CodecMain()
        {
            SNRNoise = new double[ConfigObj.allAppOpts.countNoise];
            bitErrorBefore = new double[ConfigObj.allAppOpts.countNoise];
            bitErrorAfter = new double[ConfigObj.allAppOpts.countNoise];
            worker.WorkerReportsProgress = true; worker.WorkerSupportsCancellation = true;
            worker.DoWork += backgroundWorker_DoWork;
            worker.ProgressChanged += backgroundWorker_ProgressChanged;
            worker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
        }
        double currentAmplitude = 1d;
        double stepAmplitude = 0.05;
        private static double STANDART_DEVIATION = 1;
        private static int CODEWORD_SIZE = 648;
        internal void start()
        {
            BeProData.Init();
            worker.RunWorkerAsync();
        }
        public static int compareBefore(int[] before, double[] after)
        {
            int  compare = 0;
            for (int i = 0; i < before.Length; i++)
            {
                int A = after[i] >= 0 ? 0 : 1;
                if (before[i] != A)
                {
                    compare++;
                }
            }
            return compare;
        }
        public static int compareAfter(int[] before, int[] after)
        {
            int compare = 0;
            for (int i = 0; i < before.Length; i++)
            {
                if (before[i] != after[i])
                {
                    compare++;
                }
            }
            return compare;
        }
        internal void GenCodeWordTest()
        {
            LdpcMatrixObj matrix = MatrixConverter.getMatrixFromConfig(ConfigObj.allAppOpts.LdpcMatrix);
            //int[][] standartMatrix = BeProData.convertStandartMatrix(BeProData.matrixF1Speed1);
            WordGen wg = new WordGen(matrix.standartMatrix, matrix.blovkSize);
            wg.generateMessage();
            wg.parities();
            wg.parseCodeWordToMatrix();
            int siRes = wg.calculateSindrom();
            if (siRes == 0)
            {
                WpfApp1.MainWindow.printOutput($"GOOD zero sindrom is OK!");
            }
            else
                WpfApp1.MainWindow.printOutput($"BAD {siRes}-count sindrom: ERROR!");


        }
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //            BeProData.Init();
            //            worker.RunWorkerAsync();

            LdpcMatrixObj matrix = MatrixConverter.getMatrixFromConfig(ConfigObj.allAppOpts.LdpcMatrix);
//            int[][] standartMatrix = BeProData.convertStandartMatrix(BeProData.matrixF1Speed1); 
            WordGen wg = new WordGen(matrix.standartMatrix, matrix.blovkSize);
            //           wg.generateMessage();
            //            wg.parities();
            List<List<SquareBlock>> shiftBlocks = wg.parseCodeWordToMatrix();
            Channel chn = new Channel(wg.getCodeWord(), currentAmplitude, STANDART_DEVIATION);

            BeProData bpAlg = new BeProData(matrix, ConfigObj.allAppOpts.CountOfIterations);

            int timeCnt = 0;
            DateTime[] times = new DateTime[ConfigObj.allAppOpts.countNoise + 1];
            times[timeCnt++] = DateTime.Now;
            for (int i = 0; i < ConfigObj.allAppOpts.countNoise; i++)
            {
                chn.SetAmplituda(currentAmplitude);
                SNRNoise[i] = 20 * Math.Log10(currentAmplitude * currentAmplitude / (STANDART_DEVIATION * STANDART_DEVIATION));
                for (int j = 0; j < ConfigObj.allAppOpts.countRealization; j++)
                {
                    string str = $"countNoise: {i}, countRealization: {j}";
                    WpfApp1.MainWindow.printOutput(str);
                    wg.generateMessage();
                    wg.parities();
                    chn.AddCodeWord(wg.getCodeWord());

                    double[] LLR = chn.convertToLLR();
/*                    int siRes = wg.calculateSindrom(LLR);
                    if (siRes == 0)
                    {
                        WpfApp1.MainWindow.printOutput($"GOOD zero sindrom is OK!");
                    }
                    else
                        WpfApp1.MainWindow.printOutput($"BAD {siRes}-count sindrom: ERROR!");*/

                    int cmpBefore = compareBefore(wg.getCodeWord(), LLR);
                    bitErrorBefore[i] = bitErrorBefore[i] + cmpBefore;
//                    WpfApp1.MainWindow.printOutput($"Errors count in input CodeWord= {cmpBefore };");
                    bpAlg.StartWithLLR(LLR);
                    int[] resultCodeWord = bpAlg.decode(shiftBlocks);
                    cmpBefore = compareAfter(wg.getCodeWord(), resultCodeWord);
//                    WpfApp1.MainWindow.printOutput($"Errors count in output CodeWord= {cmpBefore };");
                    bitErrorAfter[i] = bitErrorAfter[i] + cmpBefore;
                }
                bitErrorBefore[i] = bitErrorBefore[i] / ConfigObj.allAppOpts.countRealization / CODEWORD_SIZE;
                bitErrorAfter[i] = bitErrorAfter[i] / ConfigObj.allAppOpts.countRealization / CODEWORD_SIZE;

                currentAmplitude = currentAmplitude + stepAmplitude;
                times[timeCnt++] = DateTime.Now;
            }

            WpfApp1.MainWindow.printOutput($"Time of Start = {times[0]};");
            for (int i = 1; i < timeCnt;i++)
            {
                WpfApp1.MainWindow.printOutput($"Time of {i} Loop end = {times[i]};");
            }
        }
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //progressBar.Value = e.ProgressPercentage;
        }
//        const int countNoise = 20;
//        const int countRealization = 10;
        double[] SNRNoise;// = new double[countNoise];
        double[] bitErrorBefore;// = new double[countNoise];
        double[] bitErrorAfter;// = new double[countNoise];
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                //                ShowAllSids(this, null);
                //MessageBox.Show("Search cancelled.");
            }
            else if (e.Error != null)
            {
                // An error was thrown by the DoWork event handler.
                //MessageBox.Show(e.Error.Message, "An Error Occurred");
            }
            else
            {
                //ShowAllSids(this, null);
            }
            WpfApp1.MainWindow.makeGraph(bitErrorBefore, SNRNoise, bitErrorAfter);
            //            MainMenu.IsEnabled = true;
            //            progressBar.Value = 0;
        }

    }
}
