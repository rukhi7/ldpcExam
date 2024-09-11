package com.ldpc.ruDecoder;

import org.python.google.common.primitives.Doubles;

import java.util.Arrays;

public class SquareBlock {
    public static int blkSz;
    Double[] LoadedMessage;
    int shft;
    int str;
    int column;
    Double[] DiagValues;

    public SquareBlock(int shft, int str, int column) {
//        this.message = message;
        this.shft = shft;
        this.str = str;
        this.column = column;
        LoadedMessage = new Double[blkSz];
    }

    public void LoadElement(int k, double[] message) {
        int indx = (shft + k) % blkSz;
        LoadedMessage[k] = message[column * blkSz + indx];
    }
    public double element(int k) {
        return LoadedMessage[k];
    }

    public void setArr(Double[] newLLR) {
        this.DiagValues = newLLR;
    }

    public void setArrPosition(int position, Double newLLR) {
        this.DiagValues[position] = newLLR;
    }

    public Double[] GetArr() {
        if(DiagValues == null) {
            DiagValues = new Double[blkSz];
        }
        else {
//            System.out.println("Error of SquareBlock value access!!!");
        }
        return DiagValues;
    }

    public double GetRightVal(int j) {
        int indx = ((blkSz - shft) + j) % blkSz;
        return DiagValues[indx];
    }

    public int GetShift() {
        return shft;
    }

    public void UpdateRightVal(int j, double xx) {
        int indx = ((blkSz - shft) + j) % blkSz;
        LoadedMessage[indx] = xx - DiagValues[indx];
    }

    public Double[] GetLoadedArr() {
//        Double[] inverse = Arrays.stream(LoadedMessage).boxed().toArray(Double[]::new);

        return LoadedMessage ;
    }

    public int GetCol() {
        return column;
    }
}
