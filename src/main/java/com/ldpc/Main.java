package com.ldpc;

import com.ldpc.channel.Сhannel;
import com.ldpc.generator.Generator;
import com.ldpc.ruDecoder.BelPropV1;
import javazoom.jl.decoder.JavaLayerException;

import java.io.*;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

import static com.ldpc.utils.MatrixConstants.*;
import static com.ldpc.utils.Utils.*;


public class Main {

    private static double AMPLITUDA = 1;
    private static double STANDART_DEVIATION = 1;
    private static int MAEN = 0;

    /*private static int BLOCK_SIZE = 27;
    private static int MESSAGE_SIZE = 540;
    private static int CODEWORD_SIZE = 648;*/

    private static int COUNT_APPROX = 1;
    private static int COUNT_SIZE_CODEWORD = 3;

    private static int countNoise = 20;
    private static int countRealization = 1000;
    private static double stepAmplitude = 0.025;

    public static void main(String[] args) throws IOException, JavaLayerException {

        List<String> matrixes = new ArrayList<>();
        matrixes.add(matrixF1Speed1);
        matrixes.add(matrixF2Speed1);
        matrixes.add(matrixF3Speed1);

        String[] algSPAForDifferentSizes = new String[COUNT_SIZE_CODEWORD];
        String SNRForDifferentSizes = new String();

        for (int countCodeWord = 0; countCodeWord < COUNT_SIZE_CODEWORD; countCodeWord++) {

            final int[][] standartMatrix = convertStandartMatrix(
                    matrixes.get(countCodeWord), MESSAGE_SIZE_SPEED1[countCodeWord],
                    BLOCK_SIZE_SPEED1[countCodeWord], CODEWORD_SIZE_SPEED1[countCodeWord]);

            double currentAmplitude = AMPLITUDA;

            int[] bitErrorRateBeforeDecodeNoise = new int[countNoise];
            int[] bitErrorRateAfterDecodeNoise = new int[countNoise];
            double[] SNRNoise = new double[countNoise];

            int[] bitErrorRateBeforeDecodeRealization = new int[countRealization];
            int[] bitErrorRateAfterDecodeRealization = new int[countRealization];

            double[] bitErrorBefore = new double[countNoise];
            double[] bitErrorAfter = new double[countNoise];

            double[] packegErrorBefore = new double[countNoise];
            double[] packegErrorAfter = new double[countNoise];

            double[][] packegErrorAfterMass = new double[3][countNoise];

            Date date = new Date();
            SimpleDateFormat formatter = new SimpleDateFormat("dd-MM-yyyy HH:mm:ss");

            Date[] times = new Date[COUNT_APPROX];
            for (int countAlg = 0; countAlg < COUNT_APPROX; countAlg++) {
                currentAmplitude = AMPLITUDA;
                for (int i = 0; i < countNoise; i++) {
                    SNRNoise[i] = 10 * Math.log10((currentAmplitude * currentAmplitude) / (STANDART_DEVIATION * STANDART_DEVIATION));
                    for (int j = 0; j < countRealization; j++) {
                        System.out.println("Alg: " + countAlg + ", countNoise: " + i + ", countRealization: " + j);
                        Generator generator = new Generator(
                                standartMatrix, BLOCK_SIZE_SPEED1[countCodeWord], MESSAGE_SIZE_SPEED1[countCodeWord], CODEWORD_SIZE_SPEED1[countCodeWord]);
                        generator.generateMessage();
                        generator.parities();
                        int[] codeWord = generator.getCodeword();


                        Сhannel channel = new Сhannel(codeWord, currentAmplitude, STANDART_DEVIATION, MAEN);
                        double[] LLR = channel.convertToLLR();


                        List<Integer> compareBefore = compareBefore(codeWord, LLR);
                        bitErrorBefore[i] = bitErrorBefore[i] + compareBefore.size();
                        if (compareBefore.size() > 0) {
                            packegErrorBefore[i] += 1;
                        }

                        bitErrorRateBeforeDecodeRealization[j] = compareBefore.size();
                        //System.out.println("Ошибок ДО декодирования: " + compareBefore.size());

                        BelPropV1 beliefPropagation = new BelPropV1(LLR, standartMatrix, BLOCK_SIZE_SPEED1[countCodeWord], 25);

                        List<Integer> compareAfter = compareAfter(codeWord, beliefPropagation.decode(countAlg));
                        bitErrorAfter[i] = bitErrorAfter[i] + compareAfter.size();
                        if (compareAfter.size() > 0) {
                            packegErrorAfter[i] += 1;
                        }

                        bitErrorRateAfterDecodeRealization[j] = compareAfter.size();
                        //System.out.println("break");

                        //System.out.println("Ошибок ПОСЛЕ декодирования: " + compareAfter.size());
                    }
                    bitErrorBefore[i] = bitErrorBefore[i] / countRealization / CODEWORD_SIZE_SPEED1[countCodeWord];
                    bitErrorAfter[i] = bitErrorAfter[i] / countRealization / CODEWORD_SIZE_SPEED1[countCodeWord];

                    packegErrorBefore[i] = packegErrorBefore[i] / countRealization;
                    packegErrorAfter[i] = packegErrorAfter[i] / countRealization;

                    bitErrorRateBeforeDecodeNoise[i] = arithmeticMeanOfArray(bitErrorRateBeforeDecodeRealization, countRealization);
                    bitErrorRateAfterDecodeNoise[i] = arithmeticMeanOfArray(bitErrorRateAfterDecodeRealization, countRealization);
                    currentAmplitude = currentAmplitude + stepAmplitude;
                }
                for (int l = 0; l < countNoise; l++) {
                    packegErrorAfterMass[countAlg][l] = packegErrorAfter[l];
                }
                times[countAlg] = new Date();
            }
            Date date1 = new Date();
            System.out.println("Время до: " + formatter.format(date));
            System.out.println("Время работы первого алгоритма: " + formatter.format(times[0]));
            //System.out.println("Время работы первого алгоритма: " + formatter.format(times[1]));
            //System.out.println("Время работы первого алгоритма: " + formatter.format(times[2]));
            System.out.println("Время после: " + formatter.format(date1));
            ///////////////////////////////////////////////////////////

            String algSPA = convertMassive(packegErrorAfterMass[0]);
            String algSignMinApprox = convertMassive(packegErrorAfterMass[1]);
            String algTable = convertMassive(packegErrorAfterMass[2]);
            //////////////////////////////////////////////////////////

            algSPAForDifferentSizes[countCodeWord] = algSPA;

            String bitErrorRateBefore = convertMassive(bitErrorBefore);
            String bitErrorRateAfter = convertMassive(bitErrorAfter);

            String packegErrorRateBefore = convertMassive(packegErrorBefore);
            String packegErrorRateAfter = convertMassive(packegErrorAfter);

            String bitErrorRateBeforeDecodeString = convertMassive(bitErrorRateBeforeDecodeNoise);
            String bitErrorRateAfterDecodeString = convertMassive(bitErrorRateAfterDecodeNoise);
            String SNRString = convertMassive(SNRNoise);

            SNRForDifferentSizes = SNRString;

        }

        graphBuilder(SNRForDifferentSizes, algSPAForDifferentSizes[0], algSPAForDifferentSizes[1], algSPAForDifferentSizes[2],
                "648 bit", "1296 bit", "1944 bit", "rate 1/2");

        soudPlay();
       /* String[] cmd = {
                "python",
                "C:/Users/Asus/Downloads/pythonLDPC (3).py",
                "-x",
                SNRForDifferentSizes,
                "-y",
                algSPAForDifferentSizes[0],//algSPA,
                "-y2",
                algSPAForDifferentSizes[1],//algSignMinApprox,
                "-y3",
                algSPAForDifferentSizes[2],//algTable
        };
        try {
            Runtime.getRuntime().exec(cmd);
        } catch (IOException e) {
            e.printStackTrace();
        }*/

    }

}
