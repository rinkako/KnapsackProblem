using System;
using System.Windows.Forms;

namespace Knapsack
{
	public partial class PreviewForm : Form
	{
		public PreviewForm(string titleParams, string showText)
		{
			InitializeComponent();
			this.textBox1.Text = showText;
			string[] pItems = titleParams.Split(' ');
			if (pItems.Length == 7)
			{
				this.label1.Text = String.Format("种类：{0}    总重：{1}    总价：{2}", pItems[0], pItems[1], pItems[2]);
				this.label2.Text = String.Format("质量均值：{0}    质量方差：{1}", pItems[3], pItems[4]);
				this.label3.Text = String.Format("价值均值：{0}    价值方差：{1}", pItems[5], pItems[6]);
			}
		}
	}
}
