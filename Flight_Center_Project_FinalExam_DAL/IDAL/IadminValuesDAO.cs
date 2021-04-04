﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Center_Project_FinalExam_DAL.IDAL
{
    interface IadminValuesDAO<T> : IBasicDB<T> where T : class, IPoco, new()
    {
    }
}
