
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Drawing;

namespace LiveQueryLib
{
    public enum ParamDirection
    {
        Input,
        Output,
        InputOutput
    }

    public class TParam
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public ParamDirection Direction { get; set; }

        public TParam()
        {
            Direction = ParamDirection.Input;
        }

        public string AsString
        {
            get { return Value == null || Value == DBNull.Value ? string.Empty : Value.ToString(); }
            set { Value = value; }
        }

        public string AsWideString
        {
            get { return AsString; }
            set { Value = value; }
        }

        public int AsInteger
        {
            get { return Value == null || Value == DBNull.Value ? 0 : Convert.ToInt32(Value); }
            set { Value = value; }
        }

        public int AsInt32
        {
            get { return AsInteger; }
            set { AsInteger = value; }
        }

        public long AsLargeInt
        {
            get { return Value == null || Value == DBNull.Value ? 0L : Convert.ToInt64(Value); }
            set { Value = value; }
        }

        public bool AsBoolean
        {
            get { return Value != null && Value != DBNull.Value && Convert.ToBoolean(Value); }
            set { Value = value; }
        }

        public double AsFloat
        {
            get { return Value == null || Value == DBNull.Value ? 0.0 : Convert.ToDouble(Value); }
            set { Value = value; }
        }

        public DateTime AsDateTime
        {
            get { return Value == null || Value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(Value); }
            set { Value = value; }
        }

        public Guid AsGuid
        {
            get
            {
                if (Value == null || Value == DBNull.Value) return Guid.Empty;
                if (Value is Guid) return (Guid)Value;
                return Guid.Parse(Value.ToString());
            }
            set { Value = value; }
        }

        public byte[] AsBytes
        {
            get
            {
                if (Value == null || Value == DBNull.Value) return null;
                if (Value is byte[]) return (byte[])Value;
                return Value as byte[];
            }
            set { Value = value; }
        }

        public MemoryStream AsStream
        {
            get
            {
                var bytes = AsBytes;
                if (bytes == null) return null;
                return new MemoryStream(bytes);
            }
            set
            {
                if (value == null) Value = DBNull.Value;
                else Value = value.ToArray();
            }
        }

        public string AsText
        {
            get
            {
                var bytes = AsBytes;
                if (bytes == null) return string.Empty;
                return Encoding.UTF8.GetString(bytes);
            }
            set
            {
                if (value == null) Value = DBNull.Value;
                else Value = Encoding.UTF8.GetBytes(value);
            }
        }

        public Image AsImage
        {
            get
            {
                var stream = AsStream;
                if (stream == null) return null;
                return Image.FromStream(stream);
            }
            set
            {
                if (value == null)
                {
                    Value = DBNull.Value;
                    return;
                }
                using (var ms = new MemoryStream())
                {
                    value.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    Value = ms.ToArray();
                }
            }
        }

        public object AsVariant
        {
            get { return Value; }
            set { Value = value ?? DBNull.Value; }
        }
    }

    public class TParams
    {
        private readonly List<TParam> _items = new List<TParam>();

        public IList<TParam> Items
        {
            get { return _items.AsReadOnly(); }
        }

        public TParam ParamByName(string name)
        {
            var p = _items.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (p == null)
            {
                p = new TParam { Name = name };
                _items.Add(p);
            }
            return p;
        }

        public void Clear()
        {
            _items.Clear();
        }

        public void ParseFromSQL(string sql)
        {
            Clear();
            if (string.IsNullOrEmpty(sql)) return;

            char[] separators = { ' ', '\r', '\n', '\t', ',', ')', '(', ';' };
            string[] tokens = sql.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                if (token.StartsWith(":") || token.StartsWith("@"))
                {
                    string name = token.Substring(1);
                    if (!string.IsNullOrEmpty(name))
                        ParamByName(name);
                }
            }
        }

        public string NormalizeSQL(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return sql;
            foreach (var p in _items)
            {
                sql = sql.Replace(":" + p.Name, "@" + p.Name);
            }
            return sql;
        }

        public void BindTo(SqlCommand cmd)
        {
            foreach (var p in _items)
            {
                string sqlName = "@" + p.Name;
                SqlParameter parameter;
                if (cmd.Parameters.Contains(sqlName))
                    parameter = cmd.Parameters[sqlName];
                else
                    parameter = cmd.Parameters.AddWithValue(sqlName, p.Value ?? DBNull.Value);

                switch (p.Direction)
                {
                    case ParamDirection.Input:
                        parameter.Direction = ParameterDirection.Input;
                        break;
                    case ParamDirection.Output:
                        parameter.Direction = ParameterDirection.Output;
                        break;
                    case ParamDirection.InputOutput:
                        parameter.Direction = ParameterDirection.InputOutput;
                        break;
                }
            }
        }

        public void ReadBack(SqlCommand cmd)
        {
            foreach (SqlParameter sqlParam in cmd.Parameters)
            {
                string name = sqlParam.ParameterName.TrimStart('@');
                var p = _items.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (p != null)
                {
                    p.Value = sqlParam.Value;
                }
            }
        }
    }
}
