using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.UI.Models
{
    public interface ITextGrigDataModel
    {
        event Action<int> RowChanged;

        event Action<int, int, string> CellChanged;

        event Action<int> RowInserted;

        event Action<int> RowRemoved;

        int Rows { get; }
        string[] Columns { get; }

        string[] GetRowValues(int row);
    }

    public class TextGrigDataModel : ITextGrigDataModel
    {
        public event Action<int> RowChanged;

        public event Action<int, int, string> CellChanged;

        public event Action<int> RowInserted;

        public event Action<int> RowRemoved;

        private List<string[]> data = new List<string[]>();

        public int Rows
        {
            get { return data.Count; }
        }

        public string[] Columns
        {
            get;
            private set;
        }

        public TextGrigDataModel(params string[] columns)
        {
            Columns = columns;
        }

        public string[] GetRowValues(int row)
        {
            return data[row];
        }

		private string[] TrimValues(string[] values)
		{
			if (values.Length == Columns.Length)
				return values;
			else			
			{
				var result = new string[Columns.Length];
				Array.Copy(values, result, Math.Min(values.Length, Columns.Length));
				return result;
			}
		}

		public void AddRow(params string[] values)
        {
			data.Add(TrimValues(values));

            var handler = RowInserted;

            if (null != handler)
                handler(data.Count - 1);
        }

		public void InsertRow(int index, params string[] values)
        {
			data.Insert(index, TrimValues(values));

            var handler = RowInserted;

            if (null != handler)
                handler(index);
        }

        public void RemoveRow(int index)
        {
            data.RemoveAt(index);

            var handler = RowRemoved;

            if (null != handler)
                handler(index);
        }

		public void UpdateRow(int index, params string[] values)
        {
            data[index] = TrimValues(values);

            var handler = RowChanged;

            if (null != handler)
                handler(index);
        }

        public void Set(int row, int column, string value)
        {
            data[row][column] = value;

            var handler = CellChanged;

            if (null != handler)
                handler(row, column, value);
        }
    }
}
