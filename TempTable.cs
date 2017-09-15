using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CheckShippingStatus.Domain;
using Microsoft.SqlServer.Server;

namespace CheckShippingStatus.Services
{
    /// <summary>
    /// This class is used for user defined table type paratmers. This class takes a collection of
    /// a data model/enitity and then enumerates it as sqlrecords when called. 
    /// 
    /// There's a particualr option to have a mock idenitityColum where your first column is an 
    /// int, index and is not an idenitity column but you
    /// would like it to act as one with the standerd: IDENDITTY (1,1). 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TempTable<T> : List<T>, IEnumerable<SqlDataRecord>
    {

        #region Private Methods, Fields

        private int maxSqlStringLength = 2000;
        private bool mockIdenitityColum = false;

        private SqlMetaData[] MakeMetaDataArray(PropertyInfo[] properties)
        {
            SqlMetaData[] metaDataArray = new SqlMetaData[properties.Length];
            for (int index = 0; index < properties.Length; index++)
            {
                if (properties[index].PropertyType != typeof(string))
                {
                    metaDataArray[index] = (new SqlMetaData(properties[index].Name
                        , GetClrType(properties[index].PropertyType)));
                }
                else
                {
                    metaDataArray[index] = (new SqlMetaData(properties[index].Name
                        , GetClrType(properties[index].PropertyType), maxSqlStringLength));
                }
            }
            return metaDataArray;
        }

        private static SqlDbType GetClrType(Type sqlType)
        {
            switch (sqlType.Name)
            {
                case "Int64": //SqlDbType.BigInt:
                    return SqlDbType.BigInt;

                //case SqlDbType.Binary:
                //case SqlDbType.Image:
                //case SqlDbType.Timestamp:
                //case SqlDbType.VarBinary:
                //    return typeof(byte[]);

                case "Boolean":
                    return SqlDbType.Bit;

                //case SqlDbType.Char:
                //case SqlDbType.NChar:
                //case SqlDbType.NText:
                //case SqlDbType.NVarChar:
                //case SqlDbType.Text:
                //case SqlDbType.VarChar:
                //case SqlDbType.Xml:
                case "String":
                    return SqlDbType.NVarChar;

                //case SqlDbType.DateTime:
                //case SqlDbType.SmallDateTime:
                //case SqlDbType.Date:
                //case SqlDbType.Time:
                //case SqlDbType.DateTime2:
                //return typeof(DateTime?);
                case "DateTime":
                    return SqlDbType.DateTime2;

                //case SqlDbType.Decimal:
                //case SqlDbType.Money:
                //case SqlDbType.SmallMoney:
                //    return typeof(decimal?);


                case "Int32":
                    return SqlDbType.Int;

                case "Double":
                case "Single":
                    return SqlDbType.Float;

                //case SqlDbType.Real:
                //    return typeof(float?);

                //case SqlDbType.UniqueIdentifier:
                //    return typeof(Guid?);

                //case SqlDbType.SmallInt:
                //    return typeof(short?);

                //case SqlDbType.TinyInt:
                //    return typeof(byte?);

                //case SqlDbType.Variant:
                //case SqlDbType.Udt:
                //    return typeof(object);

                //case SqlDbType.Structured:
                //    return typeof(DataTable);

                //case SqlDbType.DateTimeOffset:
                //    return typeof(DateTimeOffset?);

                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }

        #endregion

        public TempTable()
        {
        }
        public TempTable(IEnumerable<T> collection)
        {
            this.AddRange(collection);
        }

        public TempTable(IEnumerable<T> collection, bool hasmockIdenitityColum)
        {
            this.AddRange(collection);
            mockIdenitityColum = hasmockIdenitityColum;
        }

        public TempTable(IEnumerable<T> collection, int maxSqlColStringLength)
        {
            this.AddRange(collection);
            maxSqlStringLength = maxSqlColStringLength;
        }

        public TempTable(IEnumerable<T> collection, int maxSqlColStringLength, bool hasmockIdenitityColum)
        {
            this.AddRange(collection);
            maxSqlStringLength = maxSqlColStringLength;
            mockIdenitityColum = hasmockIdenitityColum;
        }

        /// <summary>
        ///  set the maximum string length for your columns. So you look
        ///  at your columns and see which one is the longest and set the property MaxSqlStringLength to that.
        /// </summary>
        public int MaxSqlStringLength { get
            {
                return maxSqlStringLength;
            }
            set
            {
                maxSqlStringLength = value;
            }
        }

        /// <summary>
        /// A particualr option to have a mock idenitityColum where your first column is an 
        /// int, index and is not an idenitity column but you
        /// would like it to act as one with the standerd: IDENDITTY (1,1). 
        /// </summary>
        public bool HasMockIdenitityColumn { get
            {
                return mockIdenitityColum;
            } set
            {
                mockIdenitityColum = value;
            }
             }

        IEnumerator<SqlDataRecord> IEnumerable<SqlDataRecord>.GetEnumerator()
        {

            SqlDataRecord record = null;
            PropertyInfo[] properties = typeof(T).GetProperties();
            //int propCount = properties.Length;

            SqlMetaData[] metaDataArray = MakeMetaDataArray(properties);

            PropertyDescriptor [] propertyDescriptorCollection = TypeDescriptor
                .GetProperties(typeof(T))
                .Cast<PropertyDescriptor>()
                .ToArray();

            int id = 0;

            foreach (T data in this)
            {
                record = new SqlDataRecord(metaDataArray);
                for (int i = 0; i < properties.Length; i++)
                {
                    object obj = propertyDescriptorCollection[i].GetValue(data) == null 
                        && propertyDescriptorCollection[i].PropertyType == typeof(string)
                        ? String.Empty
                        : (i == 0 && mockIdenitityColum )
                        ? id += 1
                        : propertyDescriptorCollection[i].GetValue(data);


                    if (properties[i].PropertyType.Name == "Single")
                        record.SetValue(i, (double)(float)obj);
                    else
                        record.SetValue(i, obj);
                }
                yield return record;
            }

        }

        

    }
}
