package com.ldpc.decoder;

import java.util.ArrayList;
import java.util.List;

import static com.ldpc.utils.Utils.*;

public class BeliefPropagation {

    private double[] message;
    private int[][] matrix;
    private int columns;
    private int string;
    private int iterations;
    private int blockSize;

    public BeliefPropagation(double[] message, int[][] matrix, int blockSize, int iterations) {
        this.message = message;
        this.matrix = matrix;
        this.columns = matrix[0].length;
        this.string = matrix.length;
        this.iterations = iterations;
        this.blockSize = blockSize;
    }

    public double[] decode() {

        List<List<Double[]>> shiftBlocks = parseCodeWordToMatrix();
        int iter = 0;
        double[] correctedCodeWord = new double[message.length];
        List<Integer> compareAfter = new ArrayList<>();

        while (iter < iterations) {

            List<List<Double[]>> newLLR = iterationOfBeliefPropagation(shiftBlocks);

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
            }

            double[] summNewOldLLRs = summArrays(message, summLLRsForOneVariable(test));

            compareAfter = compareBeforeAfter(message, summNewOldLLRs);

            //int[] sindrom1 = calculateSindromTEST(shiftBlocks, newLLR);
            int[] sindrom = calculateSindrom(shiftBlocks, summNewOldLLRs);
            correctedCodeWord = threshold(summNewOldLLRs);

            List<Integer> compare = compareBeforeAfter(message, correctedCodeWord);
            System.out.println("Sindrom: " + checkToZero(sindrom));
            if (checkToZero(sindrom)) {
                System.out.println("Колличество итераций алгоритма: " + (iter + 1));
                System.out.println("Выход из алгоритма по синдрому");
                return correctedCodeWord;
            }
            shiftBlocks = calcNewShiftBlocks(shiftBlocks, test);
            iter++;
        }
        System.out.println("Колличество итераций алгоритма: " + iter);
        System.out.println("Выход из алгоритма по колличеству итерраций");
        return correctedCodeWord;
    }

    private List<List<Double[]>> calcNewShiftBlocks(List<List<Double[]>> shiftBlocks, List<List<Double[]>> newLLR) {

        List<List<Double[]>> newShiftBlocks = new ArrayList<>();
        for (int i = 0; i < string; i++) {
            List<Double[]> newList = new ArrayList<>();
            for (int j = 0; j < columns; j++) {
                Double[] newDouble = new Double[blockSize];

                if (shiftBlocks.get(i).get(j).length > 0) {

                    Double[] vektor = new Double[blockSize];
                    for (int l = 0; l < blockSize; l++) {
                        vektor[l] = message[l + j * blockSize];
                    }
                    Double[] vektorShift = calculateVectorShift(matrix[i][j], vektor);
                    for (int k = 0; k < blockSize; k++) {
                        newDouble[k] = sumColumnElements(newLLR, j, k, i) + vektor[k];
                    }
                    newList.add(calculateVectorShift(matrix[i][j], newDouble));
                } else {
                    newList.add(new Double[0]);
                }
            }
            newShiftBlocks.add(newList);
        }
        return newShiftBlocks;
    }

    private double sumColumnElements(List<List<Double[]>> newLLR, int column, int elementInBlock, int string) {
        double result = 0;

        for (int i = 0; i < this.string; i++) {
            if (i == string) {
                continue;
            }
            Double[] vektorShift = moveRight(matrix[i][column], newLLR.get(i).get(column));

            if (newLLR.get(i).get(column).length > 0) {
                result = result + vektorShift[elementInBlock];
            }
        }
        return result;
    }

    private double[] summLLRsForOneVariable(List<List<Double[]>> newLLR) {

        double[] summ = new double[message.length];

        for (int i = 0; i < columns; i++) {
            List<Double[]> oneColumn = new ArrayList<>();
            for(int j = 0; j < string; j++) {
                if (newLLR.get(j).get(i).length > 0) {
                    Double[] shiftBack = moveRight(matrix[j][i], newLLR.get(j).get(i));                //newLLR.get(j).get()) { //shiftBlocks.get(j).get(i).length > 0) {
                    oneColumn.add(shiftBack);
                }
            }
            for(int j = 0; j < blockSize; j++) {
                summ[j + i * blockSize] = 0;
                for(int k = 0; k < oneColumn.size(); k++) {
                    int check = j + i * blockSize;
                    summ[j + i * blockSize] = summ[j + i * blockSize] + oneColumn.get(k)[j];
                }
            }
        }

        return summ;
    }

    private int[] calculateSindrom(List<List<Double[]>> shiftBlocks, double[] newLLR) {

        List<List<Integer>> countElementOfString = new ArrayList<>();

        for (int i = 0; i < string; i++) {
            List<Integer> list;
            list = calcNumbersOfCheckNodes(shiftBlocks.get(i));
            countElementOfString.add(list);
        }

        int[] sindrom = new int[string * blockSize];
        for (int i = 0; i < string; i++) {
            for (int k = 0; k < blockSize; k++) {
                //sindrom[k + i * blockSize] = 0;
                for (int j = 0; j < countElementOfString.get(i).size(); j++) {
                    Double[] mass  = new Double[blockSize];
                    for (int z = 0; z < blockSize; z++) {
                        mass[z] = newLLR[countElementOfString.get(i).get(j) * blockSize + z];
                    }
                    mass = calculateVectorShift(matrix[i][countElementOfString.get(i).get(j)], mass);
                    if (mass[k] > 0) {
                        sindrom[k + i * blockSize] = sindrom[k + i * blockSize] ^ 0;
                    } else {
                        sindrom[k + i * blockSize] = sindrom[k + i * blockSize] ^ 1;
                    }
                    //sindrom[k + i * blockSize] = sindrom[k + i * blockSize] + mass[k]);
                }
            }
        }

        return sindrom;
    }

    private int[] calculateSindromTEST(List<List<Double[]>> shiftBlocks, List<List<Double[]>> newLLR) {

        List<List<Integer>> countElementOfString = new ArrayList<>();

        for (int i = 0; i < string; i++) {
            List<Integer> list;
            list = calcNumbersOfCheckNodes(shiftBlocks.get(i));
            countElementOfString.add(list);
        }

        int[] sindrom = new int[string * blockSize];
        for (int i = 0; i < string; i++) {
            for (int k = 0; k < blockSize; k++) {
                for (int j = 0; j < countElementOfString.get(i).size(); j++) {
                    if (newLLR.get(i).get(j)[k] > 0) {//newLLR.get(j + iterator).get(k) > 0) {
                        sindrom[k + i * blockSize] = sindrom[k + i * blockSize] ^ 0;
                    } else {
                        sindrom[k + i * blockSize] = sindrom[k + i * blockSize] ^ 1;
                    }
                }
                /*for (int j = 0; j < countElementOfString.get(i).size(); j++) {
                    if (newLLR.get(i).get(j).length > 0 ) {
                        if (newLLR.get(i).get(j)[k] > 0) {//newLLR.get(j + iterator).get(k) > 0) {
                            sindrom[k + i * blockSize] = sindrom[k + i * blockSize] ^ 0;
                        } else {
                            sindrom[k + i * blockSize] = sindrom[k + i * blockSize] ^ 1;
                        }
                    }
                }*/
            }
        }

        return sindrom;
    }

    private List<List<Double[]>> iterationOfBeliefPropagation(List<List<Double[]>> shiftBlocks) {
        List<List<Double[]>> newLLR = new ArrayList<>();
        for (int i = 0; i < shiftBlocks.size(); i++) {
            List<Integer> checkNodes = calcNumbersOfCheckNodes(shiftBlocks.get(i));

            List<Double[]> newStringLLR = new ArrayList<>();
            for (int j = 0; j < checkNodes.size(); j++) {
                //List<Double[]> newStringLLR = new ArrayList<>();
                Double[] element = new Double[blockSize];

                List<Integer> checkNodesWithoutNodForCorrect = new ArrayList<>(checkNodes);
                checkNodesWithoutNodForCorrect.remove(j);
                for (int k = 0; k < blockSize; k++) {
                    Double newElementLLR = new Double(0);
                    for (int l = 0; l < checkNodesWithoutNodForCorrect.size() - 1; l++) {
                        if (l == 0) {
                            newElementLLR = Math.log((1 + Math.exp(shiftBlocks.get(i).get(checkNodesWithoutNodForCorrect.get(l))[k] + shiftBlocks.get(i).get(checkNodesWithoutNodForCorrect.get(l + 1))[k]))
                                    / (Math.exp(shiftBlocks.get(i).get(checkNodesWithoutNodForCorrect.get(l))[k]) + Math.exp(shiftBlocks.get(i).get(checkNodesWithoutNodForCorrect.get(l + 1))[k])));
                        } else {
                            newElementLLR = Math.log((1 + Math.exp(newElementLLR + shiftBlocks.get(i).get(checkNodesWithoutNodForCorrect.get(l + 1))[k]))
                                    / (Math.exp(newElementLLR) + Math.exp(shiftBlocks.get(i).get(checkNodesWithoutNodForCorrect.get(l + 1))[k])));
                        }
                    }
                    //newStringLLR.add(newElementLLR);
                    element[k] = newElementLLR;
                }
                newStringLLR.add(element);
                //element[j] = newStringLLR;

            }
            newLLR.add(newStringLLR);
        }
        return newLLR;
    }

    private List<List<Double[]>> parseCodeWordToMatrix() {
        List<List<Double[]>> shiftBlocks = new ArrayList<>();
        for (int i = 0; i < matrix.length; i++) {
            Double[] shiftBlocksInString = new Double[blockSize];
            List<Double[]> listString = new ArrayList<>();
            for (int j = 0; j < matrix[0].length; j++) {
                if (matrix[i][j] == -1) {
                    listString.add(new Double[0]);
                } else {
                    shiftBlocksInString = valueToShiftBlock(i, j);
                    listString.add(shiftBlocksInString);
                }
            }
            shiftBlocks.add(listString);
        }
        return shiftBlocks;
    }

    private List<Integer> calcNumbersOfCheckNodes(List<Double[]> list) {
        List<Integer> result = new ArrayList<>();
        for (int i = 0; i < list.size(); i++) {
            if (list.get(i).length != 0) {
                result.add(i);
            }
        }
        return result;
    }

    public void test(int[] mass) {
        //int[][] m = new int[3][3];
        calculateVectorShift(1, mass);
    }

    private Double[] valueToShiftBlock(int string, int column) {
        Double[] result = new Double[blockSize];
        for (int i = blockSize * column; i < blockSize * column + blockSize; i++) {
            result[i - blockSize * column] = message[i];
        }
        return calculateVectorShift(matrix[string][column], result);
    }

    private int countOfNotEmptyElementsInString(int string) {
        int count = 0;
        for (int i = 0; i < matrix[0].length; i++) {
            if (matrix[string][i] != -1) {
                count++;
            }
        }
        return count;
    }

    private boolean checkToZero(int[] mass) {
        for (int i : mass) {
            if(i != 0) {
                return false;
            }
        }
        return true;
    }

    private double[] summArrays(double[] mass1, double[] mass2) {
        double[] result = new double[mass1.length];
        for (int i = 0; i < mass1.length; i++) {
            result[i] = mass1[i] + mass2[i];
        }
        return result;
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

}



























