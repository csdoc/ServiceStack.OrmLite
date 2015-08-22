using System;
using System.Data;

namespace ServiceStack.OrmLite
{
    public interface IOrmLiteConverter
    {
        IOrmLiteDialectProvider DialectProvider { get; set; }
        
        DbType DbType { get; }

        string ColumnDefinition { get; }

        string ToQuotedString(Type fieldType, object value);

        void InitDbParam(IDbDataParameter p, Type fieldType);

        object ToDbValue(Type fieldType, object value);

        object FromDbValue(Type fieldType, object value);

        object GetValue(IDataReader reader, int columnIndex);
    }

    public interface IHasCustomColumnDefinition
    {
        string GetColumnDefinition(int? length);
    }

    public abstract class OrmLiteConverter : IOrmLiteConverter
    {
        /// <summary>
        /// RDBMS Dialect this Converter is for. Injected at registration.
        /// </summary>
        public IOrmLiteDialectProvider DialectProvider { get; set; }

        /// <summary>
        /// SQL Column Definiton used in CREATE Table. 
        /// </summary>
        public abstract string ColumnDefinition { get; }

        /// <summary>
        /// Used in DB Params. Defaults to DbType.String
        /// </summary>
        public virtual DbType DbType
        {
            get { return DbType.String; }
        }

        /// <summary>
        /// Quoted Value in SQL Statement
        /// </summary>
        public virtual string ToQuotedString(Type fieldType, object value)
        {
            return DialectProvider.GetQuotedValue(value.ToString());
        }

        /// <summary>
        /// Customize how DB Param is initialized. Useful for supporting RDBMS-specific Types.
        /// </summary>
        public virtual void InitDbParam(IDbDataParameter p, Type fieldType)
        {
            p.DbType = DbType;
        }

        /// <summary>
        /// Parameterized value in parameterized queries
        /// </summary>
        public virtual object ToDbValue(Type fieldType, object value)
        {
            return value;
        }

        /// <summary>
        /// Value from DB to Populate on POCO Data Model with
        /// </summary>
        public virtual object FromDbValue(Type fieldType, object value)
        {
            return value;
        }

        /// <summary>
        /// Retrieve Value from ADO.NET IDataReader. Defaults to reader.GetValue()
        /// </summary>
        public virtual object GetValue(IDataReader reader, int columnIndex)
        {
            return reader.GetValue(columnIndex);
        }
    }

    /// <summary>
    /// For Types that are natively supported by RDBMS's and shouldn't be quoted
    /// </summary>
    public abstract class NativeValueOrmLiteConverter : OrmLiteConverter
    {
        public override string ToQuotedString(Type fieldType, object value)
        {
            return value.ToString();
        }
    }

    public static class OrmLiteConverterExtensions
    {
        public static object ConvertNumber(this IOrmLiteConverter converter, Type toIntegerType, object value)
        {
            var typeCode = toIntegerType.GetUnderlyingTypeCode();
            switch (typeCode)
            {
                case TypeCode.Int16:
                    return value is short ? value : Convert.ToInt16(value);
                case TypeCode.UInt16:
                    return value is ushort ? value : Convert.ToUInt16(value);
                case TypeCode.Int32:
                    return value is int ? value : Convert.ToInt32(value);
                case TypeCode.UInt32:
                    return value is uint ? value : Convert.ToUInt32(value);
                case TypeCode.Int64:
                    return value is long ? value : Convert.ToInt64(value);
                case TypeCode.UInt64:
                    if (value is ulong)
                        return value;
                    var byteValue = value as byte[];
                    if (byteValue != null)
                        return OrmLiteUtils.ConvertToULong(byteValue);
                    return Convert.ToUInt64(value);
                case TypeCode.Single:
                    return value is float ? value : Convert.ToSingle(value);
                case TypeCode.Double:
                    return value is double ? value : Convert.ToDouble(value);
                case TypeCode.Decimal:
                    return value is decimal ? value : Convert.ToDecimal(value);
            }

            var convertedValue = converter.DialectProvider.StringSerializer.DeserializeFromString(value.ToString(), toIntegerType);
            return convertedValue;
        }
    }
}