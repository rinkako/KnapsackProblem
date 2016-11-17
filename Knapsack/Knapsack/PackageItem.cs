using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knapsack
{
	/// <summary>
	/// 物品项类
	/// </summary>
	class PackageItem
	{
		/// <summary>
		/// 构造器
		/// </summary>
		/// <param name="id">物品标识符</param>
		/// <param name="weight">物品质量</param>
		/// <param name="value">物品价值</param>
		public PackageItem(string id, int weight, int value)
		{
			this.Id = id;
			this.Value = value;
			this.Weight = weight;
			this.Dirty = false;
		}

		/// <summary>
		/// 重写字符串化方法
		/// </summary>
		/// <returns>该物体的描述字符串</returns>
		public override string ToString()
		{
			return String.Format("Item[{0}] W:{1} V:{2}", this.Id, this.Weight, this.Value);
		}

		/// <summary>
		/// 获取物品的单位价值
		/// </summary>
		public double UnitValue
		{
			get
			{
				return (double)this.Value / (double)this.Weight;
			}
		}

		/// <summary>
		/// 物品标识符
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// 物品质量
		/// </summary>
		public int Weight { get; set; }

		/// <summary>
		/// 价值
		/// </summary>
		public int Value { get; set; }

		/// <summary>
		/// 脏位
		/// </summary>
		public bool Dirty { get; set; }
	}
}
