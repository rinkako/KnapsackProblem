using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Knapsack
{
	/// <summary>
	/// 回溯法问题解决器
	/// </summary>
	class BackTraceSolver : Solver
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
				this.Items.Add(new PackageItem((i - 2).ToString(), aW, aV));
			}
			// 对单位价值做排序
			this.Items.Sort((x, y) =>
			{
				if (x.UnitValue > y.UnitValue) { return -1; }
				else if (x.UnitValue < y.UnitValue) { return 1; }
				else { return 0; }
			});
			// 初始化算法
			this.currentMaxBound = 0;
			this.currentBestValue = 0;
			this.candidateRouterNode = null;
			BBNode recursiveNode = new BBNode(-1, null);
			// 调用回溯函数，开始递归求解
			this.BackTrace(0, recursiveNode);
			// 反算路径
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
		/// 回溯法搜索问题最优解
		/// </summary>
		/// <param name="depth">当前递归的深度</param>
		/// <param name="parent">搜索树上层节点</param>
		private void BackTrace(int depth, BBNode parent)
		{
            // 递归边界
            if (depth >= this.ItemTypeCount)
            {
                // 优于局部最小值时替换她
                if (this.candidateRouterNode == null ||
                    parent.AccValue > this.candidateRouterNode.AccValue)
                {
                    this.currentBestValue = parent.AccValue;
                    this.candidateRouterNode = parent;
                }
                return;
            }
            // 计算价值上界
            this.currentMaxBound = this.GetValueBound(depth, parent);
            // 如果下一物体可以放入就尝试放入
            if (parent.AccWeight + this.Items[depth].Weight <= this.Capacity)
            {
                // 放入
                BBNode leftNode = new BBNode(depth, parent);
                leftNode.Pick = true;
                leftNode.AccValue = parent.AccValue + this.Items[depth].Value;
                leftNode.AccWeight = parent.AccWeight + this.Items[depth].Weight;
                // 递归
                this.BackTrace(depth + 1, leftNode);
            }
            // 不放入当前物品的情况
            this.currentMaxBound = this.GetValueBound(depth + 1, parent);
            if (this.currentMaxBound >= this.currentBestValue)
            {
                BBNode rightNode = new BBNode(depth, parent);
                rightNode.AccValue = parent.AccValue;
                rightNode.AccWeight = parent.AccWeight;
                this.BackTrace(depth + 1, rightNode);
            }
        }

		/// <summary>
		/// 反向计算到达最优值的路径
		/// </summary>
		private void GetSolutionRouter()
		{
			this.PickList = new List<PackageItem>();
			var iterNode = this.candidateRouterNode;
			this.FinalWeight = 0;
			while (iterNode != null)
			{
				if (iterNode.Pick == true && iterNode.Level < this.ItemTypeCount)
				{
					var picker = this.Items[iterNode.Level];
					this.PickList.Add(picker);
					this.FinalWeight += picker.Weight;
				}
				iterNode = iterNode.Parent;
			}
		}

		/// <summary>
		/// 求子树的价值上界
		/// </summary>
		/// <param name="itemId">当前考虑的物品</param>
		/// <returns>最大上界</returns>
		private double GetValueBound(int itemId, BBNode currentNode)
		{
			double space = this.Capacity - currentNode.AccWeight;
			double maxborder = currentNode.AccValue;
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
				maxborder += ((double)this.Items[itemId].Value / (double)this.Items[itemId].Weight) * space;
			}
			return maxborder;
		}

		/// <summary>
		/// 当前最优解最后一个节点
		/// </summary>
		private BBNode candidateRouterNode;

		/// <summary>
		/// 当前价值上界
		/// </summary>
		private double currentMaxBound;

		/// <summary>
		/// 当前最大价值
		/// </summary>
		private double currentBestValue;

		/// <summary>
		/// 物品列表（编号，质量，价值，单位价值）
		/// </summary>
		private List<PackageItem> Items;
		
		/// <summary>
		/// 选中列表
		/// </summary>
		private List<PackageItem> PickList;
	}
}
