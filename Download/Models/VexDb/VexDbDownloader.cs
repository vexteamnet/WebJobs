using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Download.Models.VexDb
{
    public static class Downloader
    {
        public static async Task<ICollection<T>> Download<T>(string url)
        {
            if (!url.Contains("?"))
                url += "?";

#if DEBUG
            Debug.WriteLine($"Request is: {url}");
#endif
            int downloadIterations = 1; // number of iterations required
            const int downloadSize = 500; // number of T to be downloaded per request

            WebClient client = new WebClient();

            RootObject<T> rootObject = await await Task.Factory.StartNew(async () =>
            {
                var request = WebRequest.Create(url + "&nodata=true");
                using (Stream stream = (await request.GetResponseAsync()).GetResponseStream())
                    return JsonConvert.DeserializeObject<RootObject<T>>((new StreamReader(stream, Encoding.UTF8)).ReadToEnd());
            });
            // Determine how many download iterations to run through, rounding up
            downloadIterations = (rootObject.size - 1) / downloadSize + 1;

            // Create a collection
            var downloadStreams = new Collection<Task<WebResponse>>();
            for (int i = 0; i < downloadIterations; i++)
                downloadStreams.Add(WebRequest.Create(url + $"&limit_start={(i * downloadSize).ToString()}&limit_number={downloadSize.ToString()}").GetResponseAsync());

            Collection<T> collection = new Collection<T>();        

            while(downloadStreams.Count > 0)
            {
                Task<WebResponse> completedStream = await Task.WhenAny(downloadStreams);
                downloadStreams.Remove(completedStream);

                RootObject<T> rO = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RootObject<T>>(new StreamReader(completedStream.Result.GetResponseStream(), Encoding.UTF8).ReadToEnd()));
                foreach (var _t in rO.result)
                    collection.Add(_t);
            }

            return collection;
        }
    }
}
