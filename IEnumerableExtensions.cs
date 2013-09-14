namespace Kinect.Reactive
{
	using System;
	using System.Collections.Generic;

	public static class IEnumerableExtensions
	{
		/// <summary>
		/// Lazily applies a function to all elements in a sequence and returns the result of the function in a sequence.
		/// </summary>
		/// <typeparam name="T">The type of the source secquence.</typeparam>
		/// <typeparam name="TResult">The type of the result sequence.</typeparam>
		/// <param name="source">The source sequence.</param>
		/// <param name="func">The function to apply to the elements in the source sequence.</param>
		/// <returns>A sequence of the resulting elements.</returns>
		public static IEnumerable<TResult> ForEach<T, TResult>(this IEnumerable<T> source, Func<T, TResult> func)
		{
			foreach (var item in source)
				yield return func(item);
		}
	}
}