using System;

namespace Knapsack
{
	/// <summary>
	/// 大顶堆
	/// </summary>
	/// <typeparam name="T">T是堆元素的类型</typeparam>
	public class MaxHeap<T> where T : IComparable<T>
	{
		/// <summary>
		/// 将输入进行堆排序
		/// </summary>
		public static void HeapSort(T[] objects)
		{
			for (int i = objects.Length / 2 - 1; i >= 0; --i)
			{
				heapAdjustFromTop(objects, i, objects.Length);
			}
			for (int i = objects.Length - 1; i > 0; --i)
			{
				swap(objects, i, 0);
				heapAdjustFromTop(objects, 0, i);
			}
		}

		/// <summary>
		/// 自下维护堆的属性
		/// </summary>
		public static void heapAdjustFromBottom(T[] objects, int n)
		{
			while (n > 0 && objects[(n - 1) >> 1].CompareTo(objects[n]) < 0)
			{
				swap(objects, n, (n - 1) >> 1);
				n = (n - 1) >> 1;
			}
		}

		/// <summary>
		/// 自上维护堆的属性
		/// </summary>
		public static void heapAdjustFromTop(T[] objects, int n, int len)
		{
			while ((n << 1) + 1 < len)
			{
				int m = (n << 1) + 1;
				if (m + 1 < len && objects[m].CompareTo(objects[m + 1]) < 0)
				{
					++m;
				}
				if (objects[n].CompareTo(objects[m]) > 0)
				{
					return;
				}
				swap(objects, n, m);
				n = m;
			}
		}

		/// <summary>
		/// 辅助函数：交换元素
		/// </summary>
		private static void swap(T[] objects, int a, int b)
		{
			T tmp = objects[a];
			objects[a] = objects[b];
			objects[b] = tmp;
		}
	}
}
