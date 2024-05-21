using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class FileStreamHelper
    {
        public async Task<byte[]> ReadAsync(string path, long offset, int length)
        {
            byte[] data = new byte[length];

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                fs.Position = offset;
                int bytesRead = 0;
                while (bytesRead < length)
                {
                    int read = await fs.ReadAsync(data, bytesRead, length - bytesRead);
                    if (read == 0)
                    {
                        throw new Exception("No se pudo leer el archivo");
                    }
                    bytesRead += read;
                }
                return data;
            }
        }

        public async Task WriteAsync(string fileName, byte[] data)
        {
            if (File.Exists(fileName))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await fs.WriteAsync(data, 0, data.Length);
                }
            }
            else
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await fs.WriteAsync(data, 0, data.Length);
                }
            }
        }
    }
}
