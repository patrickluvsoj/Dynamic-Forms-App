﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using PCL.Helpers;

namespace PCL
{
	public class AzureDataService
	{
		public MobileServiceClient MobileService { get; set; }
		IMobileServiceSyncTable<FormTable5> formTable;

		public async Task Initialize()
		{

			if (MobileService?.SyncContext?.IsInitialized ?? false)
				return;
			//Create our client
			MobileService = new MobileServiceClient("https://pcldemo5.azurewebsites.net");

			if (App.USING_AUTH)
			{
				JObject payload = new JObject();
				payload["access_token"] = Settings.Current.CurrentUser.AuthToken;

				try
				{
					var user = await MobileService.LoginAsync(MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory, payload);

				}
				catch (Exception e)
				{
					
				}
			}




			var path = "syncstore.db";
			path = Path.Combine(MobileServiceClient.DefaultDatabasePath, path);

			//setup our local sqlite store and intialize our table
			var store = new MobileServiceSQLiteStore(path);
			store.DefineTable<FormTable5>();

			await MobileService.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

			//Get our sync table that will call out to azure
			formTable = MobileService.GetSyncTable<FormTable5>();


		}

		public async Task<IEnumerable<FormTable5>> GetForms()
		{
			await Initialize();
			await SyncForm();
			return await formTable.OrderByDescending(c => c.EnteredDateUTC).ToEnumerableAsync();
		}


		public async Task<FormTable5> AddForm(string firstname, string lastname, string type, string data)
		{
			await Initialize();

			var form = new FormTable5
			{
				EnteredDateUTC = DateTime.UtcNow,
				FirstName = firstname,
				LastName = lastname,
				FormType = type,
				FormData = data
			};

			//insert into our local table
			await formTable.InsertAsync(form);

			//Synchronize form
			await SyncForm();

			return form;
		}

		public async Task SyncForm()
		{
			// Pulls all items that match the associated query from the remote table
			await formTable.PullAsync("allForms", formTable.CreateQuery());

			//replays all pending local operations against the remote table
			await MobileService.SyncContext.PushAsync();

		}
	}
}
