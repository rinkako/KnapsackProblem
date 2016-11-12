using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Knapsack
{
	/// <summary>
	/// 问题解决器通用接口
	/// </summary>
	interface ISolver
	{
		/// <summary>
		/// 初始化问题解决器
		/// </summary>
		/// <param name="console">输出的UI引用</param>
		/// <param name="paras">参数向量</param>
		void Init(TextBox console, params string[] paras);

		/// <summary>
		/// 开始解决问题
		/// </summary>
		/// <param name="testdata">要解决的问题的描述字符串</param>
		void Solve(string testdata);

		/// <summary>
		/// 获取问题解决的结果
		/// </summary>
		/// <param name="costTime">[out]消耗的时间</param>
		/// <param name="returnDict">[out]返回值的字典</param>
		void GetResult(out double costTime, out Dictionary<string, string> returnDict);

		/// <summary>
		/// 获取问题解决耗时
		/// </summary>
		/// <param name="costTime">[out]消耗的时间</param>
		void GetCost(out double costTime);
	}
}
