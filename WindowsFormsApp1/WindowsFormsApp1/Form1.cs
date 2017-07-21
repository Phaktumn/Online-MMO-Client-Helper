using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using WindowsFormsApp1.Data;
using WindowsFormsApp1.Data.Factory;
using WindowsFormsApp1.Tools;
using WindowsFormsApp1.Tools.Extensions;
using WindowsFormsApp1.Tools.Html;
using static WindowsFormsApp1.Data.GameSettings;
using WindowsFormsApp1.Data.Darkorbit;
using WindowsFormsApp1.Properties;
using WindowsFormsApplication7;
using Emgu.CV;
using Emgu.CV.Structure;
using MiscUtil;

namespace WindowsFormsApp1
{
	public partial class Form1 : Form
	{
		private readonly FileIO io;

		private AccountFactory accountFactory;
		private bool accountBrowserNavigating = false;

		private Http http;
		private Factory<Item> itemFactory;
		private DarkOrbitClient client;
		private List<GalaxyGate> galaxyGates = new List<GalaxyGate>();
		private bool premium = false;
		private CaptureScreen cs;
		private SynchronizationContext mainThreadContext;
		private Dictionary<string, Image> images = new Dictionary<string, Image>();

		public Form1()
		{
			InitializeComponent();

			Start.ToggleActive();
			Stop.ToggleActive();

			mainThreadContext = SynchronizationContext.Current;
			accountFactory = new AccountFactory();
			itemFactory = new Factory<Item>();

			images.Add("bonusbox", Image.FromFile(
				@"C:\Users\DecaPod\Documents\visual studio 2017\Projects\WindowsFormsApp1\WindowsFormsApp1\Resources\box.bmp"));

			//images.Add("jump", Image.FromFile(
			//	@"C:\Users\DecaPod\Documents\visual studio 2017\Projects\WindowsFormsApp1\WindowsFormsApp1\Resources\jump.bmp"));

			images.Add("minimap",
				Image.FromFile(
					@"C:\Users\DecaPod\Documents\visual studio 2017\Projects\WindowsFormsApp1\WindowsFormsApp1\Resources\minimap.bmp"));

			cs = new CaptureScreen();

			http = new Http
			{
				Factory = accountFactory
			};
			http.OnHttpResponseErrorEvent += () =>
			{
				Console.WriteLine(@"Logged Of");
			};

			io = new FileIO("Test.json");
			io.Initialize();
			var accounts = io.GetAccounts("account");
			if (accounts != null)
			{
				foreach (var account in accounts)
				{
					accountFactory.AddData(account);
				}
			}
			io.AddDataGroup("info");
			io.AddDataGroup("account");

			SavedAccountsComboBox.SelectionChangeCommitted += (sender, args) =>
			{
				var selected = (Account) SavedAccountsComboBox.SelectedItem;
				UsernameText.Text = selected.Nick;
				PassText.Text = selected.Password;
			};

			SavedAccountsComboBox.Items.AddRange(accountFactory.GetAllDataValuesAsArray());

			Closing += (sender, args) =>
			{
				io.WriteAsync();
			};

			itemFactory.AddData(new Ammo("lcb-10", 10, CurrencyTypes.Credits, ItemSubTypes.laser, Category.battery));
			itemFactory.AddData(new Ammo("eco-10", 1500, CurrencyTypes.Credits, ItemSubTypes.rocketlauncher, Category.rocket));

			SetupDataGridView();

			galaxyGates.Add(new GalaxyGate("Alpha", 1, 34, 0));
			galaxyGates.Add(new GalaxyGate("Beta", 2, 48, 0));
			galaxyGates.Add(new GalaxyGate("Gamma", 3, 82, 0));
			galaxyGates.Add(new GalaxyGate("Delta", 4, 128, 0));
			galaxyGates.Add(new GalaxyGate("Epsilon", 5, 99, 0));
			galaxyGates.Add(new GalaxyGate("Zeta", 6, 11, 0));
			galaxyGates.Add(new GalaxyGate("Kappa", 7, 120, 0));
			galaxyGates.Add(new GalaxyGate("Lambda", 8, 45, 0));
			galaxyGates.Add(new GalaxyGate("Hades", 13, 45, 0));
			galaxyGates.Add(new GalaxyGate("Kuiper", 19, 100, 0));

			GatesComboBox.Items.AddRange(items: galaxyGates.ToArray());
			spinAmmountCb.Items.AddRange(new object[] {1, 5, 10, 100});

		    LabelPremium.ForeColor = Color.Red;
		    LabelPremium.Text = @"Not Logged";

			pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			WebBrowserExtensions.ClearCache();
            webBrowser1.Navigate("https://www.darkorbit.bigpoint.com");
		}

		private async Task SetupGates()
		{
		    var info = await http.GetGalaxyGatesInfo();

		    var counter = 0;
		    foreach (var s in info)
		    {
		        ushort converted;
		        ushort.TryParse(s, out converted);
		        galaxyGates[counter++].CurrentParts = converted;
		    }

			AlphaPartsLabel.Text = $@"{galaxyGates[0].CurrentParts}/{galaxyGates[0].TotalParts}";
			LabelBetaParts.Text = $@"{galaxyGates[1].CurrentParts}/{galaxyGates[1].TotalParts}";
			LabelGammaParts.Text = $@"{galaxyGates[2].CurrentParts}/{galaxyGates[2].TotalParts}";
			LabelDeltaPart.Text = $@"{galaxyGates[3].CurrentParts}/{galaxyGates[3].TotalParts}";
			LabelEpsilonParts.Text = $@"{galaxyGates[4].CurrentParts}/{galaxyGates[4].TotalParts}";
			LabelZetaParts.Text = $@"{galaxyGates[5].CurrentParts}/{galaxyGates[5].TotalParts}";
			LabelKappaParts.Text = $@"{galaxyGates[6].CurrentParts}/{galaxyGates[6].TotalParts}";
			LabelLambdaParts.Text = $@"{galaxyGates[7].CurrentParts}/{galaxyGates[7].TotalParts}";
			LabelHadesParts.Text = $@"{galaxyGates[8].CurrentParts}/{galaxyGates[8].TotalParts}";
			LabelKuiperParts.Text = $@"{galaxyGates[9].CurrentParts}/{galaxyGates[9].TotalParts}";
		}

		private void SetupDataGridView()
		{
			dataGridView1.ReadOnly = true;
			dataGridView1.Rows.Insert(0, "Experience");
			dataGridView1.Rows.Insert(1, "Honor");
			dataGridView1.Rows.Insert(2, "Credits");
			dataGridView1.Rows.Insert(3, "Uridium");
			dataGridView1.Rows.Insert(4, "Killed");
			dataGridView1.Rows.Insert(5, "Boxes");
			dataGridView1.Rows.Insert(6, "DOSID");


			for (var i = 0; i < 7; i++)
			{
				dataGridView1.Rows[i].Cells["Data"].Value = 0;
				dataGridView1.Rows[i].Cells["Delta"].Value = 0;
			}
		}

		private async Task UpdateDataGridView(bool ft)
		{
			var uri = new UriBuilder(
				"https://int7.darkorbit.com/indexInternal.es?action=internalStart&acceptDailyLoginBonus=1").Uri;
			await http.GET(uri, string.Empty);
			var info = HtmlUtilities.GetAccountInfoFromHtmlString(await http.GetResponseBody());

			if (int.TryParse(info[0], out accountFactory.LoggedAccount.ID) == false)
				MessageBox.Show(@"Error parsing ID");
			if (short.TryParse(info[1], out accountFactory.LoggedAccount.Level) == false)
				MessageBox.Show(@"Error parsing Level");
			if (long.TryParse(info[2], out accountFactory.LoggedAccount.Honor) == false)
				MessageBox.Show(@"Error parsing Honor");
			if (long.TryParse(info[3], out accountFactory.LoggedAccount.Exp) == false)
				MessageBox.Show(@"Error parsing Experience");
			if (long.TryParse(info[4], out accountFactory.LoggedAccount.Uridium) == false)
				MessageBox.Show(@"Error parsing Uridium");
			if (long.TryParse(info[5], out accountFactory.LoggedAccount.Credits) == false)
				MessageBox.Show(@"Error parsing Credits");

			if (ft)
			{
				accountFactory.LoggedAccount.InitialCredits = accountFactory.LoggedAccount.Credits;
				accountFactory.LoggedAccount.InitialUridium = accountFactory.LoggedAccount.Uridium;
				accountFactory.LoggedAccount.InitialExp = accountFactory.LoggedAccount.Exp;
				accountFactory.LoggedAccount.InitialHonor = accountFactory.LoggedAccount.Honor;
			}


			dataGridView1.Rows[0].Cells["Data"].Value = accountFactory.LoggedAccount.Exp;
			dataGridView1.Rows[1].Cells["Data"].Value = accountFactory.LoggedAccount.Honor;
			dataGridView1.Rows[2].Cells["Data"].Value = accountFactory.LoggedAccount.Credits;
			dataGridView1.Rows[3].Cells["Data"].Value = accountFactory.LoggedAccount.Uridium;
			dataGridView1.Rows[4].Cells["Data"].Value = 0;
			dataGridView1.Rows[5].Cells["Data"].Value = 0;
			dataGridView1.Rows[6].Cells["Data"].Value = accountFactory.LoggedAccount.DOSID;

			dataGridView1.Rows[0].Cells["Delta"].Value =
				accountFactory.LoggedAccount.Exp - accountFactory.LoggedAccount.InitialExp;
			dataGridView1.Rows[1].Cells["Delta"].Value =
				accountFactory.LoggedAccount.Honor - accountFactory.LoggedAccount.InitialHonor;
			dataGridView1.Rows[2].Cells["Delta"].Value = accountFactory.LoggedAccount.Credits -
			                                             accountFactory.LoggedAccount.InitialCredits;
			dataGridView1.Rows[3].Cells["Delta"].Value = accountFactory.LoggedAccount.Uridium -
			                                             accountFactory.LoggedAccount.InitialUridium;
			dataGridView1.Rows[4].Cells["Delta"].Value = 0;
			dataGridView1.Rows[5].Cells["Delta"].Value = 0;
			dataGridView1.Rows[6].Cells["Delta"].Value = "null";
		}

		private Account tempAccount;

		private void button2_Click(object sender, EventArgs e)
		{

			if (tempAccount == null)
				return;

			io.AddObjectToDataGroup(tempAccount, io.GetDataGroup("account"));
			SavedAccountsComboBox.Items.Add(tempAccount);
			tempAccount = null;
		}

		public async Task<string> Login(string server)
		{
			Console.WriteLine(@"Logging in");
			await http.GET(new Uri("https://www.darkorbit.bigpoint.com"), string.Empty);
			var responseText = await http.GetResponseBody();
		    if (string.IsNullOrEmpty(responseText))
		        return null;
			var authBPSecureToken = responseText.Substring("class=\"bgcdw_login_form\" action=\"", "\">");
			authBPSecureToken = authBPSecureToken.Replace("&amp;", "&");
			//whole login is made here
			var postResponse = await http.Login(new UriBuilder(authBPSecureToken).Uri, UsernameText.Text, PassText.Text);

			var uri = new UriBuilder(
				$"https://{server}.darkorbit.com/indexInternal.es?action=internalStart&acceptDailyLoginBonus=1").Uri;
			postResponse = await http.GET(uri, string.Empty);
			var body = await http.GetResponseBody();

			if (postResponse.StatusCode != HttpStatusCode.OK && postResponse.StatusCode != HttpStatusCode.Found)
			{
				Console.WriteLine(@"Login Error");
				http.Dispose();
				return null;
			}

		    if (body.Contains("id=\"header_main_noPremium\""))
            {
                LabelPremium.ForeColor = Color.Gold;
                LabelPremium.Text = @"Premium";
		    }
		    else
		    {
		        LabelPremium.ForeColor = Color.SlateGray;
                LabelPremium.Text = @"Non Premium";
            }
			Console.WriteLine(@"Logged");

			return body;
		}


		private async void button1_Click(object sender, EventArgs e)
		{
			Logging.ToggleVisible();
			button1.ToggleVisible();
			button2.ToggleVisible();
			
			var server = comboBox1.SelectedItem == null ? 
				string.Empty : comboBox1?.SelectedItem.ToString();
		    string responseAuth;

            if (server == string.Empty)
		    {
                responseAuth= await Login("int7");
            }
            else
            {
                responseAuth = await Login(server);
            }

			if (string.IsNullOrEmpty(responseAuth))
			{
				Logging.ToggleVisible();
				button1.ToggleVisible();
				button2.ToggleVisible();
			    MessageBox.Show(@"Error during LogIn");
				return;
			}
			Console.WriteLine(@"Setting Cookies");
			var authCookie = http.GetResponseHeader("Set-Cookie");
			var authBpSecureSid = authCookie.Substring("dosid=", "; path=/");
			Console.WriteLine(@"Parsing new account to memory");
			tempAccount = new Account
			{
				Nick = UsernameText.Text,
				Password = PassText.Text,
				DOSID = authBpSecureSid
			};

			accountFactory.AddData(tempAccount);
			var account =
				accountFactory.GetAccountFromMD5HashName(Cryptography.GetStringMD5Hash(MD5.Create(), UsernameText.Text));
			account.Logged = true;
		    account.Premium = premium;
		    account.Server = "int7";
			accountFactory.LoggedAccount = account;
			Console.WriteLine(@"Parsing done");

			Console.WriteLine(@"Initializing data grid view");
			await UpdateDataGridView(true);

			//UpdateWorker.RunWorkerAsync();

			await SetupGates();

		    var uri = new UriBuilder(
		        $"https://{accountFactory.LoggedAccount.Server}.darkorbit.com/indexInternal.es?action=internalStart&acceptDailyLoginBonus=1")
		        .Uri;
			Console.WriteLine($"{uri.AbsoluteUri}\nCookie");
			foreach (Cookie cookie in http.Cookies.GetCookies(uri))
			{
				Console.WriteLine($"\t{cookie.Name} : {cookie.Value} /*{cookie.Comment}*/");
			}
            
		    if (webBrowser1.Document != null)
            {
                var browserCookie = webBrowser1.Document?.Cookie;
                var cookies = browserCookie?.Split(' ');
		        browserCookie = string.Empty;
		        for (var i = 0; i < cookies?.Length; i++)
		        {
		            var temp = cookies[i];
		            var subS = cookies[i].Trim();
		            var replace = string.Empty;
		            if (subS.StartsWith("dosid"))
		            {
		                var hash = subS.Substring("dosid=", ";");
		                replace = subS.Replace(hash, accountFactory.LoggedAccount.DOSID);
		            }
		            if (!string.IsNullOrEmpty(replace))
		            {
		                browserCookie += $"{replace} ";
		            }
		            else
		            {
		                browserCookie += $"{temp} ";

		            }
		        }
                var url = new UriBuilder($"https://{accountFactory.LoggedAccount.Server}.darkorbit.bigpoint.com/" +
                                         $"indexInternal.es?action=internalMapRevolution&dosid={accountFactory.LoggedAccount.DOSID}").Uri;
                if (url.SetCookie(accountFactory.LoggedAccount.DOSID))
                {
                    try
                    {
                        webBrowser1.Navigate(url);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                else
                {
                    Console.WriteLine(@"Error Setting Cookie");
                }
                
            }
		    Logging.ToggleVisible();
			button1.ToggleVisible();
			button2.ToggleVisible();
			Start.ToggleActive();
			Stop.ToggleActive();
		}


		private async Task<bool> BuyAmmo(string ammoType, string ammoName, int ammount)
		{
			await http.BuyAmmo(ammoType, ammoName, ammount);
			var responseStatus = await http.GetResponseBody();
			return responseStatus.Contains("success");
		}

		//lcb buy10
		private async void button4_Click(object sender, EventArgs e)
		{
			var ammount = GetAmmount(sender);
			var item = itemFactory.GetData(Cryptography.GetStringMD5Hash(MD5.Create(), "lcb-10"));
			if (await BuyAmmo(item.ItemCategory.ToString(), item.GetHttpBuyContentString(), ammount))
			{
                richTextBox1.Text += $"Bought {ammount} of {item.Name} | Total Cost: {item.Cost * ammount} {item.Currency}\n";
			}
			else
			{
                richTextBox1.Text += $"Error buying {ammount} {item.Name}\n";
			}
		}

		//action=purchase&category=rocket&itemId=ammunition_rocketlauncher_eco-10&amount=10&level=-1&selectedName=
		private async void button8_Click(object sender, EventArgs e)
		{
			var ammount = GetAmmount(sender);
			var item = itemFactory.GetData(Cryptography.GetStringMD5Hash(MD5.Create(), "eco-10"));
			if (await BuyAmmo(item.ItemCategory.ToString(), item.GetHttpBuyContentString(), ammount))
			{
                richTextBox1.Text += $"Bought {ammount} of {item.Name} | Total Cost: {item.Cost * ammount} {item.Currency}\n";
			}
			else
			{
                richTextBox1.Text += $"Error buying {ammount} {item.Name}\n";
			}
		}

		private async void button6_Click(object sender, EventArgs e)
		{
			var ammount = GetAmmount(sender);
			var item = itemFactory.GetData(Cryptography.GetStringMD5Hash(MD5.Create(), "eco-10"));
			if (await BuyAmmo(item.ItemCategory.ToString(), item.GetHttpBuyContentString(), ammount))
			{
                richTextBox1.Text += $"Bought {ammount} of {item.Name} | Total Cost: {item.Cost * ammount} {item.Currency}\n";
			}
			else
			{
                richTextBox1.Text += $"Error buying {ammount} {item.Name}\n";
			}
		}

		private async void button5_Click(object sender, EventArgs e)
		{
			var ammount = GetAmmount(sender);
			var item = itemFactory.GetData(Cryptography.GetStringMD5Hash(MD5.Create(), "lcb-10"));
			if (await BuyAmmo(item.ItemCategory.ToString(), item.GetHttpBuyContentString(), ammount))
			{
                richTextBox1.Text += $"Bought {ammount} of {item.Name} | Total Cost: {item.Cost * ammount} {item.Currency}\n";
			}
			else
			{
                richTextBox1.Text += $"Error buying {ammount} {item.Name}\n";
			}
		}

		private async void button7_Click(object sender, EventArgs e)
		{
			var ammount = GetAmmount(sender);
			var item = itemFactory.GetData(Cryptography.GetStringMD5Hash(MD5.Create(), "lcb-10"));
			if (await BuyAmmo(item.ItemCategory.ToString(), item.GetHttpBuyContentString(), ammount))
			{
                richTextBox1.Text += $"Bought {ammount} of {item.Name} | Total Cost: {item.Cost * ammount} {item.Currency}\n";
			}
			else
			{
				richTextBox1.Text += $"Error buying {ammount} {item.Name}\n";
			}
		}

		private int GetAmmount(object sender)
		{
			var btn = sender as Button;
			int ammount;
		    var text = btn?.Text.ToLower();
            if (text != null && text.ToLower().Contains("k"))
            {
                text = text.Replace("k", "000");
            }

		    int.TryParse(text, out ammount);
			return ammount;
		}

	    private async void button9_Click(object sender, EventArgs e)
	    {
		    await UpdateDataGridView(false);
	        if (accountFactory.LoggedAccount.Uridium
	            < (int) spinAmmountCb.SelectedItem*accountFactory.LoggedAccount.GgSpinCost)
	        {
	            richTextBox1.Text += $"Not Enought uridium\n";
	            return;
	        }
	        await http.GalaxyGateSpin((GalaxyGate) GatesComboBox.SelectedItem,
	            (int) spinAmmountCb.SelectedItem);

			var result = HtmlUtilities.GetSpinRewards(await http.GetResponseBody());
	        if (result.Length == 0 || string.IsNullOrEmpty(result[0]))
            {
	            richTextBox1.Text += @"Error Spinning";
	            return;
	        }

            richTextBox1.Text += $"Spin Rewards:\n";
		    foreach (var s in result)
            {
		        richTextBox1.Text += $"\t->{s}\n";
		    }
		}

		public string GetIP()
		{
			using (var webClient = new WebClient())
			{
				try
				{
					var response = webClient.DownloadString($"http://int7.darkorbit.bigpoint.com/spacemap/xml/maps.php");
					var match = Regex.Match(response, $"<map id=\"1\"><gameserverIP>([0-9\\.]+)</gameserverIP></map>");
					return match.Groups[1].ToString();
				}
				catch
				{
					return null;
				}
			}
		}

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
		
		public void Toggle(ref bool arg)
		{
			arg = !arg;
		}

		private bool gridUpActive = false;
		private Task gridTask;
        private void button10_Click(object sender, EventArgs e) 
		{
			Toggle(ref gridUpActive);

			if (gridTask != null && (gridTask.Status == TaskStatus.Running || gridTask.Status == TaskStatus.RanToCompletion) &&
			    gridUpActive == false)
			{
				richTextBox1.Text += "Grid Update Stopped\n";
				return;
			}
			richTextBox1.Text += "Grid Update Started\n";
			gridTask = Task.Factory.StartNew(async () =>
			{
				while (true) {
					if (gridUpActive == false) {
						Console.WriteLine("Grid Update Stopped\n");
						//Toggle(ref gridUpActive);
						break;
					}
					await UpdateDataGridView(false);
					Console.WriteLine("Grid Updated\n");
					Thread.Sleep(5000);
				}
			});
		}















		/*
		 * Image Matching Section
		 * TODO: Create a class to handle all this stuff
		 * Match a bunch of pictures
		 */

		public enum BotState
		{
			Boxing, Roaming
		}

		private BotState botStatus;
		public BotState BotStatus
		{
		    get { return botStatus; }
		    set
			{
				if (!Enum.IsDefined(typeof(BotState), value))
					throw new InvalidEnumArgumentException(nameof(value), (int) value, typeof(BotState));
				LastBotStatus = botStatus;
				botStatus = value;
			}
		}

		private BotState LastBotStatus;

		private Image<Bgr, byte> source;
		private Image<Bgr, byte> template;

		private Point browserMax, browserMin;
		private Point miniMapPos;


		private Point lastBbPos;

		internal int internalRoamingCounter;
		internal int resetRoamCounter = 5;

		public Tuple<bool, Point> MatchImages(Image<Bgr, byte> source, Image<Bgr, byte> template, float threshold)
		{
			this.source = source;
			this.template = new Image<Bgr, byte>(template.Bitmap);
			//var imageToShow = new Image<Bgr, byte>(this.source.Bitmap);
			using (var result =
				this.source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed)) 
			{
				double[] minValues, maxValues;
				Point[] minLocations, maxLocations;
				result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

				if (!(maxValues[0] >= threshold))
					return new Tuple<bool, Point>(false, default(Point));

				var match = new Rectangle(maxLocations[0], template.Size);
				return new Tuple<bool, Point>(true, match.Center());
			}
		}

		private bool matching = false;
		private Task imageMatchingTask;

		private bool mapIsDirty = true;

		private void Start_Click(object sender, EventArgs e)
		{
			if (imageMatchingTask != null && (imageMatchingTask.Status == TaskStatus.Running && matching))
			{
				richTextBox1.Text += "Bot is already Running\n";
				return;
			}
			Toggle(ref matching);
			richTextBox1.Text += "Bot Started\n";
			var webBrowserFlash = Mouse.Flash(webBrowser1);
			imageMatchingTask = Task.Factory.StartNew(() =>
			{
				Bitmap current = null;
				while (true) 
				{
					if (matching == false) {
						//Toggle(ref matching);
						break;
					}

					try
					{
						mainThreadContext.Send(delegate
						{
							current = cs.ScreenShot(webBrowser1);
						}, null);

						if (mapIsDirty) {
							while (true)
							{
								mainThreadContext.Send(delegate
								{
									richTextBox1.Text += "Trying to find minimap\n";
								}, null);
								var result = MatchImages(new Image<Bgr, byte>(current),
									new Image<Bgr, byte>((Bitmap)images["minimap"]), 0.75f);
								if (result.Item1 == false) {
									Thread.Sleep(500);
									continue;
								}

								miniMapPos = result.Item2;
								browserMin = new Point(miniMapPos.X + 10, miniMapPos.Y + 30);
								browserMax = new Point(browserMin.X + 200, browserMin.Y + 200);
								mapIsDirty = false;
								break;
							}
						}

						if (LastBotStatus == BotState.Boxing) {
							var result = MatchImages(new Image<Bgr, byte>(current),
								new Image<Bgr, byte>((Bitmap)images["bonusbox"]), 0.80f);
							if (result.Item1) {
								continue;
							}
							BotStatus = BotState.Roaming;
							continue;
						}

						foreach (var image in images)
						{
							if (image.Key == "minimap") {
								continue;
							}

							var result = MatchImages(new Image<Bgr, byte>(current),
								new Image<Bgr, byte>((Bitmap) image.Value), 0.80f);
							Console.WriteLine(@"	Image Key : " + image.Key);

							if (result.Item1 && image.Key != "bonusbox") {
								Console.WriteLine("Ignored\n");
								continue;
							}

							if (result.Item1 && image.Key == "bonusbox") 
							{
								if (LastBotStatus == BotState.Boxing) {
									break;
								}

								BotStatus = BotState.Boxing;
								lastBbPos = result.Item2;
								mainThreadContext.Send(delegate
								{
									richTextBox1.Text += Resources.BotBoxing;
								}, null);
								Console.WriteLine("Accepted\n");
								Mouse.DoMouseLeftClick(webBrowserFlash, new Point(result.Item2.X, 
									result.Item2.Y));

								//We have found a bonus box! break the loop so we can search for a new one
								break;
							}
							
							if (LastBotStatus == BotState.Roaming) {
								internalRoamingCounter++;
								if(internalRoamingCounter < resetRoamCounter)
									continue;
								internalRoamingCounter = 0;
							}

							Console.WriteLine("Nothing Found Roaming\n");
							BotStatus = BotState.Roaming;
							mainThreadContext.Send(delegate {
								richTextBox1.Text += Resources.BotRoaming;
							}, null);
							MoveShip(webBrowserFlash);
						}
					}
					catch (Exception exception)
					{
						Console.WriteLine(exception);
						break;
					}
					Thread.Sleep(100);
				}
			});
		}

		private void Stop_Click(object sender, EventArgs e) 
		{
			if (matching == false) {
				richTextBox1.Text += "Start Bot First\n";
				return;
			}

			Toggle(ref matching);
			if (imageMatchingTask != null && ( imageMatchingTask.Status == TaskStatus.Running ||
				imageMatchingTask.Status == TaskStatus.RanToCompletion ) &&
				matching == false) {
				richTextBox1.Text += "Bot Stopped\n";
			}
		}

		public void MoveShip(IntPtr webBrowserFlash)
		{
		    if (browserMin.X > browserMax.X)
		    {
		        var bm = browserMin.X;
		        browserMin.X = browserMax.X;
		        browserMax.X = bm;
		    }

            if (browserMin.Y > browserMax.Y)
            {
                var bm = browserMin.Y;
                browserMin.Y = browserMax.Y;
                browserMax.Y = bm;
            }

            var randomPoint = new Point {
				X = StaticRandom.Next(browserMin.X, browserMax.X),
				Y = StaticRandom.Next(browserMin.Y, browserMax.Y)
			};

			Mouse.DoMouseLeftClick(webBrowserFlash, randomPoint);
		}
	}
}
