using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace Flight_Center_Project_FinalExam_BL.RedisAuxiliaries
{
    public static class RedisDataAccess
    {
        private static ConnectionMultiplexer redis;// = ConnectionMultiplexer.Connect("localhost");
        private static IDatabase redisDb;

        private static void TryToConnect()
        {

            try
            {
                redis = ConnectionMultiplexer.Connect("localhost");
                if (redis != null)
                    redisDb = redis.GetDatabase();
            }
            catch(RedisConnectionException)
            {
                startProcess();
                TryToConnect();
            }
        }

        private static void startProcess()
        {
            ProcessStartInfo psi = new ProcessStartInfo(@"e:\source\repos\Flight_Center_Project_FinalExam\_Redis\64bit\redis-server.exe");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            Process process = Process.Start(psi);

            Thread.Sleep(100);

        }

        static RedisDataAccess()
        {
            TryToConnect();
        }


        public static bool SaveWithTimeStamp(string key, string value)
        {
            bool isSuccess = false;
            
            isSuccess = redisDb.StringSet(key,
                JsonConvert.SerializeObject(new RedisObject()
                { JsonData = value, LastUpdateTime = DateTime.Now }
                ));
            
            return isSuccess;
        }


        public static IRedisObject GetWithTimeStamp(string key)
        {
            string result = null;

                result = redisDb.StringGet(key);
                    if (result != null)
                        return JsonConvert.DeserializeObject<RedisObject>(result);
                    return null;

        }

    }
}
