using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Knapsack
{
	/// <summary>
	/// 大者优先的优先队列
	/// </summary>
	/// <typeparam name="T">T是队列元素的可比较类型</typeparam>
	public class PriorityQueue<T> where T : IComparable<T>
	{
		/// <summary>
		/// 构造器
		/// </summary>
		public PriorityQueue()
		{
			this.buffer = new T[defaultCapacity];
			this.heapLength = 0;
		}

		/// <summary>
		/// 查询队列是否为空
		/// </summary>
		/// <returns>队列是否为空的布尔值</returns>
		public bool Empty()
		{
			return this.heapLength == 0;
		}

		/// <summary>
		/// 获取队列中的项目数量
		/// </summary>
		/// <returns>队列长度</returns>
		public int Count()
		{
			return this.heapLength;
		}

		/// <summary>
		/// 查看队头元素
		/// </summary>
		/// <returns>队头元素</returns>
		public T Top()
		{
			if (this.heapLength != 0)
			{
				return this.buffer[0];
			}
			throw new OverflowException("队列中没有元素");
		}

		/// <summary>
		/// 将元素排队
		/// </summary>
		/// <param name="obj">要放入的元素</param>
		public void Push(T obj)
		{
			// 如果队列不够尺寸就拓展
			if (this.heapLength == this.buffer.Length)
			{
				this.expand();
			}
			// 维护堆的属性
			this.buffer[heapLength] = obj;
			MaxHeap<T>.heapAdjustFromBottom(this.buffer, this.heapLength);
			this.heapLength++;
		}

		/// <summary>
		/// 弹出队头
		/// </summary>
		public void Pop()
		{
			if (this.heapLength == 0)
			{
				throw new OverflowException("队列为空时不能出队");
			}
			// 维护堆的属性
			heapLength--;
			this.swap(0, heapLength);
			MaxHeap<T>.heapAdjustFromTop(this.buffer, 0, this.heapLength);
		}

		/// <summary>
		/// 拓展队列长度
		/// </summary>
		private void expand()
		{
			Array.Resize<T>(ref buffer, buffer.Length * 2);
		}

		/// <summary>
		/// 辅助函数：交换元素
		/// </summary>
		private void swap(int a, int b)
		{
			T tmp = this.buffer[a];
			this.buffer[a] = this.buffer[b];
			this.buffer[b] = tmp;
		}

		/// <summary>
		/// 堆的长度
		/// </summary>
		private int heapLength;

		/// <summary>
		/// 堆向量
		/// </summary>
		private T[] buffer;

		/// <summary>
		/// 堆的原始尺寸
		/// </summary>
		private const int defaultCapacity = 1024;
	}
}
