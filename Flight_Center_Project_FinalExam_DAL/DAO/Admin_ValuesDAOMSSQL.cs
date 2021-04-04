using Flight_Center_Project_FinalExam_DAL.IDAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Center_Project_FinalExam_DAL
{
    public class Admin_ValuesDAOMSSQL<T> : DAO<T>, IadminValuesDAO<T> where T : Admin_Value, IPoco, new()
    {
        public Admin_ValuesDAOMSSQL(): base() { }

    }
}
