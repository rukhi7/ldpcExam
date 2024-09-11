package com.ldpc.channel;

import org.apache.commons.math3.complex.Complex;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class Сhannel {

    private int[] codeWord;
    private double zero;
    private double one;
    private double standardDeviation;
    private double maen;
    private double dispersOnDecoder;
    private double[] noise;
    private double[] resultSignal;

    public Сhannel(int[] codeWord, double zero, double standardDeviation, double maen) {
        this.codeWord = codeWord;
        this.zero = zero;
        this.one = - zero;
        this.standardDeviation = standardDeviation;
        this.maen = maen;
        this.noise = generateAWGN();
    }

    public double[] convertCodeWordToSignal() {
        double[] signal = new double[codeWord.length];
        for (int i = 0; i < codeWord.length; i++) {
            if (codeWord[i] == 0) {
                signal[i] = zero;
            } else {
                signal[i] = one;
            }
        }
        return signal;
    }

    private double[] generateAWGN() {
        Random random = new Random();
        double[] gauss = new double[codeWord.length];
        for (int i = 0; i < gauss.length; i++) {
            gauss[i] = maen + random.nextGaussian() * standardDeviation;//standardDeviation; // + это смещение матожидания; * это изменение среднеквадратического откланения
        }
        return gauss;
    }

    public static double[] generateComplexNoise() {
        List<Complex> noise = new ArrayList<>();
        Random rnd = new Random();
        double[] random = new double[10000000];
        double[] reley = new double[random.length];
        for (int i = 0; i < random.length; i++) {
            random[i] = 3 * Math.random();
            reley[i] = Math.sqrt(Math.pow((0 + rnd.nextGaussian() * 1), 2) + Math.pow((0 + rnd.nextGaussian() * 1), 2));
        }
        return reley;
    }

    private double[] signalAddAWGN() {
        double[] signal = convertCodeWordToSignal();
        double[] result = new double[signal.length];
        for (int i = 0; i < result.length; i++) {
            result[i] = signal[i] + noise[i];
        }
        dispersOnDecoder = calculateDisp(noise, 0);
        return result;
    }

    public double[] convertToLLR() {
        double[] LLR = new double[codeWord.length];
        double signalE = 2 * Math.abs(one - zero); // полагаем, что Eb = E0 = E1
        resultSignal = signalAddAWGN();
        double threshold = (one + zero) / 2;
        for (int i = 0; i < LLR.length; i++) {
            LLR[i] = (signalE / (standardDeviation * standardDeviation)) * ((resultSignal[i] - threshold) / (zero - one)); // дисперсию брать из расчитанной приемником
            //LLR[i] = - codeWord[i];
        }
        return LLR;
    }

    public List<Integer> compareSignals() {
        List<Integer> compare = new ArrayList<>();
        double[] nNoise = convertCodeWordToSignal();
        for (int i = 0; i < nNoise.length; i++) {
            if (nNoise[i] < 0 && resultSignal[i] < 0 || nNoise[i] > 0 && resultSignal[i] > 0) {
                continue;
            }
            compare.add(i);
        }
        return compare;
    }

    public static double calculateDisp(double[] signal, int maen) {
        double disp = 0;
        for (int i = 0; i < signal.length; i++) {
            disp += Math.pow((signal[i] - maen), 2);
        }
        return disp / signal.length;
    }
}
