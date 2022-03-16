using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace wxworktest
{
    public static class HttpUrl
    {
        private static readonly HttpClient httpClient;


        static HttpUrl()
        {
            httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.84 Safari/537.36");
        }

        public static async Task<string> SendWxWorkPostData(this string data, string url)
        {

            ByteArrayContent request = new ByteArrayContent(Encoding.UTF8.GetBytes(data));

            request.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            HttpResponseMessage message = httpClient.PostAsync(url, request).Result;

            if (message.StatusCode == HttpStatusCode.OK)
            {
                ///成功
                return await message.Content.ReadAsStringAsync();
            }
            else
            {

                //return Task.FromResult("");

                return "";
            }
        }

        public async static Task<byte[]> GetBytesAsync(this string url)
        {
            try
            {
                return await httpClient.GetByteArrayAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }





    }
}