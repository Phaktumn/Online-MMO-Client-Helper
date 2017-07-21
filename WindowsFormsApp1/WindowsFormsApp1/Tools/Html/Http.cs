using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Data.Darkorbit;
using WindowsFormsApp1.Data.Factory;

namespace WindowsFormsApp1.Tools.Html
{
	class Http : IDisposable
	{
		public delegate void OnHttpResponseError();
		public event OnHttpResponseError OnHttpResponseErrorEvent;

	    private HttpRequestMessage request;
	    private HttpResponseMessage response;

	    private Task<HttpResponseMessage> responseTask;

	    private HttpClient client;

	    private bool awaitingResponse;

	    private Uri lastUri;

        public CookieContainer Cookies;
	    public AccountFactory Factory;

        public Http() 
		{
            Cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler {
                CookieContainer = Cookies
            };

            lastUri = new UriBuilder("https://www.darkorbit.bigpoint.com").Uri;

            client = new HttpClient(handler);

            client.DefaultRequestHeaders.Connection.Add("Keep-Alive");

            //Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8
		    client.DefaultRequestHeaders.Add("Accept",
				"text/html,application/xhtml+xml,application/x-www-form-urlencoded,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");

            //Accept - Encoding: gzip, deflate, br
		    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

            //Accept-Language: pt-PT,pt;q=0.8,en-US;q=0.6,en;q=0.4
		    client.DefaultRequestHeaders.Add("Accept-Language", "pt-PT,pt;q=0.8,en-US;q=0.6,en;q=0.4");

            //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36
		    client.DefaultRequestHeaders.Add("User-Agent",
		        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36");

		    //client.DefaultRequestHeaders.Accept.Add(
		    //    new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
		}


        public async Task<HttpResponseMessage> GET(Uri uri, string host = null)
        {
            WaitUntilRequestPossible();

            client.DefaultRequestHeaders.Host = host;
            client.DefaultRequestHeaders.Referrer = lastUri;
            responseTask = client.GetAsync(uri, HttpCompletionOption.ResponseContentRead);
            lastUri = uri;
            response = await responseTask;
	        Console.WriteLine(
		        $@"Code: {response.StatusCode} {(int)response.StatusCode}");
			return response;
        }

	    private async Task<HttpResponseMessage> POST(Uri uri, HttpContent content)
	    {
            WaitUntilRequestPossible();

	        client.DefaultRequestHeaders.Referrer = uri;
            responseTask = client.PostAsync(uri, content);
            lastUri = uri;
            response = await responseTask;
			Console.WriteLine(
				$@"Code: {response.StatusCode} {response.StatusCode.GetType().GetEnumName(response.StatusCode)}");
			return response;
	    }

        public async Task<HttpResponseMessage> Login(Uri uri, string username, string password)
        {
            client.DefaultRequestHeaders.Host = "auth3.bpsecure.com";
            var content = new StringContent($"username={username}&password={password}",
                Encoding.UTF8, "application/x-www-form-urlencoded");
            return await POST(uri, content);
        }

        //https://int7.darkorbit.com/ajax/shop.php
        //action=purchase&category=rocket&itemId=ammunition_rocketlauncher_eco-10&amount=1000&level=-1&selectedName=
        public async Task<HttpResponseMessage> BuyAmmo(string category, string ammoName, int ammount)
        {

            Uri uri = new UriBuilder("https://int7.darkorbit.com/ajax/shop.php").Uri;
            string contentString =
                $"action=purchase&category={category}&itemId={ammoName}&amount={ammount}&level=-1&selectedName=";
            var content = new StringContent(contentString, Encoding.UTF8, "application/x-www-form-urlencoded");

            client.DefaultRequestHeaders.Referrer =
                new Uri("https://int7.darkorbit.com/indexInternal.es?action=internalDock&tpl=internalDockAmmo");
            client.DefaultRequestHeaders.Host = "int7.darkorbit.com";
            return await POST(uri, content);
        }

        //GET /flashinput/galaxyGates.php?userID=90140621&sid=9e052481361323b0053243fc39479887&action=saveSpinAmount&value=5
        //GET /flashinput/galaxyGates.php?userID=90140621&sid=9e052481361323b0053243fc39479887&action=saveSpinAmount&value=1
        //GET /flashinput/galaxyGates.php?userID=90140621&action=multiEnergy&sid=9e052481361323b0053243fc39479887&gateID=7&kappa=1
        //GET /flashinput/galaxyGates.php?userID=90140621&action=multiEnergy&sid=0f00ad46cacb5b87aa31ff8b4dff9213&gateID=7&kappa=1
        //GET /flashinput/galaxyGates.php?userID=90140621&action=init&sid=8b28d5df019ab57c924253529f9f66ae
        //    /flashinput/galaxyGates.php?userID=90140621&action=multiEnergy&sid=3a82c9e4779e8bf9c4815884ebbde965&gateID=7&kappa=1&spinamount=5
        public async Task<HttpResponseMessage> GalaxyGateSpin(GalaxyGate gate, int numberOfSpins)
        {
            //await SetGalaxyGatesSpin();
            var secondUri =
                new UriBuilder("https://int7.darkorbit.com/flashinput/galaxyGates.php?" +
                               $"userID={Factory.LoggedAccount.ID}" +
							   "&action=multiEnergy" +
                               $"&sid={Factory.LoggedAccount.DOSID}" +
                               $"&gateID={gate.Id}" +
                               $"&{gate.Name.ToLower()}=1" +
                               $"&spinamount={numberOfSpins}").Uri;

            await GET(secondUri, "int7.darkorbit.com");
            return response;
        }

		public async Task<HttpResponseMessage> SetGalaxyGatesSpin() 
		{
			var firstUri =
				new UriBuilder("https://int7.darkorbit.com/flashinput/galaxyGates.php?" +
				   $"userID={Factory.LoggedAccount.ID}" +
				   $"&sid={Factory.LoggedAccount.DOSID}" +
				   "&action=saveSpinAmount" +
				   $"&value={1}").Uri;
            // Referer: https://int7.darkorbit.com/indexInternal.es?action=internalGalaxyGates
		    lastUri = new UriBuilder("https://int7.darkorbit.com/indexInternal.es?" +
		                             "action=internalGalaxyGates").Uri;
		    await GET(firstUri, "int7.darkorbit.com");
			return response;
		}

	    public async Task<string[]> GetGalaxyGatesInfo()
	    {
	        var firstUri =
	            new UriBuilder("https://int7.darkorbit.com/flashinput/galaxyGates.php?" +
                               $"userID={Factory.LoggedAccount.ID}" +
	                           "&action=init" +
	                           $"&sid={Factory.LoggedAccount.DOSID}").Uri;
            lastUri = new UriBuilder("https://int7.darkorbit.com/indexInternal.es?" +
                                     "action=internalGalaxyGates").Uri;
            await GET(firstUri, "int7.darkorbit.com");
	        return HtmlUtilities.GetGatesInfoFromHtmlString(await GetHtmlText());
	    }


        //GET /indexInternal.es?action=internalMapRevolution&dontShow=1
        //Host: int7.darkorbit.com
        public async Task<HttpResponseMessage> OpenSpaceMap()
        {
            var uri =
                new UriBuilder("https://int7.darkorbit.com/indexInternal.es?" +
                               "action=internalMapRevolution" +
                               "&dontShow=1").Uri;
           return await GET(uri, "int7.darkorbit.com");
        }

	    public string GetResponseHeader(string header)
        {
            IEnumerable<string> values;
            if (response.Headers.TryGetValues(header, out values))
            {
                return values.FirstOrDefault();
            }
            return string.Empty;
        }

        public Task<string> GetResponseBody()
	    {
	        return GetHtmlText();
	    }
        
        private async Task<string> GetHtmlText()
        {
            WaitUntilRequestPossible();

			if (response == null || response.StatusCode == HttpStatusCode.InternalServerError) 
			{
                return string.Empty;
            }

			GZipStream gZipReader = null;
            StreamReader reader = null;
            try
            {
				gZipReader = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
                reader = new StreamReader(gZipReader);
            }
            catch (Exception e)
            {
	            Console.WriteLine($@"[ERROR]{e.GetType()} -> {e.Message}");
                return string.Empty;
            }
	        bool isGzip = true;
            string html = null;
			try
			{
			    html = reader.ReadToEnd();
			}
			catch (Exception)
			{
			    Console.WriteLine(@"Not a gzip Stream");
			    isGzip = false;
			}

			if (isGzip == false) 
			{
				try
				{
					reader = new StreamReader(await response.Content.ReadAsStreamAsync(), true);
					using (reader) {

						html = reader.ReadToEnd();
					}
				}
				catch (Exception e) {
					Console.WriteLine($@"[ERROR]{e.GetType()} -> {e.Message} {e.Source}");
					return null;
				}
			}
			Console.WriteLine(reader.CurrentEncoding);
			gZipReader.Dispose();
			reader.Dispose();

			return html;
        }

	    private void WaitUntilRequestPossible()
	    {
	        if (responseTask == null)
                return;

            while (true)
            {
	            if (responseTask.Status != TaskStatus.Running) {
	                break;
	            }
                Thread.Sleep(100);
            }

	        if (responseTask.Status == TaskStatus.RanToCompletion)
	            Thread.Sleep(500);
	    }

	    public void Dispose()
	    {
	        client.Dispose();
	        request.Dispose();
	        response.Dispose();
	    }
	}
}
