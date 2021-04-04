﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Center_Project_FinalExam_DAL.IDAL
{
    public interface IHistoryTrackingDAO<T> : IBasicDB<T> where T : class, IPoco, new()
    {
    }
}
