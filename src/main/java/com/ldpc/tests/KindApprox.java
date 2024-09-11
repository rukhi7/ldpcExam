package com.ldpc.tests;

import com.ldpc.channel.Сhannel;
import com.ldpc.generator.Generator;
import com.ldpc.ruDecoder.BelPropV1;
import javazoom.jl.decoder.JavaLayerException;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;

import static com.ldpc.utils.MatrixConstants.*;
import static com.ldpc.utils.Utils.*;
import static com.ldpc.utils.Utils.convertMassive;

public class KindApprox {

    private static double AMPLITUDA = 1.3;
    private static double STANDART_DEVIATION = 1;
    private static int MAEN = 0;

    private static int BLOCK_SIZE = 54;
    private static int MESSAGE_SIZE = 864;
    private static int CODEWORD_SIZE = 1296;

    private static int COUNT_APPROX = 3;
    private static int COUNT_SIZE_CODEWORD = 3;

    private static int COUNT_ITERRATIONS = 30;

    private static int countNoise = 16;
    private static int countRealization = 10000;
    private static double stepAmplitude = 0.04;

    public static void main(String[] args) throws IOException, JavaLayerException {

        final int[][] standartMatrix = convertStandartMatrix(
                matrixF2Speed2, MESSAGE_SIZE,
                BLOCK_SIZE, CODEWORD_SIZE);

        double currentAmplitude;

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
                            standartMatrix, BLOCK_SIZE, MESSAGE_SIZE, CODEWORD_SIZE);
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

                    BelPropV1 beliefPropagation = new BelPropV1(LLR, standartMatrix, BLOCK_SIZE, COUNT_ITERRATIONS);

                    List<Integer> compareAfter = compareAfter(codeWord, beliefPropagation.decode(countAlg));
                    bitErrorAfter[i] = bitErrorAfter[i] + compareAfter.size();
                    if (compareAfter.size() > 0) {
                        packegErrorAfter[i] += 1;
                    }

                    bitErrorRateAfterDecodeRealization[j] = compareAfter.size();
                }
                bitErrorBefore[i] = bitErrorBefore[i] / countRealization / CODEWORD_SIZE;
                bitErrorAfter[i] = bitErrorAfter[i] / countRealization / CODEWORD_SIZE;

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
        System.out.println("Время работы первого алгоритма: " + formatter.format(times[1]));
        System.out.println("Время работы первого алгоритма: " + formatter.format(times[2]));
        System.out.println("Время после: " + formatter.format(date1));
        ///////////////////////////////////////////////////////////

        String algSPA = convertMassive(packegErrorAfterMass[0]);
        String algSignMinApprox = convertMassive(packegErrorAfterMass[1]);
        String algTable = convertMassive(packegErrorAfterMass[2]);
        //////////////////////////////////////////////////////////

        String SNRString = convertMassive(SNRNoise);


        graphBuilder(SNRString, algSPA, algSignMinApprox, algTable,
                "SPA", "sign-min approx", "table look-up", "N = " + CODEWORD_SIZE + ", K = " + MESSAGE_SIZE);

        soudPlay();

    }

}
