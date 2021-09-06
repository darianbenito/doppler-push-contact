using AutoFixture;
using Doppler.PushContact.Models;
using Doppler.PushContact.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Doppler.PushContact.Test.Controllers
{
    public class PushContactControllerTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string TOKEN_EMPTY = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.e30.Nbd00AAiP2vJjxr78oPZoPRsDml5dx2bdD1Y6SXomfZN8pzJdKel2zrplvXCGBBYNBOo90rdYSlBCCo15rxsVydiFcAP84qZv-2mh4pFED9tVyDbxV5hvYDSg2bHPFyYFAi26fJusu_oYY3ne8OWxx-W1MEzNxh2hPfEKTkd0zVBm4dZv_irizRpa_qBwjn3hbCLUtOhBFbTTFItM9hESo6RwHvtQaB0667Sj8N97-bleCY5Ppf6bUUMz2A35PDb8-roF5Scf97lTZfug_DymgpPRSNK2VcRjfAynKfbBSih4QqVeaxR5AhYtXVFbQgByrynYNLok1SFD-M48WpzSA";
        private const string TOKEN_BROKEN = "eyJhbGciOiJSzI1NiIsInR5cCI6IkpXVCJ9.e0.Nbd00AAiP2vJjxr8oPZoPRsDml5dx2bdD1Y6SXomfZN8pzJdKel2zrplvXCGBBYNBOo90rdYSlBCCo15rxsVydiFcAP84qZv-2mh4pFED9tVyDbxV5hvYDSg2bHPFyYFAi26fJusu_oYY3ne8OWxx-W1MEzNxh2hPfEKTkd0zVBm4dZv_irizRpa_qBwjn3hbCLUtOhBFbTTFItM9hESo6RwHvtQaB0667Sj8N97-bleCY5Ppf6bUUMz2A35PDb8-roF5Scf97lTZfug_DymgpPRSNK2VcRjfAynKfbBSih4QqVeaxR5AhYtXVbQgByrynYNLok1SFD-M48WpzSA";

        private const string TOKEN_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjIwMDAwMDAwMDB9.mll33c0kstVIN9Moo4HSw0CwRjn0IuDc2h1wkRrv2ahQtIG1KV5KIxYw-H3oRfd-PiCWHhIVIYDP3mWDZbsOHTlnpRGpHp4f26LAu1Xp1hDJfOfxKYEGEE62Xt_0qp7jSGQjrx-vQey4l2mNcWkOWiE0plOws7cX-wLUvA3NLPoOvEegjM0Wx6JFcvYLdMGcTGT5tPd8Pq8pe9VYstCbhOClzI0bp81iON3f7VQP5d0n64eb_lvEPFu5OfURD4yZK2htyQK7agcNNkP1c5mLEfUi39C7Qtx96aAhOjir6Wfhzv_UEs2GQKXGTHl6_-HH-ecgOdIvvbqXGLeDmTkXUQ";

        private const string TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOjEyMywidW5pcXVlX25hbWUiOiJ0ZXN0MUB0ZXN0LmNvbSIsInJvbGUiOiJVU0VSIiwiZXhwIjoyMDAwMDAwMDAwfQ.E3RHjKx9p0a-64RN2YPtlEMysGM45QBO9eATLBhtP4tUQNZnkraUr56hAWA-FuGmhiuMptnKNk_dU3VnbyL6SbHrMWUbquxWjyoqsd7stFs1K_nW6XIzsTjh8Bg6hB5hmsSV-M5_hPS24JwJaCdMQeWrh6cIEp2Sjft7I1V4HQrgzrkMh15sDFAw3i1_ZZasQsDYKyYbO9Jp7lx42ognPrz_KuvPzLjEXvBBNTFsVXUE-ur5adLNMvt-uXzcJ1rcwhjHWItUf5YvgRQbbBnd9f-LsJIhfkDgCJcvZmGDZrtlCKaU1UjHv5c3faZED-cjL59MbibofhPjv87MK8hhdg";

        private const string TOKEN_SUPERUSER_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";
        private const string TOKEN_SUPERUSER_EXPIRE_20010908 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjEwMDAwMDAwMDB9.FYOpOxrXSHDif3lbQLPEStMllzEktWPKQ2T4vKUq5qgVjiH_ki0W0Ansvt0PMlaLHqq7OOL9XGFebtgUcyU6aXPO9cZuq6Od196TWDLMdnxZ-Ct0NxWxulyMbjTglUiI3V6g3htcM5EaurGvfu66kbNDuHO-WIQRYFfJtbm7EuOP7vYBZ26hf5Vk5KvGtCWha4zRM55i1-CKMhXvhPN_lypn6JLENzJGYHkBC9Cx2DwzaT683NWtXiVzeMJq3ohC6jvRpkezv89QRes2xUW4fRgvgRGQvaeQ4huNW_TwQKTTikH2Jg7iHbuRqqwYuPZiWuRkjqfd8_80EdlSAnO94Q";
        private const string TOKEN_SUPERUSER_FALSE_EXPIRE_20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjpmYWxzZSwiZXhwIjoyMDAwMDAwMDAwfQ.qMY3h8VhNxuOBciqrmXpTrRk8ElwDlT_3CYFzqJdXNjnJhKihFVMwjkWVw1EEckCWbKsRoBr-NgRV0SZ0JKWbMr2oGhZJWtqmKA05d8-i_MuuYbxtt--NUoQxg6AsMX989PGf6fSBzo_4szb7J0G6nUvvRxXfMnHMpaIAQUiBLNOoeKwnzsZFfI1ehmYGNmtc-2XyXOEHAnfZeBZw8uMWOp4A5hFBpVsaVCUiRirokjeCMWViVWT9NnVWbA60e_kfLjghEcXWaZfNnX9qtj4OC8QUB33ByUmwuYlTxNnu-qiEaJmbaaTeDD2JrKHf6MR59MlCHbb6BDWt20DBy73WQ";

        private readonly WebApplicationFactory<Startup> _factory;
        private readonly ITestOutputHelper _output;

        public PushContactControllerTest(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Theory]
        [InlineData(TOKEN_EMPTY)]
        [InlineData(TOKEN_BROKEN)]
        public async Task Add_should_return_unauthorized_when_token_is_not_valid(string token)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Post, "push-contacts")
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_SUPERUSER_EXPIRE_20010908)]
        public async Task Add_should_return_unauthorized_when_token_is_a_expired_superuser_token(string token)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Post, "push-contacts")
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_EXPIRE_20330518)]
        [InlineData(TOKEN_SUPERUSER_FALSE_EXPIRE_20330518)]
        [InlineData(TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518)]
        public async Task Add_should_require_a_valid_token_with_isSU_flag(string token)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Post, "push-contacts")
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Add_should_return_unauthorized_when_authorization_header_is_empty()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Post, "push-contacts");

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData(false, HttpStatusCode.OK)]
        [InlineData(true, HttpStatusCode.InternalServerError)]
        public async Task Add_should_return_expected_status_code_depending_on_service_add_result
            (bool addResultWithException, HttpStatusCode expectedHttpStatusCode)
        {
            // Arrange
            var fixture = new Fixture();

            var pushContactServiceMock = new Mock<IPushContactService>();

            pushContactServiceMock
                .Setup(x => x.AddAsync(It.IsAny<PushContactModel>()))
                .Returns(addResultWithException ? Task.FromException(new Exception()) : Task.CompletedTask);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(pushContactServiceMock.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Post, "push-contacts")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_SUPERUSER_EXPIRE_20330518}" } },
                Content = JsonContent.Create(fixture.Create<PushContactModel>())
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(expectedHttpStatusCode, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_EMPTY)]
        [InlineData(TOKEN_BROKEN)]
        public async Task UpdateEmail_should_return_unauthorized_when_token_is_not_valid(string token)
        {
            // Arrange
            var fixture = new Fixture();

            var deviceToken = fixture.Create<string>();
            var email = fixture.Create<string>();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Put, $"push-contacts/{deviceToken}/email")
            {
                Headers = { { "Authorization", $"Bearer {token}" } },
                Content = JsonContent.Create(email)
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_SUPERUSER_EXPIRE_20010908)]
        public async Task UpdateEmail_should_return_unauthorized_when_token_is_a_expired_superuser_token(string token)
        {
            // Arrange
            var fixture = new Fixture();

            var deviceToken = fixture.Create<string>();
            var email = fixture.Create<string>();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Put, $"push-contacts/{deviceToken}/email")
            {
                Headers = { { "Authorization", $"Bearer {token}" } },
                Content = JsonContent.Create(email)
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_EXPIRE_20330518)]
        [InlineData(TOKEN_SUPERUSER_FALSE_EXPIRE_20330518)]
        [InlineData(TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518)]
        public async Task UpdateEmail_should_require_a_valid_token_with_isSU_flag(string token)
        {
            // Arrange
            var fixture = new Fixture();

            var deviceToken = fixture.Create<string>();
            var email = fixture.Create<string>();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Put, $"push-contacts/{deviceToken}/email")
            {
                Headers = { { "Authorization", $"Bearer {token}" } },
                Content = JsonContent.Create(email)
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEmail_should_return_unauthorized_when_authorization_header_is_empty()
        {
            // Arrange
            var fixture = new Fixture();

            var deviceToken = fixture.Create<string>();
            var email = fixture.Create<string>();

            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Put, $"push-contacts/{deviceToken}/email")
            {
                Content = JsonContent.Create(email)
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEmail_should_return_ok_when_service_does_not_throw_an_exception()
        {
            // Arrange
            var fixture = new Fixture();

            var deviceToken = fixture.Create<string>();
            var email = fixture.Create<string>();

            var pushContactServiceMock = new Mock<IPushContactService>();

            pushContactServiceMock
                .Setup(x => x.UpdateEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(pushContactServiceMock.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Put, $"push-contacts/{deviceToken}/email")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_SUPERUSER_EXPIRE_20330518}" } },
                Content = JsonContent.Create(email)
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UpdateEmail_should_return_internal_server_error_when_service_throw_an_exception()
        {
            // Arrange
            var fixture = new Fixture();

            var deviceToken = fixture.Create<string>();
            var email = fixture.Create<string>();

            var pushContactServiceMock = new Mock<IPushContactService>();

            pushContactServiceMock
                .Setup(x => x.UpdateEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(pushContactServiceMock.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Put, $"push-contacts/{deviceToken}/email")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_SUPERUSER_EXPIRE_20330518}" } },
                Content = JsonContent.Create(email)
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_EMPTY)]
        [InlineData(TOKEN_BROKEN)]
        public async Task GetBy_should_return_unauthorized_when_token_is_not_valid(string token)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var fixture = new Fixture();
            var domain = fixture.Create<string>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"push-contacts/{domain}")
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_SUPERUSER_EXPIRE_20010908)]
        public async Task GetBy_should_return_unauthorized_when_token_is_a_expired_superuser_token(string token)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var fixture = new Fixture();
            var domain = fixture.Create<string>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"push-contacts/{domain}")
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_EXPIRE_20330518)]
        [InlineData(TOKEN_SUPERUSER_FALSE_EXPIRE_20330518)]
        [InlineData(TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518)]
        public async Task GetBy_should_require_a_valid_token_with_isSU_flag(string token)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var fixture = new Fixture();
            var domain = fixture.Create<string>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"push-contacts/{domain}")
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetBy_should_return_unauthorized_when_authorization_header_is_empty()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var fixture = new Fixture();
            var domain = fixture.Create<string>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"push-contacts/{domain}");

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetBy_should_return_push_contacts_that_service_get_method_return()
        {
            // Arrange
            var fixture = new Fixture();

            var pushContacts = fixture.CreateMany<PushContactModel>(10);

            var pushContactServiceMock = new Mock<IPushContactService>();

            pushContactServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PushContactFilter>()))
                .ReturnsAsync(pushContacts);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(pushContactServiceMock.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var domain = fixture.Create<string>();
            var email = fixture.Create<string>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"push-contacts/{domain}?email={email}")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_SUPERUSER_EXPIRE_20330518}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseAsString = await response.Content.ReadAsStringAsync();
            var pushContactsResponse = JsonSerializer.Deserialize<IEnumerable<PushContactModel>>
                (responseAsString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal(pushContacts.Count(), pushContactsResponse.Count());

            var pushContactsEnumerator = pushContacts.GetEnumerator();
            var pushContactsResponseEnumerator = pushContactsResponse.GetEnumerator();

            while (pushContactsEnumerator.MoveNext() && pushContactsResponseEnumerator.MoveNext())
            {
                Assert.True(pushContactsEnumerator.Current.Domain == pushContactsResponseEnumerator.Current.Domain);
                Assert.True(pushContactsEnumerator.Current.DeviceToken == pushContactsResponseEnumerator.Current.DeviceToken);
                Assert.True(pushContactsEnumerator.Current.Email == pushContactsResponseEnumerator.Current.Email);
            }
        }

        [Fact]
        public async Task GetBy_should_return_not_found_when_service_get_method_return_a_empty_push_contacts_collection()
        {
            // Arrange
            var fixture = new Fixture();

            var pushContacts = Enumerable.Empty<PushContactModel>();

            var pushContactServiceMock = new Mock<IPushContactService>();

            pushContactServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PushContactFilter>()))
                .ReturnsAsync(pushContacts);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(pushContactServiceMock.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var domain = fixture.Create<string>();
            var email = fixture.Create<string>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"push-contacts/{domain}?email={email}")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_SUPERUSER_EXPIRE_20330518}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetBy_should_return_not_found_when_service_get_method_return_null()
        {
            // Arrange
            var fixture = new Fixture();

            IEnumerable<PushContactModel> pushContacts = null;

            var pushContactServiceMock = new Mock<IPushContactService>();

            pushContactServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PushContactFilter>()))
                .ReturnsAsync(pushContacts);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(pushContactServiceMock.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var domain = fixture.Create<string>();
            var email = fixture.Create<string>();

            var request = new HttpRequestMessage(HttpMethod.Get, $"push-contacts/{domain}?email={email}")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_SUPERUSER_EXPIRE_20330518}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_EMPTY)]
        [InlineData(TOKEN_BROKEN)]
        public async Task BulkDelete_should_return_unauthorized_when_token_is_not_valid(string token)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Delete, "push-contacts/_bulk")
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_SUPERUSER_EXPIRE_20010908)]
        public async Task BulkDelete_should_return_unauthorized_when_token_is_a_expired_superuser_token(string token)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Delete, "push-contacts/_bulk")
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData(TOKEN_EXPIRE_20330518)]
        [InlineData(TOKEN_SUPERUSER_FALSE_EXPIRE_20330518)]
        [InlineData(TOKEN_ACCOUNT_123_TEST1_AT_TEST_DOT_COM_EXPIRE_20330518)]
        public async Task BulkDelete_should_require_a_valid_token_with_isSU_flag(string token)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Delete, "push-contacts/_bulk")
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task BulkDelete_should_return_unauthorized_when_authorization_header_is_empty()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Delete, "push-contacts/_bulk");

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task BulkDelete_should_return_ok_and_deleted_count_when_service_does_not_throw_an_exception()
        {
            // Arrange
            var fixture = new Fixture();

            var deletedCount = fixture.Create<int>();

            var pushContactServiceMock = new Mock<IPushContactService>();

            pushContactServiceMock
                .Setup(x => x.DeleteByDeviceTokenAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(deletedCount);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(pushContactServiceMock.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Delete, "push-contacts/_bulk")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_SUPERUSER_EXPIRE_20330518}" } },
                Content = JsonContent.Create(fixture.Create<IEnumerable<string>>())
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseAsString = await response.Content.ReadAsStringAsync();
            Assert.Equal(deletedCount.ToString(), responseAsString);
        }

        [Fact]
        public async Task BulkDelete_should_return_internal_server_error_when_service_throw_an_exception()
        {
            // Arrange
            var fixture = new Fixture();

            var pushContactServiceMock = new Mock<IPushContactService>();

            pushContactServiceMock
                .Setup(x => x.DeleteByDeviceTokenAsync(It.IsAny<IEnumerable<string>>()))
                .ThrowsAsync(new Exception());

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(pushContactServiceMock.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Delete, "push-contacts/_bulk")
            {
                Headers = { { "Authorization", $"Bearer {TOKEN_SUPERUSER_EXPIRE_20330518}" } },
                Content = JsonContent.Create(fixture.Create<IEnumerable<string>>())
            };

            // Act
            var response = await client.SendAsync(request);
            _output.WriteLine(response.GetHeadersAsString());

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}
