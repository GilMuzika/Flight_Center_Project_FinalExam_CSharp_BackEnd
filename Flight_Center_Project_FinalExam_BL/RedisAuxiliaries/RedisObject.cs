using System;
using System.Collections.Generic;
using System.Text;

namespace Flight_Center_Project_FinalExam_BL.RedisAuxiliaries
{

    public class RedisObject : IRedisObject
    {
        public DateTime LastUpdateTime { get; set; }
        public string JsonData { get; set; }
    }
    
}
