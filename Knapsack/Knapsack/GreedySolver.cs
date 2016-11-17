using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Knapsack
{
    /// <summary>
    /// 贪心算法的问题解决器
    /// </summary>
	class GreedySolver : Solver
	{
		/// <summary>
		/// 初始化问题解决器
		/// </summary>
		/// <param name="console">输出的UI引用</param>
		/// <param name="paras">参数向量</param>
		public override void Init(TextBox console, params string[] paras)
		{
			this.UIReference = console;
		}

		/// <summary>
		/// 开始解决问题
		/// </summary>
		/// <param name="testdata">要解决的问题的描述字符串</param>
        public override void Solve(string testdata)
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
			this.Items = new List<PackageItem>();
			// 读取每种物品的属性
			for (int i = 2; i < lineitem.Length; i++)
			{
				var aLine = lineitem[i].Split('\t');
				var aW = Convert.ToInt32(aLine[1]);
				var aV = Convert.ToInt32(aLine[2]);
                this.Items.Add(new PackageItem((i - 2).ToString(), aW, aV));
			}
			// 对单位价值做排序
			this.Items.Sort((x, y) =>
			{
				if (x.UnitValue > y.UnitValue) { return -1; }
				else if (x.UnitValue < y.UnitValue) { return 1; }
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
					if (this.Items[i].Dirty != true &&
						(currentWeight + this.Items[i].Weight <= this.Capacity))
					{
						currentWeight += this.Items[i].Weight;
						// 标记脏位
						this.Items[i].Dirty = true;
						// 标记本轮有pick
						pickFlag = true;
						// 记录pick的项目
						this.PickList.Add(i);
                    }
				}
			} while (pickFlag != false);
			this.FinalWeight = currentWeight;
			// 算法结束
			this.EndTimeStamp = DateTime.Now;
        }

		/// <summary>
		/// 获取问题解决的结果
		/// </summary>
		/// <param name="costTime">[out]消耗的时间</param>
		/// <param name="returnDict">[out]返回值的字典</param>
        public override void GetResult(out double costTime, out Dictionary<string, string> returnDict)
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
			double sumValue = 0;
			this.UIReference.Text += "ID\tW\tV\tW/V" + Environment.NewLine;
			for (int i = 0; i < this.PickList.Count; i++)
			{
				var aItem = this.Items[this.PickList[i]];
				var outStr = String.Format("[{0}]\t{1}\t{2}\t{3}", aItem.Id, aItem.Weight, aItem.Value, aItem.UnitValue.ToString("0.000"));
                sb.AppendLine(outStr);
				this.UIReference.Text += outStr + Environment.NewLine;
				sumValue += aItem.Value;
            }
			retDict.Add("Output", sb.ToString());
			// 装入总重量
			string loadRate = (((double)this.FinalWeight / (double)this.Capacity) * 100.0).ToString("0.0000");
			retDict.Add("LoadRate", loadRate);
			retDict.Add("TotalValue", sumValue.ToString("0"));
			retDict.Add("TotalWeight", this.FinalWeight.ToString());
			this.UIReference.Text += String.Format("Knapsack Capacity:{0}", this.Capacity) + Environment.NewLine;
			this.UIReference.Text += String.Format("TotalW:{0} Load-Rate:{1}%", this.FinalWeight, loadRate) + Environment.NewLine;
			this.UIReference.Text += String.Format("TotalV:{0}", sumValue.ToString("0")) + Environment.NewLine;
			returnDict = retDict;
        }
		
		/// <summary>
		/// 获取问题解决的结果并写入文件
		/// </summary>
		/// <param name="filename">要写的文件路径</param>
        public override void GetResultFile(string filename)
		{
			if (this.UIReference == null)
			{
				throw new Exception("问题解决器尚未初始化就被使用");
			}
			// 选中的项目
			StringBuilder sb = new StringBuilder();
			double sumValue = 0;
			sb.AppendLine("ID\tW\tV\tW/V");
			for (int i = 0; i < this.PickList.Count; i++)
			{
				var aItem = this.Items[this.PickList[i]];
				var outStr = String.Format("[{0}]\t{1}\t{2}\t{3}", aItem.Id, aItem.Weight, aItem.Value, aItem.UnitValue.ToString("0.000"));
				sb.AppendLine(outStr);
				sumValue += aItem.Value;
			}
			// 装入总重量
			string loadRate = (((double)this.FinalWeight / (double)this.Capacity) * 100.0).ToString("0.0000");
			// 写文件
			FileStream fs = new FileStream(filename, FileMode.Create);
			StreamWriter sw = new StreamWriter(fs);
			sw.WriteLine(sb.ToString());
			sw.WriteLine("LoadRate: " + loadRate + "%");
			sw.WriteLine("TotalWeight: " + this.FinalWeight.ToString() + "/" + this.Capacity.ToString());
			sw.WriteLine("TotalValue: " + sumValue.ToString("0"));
			sw.Close();
			fs.Close();
		}

		/// <summary>
		/// 获取问题解决耗时
		/// </summary>
		/// <param name="costTime">[out]消耗的时间</param>
        public override void GetCost(out double costTime)
		{
			costTime = (this.EndTimeStamp - this.BeginTimeStamp).TotalMilliseconds;
		}

		/// <summary>
		/// 物品列表
		/// </summary>
		private List<PackageItem> Items;

		/// <summary>
		/// 选中的物品
		/// </summary>
		private List<int> PickList;
	}
}
