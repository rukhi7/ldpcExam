package com.ldpc.generator;

import java.util.ArrayList;
import java.util.List;

import static com.ldpc.utils.Utils.calculateVectorShift;
import static com.ldpc.utils.Utils.createZeroVector;

public class Generator {

    public static final int ZERO_MATRIX = -1;

    private int[][] ldpcMatrix;
    private int[][] splitMessage;
    private int[] message;
    private int[] codeWord;
    private int blockSize;
    private int messageSize;
    private int codewordSize;
    private int strings;
    private int columns;
    private double codeRate;

    public Generator(int[][] ldpcMatrix, int blockSize, int messageSize, int codewordSize) {
        this.blockSize = blockSize;
        this.messageSize = messageSize;
        this.codewordSize = codewordSize;
        this.codeRate = codewordSize / messageSize;
        this.ldpcMatrix = ldpcMatrix;
        this.columns = codewordSize / blockSize;
        this.strings = this.columns - messageSize / blockSize;
    }

    public void generateMessage() {
        double[] result = new double[messageSize];

        for (int i = 0; i < result.length; i++) {
            result[i] = Math.random();
        }

        message = new int[messageSize];
        for (int i = 0; i < messageSize; i++) {
            result[i] = Math.random();
            if (result[i] < 0.5) {
                message[i] = 0;
            } else {
                message[i] = 1;
            }
        }
    }

    private int[][] splitMessage() {
        int[][] splitMessage = new int[columns - strings][blockSize];
        int iterator = 0;
        for (int i = 0; i < splitMessage.length; i++) {
            for(int j = 0; j < splitMessage[0].length; j++) {
                splitMessage[i][j] = message[iterator];
                iterator++;
            }
        }
        return splitMessage;
    }

    private int[] calculateParity0() {
        splitMessage = splitMessage();
        int[] resultVector = createZeroVector(splitMessage[0].length);

        for (int i = 0; i < strings; i++) {
            for (int j = 0; j < columns - strings; j++) {
                resultVector = summVectorsModul2(resultVector, calculateVectorShift(ldpcMatrix[i][j], splitMessage[j]));
            }
        }
        return resultVector;
    }

    private int[][] calculateLamdas() {
        int[][] lamdas = new int[strings][blockSize];

        for (int i = 0; i < lamdas.length; i++) {
            lamdas[i] = createZeroVector(splitMessage[0].length);
        }

        for (int i = 0; i < strings; i++) {
            for (int j = 0; j < columns - strings; j++) {
                lamdas[i] = summVectorsModul2(lamdas[i], calculateVectorShift(ldpcMatrix[i][j], splitMessage[j]));
            }
        }
        return lamdas;
    }

    public List<int[]> parities() {
        int[] parity0 = calculateParity0();
        int[][] lamdas = calculateLamdas();
        List<int[]> parities = new ArrayList<>(strings);

        int[] parity1 = summVectorsModul2(lamdas[0], calculateVectorShift(1, parity0));

        int[] parityLast = summVectorsModul2(lamdas[strings - 1], calculateVectorShift(1, parity0));

        parities.add(0, parity0);
        parities.add(1, parity1);
        for (int i = 2; i < strings - 1; i++) {
            parities.add(i, new int[0]);
        }
        parities.add(strings - 1, parityLast);

        int tmp = strings / 2 - 1;
        for (int i = 1; i < tmp; i++) {
            parities.set(i + 1, summVectorsModul2(lamdas[i], parities.get(i)));
        }
        tmp++;

        for (int i = strings - 2; i > tmp; i--) {
            parities.set(i, summVectorsModul2(lamdas[i], parities.get(i + 1)));
        }

        parities.set(tmp, summVectorsModul2(summVectorsModul2(lamdas[tmp], parities.get(0)), parities.get(tmp + 1)));

        convertToCodeWord(parities);

        return parities;
    }

    private void convertToCodeWord(List<int[]> parities) {
        codeWord = new int[codewordSize];
        int iterator = -1;
        for (int i = 0; i < codewordSize; i++) {
            if (i < messageSize) {
                codeWord[i] = message[i];
            } else {
                if (i % blockSize == 0) {
                    iterator = iterator + 1;
                }
                codeWord[i] = parities.get(iterator)[(i - messageSize) % blockSize];
            }
        }
    }

    private int[] summVectorsModul2(int[] vector1, int[] vector2) {
        int[] result = new int[vector1.length];
        for (int i = 0; i < result.length; i++) {
            result[i] = vector1[i] ^ vector2[i];
        }
        return result;
    }

    public int[] getCodeword() {
        return codeWord;
    }
}
