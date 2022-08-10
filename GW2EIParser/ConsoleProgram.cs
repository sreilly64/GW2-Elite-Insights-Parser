﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GW2EIParser.Exceptions;

namespace GW2EIParser
{
    internal class ConsoleProgram
    {

        public ConsoleProgram(IEnumerable<string> logFiles)
        {
            if (ProgramHelper.ParseMultipleLogs())
            {
                var splitLogFiles = new List<List<string>>();
                var sizeSortedLogFiles = new List<string>(logFiles);
                for (int i = 0; i < ProgramHelper.GetMaxParallelRunning(); i++)
                {
                    splitLogFiles.Add(new List<string>());
                }
                sizeSortedLogFiles.Sort((x, y) =>
                {
                    var fInfoX = new FileInfo(x);
                    long xValue = fInfoX.Exists ? fInfoX.Length : 0;
                    var fInfoY = new FileInfo(y);
                    long yValue = fInfoY.Exists ? fInfoY.Length : 0;
                    return xValue.CompareTo(yValue);
                });
                int index = 0;
                foreach (string file in sizeSortedLogFiles)
                {
                    splitLogFiles[index].Add(file);
                    index = (index + 1) % ProgramHelper.GetMaxParallelRunning();
                }
                Parallel.ForEach(splitLogFiles, files =>
                {
                    foreach (string file in files)
                    {
                        ParseLog(file);
                    }
                });
            }
            else
            {
                foreach (string file in logFiles)
                {
                    ParseLog(file);
                }
            }
        }

        private static void ParseLog(string logFile)
        {
            var operation = new ConsoleOperationController(logFile);
            try
            {
                ProgramHelper.DoWork(operation);
                operation.FinalizeStatus("Parsing Successful - ");
            }
            catch (ProgramException ex)
            {
                operation.UpdateProgress(ex.InnerException.Message);
                operation.FinalizeStatus("Parsing Failure - ");
            }
            catch (Exception)
            {
                operation.UpdateProgress("Something terrible has happened");
                operation.FinalizeStatus("Parsing Failure - ");
            }
            finally
            {
                ProgramHelper.GenerateTraceFile(operation);
            }
        }
    }
}
