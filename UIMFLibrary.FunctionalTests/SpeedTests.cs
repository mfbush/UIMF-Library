﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpeedTests.cs" company="">
//   
// </copyright>
// <summary>
//   TODO The speed tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UIMFLibrary.UnitTests
{
	using System;
	using System.Diagnostics;

	using NUnit.Framework;

	/// <summary>
	/// TODO The speed tests.
	/// </summary>
	public class SpeedTests
	{
		#region Fields

		/// <summary>
		/// TODO The uimf standard file 1.
		/// </summary>
		private string uimfStandardFile1 =
			@"\\protoapps\UserData\Slysz\DeconTools_TestFiles\UIMF\Sarc_MS_90_21Aug10_Cheetah_10-08-02_0000.uimf";

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// TODO The get summed massspectrum speed tests.
		/// </summary>
		[Test]
		public void GetSummedMassspectrumSpeedTests()
		{
			int numIterations = 100;

			// int numFramesToSum = 1;
			int numIMSScansToSum = 7;

			using (DataReader dr = new DataReader(this.uimfStandardFile1))
			{
				GlobalParameters gp = dr.GetGlobalParameters();

				int frameStart = 500;
				int frameStop = frameStart + numIterations;
				int scanStart = 250;
				int scanStop = scanStart + numIMSScansToSum - 1;

				Stopwatch sw = new Stopwatch();
				sw.Start();
				for (int frame = frameStart; frame < frameStop; frame++)
				{
					int[] intensities = null;
					double[] mzValues = null;

					int nonZeros = dr.GetSpectrum(
						frame, 
						frame, 
						DataReader.FrameType.MS1, 
						scanStart, 
						scanStop, 
						out mzValues, 
						out intensities);
				}

				sw.Stop();

				Console.WriteLine("Total time to read " + numIterations + " scans = " + sw.ElapsedMilliseconds);
				Console.WriteLine("Average time (milliseconds) = " + (double)sw.ElapsedMilliseconds / (double)numIterations);
			}
		}

		/// <summary>
		/// TODO The get single summed mass spectrum test 1.
		/// </summary>
		[Test]
		public void getSingleSummedMassSpectrumTest1()
		{
			DataReader dr = new DataReader(this.uimfStandardFile1);
			GlobalParameters gp = dr.GetGlobalParameters();

			int[] intensities = new int[gp.Bins];
			double[] mzValues = new double[gp.Bins];

			// int startFrame = 500;
			// int stopFrame = 502;
			// int startScan = 250;
			// int stopScan = 256;

			// int nonZeros = dr.SumScansNonCached(mzValues, intensities, 0, startFrame, stopFrame, startScan, stopScan);

			// TestUtilities.displayRawMassSpectrum(mzValues, intensities);
		}

		#endregion
	}
}