namespace Kinect.Reactive
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public static class IEnumerableExtensions
	{
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
			{
				action(item);
				yield return item;
			}
		}

		public static IEnumerable<TResult> ForEach<T, TResult>(this IEnumerable<T> source, Func<T, TResult> func)
		{
			foreach (var item in source)
				yield return func(item);
		}

		public static void ExecuteForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
			{
				action(item);
			}
		}
	}
}