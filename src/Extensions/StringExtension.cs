using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTemplate.Extensions
{
    public static class StringExtension
    {
        public static (string, Exception) ToJson(this object obj)
        {
            if (obj == null)
            {
                return (string.Empty, new ArgumentNullException(nameof(obj)));
            }

            try
            {
                return (JsonConvert.SerializeObject(obj), null);
            }
            catch (Exception e)
            {
                return (string.Empty, e);
            }
        }

        public static (T, Exception) ToObj<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return (default, null);
            }

            try
            {
                return (JsonConvert.DeserializeObject<T>(json), null);
            }
            catch (Exception e)
            {
                return (default, e);
            }
        }

        public static (string, Exception) Md5(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return (input, null);
            }

            try
            {
                using (var md5Hash = MD5.Create())
                {
                    // Convert the input string to a byte array and compute the hash.
                    var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                    // Create a new Stringbuilder to collect the bytes
                    // and create a string.
                    var sBuilder = new StringBuilder();

                    // Loop through each byte of the hashed data 
                    // and format each one as a hexadecimal string.
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }

                    // Return the hexadecimal string.
                    return (sBuilder.ToString(), null);
                }
            }
            catch (Exception e)
            {
                return (input, e);
            }
        }

        public static (string, Exception) Sha1(this string content)
        {
            try
            {
                using (var sha1 = SHA1.Create())
                {
                    return (BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(content))).Replace("-", ""), null);
                }
            }
            catch (Exception e)
            {
                return (content, e);
            }
        }

        #region gzip
        /// <summary>
        /// GZip 解压
        /// </summary>
        /// <param name="zippedString"></param>
        /// <returns></returns>
        public static (string, Exception) GZipDecompress(this string zippedString)
        {
            if (string.IsNullOrWhiteSpace(zippedString))
            {
                return (string.Empty, null);
            }

            var (bytes, e) = GZipDecompress(Convert.FromBase64String(zippedString));
            if (e != null)
            {
                return (string.Empty, e);
            }

            return (Encoding.UTF8.GetString(bytes), null);
        }

        /// <summary>
        /// GZip 解压
        /// </summary>
        /// <param name="zippedData"></param>
        /// <returns></returns>
        public static (byte[], Exception) GZipDecompress(byte[] zippedData)
        {
            try
            {
                using (var stream = new GZipStream(new MemoryStream(zippedData), CompressionMode.Decompress))
                {
                    const int size = 4096;
                    var buffer = new byte[size];
                    using (var memory = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if (count > 0)
                            {
                                memory.Write(buffer, 0, count);
                            }
                        }
                        while (count > 0);
                        return (memory.ToArray(), null);
                    }
                }
            }
            catch (Exception e)
            {
                return (new byte[0], e);
            }
        }

        public static (string, Exception) GZipCompress(this string rawString)
        {
            if (string.IsNullOrWhiteSpace(rawString))
            {
                return (string.Empty, null);
            }

            var (bytes, e) = GZipCompress(Encoding.UTF8.GetBytes(rawString));
            if (e != null)
            {
                return (string.Empty, e);
            }

            return (Convert.ToBase64String(bytes), e);
        }

        private static (byte[], Exception) GZipCompress(byte[] rawData)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true);
                    gzipStream.Write(rawData, 0, rawData.Length);
                    gzipStream.Close();
                    return (memoryStream.ToArray(), null);
                }
            }
            catch (Exception e)
            {
                return (new byte[0], e);
            }
        }
        #endregion
    }
}
