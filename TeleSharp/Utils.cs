using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using RestSharp;

namespace TeleSharp
{
    internal class Utils
    {
        /// <summary>
        /// Generate RestRequest using given http informations
        /// </summary>
        /// <param name="resource">Method name of action</param>
        /// <param name="method">Determine action method (post, get or etc)</param>
        /// <param name="headers">HTTP headers</param>
        /// <param name="parameters">HTTP parameters</param>
        /// <param name="files">Files that must upload using HTTP request</param>
        /// <returns>Generated RestRequest</returns>
        public static RestRequest GenerateRestRequest(string resource, Method method,
            Dictionary<string, string> headers = null,
            Dictionary<string, object> parameters = null, List<Tuple<string, byte[], string>> files = null)
        {
            var request = new RestRequest(resource, method)
            {
                RootElement = "result"
            };

            if (headers != null)
                foreach (KeyValuePair<string, string> header in headers)
                    request.AddHeader(header.Key, header.Value);

            if (parameters != null)
                foreach (KeyValuePair<string, object> parameter in parameters)
                    request.AddParameter(parameter.Key, parameter.Value);

            if (files != null)
                foreach (Tuple<string, byte[], string> file in files)
                    request.AddFile(file.Item1, file.Item2, file.Item3);

            return request;
        }

        /// <summary>
        /// Calculcate MD5 of given buffer
        /// </summary>
        /// <param name="fileBuffer">Buffer that MD5 of it must calculate</param>
        /// <returns>MD5 of buffer</returns>
        public static string ComputeFileMd5Hash(byte[] fileBuffer)
        {
            if ((fileBuffer == null) || (!fileBuffer.Any()))
                return null;

            try
            {
                using (var md5 = MD5.Create())
                {
                    byte[] crcBytes = md5.ComputeHash(fileBuffer);
                    return BitConverter.ToString(crcBytes).Replace("-", string.Empty);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}