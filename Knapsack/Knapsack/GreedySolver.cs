using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Knapsack
{
	class GreedySolver : ISolver
	{
		/// <summary>
		/// 初始化问题解决器
		/// </summary>
		/// <param name="console">输出的UI引用</param>
		/// <param name="paras">参数向量</param>
		public void Init(TextBox console, params string[] paras)
		{
			this.UIReference = console;
		}

		/// <summary>
		/// 开始解决问题
		/// </summary>
		/// <param name="testdata">要解决的问题的描述字符串</param>
		public void Solve(string testdata)
		{
			if (this.UIReference == null)
			{
				throw new Exception("问题解决器尚未初始化就被使用");
			}
			// 算法开始
			this.BeginTimeStamp = DateTime.Now;
			// 分析测试数据字符串
			this.testData = testdata;
			string[] lineitem = this.testData.Split(new char[]{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
			this.Capacity = Convert.ToInt32(lineitem[0]);
			// 读取背包大小和物品种类数
			this.ItemTypeCount = Convert.ToInt32(lineitem[1]);
			this.Items = new List<Tuple<int, int, int, double, bool>>();
			// 读取每种物品的属性
			for (int i = 2; i < lineitem.Length; i++)
			{
				var aLine = lineitem[i].Split('\t');
				var aW = Convert.ToInt32(aLine[1]);
				var aV = Convert.ToInt32(aLine[2]);
                this.Items.Add(new Tuple<int, int, int, double, bool>(i - 2, aW, aV, ((double)aV / (double)aW), false));
			}
			// 对单位价值做排序
			this.Items.Sort((x, y) =>
			{
				if (x.Item4 > y.Item4) { return -1; }
				else if (x.Item4 < y.Item4) { return 1; }
				else { return 0; }
			});
			// 解决问题，每步取能放入背包的最大价值物
			int currentWeight = 0;
			bool pickFlag = false;
			this.PickList = new List<int>();
			do
			{
				pickFlag = false;
				for (int i = 0; i < this.Items.Count; i++)
				{
					if (this.Items[i].Item5 != true &&
						(currentWeight + this.Items[i].Item3 <= this.Capacity))
					{
						currentWeight += this.Items[i].Item3;
						// 标记脏位
						this.Items[i] = new Tuple<int, int, int, double, bool>(
							this.Items[i].Item1,
							this.Items[i].Item2,
							this.Items[i].Item3,
							this.Items[i].Item4,
							true);
						// 标记本轮有pick
						pickFlag = true;
						// 记录pick的项目
						this.PickList.Add(i);
                    }
				}
			} while (pickFlag == false);
			this.FinalWeight = currentWeight;
			// 算法结束
			this.EndTimeStamp = DateTime.Now;
        }

		/// <summary>
		/// 获取问题解决的结果
		/// </summary>
		/// <param name="costTime">[out]消耗的时间</param>
		/// <param name="returnDict">[out]返回值的字典</param>
		public void GetResult(out double costTime, out Dictionary<string, string> returnDict)
		{
			if (this.UIReference == null)
			{
				throw new Exception("问题解决器尚未初始化就被使用");
			}
			this.UIReference.Text = "";
			// 耗时
			costTime = (this.EndTimeStamp - this.BeginTimeStamp).TotalMilliseconds;
			Dictionary<string, string> retDict = new Dictionary<string, string>();
			// 选中的项目
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < this.PickList.Count; i++)
			{
				var aItem = this.Items[i];
				var outStr = String.Format("[{0}]\tWeight:{1}\tValue:{2}\tW/V:{3}", aItem.Item1, aItem.Item2, aItem.Item3, aItem.Item4.ToString("0.000"));
                sb.AppendLine(outStr);
				this.UIReference.Text += outStr + Environment.NewLine;
            }
			retDict.Add("Output", sb.ToString());
			// 装入总重量
			string loadRate = (((double)this.FinalWeight / (double)this.Capacity) * 100.0).ToString("0.00");
			retDict.Add("LoadRate", loadRate);
            this.UIReference.Text += String.Format("Total:{0}  Load-Rate:{1}", this.FinalWeight, loadRate) + Environment.NewLine;
			returnDict = retDict;
        }

		/// <summary>
		/// 物品列表（编号，质量，价值，单位价值，脏位）
		/// </summary>
		private List<Tuple<int, int, int, double, bool>> Items;

		/// <summary>
		/// 背包容量
		/// </summary>
		private int Capacity = 0;

		/// <summary>
		/// 物品项目数
		/// </summary>
		private int ItemTypeCount = 0;

		/// <summary>
		/// 选中的物品
		/// </summary>
		private List<int> PickList;

		/// <summary>
		/// 最终装入背包的重量
		/// </summary>
		private int FinalWeight = 0;

		/// <summary>
		/// 用户输入的测试数据
		/// </summary>
		private string testData = String.Empty;

		/// <summary>
		/// 输出UI的引用
		/// </summary>
		private TextBox UIReference = null;

		/// <summary>
		/// 运算开始时间戳
		/// </summary>
		private DateTime BeginTimeStamp;

		/// <summary>
		/// 运算结束时间戳
		/// </summary>
		private DateTime EndTimeStamp;
	}
}
