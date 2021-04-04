using System;
using System.Collections.Generic;
using System.Text;

namespace Flight_Center_Project_FinalExam_BL.RedisAuxiliaries
{
    public interface IRedisObject
    {
        DateTime LastUpdateTime { get; set; }
        string JsonData { get; set; }
    }
}
