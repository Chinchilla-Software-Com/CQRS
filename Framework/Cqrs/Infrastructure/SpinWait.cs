using System;
using System.Security.Permissions;
using System.Threading;

namespace Cqrs.Infrastructure
{
	/// <summary>
	/// Provides support for spin-based waiting.
	/// </summary>
	[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true, Synchronization = true)]
	public struct SpinWait
	{
		/// <summary>
		/// A recommended sleep value of 50.
		/// </summary>
		public const short DefaultSleepInMilliseconds = 50;

		internal const int YieldThreshold = 10;
		internal const int Sleep0EveryHowManyTimes = 5;
		internal const int Sleep1EveryHowManyTimes = 20;
		private int _count;

		/// <summary>
		/// Gets whether the next call to <see cref="M:System.Threading.SpinWait.SpinOnce"/> will yield the processor, triggering a forced context switch.
		/// </summary>
		/// 
		/// <returns>
		/// Whether the next call to <see cref="M:System.Threading.SpinWait.SpinOnce"/> will yield the processor, triggering a forced context switch.
		/// </returns>
		public bool NextSpinWillYield
		{
			get
			{
				if (_count <= 10)
					return Environment.ProcessorCount == 1;
				return true;
			}
		}

		/// <summary>
		/// Performs a single spin.
		/// </summary>
		/// <param name="sleepInMilliseconds">The amount of milliseconds the thread will sleep for.</param>
		public void SpinOnce(short sleepInMilliseconds = 0)
		{
			if (NextSpinWillYield)
			{
				int num = _count >= 10 ? _count - 10 : _count;
				if (num % Sleep1EveryHowManyTimes == Sleep1EveryHowManyTimes - 1)
					Thread.Sleep(sleepInMilliseconds == 0 ? 1 : sleepInMilliseconds * 2);
				else if (num % Sleep0EveryHowManyTimes == Sleep0EveryHowManyTimes - 1)
					Thread.Sleep(sleepInMilliseconds);
				else
				{
					Thread.Yield();
					if (sleepInMilliseconds >= DefaultSleepInMilliseconds)
						Thread.Sleep(sleepInMilliseconds / 10);
				}
			}
			else
				Thread.SpinWait(4 << _count);
			_count = _count == int.MaxValue ? 10 : _count + 1;
		}

		/// <summary>
		/// Resets the spin counter.
		/// </summary>
		public void Reset()
		{
			_count = 0;
		}

		/// <summary>
		/// Spins until the specified condition is satisfied.
		/// </summary>
		/// <param name="condition">A delegate to be executed over and over until it returns true.</param>
		/// <param name="sleepInMilliseconds">The amount of milliseconds the thread will sleep for.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="condition"/> argument is null.</exception>
		public static void SpinUntil(Func<bool> condition, short sleepInMilliseconds = 0)
		{
			SpinUntil(condition, -1, sleepInMilliseconds);
		}

		/// <summary>
		/// Spins until the specified condition is satisfied or until the specified timeout is expired.
		/// </summary>
		/// 
		/// <returns>
		/// True if the condition is satisfied within the timeout; otherwise, false
		/// </returns>
		/// <param name="condition">A delegate to be executed over and over until it returns true.</param>
		/// <param name="timeout">A <see cref="T:System.TimeSpan"/> that represents the number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="sleepInMilliseconds">The amount of milliseconds the thread will sleep for.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="condition"/> argument is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- timeout is greater than <see cref="F:System.Int32.MaxValue"/>.</exception>
		public static bool SpinUntil(Func<bool> condition, TimeSpan timeout, short sleepInMilliseconds = 0)
		{
			long num = (long) timeout.TotalMilliseconds;
			if (num < -1L || num > int.MaxValue)
				throw new ArgumentOutOfRangeException("timeout", timeout, "SpinWait_SpinUntil_TimeoutWrong");
			return SpinUntil(condition, (int) timeout.TotalMilliseconds, sleepInMilliseconds);
		}

		/// <summary>
		/// Spins until the specified condition is satisfied or until the specified timeout is expired.
		/// </summary>
		/// 
		/// <returns>
		/// True if the condition is satisfied within the timeout; otherwise, false
		/// </returns>
		/// <param name="condition">A delegate to be executed over and over until it returns true.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="F:System.Threading.Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="sleepInMilliseconds">The amount of milliseconds the thread will sleep for.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="condition"/> argument is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1, which represents an infinite time-out.</exception>
		public static bool SpinUntil(Func<bool> condition, int millisecondsTimeout, short sleepInMilliseconds = 0)
		{
			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, "SpinWait_SpinUntil_TimeoutWrong");
			if (condition == null)
				throw new ArgumentNullException("condition", "SpinWait_SpinUntil_ArgumentNull");
			uint num = 0U;
			if (millisecondsTimeout != 0 && millisecondsTimeout != -1)
				num = TimeoutHelper.GetTime();
			SpinWait spinWait = new SpinWait();
			while (!condition())
			{
				if (millisecondsTimeout == 0)
					return false;
				spinWait.SpinOnce(sleepInMilliseconds);
				if (millisecondsTimeout != -1 && spinWait.NextSpinWillYield && millisecondsTimeout <= (TimeoutHelper.GetTime() - num))
					return false;
			}
			return true;
		}

		internal static class TimeoutHelper
		{
			public static uint GetTime()
			{
				return (uint)Environment.TickCount;
			}

			public static int UpdateTimeOut(uint startTime, int originalWaitMillisecondsTimeout)
			{
				uint num1 = GetTime() - startTime;
				if (num1 > int.MaxValue)
					return 0;
				int num2 = originalWaitMillisecondsTimeout - (int)num1;
				if (num2 <= 0)
					return 0;
				return num2;
			}
		}
	}
}