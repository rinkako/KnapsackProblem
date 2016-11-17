using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Knapsack
{
	/// <summary>
	/// 问题解决器的公共类
	/// </summary>
	abstract class Solver
	{
		/// <summary>
		/// 初始化问题解决器
		/// </summary>
		/// <param name="console">输出的UI引用</param>
		/// <param name="paras">参数向量</param>
		abstract public void Init(TextBox console, params string[] paras);

		/// <summary>
		/// 开始解决问题
		/// </summary>
		/// <param name="testdata">要解决的问题的描述字符串</param>
        abstract public void Solve(string testdata);

		/// <summary>
		/// 获取问题解决的结果
		/// </summary>
		/// <param name="costTime">[out]消耗的时间</param>
		/// <param name="returnDict">[out]返回值的字典</param>
        abstract public void GetResult(out double costTime, out Dictionary<string, string> returnDict);

		/// <summary>
		/// 获取问题解决的结果并写入文件
		/// </summary>
		/// <param name="filename">要写的文件路径</param>
        abstract public void GetResultFile(string filename);

		/// <summary>
		/// 获取问题解决耗时
		/// </summary>
		/// <param name="costTime">[out]消耗的时间</param>
        abstract public void GetCost(out double costTime);

        /// <summary>
        /// 背包容量
        /// </summary>
        protected long Capacity = 0;

        /// <summary>
        /// 物品项目数
        /// </summary>
        protected int ItemTypeCount = 0;

        /// <summary>
        /// 最终装入背包的重量
        /// </summary>
        protected long FinalWeight = 0;

        /// <summary>
        /// 用户输入的测试数据
        /// </summary>
        protected string testData = String.Empty;

        /// <summary>
        /// 输出UI的引用
        /// </summary>
        protected TextBox UIReference = null;

        /// <summary>
        /// 运算开始时间戳
        /// </summary>
        protected DateTime BeginTimeStamp;

        /// <summary>
        /// 运算结束时间戳
        /// </summary>
        protected DateTime EndTimeStamp;
	}
}
