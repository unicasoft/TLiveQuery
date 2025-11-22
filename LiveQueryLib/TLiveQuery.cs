
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace LiveQueryLib
{
    [Flags]
    public enum LocateOptions
    {
        None = 0,
        CaseInsensitive = 1,
        PartialKey = 2
    }

    public class Bookmark
    {
        public int Position { get; set; }
    }

    public class TLiveQuery
    {
        public SqlConnection Connection { get; set; }
        public string SQL { get; set; }
        public TParams Params { get; private set; }
        public BindingSource Binding { get; private set; }
        public DataTable Table { get; private set; }
        public TFieldDefs FieldDefs { get; private set; }
        public DatasetState State { get; private set; }

        public event Action<DataRow> OnCalcFields;
        public event Action OnAfterScroll;

        public bool AutoCalcFields { get; set; }

        public string FilterExpression { get; private set; }
        public bool Filtered { get; private set; }
        public string OrderByExpression { get; private set; }

        private string _rangeField;
        private object _rangeStart;
        private object _rangeEnd;
        private bool _rangeActive;

        public TLiveQuery(string sql, SqlConnection conn)
        {
            SQL = sql;
            Connection = conn;
            Params = new TParams();
            FieldDefs = new TFieldDefs();
            State = DatasetState.Inactive;
            AutoCalcFields = true;
        }

        public void Close()
        {
            if (Binding != null)
            {
                Binding.PositionChanged -= Binding_PositionChanged;
            }
            if (Table != null)
            {
                Table.RowChanged -= Table_RowChanged;
                Table.RowDeleted -= Table_RowChanged;
            }
            Binding = null;
            Table = null;
            FieldDefs.Clear();
            State = DatasetState.Inactive;
        }

        public void Open()
        {
            if (Connection == null)
                throw new InvalidOperationException("Connection is null.");

            Params.ParseFromSQL(SQL);
            string sqlText = Params.NormalizeSQL(SQL);

            SqlDataAdapter da = new SqlDataAdapter(sqlText, Connection);
            Table = new DataTable();
            da.Fill(Table);

            BuildFieldDefs();

            Binding = new BindingSource();
            Binding.DataSource = Table;
            Binding.PositionChanged += Binding_PositionChanged;

            Table.RowChanged += Table_RowChanged;
            Table.RowDeleted += Table_RowChanged;

            State = DatasetState.Browse;
            DoCalcFields(CurrentRow);
        }

        private void BuildFieldDefs()
        {
            FieldDefs.Clear();
            if (Table == null) return;
            int idx = 0;
            foreach (DataColumn col in Table.Columns)
            {
                FieldDefs.Add(new TFieldDef
                {
                    Name = col.ColumnName,
                    DataType = col.DataType,
                    Size = col.MaxLength,
                    AllowNull = col.AllowDBNull,
                    AutoIncrement = col.AutoIncrement,
                    Index = idx++
                });
            }
        }

        void Binding_PositionChanged(object sender, EventArgs e)
        {
            DoCalcFields(CurrentRow);
            if (OnAfterScroll != null)
                OnAfterScroll();
        }

        void Table_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (AutoCalcFields && e.Row.RowState != DataRowState.Deleted)
            {
                DoCalcFields(e.Row);
            }
        }

        private void DoCalcFields(DataRow row)
        {
            if (!AutoCalcFields) return;
            if (row == null) return;
            if (OnCalcFields != null)
                OnCalcFields(row);
        }

        public DataRow CurrentRow
        {
            get
            {
                if (Binding == null || Binding.Current == null) return null;
                DataRowView rv = Binding.Current as DataRowView;
                return rv == null ? null : rv.Row;
            }
        }

        public int RecNo
        {
            get { return Binding == null ? 0 : Binding.Position + 1; }
        }

        public int RecordCount
        {
            get { return Binding == null ? 0 : Binding.Count; }
        }

        public bool Eof
        {
            get { return Binding == null || Binding.Position >= Binding.Count - 1; }
        }

        public bool Bof
        {
            get { return Binding == null || Binding.Position <= 0; }
        }

        public void Append()
        {
            if (Table == null)
                throw new InvalidOperationException("Table is null. Call Open first.");

            DataRow row = Table.NewRow();
            Table.Rows.Add(row);
            Binding.Position = Table.Rows.Count - 1;
            State = DatasetState.Insert;
            DoCalcFields(row);
        }

        public void Edit()
        {
            if (CurrentRow == null)
                throw new InvalidOperationException("No current row.");
            State = DatasetState.Edit;
        }

        public void Cancel()
        {
            if (CurrentRow == null) return;

            if (State == DatasetState.Insert)
            {
                CurrentRow.Delete();
            }
            else if (State == DatasetState.Edit)
            {
                CurrentRow.RejectChanges();
            }

            State = DatasetState.Browse;
        }

        public void Post()
        {
            if (Table == null)
                throw new InvalidOperationException("Table is null.");

            Params.ParseFromSQL(SQL);
            string sqlText = Params.NormalizeSQL(SQL);

            SqlDataAdapter da = new SqlDataAdapter(sqlText, Connection);
            SqlCommandBuilder cb = new SqlCommandBuilder(da);
            cb.SetAllValues = false;

            da.RowUpdated += Da_RowUpdated;

            da.Update(Table);

            Table.AcceptChanges();
            State = DatasetState.Browse;
        }

        private void Da_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
            {
                // try to get identity column name
                string tableName = ExtractTableNameFromSQL(SQL);
                if (string.IsNullOrEmpty(tableName))
                    return;

                string identityColumn = null;

                string identitySQL = @"
                    SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = @table
                      AND COLUMNPROPERTY(OBJECT_ID(TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1";

                using (var cmd = new SqlCommand(identitySQL, Connection))
                {
                    cmd.Parameters.AddWithValue("@table", tableName);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        identityColumn = result.ToString();
                }

                if (!string.IsNullOrEmpty(identityColumn))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT SCOPE_IDENTITY()", Connection))
                    {
                        object newId = cmd.ExecuteScalar();
                        if (newId != null && newId != DBNull.Value)
                        {
                            e.Row[identityColumn] = Convert.ToInt32(newId);
                        }
                    }
                }
            }
        }

        private string ExtractTableNameFromSQL(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return null;
            var match = Regex.Match(sql, @"from\s+([\[\]\w]+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string name = match.Groups[1].Value;
                name = name.Trim().Trim('[', ']');
                return name;
            }
            return null;
        }

        public void Delete()
        {
            if (CurrentRow == null) return;
            CurrentRow.Delete();
            Post();
        }

        public DataField FieldByName(string name)
        {
            if (CurrentRow == null) return null;
            if (!Table.Columns.Contains(name)) return null;
            return new DataField(CurrentRow, Table.Columns[name]);
        }

        public DataField FieldByIndex(int index)
        {
            if (CurrentRow == null) return null;
            if (index < 0 || index >= Table.Columns.Count) return null;
            return new DataField(CurrentRow, Table.Columns[index]);
        }

        public Bookmark GetBookmark()
        {
            if (Binding == null) return null;
            return new Bookmark { Position = Binding.Position };
        }

        public void GotoBookmark(Bookmark bm)
        {
            if (Binding == null || bm == null) return;
            if (bm.Position >= 0 && bm.Position < Binding.Count)
                Binding.Position = bm.Position;
        }

        public bool Locate(string fieldName, object value, LocateOptions options)
        {
            if (Table == null || !Table.Columns.Contains(fieldName))
                return false;

            StringComparison cmp = (options & LocateOptions.CaseInsensitive) != 0
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            for (int i = 0; i < Table.Rows.Count; i++)
            {
                DataRow row = Table.Rows[i];
                object cell = row[fieldName];
                if (cell == DBNull.Value) continue;
                if (value == null) continue;

                string sCell = cell.ToString();
                string sVal = value.ToString();

                bool match;
                if ((options & LocateOptions.PartialKey) != 0)
                    match = sCell.StartsWith(sVal, cmp);
                else
                    match = string.Equals(sCell, sVal, cmp);

                if (match)
                {
                    Binding.Position = i;
                    return true;
                }
            }
            return false;
        }

        public void SetFilter(string expression)
        {
            FilterExpression = expression;
            Filtered = !string.IsNullOrWhiteSpace(FilterExpression);
            ApplyFilterRangeSort();
        }

        public void SetFiltered(bool value)
        {
            Filtered = value;
            ApplyFilterRangeSort();
        }

        public void SetRange(string fieldName, object startValue, object endValue)
        {
            _rangeField = fieldName;
            _rangeStart = startValue;
            _rangeEnd = endValue;
            _rangeActive = true;
            ApplyFilterRangeSort();
        }

        public void CancelRange()
        {
            _rangeActive = false;
            ApplyFilterRangeSort();
        }

        public void OrderBy(string expression)
        {
            OrderByExpression = ParseOrderByExpression(expression);
            ApplyFilterRangeSort();
        }

        private void ApplyFilterRangeSort()
        {
            if (Table == null) return;

            string filter = "";

            if (Filtered && !string.IsNullOrWhiteSpace(FilterExpression))
                filter = "(" + FilterExpression + ")";

            if (_rangeActive && !string.IsNullOrEmpty(_rangeField) && Table.Columns.Contains(_rangeField))
            {
                string rangeExpr = BuildRangeExpression();
                if (!string.IsNullOrEmpty(rangeExpr))
                {
                    if (!string.IsNullOrEmpty(filter))
                        filter += " AND ";
                    filter += rangeExpr;
                }
            }

            try
            {
                Table.DefaultView.RowFilter = filter;
            }
            catch
            {
                Table.DefaultView.RowFilter = "";
            }

            try
            {
                Table.DefaultView.Sort = OrderByExpression;
            }
            catch
            {
                Table.DefaultView.Sort = "";
            }
        }

        private string BuildRangeExpression()
        {
            if (!_rangeActive) return null;

            DataColumn col = Table.Columns[_rangeField];
            if (col == null) return null;

            string expr = "";

            if (_rangeStart != null)
                expr += "[" + _rangeField + "] >= " + FormatValueForFilter(col, _rangeStart);

            if (_rangeEnd != null)
            {
                if (!string.IsNullOrEmpty(expr))
                    expr += " AND ";
                expr += "[" + _rangeField + "] <= " + FormatValueForFilter(col, _rangeEnd);
            }

            return expr;
        }

        private string FormatValueForFilter(DataColumn col, object value)
        {
            if (value == null || value == DBNull.Value)
                return "NULL";

            Type t = col.DataType;

            if (t == typeof(string) || t == typeof(char))
                return "'" + value.ToString().Replace("'", "''") + "'";

            if (t == typeof(DateTime))
            {
                DateTime dt = Convert.ToDateTime(value);
                return "#" + dt.ToString("yyyy-MM-dd") + "#";
            }

            if (t == typeof(bool))
                return ((bool)value) ? "TRUE" : "FALSE";

            if (t == typeof(Guid))
                return "'" + value.ToString() + "'";

            if (t == typeof(byte) || t == typeof(short) || t == typeof(int) ||
                t == typeof(long) || t == typeof(float) || t == typeof(double) ||
                t == typeof(decimal))
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            return "'" + value.ToString().Replace("'", "''") + "'";
        }

        private string ParseOrderByExpression(string expr)
        {
            if (string.IsNullOrWhiteSpace(expr))
                return "";

            var parts = expr.Split(',');
            List<string> result = new List<string>();

            foreach (var p in parts)
            {
                string part = p.Trim();
                if (string.IsNullOrEmpty(part)) continue;

                var tokens = part.Split(' ');
                string col = tokens[0].Trim();
                string direction = "ASC";

                if (tokens.Length > 1)
                {
                    if (tokens[1].Equals("DESC", StringComparison.OrdinalIgnoreCase))
                        direction = "DESC";
                    else if (tokens[1].Equals("ASC", StringComparison.OrdinalIgnoreCase))
                        direction = "ASC";
                }

                result.Add("[" + col + "] " + direction);
            }

            return string.Join(", ", result);
        }
    }
}
