using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Knapsack
{
	/// <summary>
	/// 生成背包实验数据的类
	/// </summary>
	class ProblemGenerator
	{
		/// <summary>
		/// 生成测试数据
		/// </summary>
		/// <param name="nMin">问题规模下界</param>
		/// <param name="nMax">问题规模上界</param>
		/// <param name="iMin">物体重量下界</param>
		/// <param name="iMax">物体重量上界</param>
		/// <param name="vMin">物体价值下界</param>
		/// <param name="vMax">物体价值上界</param>
		/// <param name="ratio">背包尺寸占总物品重量比例</param>
		public void Generate(int nMin, int nMax, int iMin, int iMax, int vMin, int vMax, double ratio)
		{
			// 产生数据
			this.Ratio = ratio;
			Random rd = new Random();
			this.ItemTypeCount = rd.Next(nMin, nMax);
			this.Items = new List<KeyValuePair<int, int>>();
			for (int i = 0; i < this.ItemTypeCount; i++)
			{
				this.Items.Add(new KeyValuePair<int, int>(rd.Next(iMin, iMax), rd.Next(vMin, vMax)));
			}
			int WeightSum = 0;
			this.Items.ForEach((x) => WeightSum += x.Key);
			this.PackageCapacity = (int)Math.Round(WeightSum * this.Ratio);
			// 生成输出字符串
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(this.PackageCapacity.ToString());
			sb.AppendLine(this.ItemTypeCount.ToString());
			int encounter = 0;
			this.Items.ForEach((x) =>
			{
				encounter++;
				sb.AppendLine(String.Format("{0}\t{1}\t{2}", encounter, x.Key, x.Value));
			});
			this.TestDataString = sb.ToString();
		}

		/// <summary>
		/// 将生成的数据保存到文件中
		/// </summary>
		/// <param name="filePath">文件路径</param>
		public void Save(string filePath)
		{
			try
			{
				FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
				StreamWriter sw = new StreamWriter(fs);
				sw.Write(this.TestDataString);
				sw.Close();
				fs.Close();
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		/// <summary>
		/// 获取生成的测试数据
		/// </summary>
		/// <returns>代表测试数据的字符串</returns>
		public string Get()
		{
			return this.TestDataString;
		}

		/// <summary>
		/// 获取生成的测试数据的描述
		/// </summary>
		/// <returns>代表测试数据描述文本的字符串</returns>
		public string GetDescription()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(this.ItemTypeCount).Append(' ');
			int Wsum = 0, Vsum = 0;
			this.Items.ForEach((x) => Wsum += x.Key);
			this.Items.ForEach((x) => Vsum += x.Value);
			double avgW = Wsum / this.ItemTypeCount;
			double avgV = Vsum / this.ItemTypeCount;
            sb.Append(Wsum).Append(' ').Append(Vsum).Append(' ');
			double varW = 0.0, varV = 0.0;
			for (int i = 0; i < this.Items.Count; i++)
			{
				varW += Math.Pow(this.Items[i].Key - avgW, 2);
				varV += Math.Pow(this.Items[i].Value - avgV, 2);
			}
			sb.Append(avgW.ToString("0.00")).Append(' ');
			sb.Append(varW.ToString("0.00")).Append(' ');
			sb.Append(avgV.ToString("0.00")).Append(' ');
			sb.Append(varV.ToString("0.00"));
			return sb.ToString();
		}

		/// <summary>
		/// 私有的构造器
		/// </summary>
		private ProblemGenerator()
		{

		}

		/// <summary>
		/// 工厂方法：获得类的唯一实例
		/// </summary>
		/// <returns>返回实验数据生成器的唯一实例</returns>
		public static ProblemGenerator GetInstance()
		{
			return ProblemGenerator.syncObject;
		}

		/// <summary>
		/// 生成的测试数据
		/// </summary>
		private string TestDataString = String.Empty;

		/// <summary>
		/// 物品列表
		/// </summary>
		private List<KeyValuePair<int, int>> Items;

		/// <summary>
		/// 背包和总容量的比率
		/// </summary>
		private double Ratio = 0.5f;

		/// <summary>
		/// 物品种类数
		/// </summary>
		private int ItemTypeCount = 0;

		/// <summary>
		/// 背包容量
		/// </summary>
		private int PackageCapacity = 0;

		/// <summary>
		/// 唯一实例
		/// </summary>
		private static readonly ProblemGenerator syncObject = new ProblemGenerator();
	}
}
