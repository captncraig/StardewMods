﻿using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HashidsNet;

namespace Graph.Farm.Website.Controllers
{
    public class NewAccountResponse
    {
        public string SecretToken { get; set; }
        public string ID { get; set; }
    }

    public class NewGameRequest
    {
        public float UniqueSeed { get; set; }
        public string Name { get; set; }
        public string FarmName { get; set; }
        public string FavoriteThing { get; set; }
    }

    [Route("api")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private async Task<MySqlConnection> DB()
        {
            var db = new MySqlConnection(Settings.MysqlConnectionString);
            await db.OpenAsync();
            return db;
        }

        private string genAPIKey()
        {
            const int size = 18;
            char[] chars =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        static Hashids hid = new Hashids(Settings.HashIDSalt,6);

        private string sha(string target)
        {
            StringBuilder Sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(target));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

        // POST api/newAccount
        [HttpPost("newAccount")]
        public async Task<ActionResult<NewAccountResponse>> NewAccount()
        {
            using(var db = await DB())
            {
                var api = genAPIKey();
                
                var id = (await db.QueryAsync<int>(@"INSERT INTO Accounts (ApiKeyHash) VALUES (@keyhash);
SELECT LAST_INSERT_ID();", new { keyhash = sha(api) })).Single();

                return new NewAccountResponse { SecretToken = api, ID = hid.Encode(id) };
            }
            
        }

        // POST api/newGame
        [HttpPost("newGame")]
        public async Task<ActionResult<string>> NewGame([FromBody] NewGameRequest req)
        {
            using (await DB())
            {
                return "aaaaaa";
            }
        }

    }
}