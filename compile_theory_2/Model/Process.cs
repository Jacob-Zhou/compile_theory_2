using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compile_theory_2.Model
{
	class Detail
	{
		public Detail(string production, int offset = 0, int length = 0)
		{
			this.production = production;
			this.offset = offset;
			this.length = length;
		}

		public string production { get; set; }
		public int offset { get; set; }
		public int length { get; set; }
	}

	class Process : INotifyPropertyChanged
	{
		public Process(string result, string production)
		{
			this.result = result;
			detail = new ObservableCollection<Detail>();
			detail.Add(new Detail(production));
		}
		private bool IsExpanded;
		public bool isExpanded
		{
			get { return IsExpanded; }
			set
			{
				IsExpanded = value;
				OnPropertyChanged(new PropertyChangedEventArgs("isExpanded"));
			}
		}
		private string Result;
		public string result
		{
			get { return Result; }
			set
			{
				Result = value;
				OnPropertyChanged(new PropertyChangedEventArgs("result"));
			}
		}
		private ObservableCollection<Detail> Detail;
		public ObservableCollection<Detail> detail
		{
			get { return Detail; }
			set
			{
				Detail = value;
				OnPropertyChanged(new PropertyChangedEventArgs("detail"));
			}
		}

		public void SetOffsetAddLength(int offset, int length)
		{
			detail[0].offset = offset;
			detail[0].length = length;
			OnPropertyChanged(new PropertyChangedEventArgs("detail"));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}
	}
}
