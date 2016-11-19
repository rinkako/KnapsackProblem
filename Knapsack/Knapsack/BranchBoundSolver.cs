using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text;

namespace Knapsack
{
	/// <summary>
	///  分支界限解决器
	/// </summary>
	class BranchBoundSolver : Solver
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
				var addItem = new PackageItem((i - 2).ToString(), aW, aV);
                this.Items.Add(addItem);
			}
			// 对单位价值做排序
			this.Items.Sort((x, y) =>
			{
				if (x.UnitValue > y.UnitValue) { return -1; }
				else if (x.UnitValue < y.UnitValue) { return 1; }
				else { return 0; }
			});
			// 补位
			PackageItem AppendBack = new PackageItem("AppenderBack", 0, 0);
			this.Items.Add(AppendBack);
			// 复原当前背包状态
			this.currentWeight = 0;
			this.currentValue = 0;
			this.currentBestValue = 0;
			this.heap = new Stack<BBNode>();
			// 搜索
			this.BranchAndBound();
			// 反算路径
			this.GetSolutionRouter();
			// 算法结束
			this.EndTimeStamp = DateTime.Now;
		}
		
		/// <summary>
		/// 求子树的价值上界
		/// </summary>
		/// <param name="itemId">当前考虑的物品</param>
		/// <returns>最大上界</returns>
		private double GetMaxBound(int itemId)
		{
			double space = this.Capacity - currentWeight;
			double maxborder = currentValue;
			// 要放入的所有物品应该比当前剩余空间还要小
			while (itemId < this.ItemTypeCount && this.Items[itemId].Weight <= space)
			{
				space -= this.Items[itemId].Weight;
				maxborder += this.Items[itemId].Value;
				itemId++;
			}
			// 装填剩余容量装满背包
			if (itemId < this.ItemTypeCount)
			{
				maxborder += (this.Items[itemId].Value / this.Items[itemId].Weight) * space;
			}
			return maxborder;
		}
		
		/// <summary>
		/// 分支界限求最大价值路径
		/// </summary>
		/// <returns>能够达到的最大价值</returns>
		private void BranchAndBound()
		{
			int itemId = 0;
			double currentUpperBound = this.GetMaxBound(itemId);
			this.heap = new Stack<BBNode>();
			BBNode currentExpandNode = null;
			// 分支界限搜索
			while (true)
			{
				// 左子树（放入）可以拓展
				double LeftWeight = currentWeight + this.Items[itemId].Weight;
				if (LeftWeight <= this.Capacity)         
				{
					if (currentValue + this.Items[itemId].Value > this.currentBestValue)
					{
						this.currentBestValue = currentValue + this.Items[itemId].Value;
						this.CandidateRouterDestination = this.InsertToHeap(currentUpperBound,
							currentValue + this.Items[itemId].Value, currentWeight + this.Items[itemId].Weight,
							itemId + 1, true, currentExpandNode);
					}
					else
					{
						this.InsertToHeap(currentUpperBound, currentValue + this.Items[itemId].Value,
						currentWeight + this.Items[itemId].Weight, itemId + 1, true, currentExpandNode);
					}
				}
				currentUpperBound = this.GetMaxBound(itemId + 1);
				// 右子树的价值上界比当前最大值还大才有拓展的意义
				if (currentUpperBound >= this.currentBestValue)
				{
					this.CandidateRouterDestination = this.InsertToHeap(currentUpperBound,
						currentValue, currentWeight, itemId + 1, false, currentExpandNode);
				}
				// 所有节点都已经展开就返回
				if (heap.Count == 0)
				{
					return;
				}
				// 取下一个要生长的节点
				currentExpandNode = heap.Pop();
                currentWeight = currentExpandNode.AccWeight;
				currentValue = currentExpandNode.AccValue;
				currentUpperBound = currentExpandNode.ValueUpperBound;
				itemId = currentExpandNode.Level;
			}
		}

		/// <summary>
		/// 将一个新的活结点插入到子集树和最大堆heap中
		/// </summary>
		/// <param name="maxValue">价值上界</param>
		/// <param name="accValue">节点累积价值</param>
		/// <param name="accWeight">节点累积质量</param>
		/// <param name="level">节点层次</param>
		/// <param name="pickFlag">是否挑选当前物品</param>
		/// <param name="parent">上层节点</param>
		/// <returns>加入的节点</returns>
		private BBNode InsertToHeap(double maxValue, double accValue, double accWeight, int level, bool pickFlag, BBNode parent)
		{
			BBNode node = new BBNode(level, null);
			node.ValueUpperBound = maxValue;
			node.AccValue = accValue;
			node.AccWeight = accWeight;
			node.Pick = pickFlag;
			node.Parent = parent;
			if (level <= this.ItemTypeCount)
			{
				heap.Push(node);
			}
			return node;
		}

		/// <summary>
		/// 反向计算到达最优值的路径
		/// </summary>
		private void GetSolutionRouter()
		{
			this.PickList = new List<PackageItem>();
			var iterNode = this.CandidateRouterDestination;
			this.FinalWeight = 0;
			while (iterNode != null)
			{
				if (iterNode.Pick == true && iterNode.Level < this.ItemTypeCount)
				{
					var picker = this.Items[iterNode.Level - 1];
                    this.PickList.Add(picker);
					this.FinalWeight += picker.Weight;
                }
				iterNode = iterNode.Parent;
			}
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
			this.UIReference.Text += "ID\tW\tV" + Environment.NewLine;
			for (int i = 0; i < this.PickList.Count; i++)
			{
				var aItem = this.PickList[i];
				var outStr = String.Format("[{0}]\t{1}\t{2}", aItem.Id, aItem.Weight, aItem.Value);
				sb.AppendLine(outStr);
				this.UIReference.Text += outStr + Environment.NewLine;
			}
			retDict.Add("Output", sb.ToString());
			// 装入总重量
			string loadRate = (((double)this.FinalWeight / (double)this.Capacity) * 100.0).ToString("0.0000");
			retDict.Add("LoadRate", loadRate);
			retDict.Add("TotalValue", this.currentBestValue.ToString("0"));
			retDict.Add("TotalWeight", this.FinalWeight.ToString());
			this.UIReference.Text += String.Format("Knapsack Capacity:{0}", this.Capacity) + Environment.NewLine;
			this.UIReference.Text += String.Format("MaxDepth of Solution Tree:{0}", this.CandidateRouterDestination.Level) + Environment.NewLine;
			this.UIReference.Text += String.Format("TotalW:{0} Load-Rate:{1}%", this.FinalWeight, loadRate) + Environment.NewLine;
			this.UIReference.Text += String.Format("TotalV:{0}", this.currentBestValue.ToString("0")) + Environment.NewLine;
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
			sw.WriteLine("MaxDepth of Solution Tree: " + (this.CandidateRouterDestination.Level).ToString());
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
		/// 当前背包重量
		/// </summary>
		private double currentWeight;

		/// <summary>
		/// 当前背包价值
		/// </summary>
		private double currentValue;

		/// <summary>
		/// 最优的背包价值
		/// </summary>
		private double currentBestValue;

		/// <summary>
		/// 最大堆
		/// </summary>
		private Stack<BBNode> heap;

		/// <summary>
		/// 物品列表（编号，质量，价值，单位价值）
		/// </summary>
		private List<PackageItem> Items;

		/// <summary>
		/// 候选路径
		/// </summary>
		private BBNode CandidateRouterDestination;

		/// <summary>
		/// Pick表
		/// </summary>
		private List<PackageItem> PickList;
	}
}
