using System;
using System.Collections.Generic;
using System.Linq;
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
				this.Items.Add(new PackageItem((i - 2).ToString(), aW, aV));
			}
			// 对单位价值做排序
			this.Items.Sort((x, y) =>
			{
				if (x.UnitValue > y.UnitValue) { return -1; }
				else if (x.UnitValue < y.UnitValue) { return 1; }
				else { return 0; }
			});
			// 初始化解空间
			this.SolutionTreeRoot = new BBNode(0, null);
			this.SolutionTreeRoot.Activation = Double.MaxValue;
			this.SolutionTreeRoot.IsLeftNode = false;
			this.CandidateRouterDestination = this.SolutionTreeRoot;
			// 搜索解空间
			this.BranchBounding();
			// 算法结束
			this.EndTimeStamp = DateTime.Now;
		}

		/// <summary>
		/// 分支界限求解
		/// </summary>
		private void BranchBounding()
		{
			// 初始化优先队列
			List<BBNode> OpenList = new List<BBNode>();
			OpenList.Add(this.SolutionTreeRoot);
			// 分支界限
			while (OpenList.Count != 0)
			{
				// 取出最优先的展开节点
				var expandItem = OpenList[0];
				OpenList.RemoveAt(0);
				// 如果展开到最底层时就直接判断是否能装入了
				if (expandItem.Level + 1 == this.Items.Count)
				{
					// 如果比当前的解更优就替换掉她
					if (expandItem.AccValue > this.CurrentMaxSolutonValue)
					{
						this.CurrentMaxSolutonValue = expandItem.AccValue;
						this.CandidateRouterDestination = expandItem;
					}
					// DEBUG
					break;
				}
				// 非最低层需要生长树时
				else
				{
					// 一次性展开全部子节点
					var lc = expandItem.LeftChild = new BBNode(expandItem.Level + 1, expandItem);
					var rc = expandItem.RightChild = new BBNode(expandItem.Level + 1, expandItem);
					// 计算启发值，左子树代表放入，右子树不放
					var curItem = this.Items[expandItem.Level];
					lc.Activation = curItem.Value + (this.Capacity - curItem.Weight) * this.Items[expandItem.Level + 1].UnitValue;
					rc.Activation = 0 + (this.Capacity - 0) * this.Items[expandItem.Level + 1].UnitValue;
					lc.IsLeftNode = true;
					rc.IsLeftNode = false;
					// 将合法的子节点加入到优先队列
					if (expandItem.AccWeight + curItem.Weight <= this.Capacity)
					{
						// 比当前最优解差就抛弃
						OpenList.Add(lc);
						lc.AccValue += curItem.Value;
						lc.AccWeight += curItem.Weight;
					}
					// 左剪枝
					else
					{
						expandItem.LeftChild = null;
						// 如果比当前的解更优就替换掉她
						if (expandItem.AccValue > this.CurrentMaxSolutonValue)
						{
							this.CurrentMaxSolutonValue = expandItem.AccValue;
							this.CandidateRouterDestination = expandItem;
						}
						// DEBUG
						break;
					}
					// 由于右子树没有放入，所以必然是可行分支
					OpenList.Add(rc);
					// 继承父节点的累积属性
					rc.AccValue = expandItem.AccValue;
					rc.AccWeight = expandItem.AccWeight;
					// 调整优先队列
					OpenList.Sort((x, y) =>
					{
						if (x.Activation > y.Activation) { return -1; }
						else if (x.Activation < y.Activation) { return 1; }
						else { return 0; }
					});
				}
			}
			// 反算路径
			this.PickList = new List<PackageItem>();
			BBNode curPtr = this.CandidateRouterDestination;
			if (curPtr != this.SolutionTreeRoot)
			{
				do
				{
					// 只有左子树才是收入此物品
					if (curPtr.IsLeftNode)
					{
						this.PickList.Add(this.Items[curPtr.Level - 1]);
						this.FinalWeight += this.Items[curPtr.Level - 1].Weight;
                    }
					curPtr = curPtr.Parent;
				} while (curPtr.Parent != null);
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
			throw new NotImplementedException();
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
		/// 解空间树根
		/// </summary>
		private BBNode SolutionTreeRoot;

		/// <summary>
		/// 物品列表（编号，质量，价值，单位价值）
		/// </summary>
		private List<PackageItem> Items;

		/// <summary>
		/// 当前局部最优解的价值
		/// </summary>
		private int CurrentMaxSolutonValue;

		/// <summary>
		/// 候选路径
		/// </summary>
		private BBNode CandidateRouterDestination;

		/// <summary>
		/// Pick表
		/// </summary>
		private List<PackageItem> PickList;

		/// <summary>
		/// 解空间树节点类
		/// </summary>
		private class BBNode
		{
			/// <summary>
			/// 构造器
			/// </summary>
			/// <param name="lv">节点在树上的高度</param>
			/// <param name="parent">父节点</param>
			/// <param name="lc">左孩子</param>
			/// <param name="rc">右孩子</param>
			public BBNode(int lv, BBNode parent, BBNode lc = null, BBNode rc = null)
			{
				this.Level = lv;
				this.Parent = parent;
				this.LeftChild = lc;
				this.RightChild = rc;
				this.Activation = 0;
			}

			/// <summary>
			/// 获取或设置到本节点为止搜索路径上价值的累和
			/// </summary>
			public int AccValue { get; set; }

			/// <summary>
			/// 获取或设置到本节点为止搜索路径上质量的累和
			/// </summary>
			public int AccWeight { get; set; }

			/// <summary>
			/// 获取或设置节点的启发值
			/// </summary>
			public double Activation { get; set; }

			/// <summary>
			/// 获取或设置节点在树上的高度
			/// </summary>
			public int Level { get; set; }

			/// <summary>
			/// 获取或设置节点在父节点下是否为左子树
			/// </summary>
			public bool IsLeftNode { get; set; }

			/// <summary>
			/// 获取或设置父节点
			/// </summary>
			public BBNode Parent { get; set; }

			/// <summary>
			/// 获取或设置右子树
			/// </summary>
			public BBNode LeftChild { get; set; }

			/// <summary>
			/// 获取或设置左子树
			/// </summary>
			public BBNode RightChild { get; set; }
		}
	}
}
