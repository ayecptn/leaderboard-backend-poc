using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeaderboardBackend.Models.Entities;
using LeaderboardBackend.Models.Requests;
using LeaderboardBackend.Test.Lib;
using LeaderboardBackend.Test.TestApi;
using LeaderboardBackend.Test.TestApi.Extensions;
using NUnit.Framework;

namespace LeaderboardBackend.Test
{
	[TestFixture]
	internal class Runs
	{
		private static TestApiFactory Factory = null!;
		private static TestApiClient ApiClient = null!;
		private static string Jwt = null!;
		private static long _categoryId;

		[SetUp]
		public static async Task SetUp()
		{
			Factory = new TestApiFactory();
			ApiClient = Factory.CreateTestApiClient();
			Jwt = (await ApiClient.LoginAdminUser()).Token;

			Leaderboard createdLeaderboard = await ApiClient.Post<Leaderboard>(
				"/api/leaderboards",
				new()
				{
					Body = new CreateLeaderboardRequest()
					{
						Name = Generators.GenerateRandomString(),
						Slug = Generators.GenerateRandomString(),
					},
					Jwt = Jwt,
				}
			);

			Category createdCategory = await ApiClient.Post<Category>(
				"/api/categories",
				new()
				{
					Body = new CreateCategoryRequest()
					{
						Name = Generators.GenerateRandomString(),
						Slug = Generators.GenerateRandomString(),
						LeaderboardId = createdLeaderboard.Id,
					},
					Jwt = Jwt,
				}
			);
			_categoryId = createdCategory.Id;
		}

		[Test]
		public static async Task CreateRun_OK()
		{
			Run created = await CreateRun();

			Run retrieved = await GetRun(created.Id);

			Assert.NotNull(created);
			Assert.AreEqual(created.Id, retrieved.Id);
		}

		[Test]
		public static async Task GetParticipation_OK()
		{
			Run createdRun = await CreateRun();

			Participation createdParticipation = await ApiClient.Post<Participation>(
				"api/participations",
				new()
				{
					Body = new CreateParticipationRequest
					{
						Comment = "comment",
						Vod = "vod",
						RunId = createdRun.Id,
						RunnerId = TestInitCommonFields.Admin.Id
					},
					Jwt = Jwt
				}
			);

			List<Participation> retrieved = await ApiClient.Get<List<Participation>>(
				$"api/runs/{createdRun.Id}/participations",
				new()
				{
					Jwt = Jwt
				}
			);

			Assert.NotNull(retrieved);
			Assert.AreEqual(createdParticipation.Id, retrieved[0].Id);
		}

		[Test]
		public static async Task GetCategory_OK()
		{
			Run createdRun = await CreateRun();

			Category category = await ApiClient.Get<Category>(
				$"api/runs/{createdRun.Id}/category",
				new()
				{
					Jwt = Jwt
				}
			);

			Assert.NotNull(category);
			Assert.AreEqual(category.Id, _categoryId);
		}

		private static async Task<Run> CreateRun()
		{
			return await ApiClient.Post<Run>(
				"/api/runs",
				new()
				{
					Body = new CreateRunRequest
					{
						Played = NodaTime.Instant.MinValue,
						Submitted = NodaTime.Instant.MaxValue,
						Status = RunStatus.CREATED,
						CategoryId = _categoryId
					},
					Jwt = Jwt
				}
			);
		}

		private static async Task<Run> GetRun(Guid id)
		{
			return await ApiClient.Get<Run>(
				$"/api/runs/{id}",
				new()
				{
					Jwt = Jwt
				}
			);
		}

	}
}
