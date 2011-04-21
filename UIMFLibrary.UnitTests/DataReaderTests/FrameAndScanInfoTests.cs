﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;

namespace UIMFLibrary.UnitTests.DataReaderTests
{
    [TestFixture]
    public class FrameAndScanInfoTests
    {
        [Test]
        public void getNumberOfFramesTest()
        {
        
            UIMFLibrary.DataReader reader = new UIMFLibrary.DataReader();
            reader.OpenUIMF(FileRefs.uimfStandardFile1);
            int numFrames = reader.GetGlobalParameters().NumFrames;

            Console.WriteLine("Number of frames = " + numFrames);
            Assert.AreEqual(3220, numFrames);
        }



        [Test]
        public void getAvgTOFLengthTest1()
        {
             UIMFLibrary.DataReader reader = new UIMFLibrary.DataReader();
            reader.OpenUIMF(FileRefs.uimfStandardFile1);
            double avgTOFLength = reader.GetFrameParameters(0).AverageTOFLength;

            Assert.AreEqual(162555.56m, (decimal)avgTOFLength);

        }


        [Test]
        public void getFrameInfo_demultiplexedFrame0_Test1()
        {
            UIMFLibrary.DataReader reader = new UIMFLibrary.DataReader();
            reader.OpenUIMF(FileRefs.uimfStandardDemultiplexedFile1);

            int testFrame = 0;

            FrameParameters fp = reader.GetFrameParameters(testFrame);
            TestUtilities.displayFrameParameters(fp);

        }



        [Test]
        public void getFrameInfo_demultiplexed_firstFrame_Test1()
        {
            UIMFLibrary.DataReader reader = new UIMFLibrary.DataReader();
            reader.OpenUIMF(FileRefs.uimfStandardDemultiplexedFile1);

            int firstFrame = 0;

            FrameParameters fp = reader.GetFrameParameters(firstFrame);
            TestUtilities.displayFrameParameters(fp);


        }




        [Test]
        public void getFrameInfo_demultiplexed_lastFrame_Test1()
        {
            UIMFLibrary.DataReader reader = new UIMFLibrary.DataReader();
            reader.OpenUIMF(FileRefs.uimfStandardDemultiplexedFile1);

            int numFrames = reader.GetGlobalParameters().NumFrames;
            int lastFrame = numFrames-1;

            Console.WriteLine("Last frame = " + lastFrame);

            FrameParameters fp = reader.GetFrameParameters(lastFrame);
            TestUtilities.displayFrameParameters(fp);


        }

        [Test]
        public void getGlobalParams_test1()
        {
            UIMFLibrary.DataReader reader = new UIMFLibrary.DataReader();

            reader.OpenUIMF(FileRefs.uimfStandardDemultiplexedFile1);
            GlobalParameters gp = reader.GetGlobalParameters();
            DateTime dt = DateTime.Parse(gp.DateStarted);

            reader.CloseUIMF();

            Assert.AreEqual("4/7/2011 6:40:30 AM", dt.ToString());
        }


        [Test]
        public void getFramePressure_lastFrame()
        {
            UIMFLibrary.DataReader reader = new UIMFLibrary.DataReader();
            reader.OpenUIMF(FileRefs.uimfStandardFile1);

            int lastFrame = 3219;
            int secondToLastFrame = lastFrame - 1;

            double pressureBackLastFrame = reader.GetFrameParameters(lastFrame).PressureBack;
            double pressureBackSecondToLastFrame = reader.GetFrameParameters(secondToLastFrame).PressureBack;

            Console.WriteLine("Pressure for frame " + secondToLastFrame + " = " + pressureBackSecondToLastFrame);

            Console.WriteLine("Pressure for frame "+lastFrame + " = " + pressureBackLastFrame);

        }



    }
}