using System;

namespace Utilities
{
    using System;

    public static class MiscUtility
    {
        public static DataTable GetDataTable(this SqlCommand command)
        {
            using var adapter = new SqlDataAdapter(command);

            var table = new DataTable();
            adapter.Fill(table);

            return table;
        }
    }
}
