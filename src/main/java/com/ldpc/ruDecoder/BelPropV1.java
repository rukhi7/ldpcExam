package com.ldpc.ruDecoder;

import com.ldpc.decoder.QuantElement;

import java.util.ArrayList;
import java.util.List;
import static com.ldpc.utils.Utils.*;
public class BelPropV1 {
    private double[] message;
    private int[][] matrix;
    private int columns;
    private int string;
    private int iterations;
    private int blockSize;

    public BelPropV1(double[] message, int[][] matrix, int blockSize, int iterations) {
        this.message = message;
        this.matrix = matrix;
        this.columns = matrix[0].length;
        this.string = matrix.length;
        this.iterations = iterations;
        this.blockSize = blockSize;
    }

    private List<List<SquareBlock>> parseCodeWordToMatrix() {
        List<List<SquareBlock>> shiftBlocks = new ArrayList<>();
        for (int i = 0; i < matrix.length; i++) {
            List<SquareBlock> listString = new ArrayList<>();
            for (int j = 0; j < matrix[0].length; j++) {
                if (matrix[i][j] == -1) {
 //                   listString.add(null);
                } else {
//                    shiftBlocksInString = valueToShiftBlock(i, j);
                    SquareBlock shiftBlocksInString = new SquareBlock(matrix[i][j], i, j);

                    listString.add(shiftBlocksInString);
                }
            }
            shiftBlocks.add(listString);
        }
        return shiftBlocks;
    }

    public double[] decode(int alg) {
        SquareBlock.blkSz = blockSize;
        List<List<SquareBlock>> shiftBlocks = parseCodeWordToMatrix();
        LoadWordToTable(shiftBlocks);
        int iter = 0;
        double[] correctedCodeWord = new double[message.length];
        List<Integer> compareAfter = new ArrayList<>();

        while (iter < iterations) {

            List<List<SquareBlock>> newLLR = iterationOfBeliefPropagationClassic(shiftBlocks, alg);

            double[] summNewOldLLRs = summLLRsForOneVariable(newLLR);

            int[] sindrom1 = calculateSindrom(shiftBlocks, summNewOldLLRs);
            correctedCodeWord = threshold(summNewOldLLRs);
            int[] sindrom = calculateSindrom(shiftBlocks, correctedCodeWord);

            int diffCount = compareInts(sindrom, sindrom1);
            if (checkToZero(sindrom)) {
                return correctedCodeWord;
            }
//            break;
            iter++;
            shiftBlocks = newLLR;
        }

        //System.out.println("Колличество итераций алгоритма: " + iter);
        //System.out.println("Выход из алгоритма по колличеству итерраций");
        return correctedCodeWord;

    }
    private boolean checkToZero(int[] mass) {
        for (int i : mass) {
            if(i != 0) {
                return false;
            }
        }
        return true;
    }
    private int[] calculateSindrom(List<List<SquareBlock>> shiftBlocks, double[] correctedCodeWord) {

        int[] sindrom = new int[string * blockSize];
        for (int i = 0; i < shiftBlocks.size(); i++) {

            for (int k = 0; k < blockSize; k++) {
                int ssum = 0;
                double mass;
                for (int j = 0; j < shiftBlocks.get(i).size(); j++) {
                    SquareBlock blk =  shiftBlocks.get(i).get(j);
                    int shft = (blk.GetShift() + k) % blockSize;
                    int col = blk.GetCol();
                    mass = correctedCodeWord[shft + col * blockSize];
                    if (mass > 0) {
                        ssum ^= 1;
                    } else {
                        ssum ^= 0;
                    }
                }
                sindrom[k + i * blockSize] = ssum;
            }
        }

        return sindrom;
    }

        private double[] threshold(double[] l) {
        for (int i = 0; i < l.length; i++) {
            if (l[i] > 0) {
                l[i] = 0;
            } else {
                l[i] = 1;
            }
        }
        return l;
    }
    private double[] summLLRsForOneVariable(List<List<SquareBlock>> newLLR) {

        double[] summ = new double[message.length];
        int[] everyLinePos = new int[newLLR.size()];
        assert (newLLR.size() == string);
        for (int i = 0; i < columns; i++) {

            for (int j = 0; j < blockSize; j++) {
                double xx = 0;
                for (int k = 0; k < newLLR.size(); k++) {
                    int inLinePos = everyLinePos[k];
                        if(newLLR.get(k).size() > inLinePos ) {
                            if (newLLR.get(k).get(inLinePos).column == i) {
                                xx += newLLR.get(k).get(inLinePos).GetRightVal(j); //GetArr()[j];

                            }
                        }
                }

                xx += message[j + i * blockSize];
                summ[j + i * blockSize] = xx;
                for (int k = 0; k < newLLR.size(); k++) {
                    int inLinePos = everyLinePos[k];
                    if(newLLR.get(k).size() > inLinePos ) {
                        if (newLLR.get(k).get(inLinePos).column == i) {
                            newLLR.get(k).get(inLinePos).UpdateRightVal(j, xx); //GetArr()[j];

                        }
                    }
                }
            }

            for (int k = 0; k < newLLR.size(); k++) {
                if(newLLR.get(k).size() > everyLinePos[k])
                    if (newLLR.get(k).get(everyLinePos[k]).column == i) {
                        everyLinePos[k] ++;
                    }
            }
        }
        return summ;
    }

    private List<List<SquareBlock>> iterationOfBeliefPropagationClassic(List<List<SquareBlock>> shiftBlocks, int alg) {

        for (int i = 0; i < shiftBlocks.size(); i++) {
            int j = 0;
            List<SquareBlock> line = shiftBlocks.get(i);


            for (int m = 0; m < blockSize; m++) {

                int lineSize = line.size();
                int lSize = 3 * (lineSize - 2) - (lineSize - 4);
                double[] ls = new double[lSize];
                ls[0] = line.get(0).LoadedMessage[m];
                ls[lSize / 2] = line.get(lineSize - 1).LoadedMessage[m];
                Double[] newLLRs = new Double[lineSize];
                for (int l = 1; l < lineSize - 1; l++) {
                    ls[l] = coreOfIterationOptimize(line.get(l).element(m), ls[l - 1], alg);// coreOfIterationSign(line, m, l, l + 1, ls[l - 1], alg);
                }
                int count = lSize / 2 - 1;
                for (int l = lSize / 2 + 1; l < lSize; l++) {
                    ls[l] = coreOfIterationOptimize(line.get(count).element(m), ls[l - 1], alg);
                    count--;
                }
                newLLRs[0] = ls[lSize - 1];
                newLLRs[lineSize - 1] = ls[lSize / 2 - 1];

                for (int l = 1; l < lineSize - 1; l++) {
                    newLLRs[l] = coreOfIterationOptimize(ls[lSize - (l + 1)], ls[l - 1], alg);
                }
                for (int y = 0; y < lineSize; y++) {
                    line.get(y).GetArr();
                    line.get(y).setArrPosition(m, newLLRs[y]);
                }
            }
        }
        return shiftBlocks;
    }

    private double coreOfIteration(double newElementLLR, double yy) {
        return Math.log((1 + Math.exp(newElementLLR + yy))
                / (Math.exp(newElementLLR) + Math.exp(yy)));
    }

    private double coreOfIterationOptimize(double one, double two, int alg) {
        double result = 0;
        if (alg == 0) {
            result = Math.log((1 + Math.exp(one + two)) / (Math.exp(one) + Math.exp(two)));                  // SPA
        }
        if (alg == 1) {
            result = Math.signum(one) * Math.signum(two) * Math.min(Math.abs(one), Math.abs(two));           // sign-min approx
            //+ Math.log((1 + Math.exp(newElementLLR + yy))) - Math.log((Math.exp(newElementLLR) + Math.exp(yy)));
        }
        if (alg == 2) {
            result = Math.max(0, one + two) - Math.max(one, two)                                             // table look-up
                    + QuantElement.myLog(Math.abs(one + two)) - QuantElement.myLog(Math.abs(one - two));
        }

        return result;
    }

    private double coreOfIterationSign(List<SquareBlock> line, int k, int startIndex, int endIndx, double newElementLLR, int alg) {
        for (int l = startIndex; l < endIndx; l++) {
            double yy = line.get(l).element(k);
            if (alg == 0) {
                newElementLLR = Math.log((1 + Math.exp(newElementLLR + yy)) / (Math.exp(newElementLLR) + Math.exp(yy)));                  // SPA
            }
            if (alg == 1) {
                newElementLLR = Math.signum(newElementLLR) * Math.signum(yy) * Math.min(Math.abs(newElementLLR), Math.abs(yy));           // sign-min approx
                //+ Math.log((1 + Math.exp(newElementLLR + yy))) - Math.log((Math.exp(newElementLLR) + Math.exp(yy)));
            }
            if (alg == 2) {
                newElementLLR = Math.max(0, newElementLLR + yy) - Math.max(newElementLLR, yy)                                             // table look-up
                        + QuantElement.myLog(Math.abs(newElementLLR + yy)) - QuantElement.myLog(Math.abs(newElementLLR - yy));
            }
        }
        return newElementLLR;
    }

    private List<List<SquareBlock>> LoadWordToTable(List<List<SquareBlock>> shiftBlocks) {

        for (int i = 0; i < shiftBlocks.size(); i++) {
            List<SquareBlock> line = shiftBlocks.get(i);
            int j = 0;

            for (int k = 0; k < blockSize; k++) {
                line.get(1).LoadElement(k, message);
                for (int l = 2; l < line.size(); l++) {
                    line.get(l).LoadElement(k, message);
                }
            }
            for (j = 1; j < line.size(); j++) {
                for (int k = 0; k < blockSize; k++) {
                    line.get(0).LoadElement(k, message);

                    for (int l = 1; l < j; l++) {
                        line.get(l).LoadElement(k, message);
                    }

                    for (int l = j + 1; l < line.size(); l++) {
                        line.get(l).LoadElement(k, message);
                    }
                }
            }
        }
        return shiftBlocks;
    }
    }
