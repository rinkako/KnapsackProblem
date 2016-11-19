using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knapsack
{
	/// <summary>
	/// 解空间树节点类
	/// </summary>
	public class BBNode
	{
		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="lv">节点在树上的高度</param>
		/// <param name="parent">父节点</param>
		public BBNode(int lv, BBNode parent)
		{
			this.AccValue = 0;
			this.AccWeight = 0;
			this.Level = lv;
			this.Parent = parent;
			this.Pick = false;
		}

		/// <summary>
		/// 重写字符串化方法
		/// </summary>
		/// <returns>节点的描述字符串</returns>
		public override string ToString()
		{
			return String.Format("Node Lv:{0} Pick:{1} AccV:{2} AccW:{3} ",
				this.Level, this.Pick, this.AccValue, this.AccWeight, this.ValueUpperBound);
		}
		
		/// <summary>
		/// 获取或设置结点的价值上界
		/// </summary>
		public double ValueUpperBound { get; set; }

		/// <summary>
		/// 获取或设置到本节点为止搜索路径上价值的累和
		/// </summary>
		public double AccValue { get; set; }

		/// <summary>
		/// 获取或设置到本节点为止搜索路径上质量的累和
		/// </summary>
		public double AccWeight { get; set; }

		/// <summary>
		/// 获取或设置该节点对应的物品是否被挑选
		/// </summary>
		public bool Pick { get; set; }

		/// <summary>
		/// 获取或设置节点在树上的高度
		/// </summary>
		public int Level { get; set; }

		/// <summary>
		/// 获取或设置父节点
		/// </summary>
		public BBNode Parent { get; set; }
	}
}
