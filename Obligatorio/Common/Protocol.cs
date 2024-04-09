namespace Common
{
    public static class Protocol
    {
        public static int DataLengthSize = 4;// vamos a usar 4 bytes para enviar el largo del mensaje
        public static int DirectionLength = 3;
        public static int CommandLength = 2;
        public static string Request = "REQ";
        public static string Response = "RES";

    }
}