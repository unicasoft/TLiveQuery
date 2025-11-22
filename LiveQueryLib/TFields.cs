
using System;
using System.Data;
using System.Collections.Generic;

namespace LiveQueryLib
{
    public enum DatasetState
    {
        Inactive,
        Browse,
        Edit,
        Insert
    }

    public class TFieldDef
    {
        public string Name { get; set; }
        public Type DataType { get; set; }
        public int Size { get; set; }
        public bool AllowNull { get; set; }
        public bool AutoIncrement { get; set; }
        public int Index { get; set; }
    }

    public class TFieldDefs : List<TFieldDef>
    {
        public TFieldDef this[string name]
        {
            get { return this.Find(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)); }
        }
    }

    public class DataField
    {
        private readonly DataRow _row;
        private readonly DataColumn _col;

        public DataField(DataRow row, DataColumn col)
        {
            _row = row;
            _col = col;
        }

        public string FieldName
        {
            get { return _col.ColumnName; }
        }

        public string AsString
        {
            get { return _row[_col] == DBNull.Value ? string.Empty : _row[_col].ToString(); }
            set { _row[_col] = value ?? (object)DBNull.Value; }
        }

        public string AsWideString
        {
            get { return AsString; }
            set { AsString = value; }
        }

        public int AsInteger
        {
            get { return _row[_col] == DBNull.Value ? 0 : Convert.ToInt32(_row[_col]); }
            set { _row[_col] = value; }
        }

        public int AsInt32
        {
            get { return AsInteger; }
            set { AsInteger = value; }
        }

        public long AsLargeInt
        {
            get { return _row[_col] == DBNull.Value ? 0L : Convert.ToInt64(_row[_col]); }
            set { _row[_col] = value; }
        }

        public bool AsBoolean
        {
            get { return _row[_col] != DBNull.Value && Convert.ToBoolean(_row[_col]); }
            set { _row[_col] = value; }
        }

        public double AsFloat
        {
            get { return _row[_col] == DBNull.Value ? 0.0 : Convert.ToDouble(_row[_col]); }
            set { _row[_col] = value; }
        }

        public DateTime AsDateTime
        {
            get { return _row[_col] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(_row[_col]); }
            set { _row[_col] = value; }
        }

        public Guid AsGuid
        {
            get
            {
                if (_row[_col] == DBNull.Value) return Guid.Empty;
                if (_row[_col] is Guid) return (Guid)_row[_col];
                return Guid.Parse(_row[_col].ToString());
            }
            set { _row[_col] = value; }
        }

        public byte[] AsBytes
        {
            get
            {
                if (_row[_col] == DBNull.Value) return null;
                return _row[_col] as byte[];
            }
            set { _row[_col] = value ?? (object)DBNull.Value; }
        }

        public object AsVariant
        {
            get { return _row[_col]; }
            set { _row[_col] = value ?? DBNull.Value; }
        }
    }
}
