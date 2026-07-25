using System;
using System.Data;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Database
{
    public class SqlDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly date)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = date.ToDateTime(new TimeOnly(0, 0));
        }

        public override DateOnly Parse(object value)
        {
            if (value is DateTime dateTime)
            {
                return DateOnly.FromDateTime(dateTime);
            }
            return (DateOnly)value;
        }
    }
}
