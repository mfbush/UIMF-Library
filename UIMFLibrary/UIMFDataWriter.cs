﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="UIMFDataWriter.cs">
//   
// </copyright>
// <summary>
//   TODO The data writer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace UIMFLibrary
{
	using System;
	using System.Collections.Generic;
	using System.Data.SQLite;
	using System.Text;

	/// <summary>
	/// TODO The data writer.
	/// </summary>
	public class DataWriter : IDisposable
	{
		#region Fields

		/// <summary>
		/// TODO The m_ frame parameter columns verified.
		/// </summary>
		private bool m_FrameParameterColumnsVerified;

		/// <summary>
		/// TODO The m_db command prepare insert frame.
		/// </summary>
		private SQLiteCommand m_dbCommandPrepareInsertFrame;

		/// <summary>
		/// TODO The m_db command prepare insert scan.
		/// </summary>
		private SQLiteCommand m_dbCommandPrepareInsertScan;

		/// <summary>
		/// TODO The m_db command prepare insert scan parameters.
		/// </summary>
		private SQLiteCommand m_dbCommandPrepareInsertScanParameters;

		/// <summary>
		/// TODO The m_db command uimf.
		/// </summary>
		private SQLiteCommand m_dbCommandUimf;

		/// <summary>
		/// TODO The m_db connection.
		/// </summary>
		private SQLiteConnection m_dbConnection;

		/// <summary>
		/// TODO The m_file name.
		/// </summary>
		private string m_fileName;

		/// <summary>
		/// TODO The m_global parameters.
		/// </summary>
		private GlobalParameters m_globalParameters;

		/// <summary>
		/// TODO The m_is scan parameter table.
		/// </summary>
		private bool m_isScanParameterTable;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DataWriter"/> class. 
		/// Constructor for UIMF datawriter that takes the filename and begins the transaction. 
		/// </summary>
		/// <param name="fileName">
		/// </param>
		/// <param name="createScanParameters">
		/// </param>
		public DataWriter(string fileName, bool createScanParameters = false)
		{
			this.m_fileName = fileName;
			this.m_isScanParameterTable = createScanParameters;
			string connectionString = "Data Source = " + fileName + "; Version=3; DateTimeFormat=Ticks;";
			this.m_dbConnection = new SQLiteConnection(connectionString);
			try
			{
				this.m_dbConnection.Open();

				// Note that the following call will instantiate m_dbCommandUimf
				this.TransactionBegin();

				this.PrepareInsertFrame();
				this.PrepareInsertScan();

				this.m_FrameParameterColumnsVerified = false;
			}
			catch (Exception ex)
			{
			}
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// Post a new log entry to table Log_Entries
		/// </summary>
		/// <param name="oConnection">
		/// Database connection object
		/// </param>
		/// <param name="EntryType">
		/// Log entry type (typically Normal, Error, or Warning)
		/// </param>
		/// <param name="Message">
		/// Log message
		/// </param>
		/// <param name="PostedBy">
		/// Process or application posting the log message
		/// </param>
		/// <remarks>
		/// The Log_Entries table will be created if it doesn't exist
		/// </remarks>
		public static void PostLogEntry(SQLiteConnection oConnection, string EntryType, string Message, string PostedBy)
		{
			// Check whether the Log_Entries table needs to be created
			SQLiteCommand cmdPostLogEntry;
			cmdPostLogEntry = oConnection.CreateCommand();

			if (!DataReader.TableExists(oConnection, "Log_Entries"))
			{
				// Log_Entries not found; need to create it
				cmdPostLogEntry.CommandText = "CREATE TABLE Log_Entries ( " + "Entry_ID INTEGER PRIMARY KEY, "
				                              + "Posted_By STRING, " + "Posting_Time STRING, " + "Type STRING, " + "Message STRING)";

				cmdPostLogEntry.ExecuteNonQuery();
			}

			if (string.IsNullOrEmpty(EntryType))
			{
				EntryType = "Normal";
			}

			if (string.IsNullOrEmpty(PostedBy))
			{
				PostedBy = string.Empty;
			}

			if (string.IsNullOrEmpty(Message))
			{
				Message = string.Empty;
			}

			// Now add a log entry
			cmdPostLogEntry.CommandText = "INSERT INTO Log_Entries (Posting_Time, Posted_By, Type, Message) " + "VALUES ("
			                              + "datetime('now'), " + "'" + PostedBy + "', " + "'" + EntryType + "', " + "'"
			                              + Message + "')";

			cmdPostLogEntry.ExecuteNonQuery();
			cmdPostLogEntry.Dispose();
		}

		/// <summary>
		/// Add a column to the Frame_Parameters table
		/// </summary>
		/// <param name="parameterName">
		/// </param>
		/// <param name="parameterType">
		/// </param>
		/// <remarks>
		/// The new column will have Null values for all existing rows
		/// </remarks>
		public void AddFrameParameter(string parameterName, string parameterType)
		{
			try
			{
				this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
				this.m_dbCommandUimf.CommandText = "Alter TABLE Frame_Parameters Add " + parameterName + " " + parameterType;
				this.m_dbCommandUimf.ExecuteNonQuery();
				this.m_dbCommandUimf.Dispose();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error adding parameter " + parameterName + " to the Frame_Parameters table:" + ex.Message);
			}
		}

		/// <summary>
		/// Add a column to the Frame_Parameters table
		/// </summary>
		/// <param name="parameterName">
		/// Parameter name (aka column name in the database)
		/// </param>
		/// <param name="parameterType">
		/// Parameter type
		/// </param>
		/// <param name="defaultValue">
		/// Value to assign to all rows
		/// </param>
		public void AddFrameParameter(string parameterName, string parameterType, int defaultValue)
		{
			this.AddFrameParameter(parameterName, parameterType);

			try
			{
				this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
				this.m_dbCommandUimf.CommandText = "UPDATE Frame_Parameters SET " + parameterName + " = " + defaultValue.ToString()
				                                   + " WHERE " + parameterName + " IS NULL";
				this.m_dbCommandUimf.ExecuteNonQuery();

				this.m_dbCommandUimf.Dispose();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error setting default value for parameter " + parameterName + ": " + ex.Message);
			}
		}

		/// <summary>
		/// Add a column to the Frame_Parameters table
		/// </summary>
		/// <param name="parameterName">
		/// Parameter name (aka column name in the database)
		/// </param>
		/// <param name="parameterType">
		/// Parameter type
		/// </param>
		/// <param name="defaultValue">
		/// Value to assign to all rows
		/// </param>
		public void AddFrameParameter(string parameterName, string parameterType, string defaultValue)
		{
			this.AddFrameParameter(parameterName, parameterType);

			try
			{
				this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
				this.m_dbCommandUimf.CommandText = "UPDATE Frame_Parameters SET " + parameterName + " = '" + defaultValue
				                                   + "' WHERE " + parameterName + " IS NULL";
				this.m_dbCommandUimf.ExecuteNonQuery();

				this.m_dbCommandUimf.Dispose();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error setting default value for parameter " + parameterName + ": " + ex.Message);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="parameterName">
		/// </param>
		/// <param name="parameterType">
		/// </param>
		/// <param name="parameterValue">
		/// </param>
		public void AddGlobalParameter(string parameterName, string parameterType, string parameterValue)
		{
			try
			{
				this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
				this.m_dbCommandUimf.CommandText = "Alter TABLE Global_Parameters Add " + parameterName.ToString() + " "
				                                   + parameterType.ToString();
				this.m_dbCommandUimf.CommandText += " UPDATE Global_Parameters SET " + parameterName.ToString() + " = "
				                                    + parameterValue;
				this.m_dbCommandUimf.ExecuteNonQuery();
				this.m_dbCommandUimf.Dispose();
			}
			catch
			{
				this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
				this.m_dbCommandUimf.CommandText = "UPDATE Global_Parameters SET " + parameterName.ToString() + " = "
				                                   + parameterValue;
				this.m_dbCommandUimf.ExecuteNonQuery();
				this.m_dbCommandUimf.Dispose();
				Console.WriteLine("Parameter " + parameterName + " already exists, its value will be updated to " + parameterValue);
			}
		}

		/// <summary>
		/// This function will create tables that are bin centric (as opposed to scan centric) to allow querying of the data in 2 different ways. 
		/// Bin centric data is important for data access speed in informed workflows.
		/// </summary>
		public void CreateBinCentricTables()
		{
			this.CreateBinCentricTables(string.Empty);
		}

		/// <summary>
		/// This function will create tables that are bin centric (as opposed to scan centric) to allow querying of the data in 2 different ways. 
		/// Bin centric data is important for data access speed in informed workflows.
		/// </summary>
		/// <param name="workingDirectory">
		/// Path to the working directory in which a temporary SqLite database file should be created
		/// </param>
		public void CreateBinCentricTables(string workingDirectory)
		{
			using (DataReader uimfReader = new DataReader(this.m_fileName))
			{
				var binCentricTableCreator = new BinCentricTableCreation();
				binCentricTableCreator.CreateBinCentricTable(this.m_dbConnection, uimfReader, workingDirectory);
			}
		}

		/// <summary>
		/// Method to create the table struture within a UIMF file. THis must be called
		/// after open to create the default tables that are required for IMS data.
		/// </summary>
		/// <param name="dataType">
		/// </param>
		public void CreateTables(string dataType)
		{
			// https://prismwiki.pnl.gov/wiki/IMS_Data_Processing

			// Create Global_Parameters Table  
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
			this.m_dbCommandUimf.CommandText = "CREATE TABLE Global_Parameters ( " + "DateStarted STRING, "
			                                   + // date experiment was started
			                                   "NumFrames INT(4) NOT NULL, " + // Number of frames in dataset  
			                                   "TimeOffset INT(4) NOT NULL, "
			                                   + // Offset from 0. All bin numbers must be offset by this amount  
			                                   "BinWidth DOUBLE NOT NULL, " + // Width of TOF bins (in ns)  
			                                   "Bins INT(4) NOT NULL, " + // Total TOF bins in a frame
			                                   "TOFCorrectionTime FLOAT NOT NULL, " + // Instrument delay time
			                                   "FrameDataBlobVersion FLOAT NOT NULL, " + // Version of FrameDataBlob in T_Frame  
			                                   "ScanDataBlobVersion FLOAT NOT NULL, " + // Version of ScanInfoBlob in T_Frame  
			                                   "TOFIntensityType TEXT NOT NULL, "
			                                   +
			                                   // Data type of intensity in each TOF record (ADC is int/TDC is short/FOLDED is float) 
			                                   "DatasetType TEXT, " + "Prescan_TOFPulses INT(4), "
			                                   + "Prescan_Accumulations INT(4), " + "Prescan_TICThreshold INT(4), "
			                                   + "Prescan_Continuous BOOL, " + "Prescan_Profile STRING, "
			                                   + "Instrument_Name STRING)";

			this.m_dbCommandUimf.ExecuteNonQuery();

			// Create Frame_parameters Table
			this.m_dbCommandUimf.CommandText = "CREATE TABLE Frame_Parameters (" + "FrameNum INT(4) PRIMARY KEY, "
			                                   + // 0, Frame number (primary key)
			                                   "StartTime DOUBLE, " + // 1, Start time of frame, in minutes
			                                   "Duration DOUBLE, " + // 2, Duration of frame, in seconds 
			                                   "Accumulations INT(2), "
			                                   + // 3, Number of collected and summed acquisitions in a frame 
			                                   "FrameType SHORT, "
			                                   +
			                                   // 4, Bitmap: 0=MS (Legacy); 1=MS (Regular); 2=MS/MS (Frag); 3=Calibration; 4=Prescan
			                                   "Scans INT(4), " + // 5, Number of TOF scans  
			                                   "IMFProfile STRING, "
			                                   +
			                                   // 6, IMFProfile Name; this stores the name of the sequence used to encode the data when acquiring data multiplexed
			                                   "TOFLosses DOUBLE, " + // 7, Number of TOF Losses
			                                   "AverageTOFLength DOUBLE NOT NULL, "
			                                   + // 8, Average time between TOF trigger pulses
			                                   "CalibrationSlope DOUBLE, " + // 9, Value of k0  
			                                   "CalibrationIntercept DOUBLE, " + // 10, Value of t0  
			                                   "a2 DOUBLE, "
			                                   +
			                                   // 11, These six parameters below are coefficients for residual mass error correction
			                                   "b2 DOUBLE, " + // 12, ResidualMassError=a2t+b2t^3+c2t^5+d2t^7+e2t^9+f2t^11
			                                   "c2 DOUBLE, " + // 13
			                                   "d2 DOUBLE, " + // 14
			                                   "e2 DOUBLE, " + // 15
			                                   "f2 DOUBLE, " + // 16
			                                   "Temperature DOUBLE, " + // 17, Ambient temperature
			                                   "voltHVRack1 DOUBLE, " + // 18, Voltage setting in the IMS system
			                                   "voltHVRack2 DOUBLE, " + // 19, Voltage setting in the IMS system
			                                   "voltHVRack3 DOUBLE, " + // 20, Voltage setting in the IMS system
			                                   "voltHVRack4 DOUBLE, " + // 21, Voltage setting in the IMS system
			                                   "voltCapInlet DOUBLE, " + // 22, Capillary Inlet Voltage
			                                   "voltEntranceHPFIn DOUBLE, "
			                                   +
			                                   // 23, HPF In Voltage  (renamed from voltEntranceIFTIn  to voltEntranceHPFIn  in July 2011)
			                                   "voltEntranceHPFOut DOUBLE, "
			                                   +
			                                   // 24, HPF Out Voltage (renamed from voltEntranceIFTOut to voltEntranceHPFOut in July 2011)
			                                   "voltEntranceCondLmt DOUBLE, " + // 25, Cond Limit Voltage
			                                   "voltTrapOut DOUBLE, " + // 26, Trap Out Voltage
			                                   "voltTrapIn DOUBLE, " + // 27, Trap In Voltage
			                                   "voltJetDist DOUBLE, " + // 28, Jet Disruptor Voltage
			                                   "voltQuad1 DOUBLE, " + // 29, Fragmentation Quadrupole Voltage
			                                   "voltCond1 DOUBLE, " + // 30, Fragmentation Conductance Voltage
			                                   "voltQuad2 DOUBLE, " + // 31, Fragmentation Quadrupole Voltage
			                                   "voltCond2 DOUBLE, " + // 32, Fragmentation Conductance Voltage
			                                   "voltIMSOut DOUBLE, " + // 33, IMS Out Voltage
			                                   "voltExitHPFIn DOUBLE, "
			                                   +
			                                   // 34, HPF In Voltage   (renamed from voltExitIFTIn  to voltExitHPFIn  in July 2011)
			                                   "voltExitHPFOut DOUBLE, "
			                                   +
			                                   // 35, HPF Out Voltage  (renamed from voltExitIFTOut to voltExitHPFOut in July 2011)
			                                   "voltExitCondLmt DOUBLE, " + // 36, Cond Limit Voltage
			                                   "PressureFront DOUBLE, " + // 37, Pressure at front of Drift Tube 
			                                   "PressureBack DOUBLE, " + // 38, Pressure at back of Drift Tube 
			                                   "MPBitOrder INT(1), " + // 39, Determines original size of bit sequence 
			                                   "FragmentationProfile BLOB," + // 40, Voltage profile used in fragmentation
			                                   "HighPressureFunnelPressure DOUBLE, " + // 41
			                                   "IonFunnelTrapPressure DOUBLE , " + // 42
			                                   "RearIonFunnelPressure DOUBLE, " + // 43
			                                   "QuadrupolePressure DOUBLE, " + // 44
			                                   "ESIVoltage DOUBLE, " + // 45
			                                   "FloatVoltage DOUBLE, " + // 46
			                                   "CalibrationDone INT, " + // 47, Set to 1 after a frame has been calibrated
			                                   "Decoded INT);";

			// 48, Set to 1 after a frame has been decoded (added June 27, 2011)

			// Voltage profile used in fragmentation, Length number of Scans 
			this.m_dbCommandUimf.ExecuteNonQuery();

			// Re-initialize m_dbCommandPrepareInsertFrame so that it uses voltEntranceHPFIn and voltEntranceHPFOut
			this.PrepareInsertFrame();

			// Create Frame_Scans Table
			if (string.Equals(dataType, "double"))
			{
				this.m_dbCommandUimf.CommandText = "CREATE TABLE Frame_Scans ( " + "FrameNum INT(4) NOT NULL, "
				                                   + // Contains the frame number
				                                   "ScanNum INT(2) NOT NULL, " + // Scan number
				                                   "NonZeroCount INT(4) NOT NULL, "
				                                   + "BPI DOUBLE NOT NULL, BPI_MZ DOUBLE NOT NULL, "
				                                   + // base peak intensity and assocaited mz
				                                   "TIC DOUBLE NOT NULL, " + // Total Ion Chromatogram
				                                   "Intensities BLOB);"; // Intensities  
			}
			else if (string.Equals(dataType, "float"))
			{
				this.m_dbCommandUimf.CommandText = "CREATE TABLE Frame_Scans ( " + "FrameNum INT(4) NOT NULL, "
				                                   + // Contains the frame number
				                                   "ScanNum INT(2) NOT NULL, " + // Scan number
				                                   "BPI FLOAT NOT NULL, BPI_MZ DOUBLE NOT NULL, "
				                                   + // base peak intensity and assocaited mz
				                                   "NonZeroCount INT(4) NOT NULL, " + "TIC FLOAT NOT NULL, "
				                                   + // Total Ion Chromatogram
				                                   "Intensities BLOB);"; // Intensities  
			}
			else if (string.Equals(dataType, "short"))
			{
				this.m_dbCommandUimf.CommandText = "CREATE TABLE Frame_Scans ( " + "FrameNum INT(4) NOT NULL, "
				                                   + // Contains the frame number
				                                   "ScanNum INT(2) NOT NULL, " + // Scan number
				                                   "NonZeroCount INT(4) NOT NULL, " + // Non zero count
				                                   "BPI INT(2) NOT NULL, BPI_MZ DOUBLE NOT NULL, "
				                                   + // base peak intensity and assocaited mz
				                                   "TIC INT(2) NOT NULL, " + // Total Ion Chromatogram
				                                   "Intensities BLOB);"; // Intensities  
			}
			else
			{
				this.m_dbCommandUimf.CommandText = "CREATE TABLE Frame_Scans ( " + "FrameNum INT(4) NOT NULL, "
				                                   + // Contains the frame number
				                                   "ScanNum INT(2) NOT NULL, " + // Scan number
				                                   "NonZeroCount INT(4) NOT NULL, " + // non zero count
				                                   "BPI INT(4) NOT NULL, BPI_MZ DOUBLE NOT NULL, "
				                                   + // base peak intensity and assocaited mz
				                                   "TIC INT(4) NOT NULL, " + // Total Ion Chromatogram
				                                   "Intensities BLOB);"; // Intensities  
			}

			// ARS made this change to facilitate faster retrieval of scans/spectrums.
			this.m_dbCommandUimf.CommandText += "CREATE UNIQUE INDEX pk_index on Frame_Scans(FrameNum, ScanNum);";

			// m_dbCommandUimf.CommandText += "CREATE UNIQUE INDEX pk_index1 on Frame_Parameters(FrameNum, FrameType);";
			// ARS change ends
			this.m_dbCommandUimf.ExecuteNonQuery();

			if (this.m_isScanParameterTable)
			{
				this.m_dbCommandUimf.CommandText = "CREATE TABLE Scan_Parameters ( " + "ScanNum INT(2) NOT NULL, " + // Scan number
				                                   "MS_Level INT(2) NOT NULL);"; // MS_Level  
				this.m_dbCommandUimf.CommandText += "CREATE UNIQUE INDEX scan_index on Scan_Parameters(ScanNum, MS_Level);";
				this.m_dbCommandUimf.ExecuteNonQuery();
			}

			this.m_dbCommandUimf.Dispose();
		}

		/// <summary>
		/// Deletes the scans for all frames in the file.  In addition, updates the Scans column to 0 in Frame_Parameters for all frames.
		/// </summary>
		/// <param name="frameType">
		/// </param>
		/// <param name="updateScanCountInFrameParams">
		/// If true, then will update the Scans column to be 0 for the deleted frames
		/// </param>
		/// <param name="bShrinkDatabaseAfterDelete">
		/// </param>
		/// <remarks>
		/// As an alternative to using this function, use CloneUIMF() in the DataReader class
		/// </remarks>
		public void DeleteAllFrameScans(int frameType, bool updateScanCountInFrameParams, bool bShrinkDatabaseAfterDelete)
		{
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();

			this.m_dbCommandUimf.CommandText = "DELETE FROM Frame_Scans " + "WHERE FrameNum IN (SELECT FrameNum "
			                                   + "FROM Frame_Parameters " + "WHERE FrameType = " + frameType.ToString() + ");";
			this.m_dbCommandUimf.ExecuteNonQuery();

			if (updateScanCountInFrameParams)
			{
				this.m_dbCommandUimf.CommandText = "UPDATE Frame_Parameters " + "SET Scans = 0 " + "WHERE FrameType = "
				                                   + frameType.ToString() + ";";
				this.m_dbCommandUimf.ExecuteNonQuery();
			}

			// Commmit the currently open transaction
			this.TransactionCommit();
			System.Threading.Thread.Sleep(100);

			if (bShrinkDatabaseAfterDelete)
			{
				this.m_dbCommandUimf.CommandText = "VACUUM;";
				this.m_dbCommandUimf.ExecuteNonQuery();
			}

			// Open a new transaction
			this.TransactionBegin();

			this.m_dbCommandUimf.Dispose();
		}

		/// <summary>
		/// Deletes the frame from the Frame_Parameters table and from the Frame_Scans table
		/// </summary>
		/// <param name="frameNum">
		/// </param>
		/// <param name="updateGlobalParameters">
		/// If true, then decrements the NumFrames value in the Global_Parameters table
		/// </param>
		public void DeleteFrame(int frameNum, bool updateGlobalParameters)
		{
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();

			this.m_dbCommandUimf.CommandText = "DELETE FROM Frame_Scans WHERE FrameNum = " + frameNum.ToString() + "; ";
			this.m_dbCommandUimf.ExecuteNonQuery();

			this.m_dbCommandUimf.CommandText = "DELETE FROM Frame_Parameters WHERE FrameNum = " + frameNum.ToString() + "; ";
			this.m_dbCommandUimf.ExecuteNonQuery();

			if (updateGlobalParameters)
			{
				this.m_dbCommandUimf.CommandText = "UPDATE Global_Parameters SET NumFrames = NumFrames - 1 WHERE NumFrames > 0; ";
				this.m_dbCommandUimf.ExecuteNonQuery();
			}

			this.m_dbCommandUimf.Dispose();

			this.FlushUimf();
		}

		/// <summary>
		/// Deletes all of the scans for the specified frame
		/// </summary>
		/// <param name="frameNum">
		/// The frame number to delete
		/// </param>
		/// <param name="updateScanCountInFrameParams">
		/// If true, then will update the Scans column to be 0 for the deleted frames
		/// </param>
		public void DeleteFrameScans(int frameNum, bool updateScanCountInFrameParams)
		{
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();

			this.m_dbCommandUimf.CommandText = "DELETE FROM Frame_Scans WHERE FrameNum = " + frameNum.ToString() + "; ";
			this.m_dbCommandUimf.ExecuteNonQuery();

			if (updateScanCountInFrameParams)
			{
				this.m_dbCommandUimf.CommandText = "UPDATE Frame_Parameters SET Scans = 0 WHERE FrameNum = " + frameNum.ToString()
				                                   + "; ";
				this.m_dbCommandUimf.ExecuteNonQuery();
			}

			this.m_dbCommandUimf.Dispose();

			this.FlushUimf();
		}

		/// <summary>
		/// Deletes given frames from the UIMF file. 
		/// </summary>
		/// <param name="frameNums">
		/// </param>
		/// <param name="updateGlobalParameters">
		/// </param>
		public void DeleteFrames(List<int> frameNums, bool updateGlobalParameters)
		{
			StringBuilder sFrameList = new StringBuilder();

			// Construct a comma-separated list of frame numbers
			foreach (int frameNum in frameNums)
			{
				sFrameList.Append(frameNum + ",");
			}

			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();

			this.m_dbCommandUimf.CommandText = "DELETE FROM Frame_Scans WHERE FrameNum IN (" + sFrameList.ToString().TrimEnd(',')
			                                   + "); ";
			this.m_dbCommandUimf.ExecuteNonQuery();

			this.m_dbCommandUimf.CommandText = "DELETE FROM Frame_Parameters WHERE FrameNum IN ("
			                                   + sFrameList.ToString().TrimEnd(',') + "); ";
			this.m_dbCommandUimf.ExecuteNonQuery();

			if (updateGlobalParameters)
			{
				this.m_dbCommandUimf.CommandText = "UPDATE Global_Parameters SET NumFrames = NumFrames - " + frameNums.Count + "; ";
				this.m_dbCommandUimf.ExecuteNonQuery();

				// Make sure NumFrames is >= 0
				this.m_dbCommandUimf.CommandText = "SELECT NumFrames FROM Global_Parameters; ";
				object objResult = this.m_dbCommandUimf.ExecuteScalar();

				if (Convert.ToInt32(objResult) < 0)
				{
					this.m_dbCommandUimf.CommandText = "UPDATE Global_Parameters SET NumFrames 0; ";
					this.m_dbCommandUimf.ExecuteNonQuery();
				}
			}

			this.m_dbCommandUimf.Dispose();

			this.FlushUimf();
		}

		/// <summary>
		/// Dispose of any system resources
		/// </summary>
		public void Dispose()
		{
			try
			{
				if (this.m_dbConnection != null)
				{
					this.TransactionCommit();

					this.m_dbCommandUimf.Dispose();
					this.m_dbConnection.Close();
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Commits the currently open transaction, then starts a new one
		/// Note that a transaction is started when OpenUIMF() is called, then commited when CloseUIMF() is called
		/// </summary>
		public void FlushUimf()
		{
			this.TransactionCommit();
			System.Threading.Thread.Sleep(100);
			this.TransactionBegin();
		}

		/// <summary>
		/// </summary>
		/// <returns>
		/// The <see cref="GlobalParameters"/>.
		/// </returns>
		public GlobalParameters GetGlobalParameters()
		{
			return DataReader.GetGlobalParametersFromTable(this.m_dbConnection);
		}

		/// <summary>
		/// Method to insert details related to each IMS frame
		/// </summary>
		/// <param name="frameParameters">
		/// </param>
		public void InsertFrame(FrameParameters frameParameters)
		{
			if (this.m_dbCommandPrepareInsertFrame == null)
			{
				this.PrepareInsertFrame();
			}

			// Make sure the Frame_Parameters table has all of the required columns
			this.ValidateFrameParameterColumns();

			this.m_dbCommandPrepareInsertFrame.Parameters.Clear();
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":FrameNum", frameParameters.FrameNum));

			// 0, Frame number (primary key)     
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":StartTime", frameParameters.StartTime));

			// 1, Start time of frame, in minutes
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":Duration", frameParameters.Duration));

			// 2, Duration of frame, in seconds 
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":Accumulations", frameParameters.Accumulations));

			// 3, Number of collected and summed acquisitions in a frame 
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":FrameType", (int)frameParameters.FrameType));

			// 4, Bitmap: 0=MS (Legacy); 1=MS (Regular); 2=MS/MS (Frag); 3=Calibration; 4=Prescan
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":Scans", frameParameters.Scans));

			// 5, Number of TOF scans  
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":IMFProfile", frameParameters.IMFProfile));

			// 6, IMFProfile Name; this stores the name of the sequence used to encode the data when acquiring data multiplexed
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":TOFLosses", frameParameters.TOFLosses));

			// 7, Number of TOF Losses
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":AverageTOFLength", frameParameters.AverageTOFLength));

			// 8, Average time between TOF trigger pulses
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":CalibrationSlope", frameParameters.CalibrationSlope)); // 9, Value of k0  
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":CalibrationIntercept", frameParameters.CalibrationIntercept)); // 10, Value of t0  

			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":a2", frameParameters.a2));

			// 11, These six parameters below are coefficients for residual mass error correction            
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":b2", frameParameters.b2));

			// 12, ResidualMassError=a2t+b2t^3+c2t^5+d2t^7+e2t^9+f2t^11
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":c2", frameParameters.c2)); // 13
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":d2", frameParameters.d2)); // 14
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":e2", frameParameters.e2)); // 15
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":f2", frameParameters.f2)); // 16

			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":Temperature", frameParameters.Temperature));

			// 17, Ambient temperature
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltHVRack1", frameParameters.voltHVRack1));

			// 18, Voltage setting in the IMS system
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltHVRack2", frameParameters.voltHVRack2));

			// 19, Voltage setting in the IMS system
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltHVRack3", frameParameters.voltHVRack3));

			// 20, Voltage setting in the IMS system
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltHVRack4", frameParameters.voltHVRack4));

			// 21, Voltage setting in the IMS system
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltCapInlet", frameParameters.voltCapInlet));

			// 22, Capillary Inlet Voltage
			if (DataReader.ColumnExists(this.m_dbConnection, "Frame_Parameters", "voltEntranceHPFIn"))
			{
				this.m_dbCommandPrepareInsertFrame.Parameters.Add(
					new SQLiteParameter(":voltEntranceHPFIn", frameParameters.voltEntranceHPFIn)); // 23, HPF In Voltage
				this.m_dbCommandPrepareInsertFrame.Parameters.Add(
					new SQLiteParameter(":voltEntranceHPFOut", frameParameters.voltEntranceHPFOut)); // 24, HPF Out Voltage
			}
			else
			{
				this.m_dbCommandPrepareInsertFrame.Parameters.Add(
					new SQLiteParameter(":voltEntranceIFTIn", frameParameters.voltEntranceHPFIn)); // 23, IFT In Voltage
				this.m_dbCommandPrepareInsertFrame.Parameters.Add(
					new SQLiteParameter(":voltEntranceIFTOut", frameParameters.voltEntranceHPFOut)); // 24, IFT Out Voltage
			}

			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":voltEntranceCondLmt", frameParameters.voltEntranceCondLmt)); // 25, Cond Limit Voltage
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltTrapOut", frameParameters.voltTrapOut));

			// 26, Trap Out Voltage
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltTrapIn", frameParameters.voltTrapIn));

			// 27, Trap In Voltage
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltJetDist", frameParameters.voltJetDist));

			// 28, Jet Disruptor Voltage
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltQuad1", frameParameters.voltQuad1));

			// 29, Fragmentation Quadrupole Voltage
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltCond1", frameParameters.voltCond1));

			// 30, Fragmentation Conductance Voltage
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltQuad2", frameParameters.voltQuad2));

			// 31, Fragmentation Quadrupole Voltage
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltCond2", frameParameters.voltCond2));

			// 32, Fragmentation Conductance Voltage
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":voltIMSOut", frameParameters.voltIMSOut));

			// 33, IMS Out Voltage
			if (DataReader.ColumnExists(this.m_dbConnection, "Frame_Parameters", "voltExitHPFIn"))
			{
				this.m_dbCommandPrepareInsertFrame.Parameters.Add(
					new SQLiteParameter(":voltExitHPFIn", frameParameters.voltExitHPFIn)); // 34, HPF In Voltage
				this.m_dbCommandPrepareInsertFrame.Parameters.Add(
					new SQLiteParameter(":voltExitHPFOut", frameParameters.voltExitHPFOut)); // 35, HPF Out Voltage
			}
			else
			{
				this.m_dbCommandPrepareInsertFrame.Parameters.Add(
					new SQLiteParameter(":voltExitIFTIn", frameParameters.voltExitHPFIn)); // 34, IFT In Voltage
				this.m_dbCommandPrepareInsertFrame.Parameters.Add(
					new SQLiteParameter(":voltExitIFTOut", frameParameters.voltExitHPFOut)); // 35, IFT Out Voltage
			}

			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":voltExitCondLmt", frameParameters.voltExitCondLmt)); // 36, Cond Limit Voltage

			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":PressureFront", frameParameters.PressureFront)); // 37, Pressure at front of Drift Tube 
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":PressureBack", frameParameters.PressureBack));

			// 38, Pressure at back of Drift Tube 
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":MPBitOrder", frameParameters.MPBitOrder));

			// 39, Determines original size of bit sequence
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":FragmentationProfile", ConvertToBlob(frameParameters.FragmentationProfile)));

			// 40, Voltage profile used in fragmentation
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":HPPressure", frameParameters.HighPressureFunnelPressure)); // 41            
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":IPTrapPressure", frameParameters.IonFunnelTrapPressure)); // 42
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":RIFunnelPressure", frameParameters.RearIonFunnelPressure)); // 43
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":QuadPressure", frameParameters.QuadrupolePressure)); // 44

			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":ESIVoltage", frameParameters.ESIVoltage));

			// 45
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":FloatVoltage", frameParameters.FloatVoltage));

			// 46
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(
				new SQLiteParameter(":CalibrationDone", frameParameters.CalibrationDone));

			// 47, Set to 1 after a frame has been calibrated
			this.m_dbCommandPrepareInsertFrame.Parameters.Add(new SQLiteParameter(":Decoded", frameParameters.Decoded));

			// 48, Set to 1 after a frame has been decoded (added June 27, 2011)
			this.m_dbCommandPrepareInsertFrame.ExecuteNonQuery();
			this.m_dbCommandPrepareInsertFrame.Parameters.Clear();
		}

		/// <summary>
		/// Method to enter the details of the global parameters for the experiment
		/// </summary>
		/// <param name="header">
		/// </param>
		public void InsertGlobal(GlobalParameters header)
		{
			this.m_globalParameters = header;
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
			this.m_dbCommandUimf.CommandText = "INSERT INTO Global_Parameters "
			                                   + "(DateStarted, NumFrames, TimeOffset, BinWidth, Bins, TOFCorrectionTime, FrameDataBlobVersion, ScanDataBlobVersion, "
			                                   + "TOFIntensityType, DatasetType, Prescan_TOFPulses, Prescan_Accumulations, Prescan_TICThreshold, Prescan_Continuous, Prescan_Profile, Instrument_name) "
			                                   + "VALUES(:DateStarted, :NumFrames, :TimeOffset, :BinWidth, :Bins, :TOFCorrectionTime, :FrameDataBlobVersion, :ScanDataBlobVersion, "
			                                   + ":TOFIntensityType, :DatasetType, :Prescan_TOFPulses, :Prescan_Accumulations, :Prescan_TICThreshold, :Prescan_Continuous, :Prescan_Profile, :Instrument_name);";

			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":DateStarted", header.DateStarted));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":NumFrames", header.NumFrames));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":TimeOffset", header.TimeOffset));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":BinWidth", header.BinWidth));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":Bins", header.Bins));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":TOFCorrectionTime", header.TOFCorrectionTime));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":FrameDataBlobVersion", header.FrameDataBlobVersion));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":ScanDataBlobVersion", header.ScanDataBlobVersion));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":TOFIntensityType", header.TOFIntensityType));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":DatasetType", header.DatasetType));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":Prescan_TOFPulses", header.Prescan_TOFPulses));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":Prescan_Accumulations", header.Prescan_Accumulations));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":Prescan_TICThreshold", header.Prescan_TICThreshold));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":Prescan_Continuous", header.Prescan_Continuous));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":Prescan_Profile", header.Prescan_Profile));
			this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":Instrument_name", header.InstrumentName));

			this.m_dbCommandUimf.ExecuteNonQuery();
			this.m_dbCommandUimf.Parameters.Clear();
			this.m_dbCommandUimf.Dispose();
		}

		/// <summary>
		/// This function should be called for each scan, intensities is an array including all zeros
		/// TODO:: Deprecate this function since the bpi is calculated using an incorrect calibration function
		/// </summary>
		/// <param name="frameParameters">
		/// TODO The frame parameters.
		/// </param>
		/// <param name="scanNum">
		/// TODO The scan num.
		/// </param>
		/// <param name="counter">
		/// TODO The counter.
		/// </param>
		/// <param name="intensities">
		/// TODO The intensities.
		/// </param>
		/// <param name="binWidth">
		/// TODO The bin width.
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		[Obsolete]
		public int InsertScan(
			FrameParameters frameParameters, 
			int scanNum, 
			int counter, 
			double[] intensities, 
			double binWidth)
		{
			// RLZE - convert 0s to negative multiples as well as calculate TIC and BPI, BPI_MZ
			int nrlze = 0;
			int zeroCount = 0;
			double[] runLengthZeroEncodedData = new double[intensities.Length];
			double tic = 0;
			double bpi = 0;
			double bpiMz = 0;
			int datatypeSize = 8;

			if (this.m_globalParameters == null)
			{
				this.m_globalParameters = DataReader.GetGlobalParametersFromTable(this.m_dbConnection);
			}

			for (int i = 0; i < intensities.Length; i++)
			{
				double intensity = intensities[i];
				if (intensity > 0)
				{
					// TIC is just the sum of all intensities
					tic += intensity;
					if (intensity > bpi)
					{
						bpi = intensity;
						bpiMz = this.convertBinToMz(i, binWidth, frameParameters);
					}

					if (zeroCount < 0)
					{
						runLengthZeroEncodedData[nrlze++] = (double)zeroCount;
						zeroCount = 0;
					}

					runLengthZeroEncodedData[nrlze++] = intensity;
				}
				else
				{
					zeroCount--;
				}
			}

			// Compress intensities
			int nlzf = 0;
			byte[] compressedData = new byte[nrlze * datatypeSize * 5];
			if (nrlze > 0)
			{
				byte[] byteBuffer = new byte[nrlze * datatypeSize];
				Buffer.BlockCopy(runLengthZeroEncodedData, 0, byteBuffer, 0, nrlze * datatypeSize);
				nlzf = LZFCompressionUtil.Compress(
					ref byteBuffer, 
					nrlze * datatypeSize, 
					ref compressedData, 
					nrlze * datatypeSize * 5);
			}

			if (nlzf != 0)
			{
				byte[] spectra = new byte[nlzf];
				Array.Copy(compressedData, spectra, nlzf);

				// Insert records
				this.InsertScanAddParameters(frameParameters.FrameNum, scanNum, counter, (int)bpi, bpiMz, (int)tic, spectra);
				this.m_dbCommandPrepareInsertScan.ExecuteNonQuery();
				this.m_dbCommandPrepareInsertScan.Parameters.Clear();
			}

			return nlzf;
		}

		/// <summary>
		/// Insert a new scan using an array of intensities along with bin_width
		/// </summary>
		/// <param name="frameParameters">
		/// Frame parameters
		/// </param>
		/// <param name="scanNum">
		/// Scan number
		/// </param>
		/// <param name="intensities">
		/// Array of intensities, including all zeros
		/// </param>
		/// <param name="bin_width">
		/// Bin width (used to compute m/z value of the BPI data point)
		/// </param>
		/// <returns>
		/// Number of non-zero data points
		/// </returns>
		public int InsertScan(FrameParameters frameParameters, int scanNum, int[] intensities, double bin_width)
		{
			int nonZeroCount = 0;

			if (frameParameters != null)
			{
				int nrlze = 0;
				int zeroCount = 0;
				int[] runLengthEncodedData = new int[intensities.Length];
				int tic = 0;
				int bpi = 0;
				double bpiMz = 0;
				int datatypeSize = 4;

				if (this.m_globalParameters == null)
				{
					this.m_globalParameters = DataReader.GetGlobalParametersFromTable(this.m_dbConnection);
				}

				for (int i = 0; i < intensities.Length; i++)
				{
					int intensity = intensities[i];
					if (intensity > 0)
					{
						// TIC is just the sum of all intensities
						tic += intensity;
						if (intensity > bpi)
						{
							bpi = intensity;
							bpiMz = this.convertBinToMz(i, bin_width, frameParameters);
						}

						if (zeroCount < 0)
						{
							runLengthEncodedData[nrlze++] = zeroCount;
							zeroCount = 0;
						}

						runLengthEncodedData[nrlze++] = intensity;
					}
					else
					{
						zeroCount--;
					}
				}

				byte[] compressedData = new byte[nrlze * datatypeSize * 5];
				if (nrlze > 0)
				{
					byte[] byte_array = new byte[nrlze * datatypeSize];
					Buffer.BlockCopy(runLengthEncodedData, 0, byte_array, 0, nrlze * datatypeSize);
					nonZeroCount = LZFCompressionUtil.Compress(
						ref byte_array, 
						nrlze * datatypeSize, 
						ref compressedData, 
						nrlze * datatypeSize * 5);
				}

				if (nonZeroCount != 0)
				{
					byte[] spectra = new byte[nonZeroCount];
					Array.Copy(compressedData, spectra, nonZeroCount);

					// Insert records
					this.InsertScanAddParameters(frameParameters.FrameNum, scanNum, nonZeroCount, bpi, bpiMz, tic, spectra);
					this.m_dbCommandPrepareInsertScan.ExecuteNonQuery();
					this.m_dbCommandPrepareInsertScan.Parameters.Clear();
				}
			}

			return nonZeroCount;
		}

		/// <summary>
		/// Insert a new scan using an array of intensities (as floats), bin_width, and "counter" which should be equivalent to the count of non-zero data in intensities
		/// </summary>
		/// <param name="frameParameters">
		/// </param>
		/// <param name="scanNum">
		/// </param>
		/// <param name="counter">
		/// </param>
		/// <param name="intensities">
		/// </param>
		/// <param name="bin_width">
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		public int InsertScan(
			FrameParameters frameParameters, 
			int scanNum, 
			int counter, 
			float[] intensities, 
			double bin_width)
		{
			int nrlze = 0;
			int zeroCount = 0;
			float[] runLengthEncodedData = new float[intensities.Length];
			double tic = 0;
			double bpi = 0;
			double bpiMz = 0;
			int datatypeSize = 4;

			if (this.m_globalParameters == null)
			{
				this.m_globalParameters = DataReader.GetGlobalParametersFromTable(this.m_dbConnection);
			}

			for (int i = 0; i < intensities.Length; i++)
			{
				float intensity = intensities[i];
				if (intensity > 0)
				{
					// TIC is just the sum of all intensities
					tic += intensity;
					if (intensity > bpi)
					{
						bpi = intensity;
						bpiMz = this.convertBinToMz(i, bin_width, frameParameters);
					}

					if (zeroCount < 0)
					{
						runLengthEncodedData[nrlze++] = (float)zeroCount;
						zeroCount = 0;
					}

					runLengthEncodedData[nrlze++] = intensity;
				}
				else
				{
					zeroCount--;
				}
			}

			int nlzf = 0;
			byte[] compressedData = new byte[nrlze * datatypeSize * 5];
			if (nrlze > 0)
			{
				byte[] byte_array = new byte[nrlze * datatypeSize];
				Buffer.BlockCopy(runLengthEncodedData, 0, byte_array, 0, nrlze * datatypeSize);
				nlzf = LZFCompressionUtil.Compress(
					ref byte_array, 
					nrlze * datatypeSize, 
					ref compressedData, 
					nrlze * datatypeSize * 5);
			}

			if (nlzf != 0)
			{
				byte[] spectra = new byte[nlzf];
				Array.Copy(compressedData, spectra, nlzf);

				// Insert records
				this.InsertScanAddParameters(frameParameters.FrameNum, scanNum, counter, (int)bpi, bpiMz, (int)tic, spectra);
				this.m_dbCommandPrepareInsertScan.ExecuteNonQuery();
				this.m_dbCommandPrepareInsertScan.Parameters.Clear();
			}

			return nlzf;
		}

		/// <summary>
		/// Insert a scan using a list of bins and a list of intensities
		/// TODO:: Deprecate this function since superseded by InsertScan with: int[] intensities, double bin_width
		/// </summary>
		/// <param name="fp">
		/// </param>
		/// <param name="scanNum">
		/// </param>
		/// <param name="bins">
		/// </param>
		/// <param name="intensities">
		/// </param>
		/// <param name="binWidth">
		/// </param>
		/// <param name="timeOffset">
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		[Obsolete]
		public int InsertScan(
			FrameParameters fp, 
			int scanNum, 
			List<int> bins, 
			List<int> intensities, 
			double binWidth, 
			int timeOffset)
		{
			try
			{
				int nonZeroCount = 0;

				if (this.m_globalParameters == null)
				{
					this.m_globalParameters = DataReader.GetGlobalParametersFromTable(this.m_dbConnection);
				}

				if (fp != null)
				{
					if (bins != null && intensities != null && bins.Count != 0 && intensities.Count != 0
					    && bins.Count == intensities.Count)
					{
						// that is the total number of data points that are to be encoded
						nonZeroCount = bins.Count;

						// this is the maximum length required assuming that there are no continuous values
						int[] rlze = new int[bins.Count * 2];

						// now iterate through both arrays and attempt to run length zero encode the values
						int tic = 0;
						int bpi = 0;
						int index = 0;
						double bpiMz = 0;
						int datatypeSize = 4;

						int previousBin = int.MinValue;

						rlze[index++] = -(timeOffset + bins[0]);
						for (int i = 0; i < bins.Count; i++)
						{
							int intensity = intensities[i];
							int currentBin = bins[i];

							// the intensities will always be positive integers
							tic += intensity;
							if (bpi < intensity)
							{
								bpi = intensity;
								bpiMz = this.convertBinToMz(currentBin, binWidth, fp);
							}

							if (i != 0 && currentBin != previousBin + 1)
							{
								// since the bin numbers are not continuous, add a negative index to the array
								// and in some cases we have to add the offset from the previous index
								rlze[index++] = previousBin - currentBin + 1;
							}

							// copy the intensity value and increment the index.
							rlze[index++] = intensity;

							previousBin = currentBin;
						}

						// so now we have a run length zero encoded array
						byte[] compresedRecord = new byte[index * datatypeSize * 5];
						byte[] byteBuffer = new byte[index * datatypeSize];
						Buffer.BlockCopy(rlze, 0, byteBuffer, 0, index * datatypeSize);
						int nlzf = LZFCompressionUtil.Compress(
							ref byteBuffer, 
							index * datatypeSize, 
							ref compresedRecord, 
							compresedRecord.Length);
						byte[] spectra = new byte[nlzf];

						Array.Copy(compresedRecord, spectra, nlzf);

						// Insert records
						if (true)
						{
							this.InsertScanAddParameters(fp.FrameNum, scanNum, bins.Count, bpi, bpiMz, tic, spectra);
							this.m_dbCommandPrepareInsertScan.ExecuteNonQuery();
							this.m_dbCommandPrepareInsertScan.Parameters.Clear();
						}
					}
				}

				return nonZeroCount;
			}
			catch (Exception e)
			{
				Console.WriteLine("Error writing scan: " + scanNum);
				Console.WriteLine(e);
				Console.WriteLine(e.StackTrace);
				return 0;
			}
		}

		/// <summary>
		/// //this method takes in a list of bin numbers and intensities and converts them to a run length encoded array
		/// which is later compressed at the byte level for reduced size
		/// </summary>
		/// <param name="frameParameters">
		/// </param>
		/// <param name="scanNum">
		/// </param>
		/// <param name="bins">
		/// </param>
		/// <param name="intensities">
		/// </param>
		/// <param name="binWidth">
		/// </param>
		/// <param name="timeOffset">
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		public int InsertScan(
			FrameParameters frameParameters, 
			int scanNum, 
			int[] bins, 
			int[] intensities, 
			double binWidth, 
			int timeOffset)
		{
			int nonZeroCount = 0;

			if (this.m_globalParameters == null)
			{
				this.m_globalParameters = DataReader.GetGlobalParametersFromTable(this.m_dbConnection);
			}

			if (frameParameters != null)
			{
				if (bins != null && intensities != null && bins.Length != 0 && intensities.Length != 0
				    && bins.Length == intensities.Length)
				{
					// that is the total number of datapoints that are to be encoded
					nonZeroCount = bins.Length;

					int[] rlze = new int[bins.Length * 2];

					// this is the maximum length required assuming that there are no continuous values

					// now iterate through both arrays and attempt to run length zero encode the values
					int tic = 0;
					int bpi = 0;
					int index = 0;
					double bpiMz = 0;
					int datatypeSize = 4;

					int previousBin = int.MinValue;

					rlze[index++] = -(timeOffset + bins[0]);
					for (int i = 0; i < bins.Length; i++)
					{
						int intensity = intensities[i];
						int currentBin = bins[i];

						// the intensities will always be positive integers
						tic += intensity;
						if (bpi < intensity)
						{
							bpi = intensity;
							bpiMz = this.convertBinToMz(currentBin, binWidth, frameParameters);
						}

						if (i != 0 && currentBin != previousBin + 1)
						{
							// since the bin numbers are not continuous, add a negative index to the array
							// and in some cases we have to add the offset from the previous index
							rlze[index++] = previousBin - currentBin + 1;
						}

						// copy the intensity value and increment the index.
						rlze[index++] = intensity;

						previousBin = currentBin;
					}

					// so now we have a run length zero encoded array
					byte[] compresedRecord = new byte[index * datatypeSize * 5];
					byte[] byteBuffer = new byte[index * datatypeSize];
					Buffer.BlockCopy(rlze, 0, byteBuffer, 0, index * datatypeSize);
					int nlzf = LZFCompressionUtil.Compress(
						ref byteBuffer, 
						index * datatypeSize, 
						ref compresedRecord, 
						compresedRecord.Length);
					byte[] spectra = new byte[nlzf];

					Array.Copy(compresedRecord, spectra, nlzf);

					// Insert records
					if (true)
					{
						this.InsertScanAddParameters(frameParameters.FrameNum, scanNum, bins.Length, bpi, bpiMz, tic, spectra);
						this.m_dbCommandPrepareInsertScan.ExecuteNonQuery();
						this.m_dbCommandPrepareInsertScan.Parameters.Clear();
					}
				}
			}

			return nonZeroCount;
		}

		/// <summary>
		/// </summary>
		/// <param name="frameParameters">
		/// </param>
		/// <param name="scanNum">
		/// </param>
		/// <param name="counter">
		/// </param>
		/// <param name="intensities">
		/// </param>
		/// <param name="binWidth">
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		public int InsertScan(FrameParameters frameParameters, int scanNum, int counter, int[] intensities, double binWidth)
		{
			if (frameParameters == null)
			{
				return -1;
			}

			int arraySize = intensities.Length;

			int nrlze = 0;
			int zero_count = 0;
			List<int> rlzeDataList = new List<int>();

			// int[] rlze_data = new int[arraySize];
			int tic_scan = 0;
			int bpi = 0;
			double bpi_mz = 0;
			int datatypeSize = 4;
			int indexOfMaxIntensity = 0;

			if (this.m_globalParameters == null)
			{
				this.m_globalParameters = DataReader.GetGlobalParametersFromTable(this.m_dbConnection);
			}

			// Calculate TIC and BPI
			for (int i = 0; i < arraySize; i++)
			{
				int intensity = intensities[i];
				if (intensity > 0)
				{
					// TIC is just the sum of all intensities
					tic_scan += intensity;
					if (intensity > bpi)
					{
						bpi = intensity;
						indexOfMaxIntensity = i;
					}

					if (zero_count < 0)
					{
						rlzeDataList.Add(zero_count);
						zero_count = 0;
					}

					rlzeDataList.Add(intensity);
				}
				else
				{
					zero_count--;
				}
			}

			bpi_mz = this.convertBinToMz(indexOfMaxIntensity, binWidth, frameParameters);

			// Compress intensities
			int nlzf = 0;

			nrlze = rlzeDataList.Count;
			int[] rlze_data = rlzeDataList.ToArray();

			byte[] compresedRecord = new byte[nrlze * datatypeSize * 5];
			if (nrlze > 0)
			{
				byte[] byteBuffer = new byte[nrlze * datatypeSize];
				Buffer.BlockCopy(rlze_data, 0, byteBuffer, 0, nrlze * datatypeSize);
				nlzf = LZFCompressionUtil.Compress(
					ref byteBuffer, 
					nrlze * datatypeSize, 
					ref compresedRecord, 
					compresedRecord.Length);
			}

			if (nlzf != 0)
			{
				byte[] spectra = new byte[nlzf];
				Array.Copy(compresedRecord, spectra, nlzf);

				// Insert records
				if (true)
				{
					this.InsertScanAddParameters(frameParameters.FrameNum, scanNum, counter, (int)bpi, bpi_mz, (int)tic_scan, spectra);
					this.m_dbCommandPrepareInsertScan.ExecuteNonQuery();
					this.m_dbCommandPrepareInsertScan.Parameters.Clear();
				}
			}

			return nlzf;
		}

		/// <summary>
		/// </summary>
		/// <param name="frameParameters">
		/// </param>
		/// <param name="scanNum">
		/// </param>
		/// <param name="counter">
		/// </param>
		/// <param name="intensities">
		/// </param>
		/// <param name="bin_width">
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		public int InsertScan(
			FrameParameters frameParameters, 
			int scanNum, 
			int counter, 
			short[] intensities, 
			double bin_width)
		{
			int nrlze = 0;
			int zeroCount = 0;
			short[] runLengthEncodedData = new short[intensities.Length];
			double tic_scan = 0;
			double bpi = 0;
			double bpi_mz = 0;
			int nonZeroIntensities = 0;
			int datatypeSize = 2;

			if (this.m_globalParameters == null)
			{
				this.m_globalParameters = DataReader.GetGlobalParametersFromTable(this.m_dbConnection);
			}

			// Calculate TIC and BPI
			for (int i = 0; i < intensities.Length; i++)
			{
				short intensity = intensities[i];
				if (intensity > 0)
				{
					// TIC is just the sum of all intensities
					tic_scan += intensity;
					if (intensity > bpi)
					{
						bpi = intensity;
						bpi_mz = this.convertBinToMz(i, bin_width, frameParameters);
					}

					if (zeroCount < 0)
					{
						runLengthEncodedData[nrlze++] = (short)zeroCount;
						zeroCount = 0;
					}

					runLengthEncodedData[nrlze++] = intensity;
					nonZeroIntensities++;
				}
				else
				{
					zeroCount--;
				}
			}

			int nlzf = 0;
			byte[] compressedData = new byte[nrlze * datatypeSize * 5];
			if (nrlze > 0)
			{
				byte[] byteBuffer = new byte[nrlze * datatypeSize];
				Buffer.BlockCopy(runLengthEncodedData, 0, byteBuffer, 0, nrlze * datatypeSize);
				nlzf = LZFCompressionUtil.Compress(
					ref byteBuffer, 
					nrlze * datatypeSize, 
					ref compressedData, 
					nrlze * datatypeSize * 5);
			}

			if (nlzf != 0)
			{
				byte[] spectra = new byte[nlzf];
				Array.Copy(compressedData, spectra, nlzf);

				// Insert records
				this.InsertScanAddParameters(frameParameters.FrameNum, scanNum, counter, (int)bpi, bpi_mz, (int)tic_scan, spectra);
				this.m_dbCommandPrepareInsertScan.ExecuteNonQuery();
				this.m_dbCommandPrepareInsertScan.Parameters.Clear();
			}

			return nlzf;
		}

		/// <summary>
		/// </summary>
		/// <param name="scanNum">
		/// </param>
		/// <param name="ms_level">
		/// </param>
		/// <returns>
		/// The <see cref="int"/>.
		/// </returns>
		public int InsertScanParameters(int scanNum, int ms_level)
		{
			this.insertScanAddScanParameters(scanNum, ms_level);

			this.m_dbCommandPrepareInsertScanParameters.ExecuteNonQuery();
			this.m_dbCommandPrepareInsertScanParameters.Parameters.Clear();

			return 0;
		}

		/// <summary>
		/// Post a new log entry to table Log_Entries
		/// </summary>
		/// <param name="EntryType">
		/// Log entry type (typically Normal, Error, or Warning)
		/// </param>
		/// <param name="Message">
		/// Log message
		/// </param>
		/// <param name="PostedBy">
		/// Process or application posting the log message
		/// </param>
		/// <remarks>
		/// The Log_Entries table will be created if it doesn't exist
		/// </remarks>
		public void PostLogEntry(string EntryType, string Message, string PostedBy)
		{
			PostLogEntry(this.m_dbConnection, EntryType, Message, PostedBy);
		}

		/// <summary>
		/// </summary>
		/// <param name="frameNumber">
		/// </param>
		/// <param name="slope">
		/// </param>
		/// <param name="intercept">
		/// </param>
		public void UpdateCalibrationCoefficients(int frameNumber, float slope, float intercept)
		{
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
			this.m_dbCommandUimf.CommandText = "UPDATE Frame_Parameters SET CalibrationSlope = " + slope.ToString()
			                                   + ", CalibrationIntercept = " + intercept + " WHERE FrameNum = " + frameNumber;

			this.m_dbCommandUimf.ExecuteNonQuery();
			this.m_dbCommandUimf.Dispose();
		}

		/// <summary>
		/// </summary>
		/// <param name="frameNumber">
		/// </param>
		/// <param name="parameterName">
		/// </param>
		/// <param name="parameterValue">
		/// </param>
		public void UpdateFrameParameter(int frameNumber, string parameterName, string parameterValue)
		{
			// Make sure the Frame_Parameters table has all of the required columns
			this.ValidateFrameParameterColumns();

			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
			this.m_dbCommandUimf.CommandText = "UPDATE Frame_Parameters SET " + parameterName + " = " + parameterValue
			                                   + " WHERE FrameNum = " + frameNumber;
			this.m_dbCommandUimf.ExecuteNonQuery();
			this.m_dbCommandUimf.Dispose();
		}

		/// <summary>
		/// </summary>
		/// <param name="frameNumber">
		/// </param>
		/// <param name="parameters">
		/// </param>
		/// <param name="values">
		/// </param>
		public void UpdateFrameParameters(int frameNumber, List<string> parameters, List<string> values)
		{
			// Make sure the Frame_Parameters table has all of the required columns
			this.ValidateFrameParameterColumns();

			StringBuilder commandText = new StringBuilder("UPDATE Frame_Parameters SET ");
			for (int i = 0; i < parameters.Count - 1; i++)
			{
				commandText.Append(parameters[i] + "=" + values[i] + ",");
			}

			commandText.Append(parameters[parameters.Count - 1] + "=" + values[values.Count - 1]);

			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
			this.m_dbCommandUimf.CommandText = commandText.ToString() + " WHERE FrameNum = " + frameNumber;

			// Console.WriteLine(m_dbCommandUimf.CommandText);
			this.m_dbCommandUimf.ExecuteNonQuery();
			this.m_dbCommandUimf.Dispose();
		}

		/// <summary>
		/// Updates the scan count for the given frame
		/// </summary>
		/// <param name="frameNum">
		/// The frame number to update
		/// </param>
		/// <param name="NumScans">
		/// The new scan count
		/// </param>
		public void UpdateFrameScanCount(int frameNum, int NumScans)
		{
			this.UpdateFrameParameter(frameNum, "Scans", NumScans.ToString());
		}

		/// <summary>
		/// This function updates the frame type to 1, 2, 2, 2, 1, 2, 2, 2, etc. for the specified frame range
		/// It is used in the nunit tests
		/// </summary>
		/// <param name="startFrameNum">
		/// The start Frame Num.
		/// </param>
		/// <param name="endFrameNum">
		/// The end Frame Num.
		/// </param>
		public void UpdateFrameType(int startFrameNum, int endFrameNum)
		{
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
			this.m_dbCommandUimf.CommandText = "UPDATE FRAME_PARAMETERS SET FRAMETYPE= :FRAMETYPE WHERE FRAMENUM = :FRAMENUM";
			this.m_dbCommandUimf.Prepare();

			for (int i = startFrameNum; i <= endFrameNum; i++)
			{
				int frameType = i % 4 == 0 ? 1 : 2;
				this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter("FRAMETYPE", frameType));
				this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter("FRAMENUM", i));
				this.m_dbCommandUimf.ExecuteNonQuery();
				this.m_dbCommandUimf.Parameters.Clear();
			}
		}

		/// <summary>
		/// Assures that NumFrames in the Global_Parameters table match the number of rows in the Frame_Parameters table
		/// </summary>
		public void UpdateGlobalFrameCount()
		{
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();

			this.m_dbCommandUimf.CommandText = "SELECT Count(*) FROM Frame_Parameters";
			object frameCount = this.m_dbCommandUimf.ExecuteScalar();
			this.m_dbCommandUimf.Dispose();

			if (frameCount != null)
			{
				this.UpdateGlobalParameter("NumFrames", frameCount.ToString());
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="parameterName">
		/// </param>
		/// <param name="parameterValue">
		/// </param>
		public void UpdateGlobalParameter(string parameterName, string parameterValue)
		{
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
			this.m_dbCommandUimf.CommandText = "UPDATE Global_Parameters SET " + parameterName.ToString() + " = "
			                                   + parameterValue;
			this.m_dbCommandUimf.ExecuteNonQuery();
			this.m_dbCommandUimf.Dispose();
		}

		/// <summary>
		/// </summary>
		/// <param name="tableName">
		/// </param>
		/// <param name="fileBytesAsBuffer">
		/// </param>
		/// <returns>
		/// The <see cref="bool"/>.
		/// </returns>
		public bool WriteFileToTable(string tableName, byte[] fileBytesAsBuffer)
		{
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
			try
			{
				if (!DataReader.TableExists(this.m_dbConnection, tableName))
				{
					// Create the table
					this.m_dbCommandUimf.CommandText = "CREATE TABLE " + tableName + " (FileText BLOB);";
					this.m_dbCommandUimf.ExecuteNonQuery();
				}
				else
				{
					// Delete the data currently in the table
					this.m_dbCommandUimf.CommandText = "DELETE FROM " + tableName + ";";
					this.m_dbCommandUimf.ExecuteNonQuery();
				}

				this.m_dbCommandUimf.CommandText = "INSERT INTO " + tableName + " VALUES (:Buffer);";
				this.m_dbCommandUimf.Prepare();

				this.m_dbCommandUimf.Parameters.Add(new SQLiteParameter(":Buffer", fileBytesAsBuffer));

				this.m_dbCommandUimf.ExecuteNonQuery();
			}
			finally
			{
				this.m_dbCommandUimf.Parameters.Clear();
				this.m_dbCommandUimf.Dispose();
			}

			return true;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Assures that certain columns are present in the Frame_Parameters table
		/// </summary>
		protected void ValidateFrameParameterColumns()
		{
			if (!this.m_FrameParameterColumnsVerified)
			{
				if (!DataReader.TableHasColumn(this.m_dbConnection, "Frame_Parameters", "Decoded"))
				{
					this.AddFrameParameter("Decoded", "INT", 0);
				}

				this.m_FrameParameterColumnsVerified = true;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="frag">
		/// </param>
		/// <returns>
		/// The <see cref="byte[]"/>.
		/// </returns>
		private static byte[] ConvertToBlob(double[] frag)
		{
			// convert the fragmentation profile into an array of bytes
			int length_blob = frag.Length;
			byte[] blob_values = new byte[length_blob * 8];

			Buffer.BlockCopy(frag, 0, blob_values, 0, length_blob * 8);

			return blob_values;
		}

		/// <summary>
		/// </summary>
		/// <param name="dt">
		/// </param>
		/// <returns>
		/// The <see cref="string"/>.
		/// </returns>
		private string ConvertDateTimeToString(DateTime dt)
		{
			// Convert DateTime to String yyyy-mm-dd hh:mm:ss
			string dt_string = dt.Year.ToString("0000") + "-" + dt.Month.ToString("00") + "-" + dt.Day.ToString("00") + " "
			                   + dt.Hour.ToString("00") + ":" + dt.Minute.ToString("00") + ":" + dt.Second.ToString("00");

			return "'" + dt_string + "'";
		}

		/// <summary>
		/// </summary>
		/// <param name="frameNumber">
		/// </param>
		/// <param name="scanNum">
		/// </param>
		/// <param name="nonZeroCount">
		/// </param>
		/// <param name="bpi">
		/// </param>
		/// <param name="bpiMz">
		/// </param>
		/// <param name="tic">
		/// </param>
		/// <param name="spectraRecord">
		/// </param>
		private void InsertScanAddParameters(
			int frameNumber, 
			int scanNum, 
			int nonZeroCount, 
			int bpi, 
			double bpiMz, 
			int tic, 
			byte[] spectraRecord)
		{
			this.m_dbCommandPrepareInsertScan.Parameters.Add(new SQLiteParameter("FrameNum", frameNumber.ToString()));
			this.m_dbCommandPrepareInsertScan.Parameters.Add(new SQLiteParameter("ScanNum", scanNum.ToString()));
			this.m_dbCommandPrepareInsertScan.Parameters.Add(new SQLiteParameter("NonZeroCount", nonZeroCount.ToString()));
			this.m_dbCommandPrepareInsertScan.Parameters.Add(new SQLiteParameter("BPI", bpi.ToString()));
			this.m_dbCommandPrepareInsertScan.Parameters.Add(new SQLiteParameter("BPI_MZ", bpiMz.ToString()));
			this.m_dbCommandPrepareInsertScan.Parameters.Add(new SQLiteParameter("TIC", tic.ToString()));
			this.m_dbCommandPrepareInsertScan.Parameters.Add(new SQLiteParameter("Intensities", spectraRecord));
		}

		/// <summary>
		/// 
		/// </summary>
		private void PrepareInsertFrame()
		{
			this.m_dbCommandPrepareInsertFrame = this.m_dbConnection.CreateCommand();

			string voltEntranceHPFInColName;
			string voltEntranceHPFOutColName;

			string voltExitHPFInColName;
			string voltExitHPFOutColName;

			if (DataReader.ColumnExists(this.m_dbConnection, "Frame_Parameters", "voltEntranceHPFIn"))
			{
				voltEntranceHPFInColName = "voltEntranceHPFIn";
				voltEntranceHPFOutColName = "voltEntranceHPFOut";
			}
			else
			{
				voltEntranceHPFInColName = "voltEntranceIFTIn";
				voltEntranceHPFOutColName = "voltEntranceIFTOut";
			}

			if (DataReader.ColumnExists(this.m_dbConnection, "Frame_Parameters", "voltExitHPFIn"))
			{
				voltExitHPFInColName = "voltExitHPFIn";
				voltExitHPFOutColName = "voltExitHPFOut";
			}
			else
			{
				voltExitHPFInColName = "voltExitIFTIn";
				voltExitHPFOutColName = "voltExitIFTOut";
			}

			string cmd =
				"INSERT INTO Frame_Parameters (FrameNum, StartTime, Duration, Accumulations, FrameType, Scans, IMFProfile, TOFLosses,"
				+ "AverageTOFLength, CalibrationSlope, CalibrationIntercept,a2, b2, c2, d2, e2, f2, Temperature, voltHVRack1, voltHVRack2, voltHVRack3, voltHVRack4, "
				+ "voltCapInlet, " + voltEntranceHPFInColName + ", " + voltEntranceHPFOutColName + ", ";

			cmd += "voltEntranceCondLmt, " + "voltTrapOut, voltTrapIn, voltJetDist, voltQuad1, voltCond1, voltQuad2, voltCond2, "
			       + "voltIMSOut, " + voltExitHPFInColName + ", " + voltExitHPFOutColName
			       + ", voltExitCondLmt, PressureFront, PressureBack, MPBitOrder, FragmentationProfile, HighPressureFunnelPressure, IonFunnelTrapPressure, "
			       + "RearIonFunnelPressure, QuadrupolePressure, ESIVoltage, FloatVoltage, CalibrationDone, Decoded)"
			       + "VALUES (:FrameNum, :StartTime, :Duration, :Accumulations, :FrameType,:Scans,:IMFProfile,:TOFLosses,"
			       + ":AverageTOFLength,:CalibrationSlope,:CalibrationIntercept,:a2,:b2,:c2,:d2,:e2,:f2,:Temperature,:voltHVRack1,:voltHVRack2,:voltHVRack3,:voltHVRack4, "
			       + ":voltCapInlet,:" + voltEntranceHPFInColName + ",:" + voltEntranceHPFOutColName
			       + ",:voltEntranceCondLmt,:voltTrapOut,:voltTrapIn,:voltJetDist,:voltQuad1,:voltCond1,:voltQuad2,:voltCond2,"
			       + ":voltIMSOut,:" + voltExitHPFInColName + ",:" + voltExitHPFOutColName + ",:voltExitCondLmt, "
			       + ":PressureFront,:PressureBack,:MPBitOrder,:FragmentationProfile, " + ":HPPressure, :IPTrapPressure, "
			       + ":RIFunnelPressure, :QuadPressure, :ESIVoltage, :FloatVoltage, :CalibrationDone, :Decoded);";

			this.m_dbCommandPrepareInsertFrame.CommandText = cmd;
			this.m_dbCommandPrepareInsertFrame.Prepare();
		}

		/// <summary>
		/// 
		/// </summary>
		private void PrepareInsertScan()
		{
			// This function should be called before looping through each frame and scan
			this.m_dbCommandPrepareInsertScan = this.m_dbConnection.CreateCommand();
			this.m_dbCommandPrepareInsertScan.CommandText =
				"INSERT INTO Frame_Scans (FrameNum, ScanNum, NonZeroCount, BPI, BPI_MZ, TIC, Intensities) "
				+ "VALUES(?,?,?,?,?,?,?);";
			this.m_dbCommandPrepareInsertScan.Prepare();

			if (this.m_isScanParameterTable)
			{
				this.m_dbCommandPrepareInsertScanParameters = this.m_dbConnection.CreateCommand();
				this.m_dbCommandPrepareInsertScanParameters.CommandText =
					"INSERT INTO Scan_Parameters (ScanNum, MS_Level) VALUES (:ScanNum, :MS_Level);";

				this.m_dbCommandPrepareInsertScanParameters.Prepare();
			}
		}

		/// <summary>
		/// TODO The transaction begin.
		/// </summary>
		private void TransactionBegin()
		{
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
			this.m_dbCommandUimf.CommandText = "PRAGMA synchronous=0;BEGIN TRANSACTION;";
			this.m_dbCommandUimf.ExecuteNonQuery();
		}

		/// <summary>
		/// TODO The transaction commit.
		/// </summary>
		private void TransactionCommit()
		{
			this.m_dbCommandUimf = this.m_dbConnection.CreateCommand();
			this.m_dbCommandUimf.CommandText = "END TRANSACTION;PRAGMA synchronous=1;";
			this.m_dbCommandUimf.ExecuteNonQuery();
		}

		/// <summary>
		/// </summary>
		/// <param name="binNumber">
		/// </param>
		/// <param name="binWidth">
		/// </param>
		/// <param name="frameParameters">
		/// </param>
		/// <returns>
		/// The <see cref="double"/>.
		/// </returns>
		private double convertBinToMz(int binNumber, double binWidth, FrameParameters frameParameters)
		{
			// mz = (k * (t-t0))^2
			double t = binNumber * binWidth / 1000;
			double resMassErr = frameParameters.a2 * t + frameParameters.b2 * System.Math.Pow(t, 3)
			                    + frameParameters.c2 * System.Math.Pow(t, 5) + frameParameters.d2 * System.Math.Pow(t, 7)
			                    + frameParameters.e2 * System.Math.Pow(t, 9) + frameParameters.f2 * System.Math.Pow(t, 11);
			double mz =
				(double)
				(frameParameters.CalibrationSlope
				 * ((double)(t - (double)this.m_globalParameters.TOFCorrectionTime / 1000 - frameParameters.CalibrationIntercept)));
			mz = (mz * mz) + resMassErr;
			return mz;
		}

		/// <summary>
		/// </summary>
		/// <param name="scan_number">
		/// </param>
		/// <param name="MS_Level">
		/// </param>
		private void insertScanAddScanParameters(int scan_number, int MS_Level)
		{
			this.m_dbCommandPrepareInsertScanParameters.Parameters.Add(new SQLiteParameter("ScanNum", scan_number.ToString()));
			this.m_dbCommandPrepareInsertScanParameters.Parameters.Add(new SQLiteParameter("MS_Level", MS_Level.ToString()));
		}

		#endregion
	}
}