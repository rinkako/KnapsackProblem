using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Knapsack
{
	public partial class Form1 : Form
	{
		/// <summary>
		/// 问题生成器引用
		/// </summary>
		private ProblemGenerator ProblemGen = ProblemGenerator.GetInstance();

		/// <summary>
		/// 生成的测试数据对应的字符串
		/// </summary>
		private string testProblemString = String.Empty;

		/// <summary>
		/// 生成的测试数据的描述
		/// </summary>
		private string testStringDescriptor = String.Empty;

		/// <summary>
		/// 构造器
		/// </summary>
		public Form1()
		{
			InitializeComponent();
		}
		
		#region 控件合法性检查辅助函数
		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			this.label7.Text = "背包尺寸占物体总重比：" + (this.trackBar1.Value * 10).ToString() + "%";
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown1.Value > this.numericUpDown2.Value) { this.numericUpDown1.Value = 0; }
		}

		private void numericUpDown2_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown1.Value > this.numericUpDown2.Value) { this.numericUpDown2.Value = this.numericUpDown2.Maximum; }
		}

		private void numericUpDown4_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown4.Value > this.numericUpDown3.Value) { this.numericUpDown4.Value = 0; }
		}

		private void numericUpDown3_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown4.Value > this.numericUpDown3.Value) { this.numericUpDown3.Value = this.numericUpDown3.Maximum; }
		}

		private void numericUpDown6_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown6.Value > this.numericUpDown5.Value) { this.numericUpDown6.Value = 0; }
		}

		private void numericUpDown5_ValueChanged(object sender, EventArgs e)
		{
			if (this.numericUpDown6.Value > this.numericUpDown5.Value) { this.numericUpDown5.Value = this.numericUpDown5.Maximum; }
		}
		#endregion
		
		/// <summary>
		/// 按钮：生成
		/// </summary>
		private void button1_Click(object sender, EventArgs e)
		{
			this.ProblemGen.Generate((int)this.numericUpDown1.Value, (int)this.numericUpDown2.Value,
				(int)this.numericUpDown4.Value, (int)this.numericUpDown3.Value,
				(int)this.numericUpDown6.Value, (int)this.numericUpDown5.Value,
				(this.trackBar1.Value) / 10.0);
			this.testProblemString = this.ProblemGen.Get();
			this.testStringDescriptor = this.ProblemGen.GetDescription();
		}

		/// <summary>
		/// 按钮：浏览
		/// </summary>
		private void button2_Click(object sender, EventArgs e)
		{
			PreviewForm pForm = new PreviewForm(this.testStringDescriptor, this.testProblemString);
			pForm.ShowDialog();
		}

		/// <summary>
		/// 按钮：保存
		/// </summary>
		private void button3_Click(object sender, EventArgs e)
		{
			SaveFileDialog sForm = new SaveFileDialog();
			sForm.Filter = "txt|*.txt";
			DialogResult dr = sForm.ShowDialog();
			if (dr == DialogResult.Cancel || sForm.FileName == "") { return; }
			this.ProblemGen.Save(sForm.FileName);
		}

		/// <summary>
		/// 按钮：贪心算法
		/// </summary>
		private void button4_Click(object sender, EventArgs e)
		{
			if (this.testProblemString == String.Empty)
			{
				MessageBox.Show("请先生成测试数据");
				return;
			}
			double Cost;
			Dictionary<string, string> Rets;
			ISolver solver = new GreedySolver();
			solver.Init(this.output_textBox, null);
			solver.Solve(this.testProblemString);
			solver.GetResult(out Cost, out Rets);
			this.method_label.Text = "贪心算法";
			this.cost_label.Text = String.Format("{0}秒", Cost.ToString("0.00000000"));
		}
	}
}
