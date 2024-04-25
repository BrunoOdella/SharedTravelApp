namespace Common
{
    public static class Protocol
    {
        public static int DataLengthSize = 4;// vamos a usar 4 bytes para enviar el largo del mensaje
        public static int DirectionLength = 3;
        public static int CommandLength = 2;
        public static string Request = "REQ";
        public static string Response = "RES";

        public static int fileNameLengthSize = 4;
        public static int fileSizeLength = 8; // long
        public static int MaxPartSize = 32768;

        public static long numberOfParts(long fileLength)
        {
            long numberOfParts = fileLength / MaxPartSize;
            if (numberOfParts * MaxPartSize != fileLength)
                numberOfParts++;
            return numberOfParts;
        }
    }
}