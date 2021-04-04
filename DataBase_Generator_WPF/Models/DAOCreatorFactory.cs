using Flight_Center_Project_FinalExam_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataBase_Generator_WPF.Models
{
    class DAOCreatorFactory<T> where T : class, IPoco, new()
    {
        private delegate DAO<T> createAppropriateDAOInstance();
        private delegate UserBaseMSSQLDAO<T> createAppropriateDAOInstanceWithUlitity_class_User();
        public DAO<T> CreateAppropriateDAO()
        {

            Dictionary<Type, createAppropriateDAOInstance> pocoDaoCorrelation = new Dictionary<Type, createAppropriateDAOInstance>();
            pocoDaoCorrelation.Add(typeof(Country), () => { return new CountryDAOMSSQL<Country>() as DAO<T>; });
            pocoDaoCorrelation.Add(typeof(Flight), () => { return new FlightDAOMSSQL<Flight>() as DAO<T>; });
            pocoDaoCorrelation.Add(typeof(Ticket), () => { return new TicketDAOMSSQL<Ticket>() as DAO<T>; });


            return pocoDaoCorrelation[typeof(T)]();

        }
        public UserBaseMSSQLDAO<T> CreateAppropriateDAO_WithUtility_class_User()
        {
            Dictionary<Type, createAppropriateDAOInstanceWithUlitity_class_User> pocoDaoCorrelation = new Dictionary<Type, createAppropriateDAOInstanceWithUlitity_class_User>();
            pocoDaoCorrelation.Add(typeof(Administrator), () => { return new AdministratorDAOMSSQL<Administrator>() as UserBaseMSSQLDAO<T>; });
            pocoDaoCorrelation.Add(typeof(AirlineCompany), () => { return new AirlineDAOMSSQL<AirlineCompany>() as UserBaseMSSQLDAO<T>; });
            pocoDaoCorrelation.Add(typeof(Customer), () => { return new CustomerDAOMSSQL<Customer>() as UserBaseMSSQLDAO<T>; });
            pocoDaoCorrelation.Add(typeof(Utility_class_User), () => { return new Utility_class_UserDAOMSSQL<Utility_class_User>() as UserBaseMSSQLDAO<T>; });
            return pocoDaoCorrelation[typeof(T)]();
        }

        private bool isIUser()
        {
            return typeof(T).GetProperty("USER_ID") == null ? false : true;
        }

        //Selection isn't working, the reason is unknown
        public IBasicDB<T> SelectAppropriateDAO()
        {
            if (isIUser())
            {
                UserBaseMSSQLDAO<T> dao = CreateAppropriateDAO_WithUtility_class_User();
                return dao;
            }
            else
            {
                DAO<T> dao = CreateAppropriateDAO();
                return dao;
            }
        }

    }
}
