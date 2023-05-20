namespace Destiny2Manager.Models.Bungie
{
	public class BungieOAuthTokenRequestModel
	{
		public string grant_type { get; set; }
		public string code { get; set; }
	}
}
