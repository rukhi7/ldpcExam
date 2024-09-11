package com.ldpc.decoder;

public class QuantElement {

    public static QuantElement[] array = {new QuantElement(0.196, 0.65),
                                        new QuantElement(0.433, 0.55),
                                        new QuantElement(0.71, 0.45),
                                        new QuantElement(1.05, 0.35),
                                        new QuantElement(1.508, 0.25),
                                        new QuantElement(2.252, 0.15),
                                        new QuantElement(4.5, 0.05),
                                        new QuantElement(Double.MAX_VALUE, 0)};
    public double x;
    public double y;

    public QuantElement(double x, double y) {
        this.x = x;
        this.y = y;
    }

    public static double myLog(double x) {
        for (int i = 0; i < array.length; i++) {
            if (x < array[i].x) {
                return array[i].y;
            }
        }
        return 0;
    }


}
