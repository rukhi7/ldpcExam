package com.ldpc;

import com.ldpc.channel.Сhannel;
import com.ldpc.decoder.BeliefPropagation;
import com.ldpc.decoder.Decoder;
import com.ldpc.generator.Generator;
import com.ldpc.ruDecoder.BelPropV1;

import java.io.*;
import java.util.List;

import static com.ldpc.utils.Utils.*;


public class Main {

    private static double AMPLITUDA = 1;
    private static double STANDART_DEVIATION = 0.8;
    private static int MAEN = 0;

    public Main() throws IOException {
    }

    public static void main(String[] args) throws IOException {
        String matrix = "0 - - - 0 0 - - 0 - - 0 1 0 - - - - - - - - - -\n" +
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

        final int[][] standartMatrix = convertStandartMatrix(matrix, 324, 27, 648);

        Generator generator = new Generator(standartMatrix, 27, 324, 648);
        generator.generateMessage();
        generator.parities();
        int[] codeWord = generator.getCodeword();


        Сhannel channel = new Сhannel(codeWord, AMPLITUDA, STANDART_DEVIATION, MAEN);
        double[] LLR = channel.convertToLLR();

        if(false) {
            BufferedWriter writer = new BufferedWriter(new FileWriter("codeWordDisp0.4.txt"));
            for (int i = 0; i < codeWord.length; i++) {
                writer.write(String.valueOf(codeWord[i]));
                writer.write(",");
            }
            writer.close();

            FileWriter writerLLRs = new FileWriter("LLRsDisp0.4.txt", false);
            for (int i = 0; i < codeWord.length; i++) {
                writerLLRs.write(String.valueOf(LLR[i]));
                writerLLRs.write(",");
            }
            writerLLRs.close();
        }

        FileReader fr1 = new FileReader("codeWordDisp0.4.txt");
        BufferedReader reader1 = new BufferedReader(fr1);
        String line1 = null;
        while ((line1 = reader1.readLine()) != null) {
            int i = 0;
            for (String s : line1.split(",")) {
                codeWord[i] = Integer.parseInt(s);
                i++;
            }
        }
        fr1.close();

        FileReader fr = new FileReader("LLRsDisp0.4.txt");
            BufferedReader reader = new BufferedReader(fr);
            String line = null;
            while ((line = reader.readLine()) != null) {
                int i = 0;
                for (String s : line.split(",")) {
                    LLR[i] = Double.parseDouble(s);
                    i++;
                }
            }
        fr.close();

        //List<Integer> compareBefore = channel.compareSignals();
        List<Integer> compareBefore = compareBefore(codeWord, LLR);
        System.out.println("Ошибок ДО декодирования: " + compareBefore.size());

        boolean KoliKod = false;
        List<Integer> compareAfter;
        if(KoliKod) {
            Decoder standart = new Decoder(standartMatrix, 20);

            BeliefPropagation beliefPropagation = new BeliefPropagation(LLR, standartMatrix, 27, 25);

//            beliefPropagation.test(new int[]{0, 1, 0});
            compareAfter = compareAfter(codeWord, beliefPropagation.decode());
        }
        else
        {//papa kod
            {
                Decoder standart = new Decoder(standartMatrix, 20);

                BeliefPropagation beliefPropagation = new BeliefPropagation(LLR, standartMatrix, 27, 7);

//            beliefPropagation.test(new int[]{0, 1, 0});
                compareAfter = compareAfter(codeWord, beliefPropagation.decode());
                System.out.println("Ошибок ПОСЛЕ декодирования: " + compareAfter.size());
            }

            BelPropV1 beliefPropagation = new BelPropV1(LLR, standartMatrix, 27, 7);

//            beliefPropagation.test(new int[]{0, 1, 0});
//            beliefPropagation.decode();
            compareAfter = compareAfter(codeWord, beliefPropagation.decode());
            System.out.println("break");
        }
        System.out.println("Ошибок ПОСЛЕ декодирования: " + compareAfter.size());


        /*try(FileWriter writer = new FileWriter("codeWord.txt", false)) {
            for (int i = 0; i < codeWord.length; i++) {
                writer.write(String.valueOf(codeWord[i]));
                writer.write(",");
            }

        } catch (IOException e) {
            e.printStackTrace();
        }*/
    }

}
