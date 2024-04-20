using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class FileStreamHelper
    {
        public byte[] Read(string path, long offset, int length)
        {
            byte[] data=new byte[length];

            using (FileStream fs=new FileStream(path,FileMode.Open))
            {
                fs.Position = offset;
                int bytesRead = 0;
                while (bytesRead < length)
                {
                    int read = fs.Read(data, bytesRead, length - bytesRead);
                    if (read == 0)
                    {
                        throw new Exception("No se pudo leer el archivo");
                    }
                    bytesRead += read;
                }
                return data;
            }
        }


        public void Write(string fileName, byte[] data) { 
            if(File.Exists(fileName))
            {
                using(FileStream fs = new FileStream(fileName, FileMode.Append))
                {
                    fs.Write(data, 0, data.Length);
                }
            }
            else
            {
                using(FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    fs.Write(data,0,data.Length);
                }
            }
        }
    }
}
