package com.ldpc.utils;

import Jama.Matrix;

import java.util.ArrayList;
import java.util.List;
import java.util.stream.IntStream;

import static com.ldpc.generator.Generator.ZERO_MATRIX;

public class Utils {

    private static final String str = "0 - - - 0 0 - - 0 - - 0 1 0 - - - - - - - - - -\n" +
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
            "3 - - - 16 - - 2 25 5 - - 1 - - - - - - - - - - 0";

    public static int[][] convertStandartMatrix(String str, int messageSize, int blockSize, int codewordSize) {
        str = str.replace('\n', ' ');

        String[] massString = str.split("\\ ");
        int strings = messageSize / blockSize;
        int columns = codewordSize / blockSize;
        int[][] matrix = new int[strings][columns];
        int massLength = 0;
        for (int i = 0; i < strings; i++) {
            for (int j = 0; j < columns; j++) {
                if (massString[massLength].equals("-")) {
                    matrix[i][j] = ZERO_MATRIX;
                } else {
                    matrix[i][j] = Integer.parseInt(massString[massLength]);
                }
                massLength++;
            }
        }
        return matrix;
    }

    public static int[] calculateVectorShift(int shift, int[] vector) {
        if (shift == 0) {
            return vector;
        }
        if (shift == -1) {
            return createZeroVector(vector.length);
        }
        return IntStream.concat(
                        IntStream.range(shift, vector.length),
                        IntStream.range(0, shift)
                )
                .map(e -> vector[e])
                .toArray();
    }

    public static Double[] moveRight(int positions, Double[] array) {
        int size = array.length;
        Double[] result = new Double[array.length];
        for (int i = 0; i < array.length; i++) {
            result[i] = array[i];
        }
        for (int i = 0; i < positions; i++) {
            Double temp = result[size - 1];
            for (int j = size - 1; j > 0; j--) {
                result[j] = result[j-1];
            }
            result[0] = temp;
        }
        return result;
    }

    public static Double[] calculateVectorShift(int shift, Double[] vector) {
        double[] dVector = new double[vector.length];
        for (int i = 0; i < dVector.length; i++) {
            dVector[i] = vector[i];
        }
        if (shift == 0) {
            return vector;
        }
        if (shift == -1) {
            return createZeroDoubleVector(dVector.length);
        }

        double[] result = IntStream.concat(
                        IntStream.range(shift, dVector.length),
                        IntStream.range(0, shift)
                )
                .mapToDouble(e -> dVector[e])
                .toArray();

        Double[] rVector = new Double[vector.length];
        for (int i = 0; i < result.length; i++) {
            rVector[i] = result[i];
        }

        return rVector;
    }

    public static int[] createZeroVector(int size) {
        int[] resultVector = new int[size];
        for (int i = 0; i < resultVector.length; i++) {
            resultVector[i] = 0;
        }
        return resultVector;
    }

    public static Double[] createZeroDoubleVector(int size) {
        Double[] resultVector = new Double[size];
        for (int i = 0; i < resultVector.length; i++) {
            resultVector[i] = Double.valueOf(0);
        }
        return resultVector;
    }

    public static List<Integer> compareBeforeAfter(double[] before, double[] after) {
        List<Integer> compare = new ArrayList<>();
        double[] B = new double[before.length];
        double[] A = new double[after.length];
        for (int i = 0; i < before.length; i++) {
            if (before[i] > 0) {
                B[i] = 0;
            } else {
                B[i] = 1;
            }
            if (after[i] > 0) {
                A[i] = 1;
            } else {
                A[i] = 0;
            }
            if (B[i] == 0 && A[i] == 0 || B[i] == 1 && A[i] == 1) {
                continue;
            }
            compare.add(i);
        }
        return compare;
    }

    public static List<Integer> compareBefore(int[] before, double[] after) {
        List<Integer> compare = new ArrayList<>();
        double[] B = new double[before.length];
        double[] A = new double[after.length];
        for (int i = 0; i < before.length; i++) {
            if (before[i] == 1) {
                B[i] = 1;
            } else {
                B[i] = 0;
            }
            if (after[i] >= 0) {
                A[i] = 0;
            } else {
                A[i] = 1;
            }
            if (B[i] == 0 && A[i] == 0 || B[i] == 1 && A[i] == 1) {
                continue;
            }
            compare.add(i);
        }
        return compare;
    }
    public static List<Integer> compareInts(int[] before, int[] after) {
        List<Integer> compare = new ArrayList<>();
        for (int i = 0; i < before.length; i++) {
            if (before[i] == after[i]) {
                continue;
            }
            compare.add(i);
        }
        return compare;
    }
    public static List<Integer> compareAfter(int[] before, double[] after) {
        List<Integer> compare = new ArrayList<>();
        double[] B = new double[before.length];
        double[] A = new double[after.length];
        for (int i = 0; i < before.length; i++) {
            if (before[i] == 1) {
                B[i] = 1;
            } else {
                B[i] = 0;
            }
            if (after[i] == 1) {
                A[i] = 1;
            } else {
                A[i] = 0;
            }
            if (B[i] == 0 && A[i] == 0 || B[i] == 1 && A[i] == 1) {
                continue;
            }
            compare.add(i);
        }
        return compare;
    }

}
