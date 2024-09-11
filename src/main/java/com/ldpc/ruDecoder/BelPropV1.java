package com.ldpc.ruDecoder;

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

    public double[] decode() {

        List<List<SquareBlock>> shiftBlocks = parseCodeWordToMatrix();
        LoadWordToTable(shiftBlocks);
        int iter = 0;
        double[] correctedCodeWord = new double[message.length];
        List<Integer> compareAfter = new ArrayList<>();

        while (iter < iterations) {

            List<List<SquareBlock>> newLLR = iterationOfBeliefPropagation(shiftBlocks);
/*
            List<List<Double[]>> testShift = parseCodeWordToMatrix();
            List<List<Double[]>> test = new ArrayList<>(testShift);
            for (int i = 0; i < test.size(); i++) {
                int iterator = 0;
                for (int j = 0; j < test.get(0).size(); j++) {
                    if (test.get(i).get(j).length > 0) {
                        for (int k = 0; k < test.get(i).get(j).length; k++) {
                            test.get(i).get(j)[k] = newLLR.get(i).get(iterator)[k];
                        }
                    } else {
                        iterator--;
                    }
                    iterator++;
                }
            }*/

            double[] summNewOldLLRs = summLLRsForOneVariable(newLLR);

            //int[] sindrom1 = calculateSindromTEST(shiftBlocks, newLLR);
            int[] sindrom1 = calculateSindrom(shiftBlocks, summNewOldLLRs);
            correctedCodeWord = threshold(summNewOldLLRs);
            int[] sindrom = calculateSindrom(shiftBlocks, correctedCodeWord);
//            correctedCodeWord = threshold(summNewOldLLRs);
//            List<Integer> compare = compareBeforeAfter(message, correctedCodeWord);
            List<Integer> compare = compareInts(sindrom, sindrom1);
            System.out.println("Sindrom: " + checkToZero(sindrom));
            if (checkToZero(sindrom)) {
                System.out.println("Колличество итераций алгоритма: " + (iter + 1));
                System.out.println("Выход из алгоритма по синдрому");
                return correctedCodeWord;
            }
//            break;
            iter++;
            shiftBlocks = newLLR;
        }

        System.out.println("Колличество итераций алгоритма: " + iter);
        System.out.println("Выход из алгоритма по колличеству итерраций");
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
                        ssum ^= 0;
                    } else {
                        ssum ^= 1;
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
                l[i] = 1;
            } else {
                l[i] = 0;
            }
        }
        return l;
    }
    static int statPrint = 0;
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
                                /*if (statPrint == 0)
                                {
                                    double dbg = newLLR.get(k).get(inLinePos).GetRightVal(j);
                                    System.out.println("Sum+: " + dbg);
                                }*/
                            }
                        }
                }
                statPrint = 1;
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
        /*Double[] vektor = newLLR.get(0).get(0).GetLoadedArr();
        for (int l = 0; l < blockSize; l++) {
            double dbg = vektor[l];
            System.out.println("vSum+: " + dbg);
        }*/
        return summ;
    }
        private List<List<SquareBlock>> iterationOfBeliefPropagation(List<List<SquareBlock>> shiftBlocks) {

        for (int i = 0; i < shiftBlocks.size(); i++) {
            int j = 0;
            //List<Double[]> newStringLLR = new ArrayList<>();
            Double[] element = shiftBlocks.get(i).get(j).GetArr();

            for (int k = 0; k < blockSize; k++) {
                double newElementLLR = shiftBlocks.get(i).get(1).element(k, message);
                for (int l = 2; l < shiftBlocks.get(i).size(); l++) {
                    double yy = shiftBlocks.get(i).get(l).element(k, message);
                    newElementLLR = Math.log((1 + Math.exp(newElementLLR + yy))
                            / (Math.exp(newElementLLR) + Math.exp(yy)));
                }
                //newStringLLR.add(newElementLLR);
                element[k] = newElementLLR;
            }
            for (j = 1; j < shiftBlocks.get(i).size(); j++) {
                //List<Double[]> newStringLLR = new ArrayList<>();
                element = shiftBlocks.get(i).get(j).GetArr();

                for (int k = 0; k < blockSize; k++) {
                    double newElementLLR = shiftBlocks.get(i).get(0).element(k, message);

                    for (int l = 1; l < j; l++) {
                        double yy = shiftBlocks.get(i).get(l).element(k, message);
                        newElementLLR = Math.log((1 + Math.exp(newElementLLR + yy))
                                / (Math.exp(newElementLLR) + Math.exp(yy)));
                    }

                    for (int l = j + 1; l < shiftBlocks.get(i).size(); l++) {
                        double yy = shiftBlocks.get(i).get(l).element(k, message);
                        newElementLLR = Math.log((1 + Math.exp(newElementLLR + yy))
                                / (Math.exp(newElementLLR) + Math.exp(yy)));
                    }
                    //newStringLLR.add(newElementLLR);
                    element[k] = newElementLLR;
                }
            }
        }
        return shiftBlocks;
    }
    private List<List<SquareBlock>> LoadWordToTable(List<List<SquareBlock>> shiftBlocks) {

        for (int i = 0; i < shiftBlocks.size(); i++) {
            int j = 0;

            for (int k = 0; k < blockSize; k++) {
                shiftBlocks.get(i).get(1).LoadElement(k, message);
                for (int l = 2; l < shiftBlocks.get(i).size(); l++) {
                    shiftBlocks.get(i).get(l).LoadElement(k, message);
                }
            }
            for (j = 1; j < shiftBlocks.get(i).size(); j++) {
                for (int k = 0; k < blockSize; k++) {
                    shiftBlocks.get(i).get(0).LoadElement(k, message);

                    for (int l = 1; l < j; l++) {
                        shiftBlocks.get(i).get(l).LoadElement(k, message);
                    }

                    for (int l = j + 1; l < shiftBlocks.get(i).size(); l++) {
                        shiftBlocks.get(i).get(l).LoadElement(k, message);
                    }
                }
            }
        }
        return shiftBlocks;
    }
    }
