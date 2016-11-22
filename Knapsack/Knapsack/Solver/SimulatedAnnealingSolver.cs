using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Knapsack
{
	/// <summary>
	/// 模拟退火问题解决器
	/// </summary>
	class SimulatedAnnealingSolver : Solver
	{
		/// <summary>
		/// 初始化问题解决器
		/// </summary>
		/// <param name="console">输出的UI引用</param>
		/// <param name="paras">参数向量</param>
		public override void Init(TextBox console, params string[] paras)
		{
			this.UIReference = console;
			if (paras.Length > 0)
			{
				int epoch;
				if (Int32.TryParse(paras[0], out epoch))
				{
					this.Epoch = epoch;
				}
			}
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
			string[] lineitem = this.testData.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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
			// 初始化退火环境
			this.currentBestRouter = new bool[this.ItemTypeCount];
			this.previousBestRouter = new bool[this.ItemTypeCount];
			this.currentRouter = new bool[this.ItemTypeCount];
			this.currentBestValue = 0;
			this.previousBestValue = 0;
			this.randomer = new Random();
			// 搜索
			this.SimulatedAnealing();
			// 反算最优解组合
			this.GetSolutionRouter();
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
			this.UIReference.Text += "ID\tW\tV" + Environment.NewLine;
			for (int i = 0; i < this.PickList.Count; i++)
			{
				var aItem = this.PickList[i];
				var outStr = String.Format("[{0}]\t{1}\t{2}", aItem.Id, aItem.Weight, aItem.Value);
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
			sb.AppendLine("ID\tW\tV");
			for (int i = 0; i < this.PickList.Count; i++)
			{
				var aItem = this.PickList[i];
				var outStr = String.Format("[{0}]\t{1}\t{2}", aItem.Id, aItem.Weight, aItem.Value);
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
		/// 模拟退火搜索最优解
		/// </summary>
		private void SimulatedAnealing()
		{
			// 升温
			double currentTemperature = this.BeginTemperature;
			// 模拟退火
			while (currentTemperature > this.EndTemperature)
			{
				// 恒温搜索
				for (int i = 0; i < this.Epoch; i++)
				{
					// 扰动产生解
					int cid1 = 0, cid2 = 0;
					while (cid1 == cid2)
					{
						cid1 = this.randomer.Next(0, this.ItemTypeCount);
						cid2 = this.randomer.Next(0, this.ItemTypeCount);
					}
					this.currentRouter[cid1] = true;
					this.currentRouter[cid2] = true;
					if (this.GetCurrentWeight() > this.Capacity)
					{
						this.currentRouter[cid2] = false;
					}
					if (this.GetCurrentWeight() > this.Capacity)
					{
						this.currentRouter[cid1] = false;
					}
					// 计算增量
					double currentValue = this.GetValue(this.currentRouter);
					double delta = currentValue - this.currentBestValue;
					// 如果情况改善了局部最优值就直接接受她
					if (delta > 0)
					{
						Array.Copy(this.currentRouter, this.previousBestRouter, this.ItemTypeCount);
						Array.Copy(this.currentRouter, this.currentBestRouter, this.ItemTypeCount);
						this.previousBestValue = this.currentBestValue = currentValue;
					}
					// 否则需要进一步计算
					else
					{
						delta = currentValue - this.previousBestValue;
						// 如果能够改善上一次移动，就接受她
						if (delta > 0)
						{
							Array.Copy(this.currentRouter, this.previousBestRouter, this.ItemTypeCount);
							this.previousBestValue = currentValue;
						}
						// 否则她是一次没有正面作用的移动，那么只能以一定概率接受她
						else
						{
							// 随机移动
							double acceptCoin = this.randomer.Next(0, Int32.MaxValue) % 20001 / 20000.0;
							// 落在接受域
							if (Math.Exp(delta / currentTemperature) > acceptCoin)
							{
								Array.Copy(this.currentRouter, this.previousBestRouter, this.ItemTypeCount);
								this.previousBestValue = currentValue;
							}
							// 落在拒绝域，回滚本次改变
							else
							{
								Array.Copy(this.previousBestRouter, this.currentRouter, this.ItemTypeCount);
							}
						}
					}
				}
				// 降温
				currentTemperature *= this.AnealingRatio;
			}
		}

		/// <summary>
		/// 反向计算到达最优值的组合
		/// </summary>
		private void GetSolutionRouter()
		{
			this.PickList = new List<PackageItem>();
			for (int i = 0; i < this.ItemTypeCount; i++)
			{
				if (this.currentBestRouter[i])
				{
					this.PickList.Add(this.Items[i]);
					this.FinalWeight += this.Items[i].Weight;
				}
			}
		}

		/// <summary>
		/// 获取当前总质量
		/// </summary>
		/// <returns>当前背包总质量</returns>
		private double GetCurrentWeight()
		{
			double acc = 0;
			for (int i = 0; i < this.currentRouter.Length; i++)
			{
				if (this.currentRouter[i])
				{
					acc += this.Items[i].Weight;
				}
			}
			return acc;
		}

		/// <summary>
		/// 获取当前总价值
		/// </summary>
		/// <param name="pickVec">物品选中情况描述向量</param>
		/// <returns>当前背包总价值</returns>
		private double GetValue(bool[] pickVec)
		{
			double acc = 0;
			for (int i = 0; i < pickVec.Length; i++)
			{
				if (pickVec[i])
				{
					acc += this.Items[i].Value;
				}
			}
			return acc;
		}

		/// <summary>
		/// 退火起始温度
		/// </summary>
		private double BeginTemperature = 1000;

		/// <summary>
		/// 退火结束温度
		/// </summary>
		private double EndTemperature = 0.01;

		/// <summary>
		/// 退火率
		/// </summary>
		private double AnealingRatio = 0.95;

		/// <summary>
		/// 模拟退火内层循环次数
		/// </summary>
		private int Epoch = 2000;

		/// <summary>
		/// 随机数产生器
		/// </summary>
		private Random randomer;

		/// <summary>
		/// 当前最优价值
		/// </summary>
		private double currentBestValue;

		/// <summary>
		/// 上一改善步骤时的价值
		/// </summary>
		private double previousBestValue;

		/// <summary>
		/// 当前最优解组合
		/// </summary>
		private bool[] currentBestRouter;

		/// <summary>
		/// 上一改善步骤组合
		/// </summary>
		private bool[] previousBestRouter;

		/// <summary>
		/// 当前选中物品的组合
		/// </summary>
		private bool[] currentRouter;

		/// <summary>
		/// 物品列表
		/// </summary>
		private List<PackageItem> Items;

		/// <summary>
		/// 选中的物品
		/// </summary>
		private List<PackageItem> PickList;
	}
}
