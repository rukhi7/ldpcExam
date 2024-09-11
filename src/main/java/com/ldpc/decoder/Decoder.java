package com.ldpc.decoder;

import Jama.Matrix;

public class Decoder {

    private int[][] h;   // провероная матрица
    private int hStrings;   // количество строк проверочной матрицы
    private int hColumns;   // количество столбцов проверочной матрицы
    private int iterMax;    // ограничение количества итераций алгоритма

    public Decoder(int[][] h, int iterMax) {
        this.h = h;
        hStrings = this.h.length;
        hColumns = this.h[0].length;
        this.iterMax = iterMax;
    }

    private double[] matrixXvektor(double[][] matrix, double[] vector) {
        double[][] newVektor = new double[this.h[0].length][1];
        for (int j = 0; j < newVektor.length; j++) {
            newVektor[j][0] = vector[j];
        }

        Matrix a = new Matrix(matrix);
        Matrix b = new Matrix(newVektor);

        return a.times(b).getRowPackedCopy();
    }

    private double[][] llrMatrix(double[] r, double[][] h) {
        double[][] result = new double[h.length][h[0].length];
        for(int i = 0; i < result.length; i++) {
            for(int j = 0; j <result[0].length; j++) {
                result[i][j] = h[i][j] * r[j];
            }
        }
        return result;
    }

    private boolean checkToZero(double[] mass) {
        for (double i : mass) {
            if(i != 0) {
                return false;
            }
        }
        return true;
    }

    private double[] summMass(double[] mass1, double[] mass2) {
        double[] result = new double[mass1.length];
        for (int i = 0; i < mass1.length; i++) {
            result[i] = mass1[i] + mass2[i];
        }
        return result;
    }

    private double[] summStrings(double[][] matrix) {
        double[] result = new double[matrix[0].length];
        double sumOneColumn = 0;
        for(int i = 0; i < matrix[0].length; i++) {
            for(int j = 0; j <matrix.length; j++) {
                sumOneColumn = sumOneColumn + matrix[j][i];
            }
            result[i] = sumOneColumn;
            sumOneColumn = 0;
        }
        return result;
    }

    private double[] zeroVektor(int column) {
        double[] result = new double[column];
        for (int j = 0; j < column; j++) {
            result[j] = 0.;
        }
        return result;
    }

    private double[][] zeroMatrix(int string, int column) {
        double[][] result = new double[string][column];
        for (int k = 0; k < string; k++) {
            for (int j = 0; j < column; j++) {
                result[k][j] = 0.;
            }
        }
        return result;
    }

    private double[][] invers(double[][] h) {
        double[][] hInv = new double[h.length][h[0].length];
        for (int i = 0; i < hInv.length; i++) {
            for (int j = 0; j < hInv[0].length; j++) {
                hInv[i][j] = (h[i][j] + 1) % 2;
            }
        }
        return hInv;
    }

    private double[] threshold(double[] l) {
        for (int i = 0; i < l.length; i++) {
            if (l[i] >= 0) {
                l[i] = 0;
            } else {
                l[i] = 1;
            }
        }
        return l;
    }

    private double sumColumnElements(double[][] mass, int column) {
        double result = 0;
        for (int i = 0; i < mass.length; i++) {
            result = result + mass[i][column];
        }
        return result;
    }

    private double prodStringElements(double[] mass) {
        double result = 1;
        for (double i: mass) {
            result = result * i;
        }
        return result;
    }

    public double[][] swapCols(double[][] h, int[] columns) {
        double[][] Hstd = new double[h.length][h[0].length];
        for (int i = 0; i < h[0].length; i++) {
            for (int j = 0; j < h.length; j++) {
                Hstd[j][i] = (int) h[j][columns[i]];
            }
        }
        return Hstd;
    }
}

