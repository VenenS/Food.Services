using Microsoft.EntityFrameworkCore;
using System;

namespace Food.Data
{
    /// <summary>
    /// Методы в DbFunctions работают только с LINQ to Entities, использование этих методов с
    /// LINQ to Objects (например в юнит-тестах) приводит к исключению System.NotSupportedException.
    /// 
    /// Класс DbShim реализует методы из DbFunctions и, тем самым, позволет коду, использующему LINQ
    /// с C# объектами, работать без исключений.
    /// </summary>
    public class DbShim
    {
        [DbFunction("Edm", "TruncateTime")]
        public static DateTime? TruncateTime(DateTime? date)
        {
            return date?.Date;
        }
    }
}
