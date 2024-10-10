namespace MGDFClone.Core {
    public static class Extensions {
        public static int MapFloatToRange(float value, int minInt, int maxInt) {
            int mappedValue = (int)(minInt + value * (maxInt - minInt));
            return mappedValue;
        }
    }
}
