using Microsoft.AspNetCore.Mvc;
using Destiny2Manager.Models.Bungie;
using Destiny2Manager.Models.Bungie.Constants;
using Pathoschild.Http.Client;
using Newtonsoft.Json;
using System.Data.SQLite;

namespace Destiny2Manager.WebApp.Controllers
{
	public class DestinyController : Controller
	{
		// API Key for making requests to the Destiny 2 API. And a 'client' field for optimization when sending requests.
		public bool AuthenticatedStatus = false;
        private readonly IConfiguration _configuration;

        private readonly FluentClient client;
		
		public DestinyController(IConfiguration configuration) { 
			
			client = new FluentClient(); 
			_configuration = configuration;
		}

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> Statistics()
		{
			if (BungieConstants.Auth == null)
			{
				return RedirectToAction("Index", "Home");
			}
			await EstablishUserData();
			return View();
		}
        public async Task GetCharacterEquipmentAsync()
        {
            var apikey = _configuration.GetValue<string>("equipment_key");
            IResponse CharacterEquipmentResponse = await client
            .GetAsync("https://www.bungie.net/Platform/Destiny2/5/Profile/4611686018495570819/Character/2305843009776964460/?components=205")
            .WithOptions(ignoreHttpErrors: true)
            .WithHeader("x-api-key", apikey);
            BungieCharacterEquipmentModel BungieCharacterEquipmentModel = await CharacterEquipmentResponse.As<BungieCharacterEquipmentModel>();
            //  string[] myStrings = new string[0];
            List<string> names = new List<string>();
			List<string> icons = new List<string>();
            foreach (BungieCharacterEquipmentModelItem item in BungieCharacterEquipmentModel.Response.equipment.data.items)
            {
                if (item.itemHash != null)
                {
                    long newId = (long)item.itemHash;
                    bool isSigned = (newId & (1L << 31)) != 0; // check if the integer is signed
                    if (isSigned)
                    {
                        newId = newId - (1L << 32); // convert to signed integer
                    }

                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=X:\\Programming\\VS Projects\\Destiny2Manager\\Destiny2Manager.Models\\Bungie\\Data\\Destiny2Manifest.sqlite3"))
                    {
                        connection.Open();
                        string sql = $"SELECT json FROM DestinyInventoryItemDefinition WHERE id = {newId};";
                        using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string json = reader.GetString(0);

                                dynamic itemData = JsonConvert.DeserializeObject(json);

                                string name = itemData.displayProperties.name;
								string icon = itemData.displayProperties.icon;
                                names.Add(name);
								icons.Add(icon);
								ViewBag.Icons = icons;
                                ViewBag.Names = names;
								string imgurl = "https://www.bungie.net";
                               // Console.WriteLine(name);
                            }
                        }
                    }
                }
            }
        }

        public async Task<IActionResult> Manager()
        {
            if (BungieConstants.Auth == null)
            {
                return RedirectToAction("Index", "Home");
            }
            await GetCharacterEquipmentAsync();
            return View();
        }
        public async void EquipAurvandilAsync()
        {
            BungieEquipItemModel EquipPayload = new BungieEquipItemModel()
            {
                itemId = 6917529859030182000,
                membershipType = 5,
                characterId = 2305843009776964600
            };
            var apikey = _configuration.GetValue<string>("api_key");
            IResponse EquipAurvandil = await client
                .PostAsync("https://www.bungie.net/Platform/Destiny2/Actions/Items/EquipItem/")
                .WithOptions(ignoreHttpErrors: true)
                .WithBearerAuthentication(BungieConstants.Auth.access_token)
                .WithHeader("x-api-key", apikey)
                .WithBody(EquipPayload);
        }

        public async Task<DestinyMembership> EstablishUserData()
		{
			if (BungieConstants.Auth != null)
			{
                var apikey = _configuration.GetValue<string>("api_key");
                IResponse UserDataResponse = await client
				  .GetAsync("https://www.bungie.net/Platform/User/GetCurrentBungieNetUser/")
				  .WithOptions(ignoreHttpErrors: true)
				  .WithHeader("x-api-key", apikey)
				  .WithBearerAuthentication(BungieConstants.Auth.access_token);

				BungieUserModel ResponseDataModel = await UserDataResponse.As<BungieUserModel>();

				IResponse BungieUserMembershipsResponse = await client
				  .GetAsync($"https://www.bungie.net/Platform/User/GetMembershipsById/{ResponseDataModel.Response.membershipId}/5/")
				  .WithOptions(ignoreHttpErrors: true)
				  .WithHeader("x-api-key", apikey)
				  .WithBearerAuthentication(BungieConstants.Auth.access_token);

				BungieUserMembershipsModel MembershipData = await BungieUserMembershipsResponse.As<BungieUserMembershipsModel>();
				DestinyMembership? Membership = MembershipData.Response.destinyMemberships.FirstOrDefault(x => x.membershipType is 3 or 5);
				if (Membership != null)
				{
					IResponse GetHistoricalStatsForAccount = await client
					  .GetAsync($"https://www.bungie.net/Platform/Destiny2/{Membership.membershipType}/Account/{Membership.membershipId}/Stats/")
					  .WithOptions(ignoreHttpErrors: true)
					  .WithHeader("x-api-key", apikey);

					IResponse CharactersDataResponse = await client
					  .GetAsync($"https://www.bungie.net/Platform/Destiny2/{Membership.membershipType}/Profile/{Membership.membershipId}/?components=200")
					  .WithOptions(ignoreHttpErrors: true)
					  .WithHeader("x-api-key", apikey)
					  .WithBearerAuthentication(BungieConstants.Auth.access_token);

					BungieCharacterDataModel CharacterData = await CharactersDataResponse.As<BungieCharacterDataModel>();
					BungieHistoricalStatsModel HistoricalStats = await GetHistoricalStatsForAccount.As<BungieHistoricalStatsModel>();

					// 0 = Titan
					// 1 = Hunter
					// 2 = Warlock
					KeyValuePair<long, CharacterData> WarlockData = CharacterData.Response.characters.data.FirstOrDefault(x => x.Value.classType == 2);
					KeyValuePair<long, CharacterData> HunterData = CharacterData.Response.characters.data.FirstOrDefault(x => x.Value.classType == 1);
					KeyValuePair<long, CharacterData> TitanData = CharacterData.Response.characters.data.FirstOrDefault(x => x.Value.classType == 0);
					BungieEquipItemModel EquipPayload = new BungieEquipItemModel()
					{
						itemId = 6917529859030182332,
						membershipType = Membership.membershipType,
						characterId = Int64.Parse(WarlockData.Value.characterId)

					};
					IResponse GetItemData = await client
					  .GetAsync($"https://www.bungie.net/Platform/Destiny2/5/Profile/{ResponseDataModel.Response.membershipId}/Character/{WarlockData.Value.characterId}/?components=205")
					  .WithOptions(ignoreHttpErrors: true)
					  .WithHeader("x-api-key", apikey);
					BungieItemDataModel ItemData = await GetItemData.As<BungieItemDataModel>();
					double thoursPlayed = 0.0;
					double hhoursPlayed = 0.0;
					double whoursPlayed = 0.0;

					if (GetItemData != null)
					{
						IResponse EquipAurvandil = await client
						  .PostAsync("https://www.bungie.net/Platform/Destiny2/Actions/Items/EquipItem/")
						  .WithOptions(ignoreHttpErrors: true)
						  //   .WithHeader("Content-Type", "application/json")
						  .WithHeader("x-api-key", apikey)
						  .WithBody(EquipPayload)
						  .WithBearerAuthentication(BungieConstants.Auth.access_token);
						//string ChromaRush = $"https://www.bungie.net/{}"
						//ViewData["pee"] = ItemData.Response.equipment.data.items.itemInstanceId;

					}
					if (HistoricalStats != null)
					{
						ViewData["deaths"] = HistoricalStats.Response.mergedAllCharacters.results.allPvE.allTime.deaths.basic.displayValue;

					}
					if (TitanData.Value != null)
					{
						string titanemblembkg = $"https://www.bungie.net{TitanData.Value.emblemBackgroundPath}";
						string titanemblem = $"https://www.bungie.net{TitanData.Value.emblemPath}";
						ViewData["titanemblembkg"] = titanemblembkg;
						ViewData["titanemblem"] = titanemblem;
						int minutesPlayed = int.Parse(TitanData.Value.minutesPlayedTotal);
						thoursPlayed = minutesPlayed / 60.0;
						ViewData["tpt"] = thoursPlayed.ToString("F2");
						ViewData["tll"] = TitanData.Value.light;
					}
					if (HunterData.Value != null)
					{
						string hunteremblembkg = $"https://www.bungie.net{HunterData.Value.emblemBackgroundPath}";
						string hunteremblem = $"https://www.bungie.net{HunterData.Value.emblemPath}";
						ViewData["hunteremblembkg"] = hunteremblembkg;
						ViewData["hunteremblem"] = hunteremblem;
						int minutesPlayed = int.Parse(HunterData.Value.minutesPlayedTotal);
						hhoursPlayed = minutesPlayed / 60.0;
						ViewData["hpt"] = hhoursPlayed.ToString("F2");
						ViewData["hll"] = HunterData.Value.light;

					}
					if (WarlockData.Value != null)
					{

						string warlockemblembkg = $"https://www.bungie.net{WarlockData.Value.emblemBackgroundPath}";
						string warlockemblem = $"https://www.bungie.net{WarlockData.Value.emblemPath}";
						ViewData["wll"] = WarlockData.Value.light;
						int minutesPlayed = int.Parse(WarlockData.Value.minutesPlayedTotal);
						whoursPlayed = minutesPlayed / 60.0;
						ViewData["wlpt"] = whoursPlayed.ToString("F2");
						ViewData["lastplayed"] = WarlockData.Value.dateLastPlayed.Date.ToString("dd/MM/yyyy");
						ViewData["warlockemblembkg"] = warlockemblembkg;
						ViewData["warlockemblem"] = warlockemblem;

					}
					double totalHoursPlayed = whoursPlayed + thoursPlayed + hhoursPlayed;
					ViewData["pt"] = totalHoursPlayed.ToString("F2");

				}
				return Membership;
			}
			else
			{
				throw new Exception("User is not authenticated.");
			}
		}
		// Sends a POST request to Destiny API for an bearer token.
		// The body has a code parameter passed from the redirect of successful authentication with bungie.
		// Finally, after getting and setting the bearer token, the user is redirected to the Index Page.
		public async Task<IActionResult> Callback(string code)
		{
			IResponse ResponseData = await client
			  .PostAsync("https://www.bungie.net/platform/app/oauth/token/")
			  .WithOptions(ignoreHttpErrors: true)
			  .WithBasicAuthentication("43800", "cplvDhoGoR97kCukC0wnmyUz551tc8e0k6wuHAS7hg0")
			  .WithBody(x => x.FormUrlEncoded(new BungieOAuthTokenRequestModel
			  {
				  grant_type = "authorization_code",
				  code = code
			  }));
			BungieOAuthTokenResponseModel ResponseDataModel = await ResponseData.As<BungieOAuthTokenResponseModel>();
			BungieConstants.Auth = ResponseDataModel;

			return RedirectToAction("Index");
		}
	
	}
}