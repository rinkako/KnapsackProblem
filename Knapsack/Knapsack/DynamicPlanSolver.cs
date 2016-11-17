using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;

namespace Knapsack
{
    /// <summary>
    /// 动态规划的问题解决器
    /// </summary>
    class DynamicPlanSolver : Solver
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
            // 动态规划
            this.PickList = this.DynamicPlanning();
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
            for (int i = 0; i < this.PickList.Count; i++)
            {
                var aItem = this.Items[i];
                var outStr = String.Format("[{0}]\tW:{1}\tV:{2}", aItem.Id, aItem.Weight, aItem.Value);
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
            this.UIReference.Text += String.Format("TotalV:{0} TotalW:{1} Load-Rate:{2}%", sumValue.ToString("0"), this.FinalWeight, loadRate) + Environment.NewLine;
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
        /// 执行具体的动态规划算法
        /// </summary>
        /// <returns>物品的Pick项链</returns>
        private List<PackageItem> DynamicPlanning()
        {
            var DPTable = new int[this.ItemTypeCount, this.Capacity + 1];
            for (var j = 0; j <= this.Capacity; j++)
            {
                for (int i = this.ItemTypeCount - 1; i >= 0; i--)
                {
                    if (j == 0)
                    {
                        DPTable[j, i] = 0;
                    }
                    else if (i == this.ItemTypeCount - 1)
                    {
                        if (Items[i].Weight > j)
                        {
                            DPTable[i, j] = 0;
                        }
                        else
                        {
                            DPTable[i, j] = Items[i].Value;
                        }
                    }
                    else if (Items[i].Weight > j)
                    {
                        DPTable[i, j] = DPTable[i + 1, j];
                    }
                    else if (Items[i].Weight <= j)
                    {
                        var dpEqItem1 = DPTable[i + 1, j];
                        var dpEqItem2 = DPTable[i + 1, j - Items[i].Weight] + Items[i].Value;
                        DPTable[i, j] = Math.Max(dpEqItem1, dpEqItem2);
                    }
                }
            }
            var leftSpace = this.Capacity;
            var retList = new List<PackageItem>();
            for (var i = 0; i < Items.Count; i++)
            {
                var itemObj = Items[i];
                if (leftSpace == 0)
                {
                    break;
                }
                if (i == Items.Count - 1 && leftSpace >= itemObj.Weight)
                {
                    retList.Add(itemObj);
                }
                else if (DPTable[i, leftSpace] - DPTable[i + 1, leftSpace - itemObj.Weight] == itemObj.Value)
                {
                    retList.Add(itemObj);
                    leftSpace -= itemObj.Weight;
                }
            }
            this.FinalWeight = this.Capacity - leftSpace;
            this.EndTimeStamp = DateTime.Now;
            return retList;
        }

        /// <summary>
        /// 物品列表
        /// </summary>
        private List<PackageItem> Items;

        /// <summary>
        /// Pick表
        /// </summary>
        private List<PackageItem> PickList;

        /// <summary>
        /// 物品项类
        /// </summary>
        private class PackageItem
        {
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
            /// 构造器
            /// </summary>
            /// <param name="id">物品标识符</param>
            /// <param name="weight">物品质量</param>
            /// <param name="value"></param>
            public PackageItem(string id, int weight, int value)
            {
                this.Id = id;
                this.Value = value;
                this.Weight = weight;
            }
        }

    }
}
