using System;

namespace Graph.Farm.Website
{
    public static class Settings
    {
        public static string MysqlConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN") ?? "server=localhost;user id=graphfarm;password=qwertyuiop;port=3306;database=graph-farm;";
        public static string HashIDSalt = Environment.GetEnvironmentVariable("HASHID_SALT") ?? "insecure-salt-changeme";
    }
}
