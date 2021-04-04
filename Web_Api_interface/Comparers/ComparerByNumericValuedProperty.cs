using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Web_Api_interface.Comparers
{
    /// <summary>
    /// compares two classes of the same type by string-valued proprty of this type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ComparerByNumericValuedProperty<T> : IComparer<T>
    {
        private string _properyName;

        private object _propertyValue1;
        private object _propertyValue2;


        /// <summary>
        /// Takes the name of the property by wich the compasion should be done
        /// </summary>
        /// <param name="properyName">Name of the property by wich the compasion should be done</param>
        public ComparerByNumericValuedProperty(string properyName)
        {
            _properyName = properyName;
        }        

        public int Compare(T x, T y) 
        {
            PropertyInfo propInfo = typeof(T)?.GetProperty(_properyName);
            if (propInfo == null)
                throw new ArgumentException($"The type \"{typeof(T).Name}\" doesn't have any property named \"{_properyName}\"");


            if (!IsNumericType(propInfo.PropertyType))
                throw new ArgumentException($"The property \"{_properyName}\" nust be numeric type! Its actual type is {propInfo.PropertyType.Name}");            


            _propertyValue1 = propInfo.GetValue(x);
            _propertyValue2 = propInfo.GetValue(y);


             return Convert.ToInt32((dynamic)_propertyValue1 - (dynamic)_propertyValue2);
        }


        public override string ToString()
        {
            return $"[current comparsion by a property named \"{_properyName}\", type \"Int32\"]";
        }



        public static bool IsNumericType(Type t)
        {
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }



    }
}
