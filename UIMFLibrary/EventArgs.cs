﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Defines the MessageEventArgs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace UIMFLibrary
{
    using System;

    /// <summary>
    /// Message event args
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        #region Fields

        /// <summary>
        /// Message.
        /// </summary>
        public readonly string Message;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
        /// </summary>
        /// <param name="message">
        /// Message.
        /// </param>
        public MessageEventArgs(string message)
        {
            Message = message;
        }

        #endregion
    }

    /// <summary>
    /// FrameNum shift event args
    /// </summary>
    public class FrameNumShiftEventArgs : EventArgs
    {
        #region Fields

        /// <summary>
        /// Number of frames that frame numbers in FrameRanges were decremented by
        /// </summary>
        public readonly int DecrementAmount;

        /// <summary>
        /// Frame numbers that were shifted, for example:
        /// 37,89-200
        /// </summary>
        public readonly string FrameRanges;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameNumShiftEventArgs"/> class.
        /// </summary>
        /// <param name="decrementAmount"></param>
        /// <param name="frameRanges"></param>
        public FrameNumShiftEventArgs(int decrementAmount, string frameRanges)
        {
            DecrementAmount = decrementAmount;
            FrameRanges = frameRanges;
        }

        #endregion
    }


    /// <summary>
    /// The progress event args.
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        #region Fields

        /// <summary>
        /// Value between 0 and 100
        /// </summary>
        public readonly double PercentComplete;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressEventArgs"/> class.
        /// </summary>
        /// <param name="percentComplete">
        /// Percent complete.
        /// </param>
        public ProgressEventArgs(double percentComplete)
        {
            PercentComplete = percentComplete;
        }

        #endregion
    }
}