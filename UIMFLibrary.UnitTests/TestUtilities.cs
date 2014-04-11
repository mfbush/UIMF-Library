﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestUtilities.cs" company="">
//   
// </copyright>
// <summary>
//   TODO The test utilities.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UIMFLibrary.UnitTests
{
	using System;
	using System.Text;

	/// <summary>
	/// TODO The test utilities.
	/// </summary>
	public class TestUtilities
	{
		#region Public Methods and Operators

		/// <summary>
		/// TODO The display 2 d chromatogram.
		/// </summary>
		/// <param name="frameORScanVals">
		/// TODO The frame or scan vals.
		/// </param>
		/// <param name="intensityVals">
		/// TODO The intensity vals.
		/// </param>
		public static void display2DChromatogram(int[] frameORScanVals, int[] intensityVals)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < frameORScanVals.Length; i++)
			{
				sb.Append(frameORScanVals[i]);
				sb.Append("\t");
				sb.Append(intensityVals[i]);
				sb.Append(Environment.NewLine);
			}

			Console.WriteLine(sb.ToString());
		}

		/// <summary>
		/// TODO The display frame parameters.
		/// </summary>
		/// <param name="fp">
		/// TODO The fp.
		/// </param>
		public static void displayFrameParameters(FrameParameters fp)
		{
			StringBuilder sb = new StringBuilder();

			string separator = Environment.NewLine;

			sb.Append("avg TOF length = \t" + fp.AverageTOFLength);
			sb.Append(separator);
			sb.Append("cal intercept = \t" + fp.CalibrationIntercept);
			sb.Append(separator);
			sb.Append("cal slope = \t" + fp.CalibrationSlope);
			sb.Append(separator);
			sb.Append("frame type = \t" + fp.FrameType);
			sb.Append(separator);
			sb.Append("pressure back = \t" + fp.PressureBack);
			sb.Append(separator);
			sb.Append("pressure front = \t" + fp.PressureFront);
			sb.Append(separator);
			sb.Append("high pressure funnel pressure= \t" + fp.HighPressureFunnelPressure);
			sb.Append(separator);
			sb.Append("ion funnel trap pressure= \t" + fp.IonFunnelTrapPressure);
			sb.Append(separator);
			sb.Append("quadrupole pressure = \t" + fp.QuadrupolePressure);
			sb.Append(separator);
			sb.Append("rear ion funnel pressure = \t" + fp.RearIonFunnelPressure);
			sb.Append(separator);
			sb.Append("start time = \t" + fp.StartTime);
			sb.Append(separator);
			sb.Append("num scans = \t" + fp.Scans);
			sb.Append(separator);
			sb.Append("IMF profile = \t" + fp.IMFProfile);

			Console.WriteLine(sb.ToString());
		}

		/// <summary>
		/// TODO The display raw mass spectrum.
		/// </summary>
		/// <param name="mzValues">
		/// TODO The mz values.
		/// </param>
		/// <param name="intensities">
		/// TODO The intensities.
		/// </param>
		public static void displayRawMassSpectrum(double[] mzValues, int[] intensities)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < mzValues.Length; i++)
			{
				sb.Append(mzValues[i]);
				sb.Append("\t");
				sb.Append(intensities[i]);
				sb.Append(Environment.NewLine);
			}

			Console.WriteLine(sb.ToString());
		}

		/// <summary>
		/// TODO The get max.
		/// </summary>
		/// <param name="values">
		/// TODO The values.
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		public static int getMax(int[] values)
		{
			int max = 0;
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i] > max)
				{
					max = values[i];
				}
			}

			return max;
		}

		/// <summary>
		/// TODO The get max.
		/// </summary>
		/// <param name="values">
		/// TODO The values.
		/// </param>
		/// <param name="xcoord">
		/// TODO The xcoord.
		/// </param>
		/// <param name="ycoord">
		/// TODO The ycoord.
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		public static int getMax(int[][] values, out int xcoord, out int ycoord)
		{
			int max = 0;
			xcoord = 0;
			ycoord = 0;

			for (int i = 0; i < values.Length; i++)
			{
				for (int j = 0; j < values[i].Length; j++)
				{
					if (values[i][j] > max)
					{
						max = values[i][j];
						xcoord = i;
						ycoord = j;
					}
				}
			}

			return max;
		}

		/// <summary>
		/// TODO The print as a matrix.
		/// </summary>
		/// <param name="frameVals">
		/// TODO The frame vals.
		/// </param>
		/// <param name="intensityVals">
		/// TODO The intensity vals.
		/// </param>
		/// <param name="cutoff">
		/// TODO The cutoff.
		/// </param>
		public static void printAsAMatrix(int[] frameVals, float[] intensityVals, float cutoff)
		{
			StringBuilder sb = new StringBuilder();
			int frameValue = frameVals[0];
			for (int i = 0; i < frameVals.Length; i++)
			{
				if (frameValue != frameVals[i])
				{
					sb.Append("\n");
					frameValue = frameVals[i];
				}
				else
				{
					if (intensityVals[i] < cutoff)
					{
						sb.Append("0,");
					}
					else
					{
						sb.Append(intensityVals[i] + ",");
					}
				}
			}

			Console.WriteLine(sb.ToString());
		}

		/// <summary>
		/// TODO The print as a matrix.
		/// </summary>
		/// <param name="frameVals">
		/// TODO The frame vals.
		/// </param>
		/// <param name="intensityVals">
		/// TODO The intensity vals.
		/// </param>
		/// <param name="cutoff">
		/// TODO The cutoff.
		/// </param>
		public static void printAsAMatrix(int[] frameVals, int[] intensityVals, float cutoff)
		{
			StringBuilder sb = new StringBuilder();
			int frameValue = frameVals[0];
			for (int i = 0; i < frameVals.Length; i++)
			{
				if (frameValue != frameVals[i])
				{
					sb.Append("\n");
					frameValue = frameVals[i];
				}
				else
				{
					if (intensityVals[i] < cutoff)
					{
						sb.Append("0,");
					}
					else
					{
						sb.Append(intensityVals[i] + ",");
					}
				}
			}

			Console.WriteLine(sb.ToString());
		}

		#endregion
	}
}