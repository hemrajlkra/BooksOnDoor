using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace BooksOnDoor.Utility.Service
{
	public class ReCaptchaService
	{
		[HttpPost]
		public static async Task<bool> verifyReCaptchaV2(string response, string secretKey)
		{
			using(var client = new HttpClient())
			{
				string url = $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={response}";
				MultipartFormDataContent formData = new();
				formData.Add(new StringContent(response), "response");
				formData.Add(new StringContent(secretKey), "secretKey");
				var result = await client.PostAsync(url, formData);
				if(result.IsSuccessStatusCode)
				{
					var strResponse = await result.Content.ReadAsStringAsync();
					
					var jsonResponse = JsonNode.Parse(strResponse);
					if(jsonResponse != null)
					{
						var success= ((bool?)jsonResponse["success"]);
						if(success!=null && success == true) 
							return true;
					}
				}
			}
			return false;
		}
	}
}
