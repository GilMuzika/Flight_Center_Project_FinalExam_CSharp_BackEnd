using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Web_Api_interface.Comparers
{
    /// <summary>
    /// compares two classes of the same type by string-valued proprty of this type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ComparerByTimeValuedProperty<T> : IComparer<T>
    {
        private string _properyName;

        private object _propertyValue1;
        private object _propertyValue2;


        /// <summary>
        /// Takes the name of the property by wich the compasion should be done
        /// </summary>
        /// <param name="properyName">Name of the property by wich the compasion should be done</param>
        public ComparerByTimeValuedProperty(string properyName)
        {
            _properyName = properyName;
        }        

        public int Compare(T x, T y) 
        {
            PropertyInfo propInfo = typeof(T)?.GetProperty(_properyName);
            if (propInfo == null)
                throw new ArgumentException($"The type \"{typeof(T).Name}\" doesn't have any property named \"{_properyName}\"");


            if (!IsDateTime(propInfo.PropertyType) && !IsTimeSpan(propInfo.PropertyType))
                throw new ArgumentException($"The property \"{_properyName}\" nust be \"DateTime\" or \"TimeSpan\"! Its actual type is {propInfo.PropertyType.Name}");            


            _propertyValue1 = propInfo.GetValue(x);
            _propertyValue2 = propInfo.GetValue(y);

             if(IsDateTime(propInfo.PropertyType))
                return DateTime.Compare((DateTime)_propertyValue1, (DateTime)_propertyValue2);


             return TimeSpan.Compare((TimeSpan)_propertyValue1, (TimeSpan)_propertyValue2);
        }


        public override string ToString()
        {
            return $"[current comparsion by a property named \"{_properyName}\", type \"DateTime\"]";
        }



        public static bool IsDateTime(Type t)
        {
            return t.Name.Equals("DateTime");
        }
        public static bool IsTimeSpan(Type t)
        {
            return t.Name.Equals("TimeSpan");
        }


    }
}
