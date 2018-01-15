using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class Log
{
    const string pathMain = "Temp/mainlog";
    const string pathTmp = "Temp/tmplog";

    public static void WriteString(string logToWrite)
    {
        StreamWriter writerMain = new StreamWriter(pathMain, true);
        StreamWriter writerTmp = new StreamWriter(pathTmp, true);

        writerMain.WriteLine(logToWrite);
        writerMain.Close();

        writerTmp.WriteLine(logToWrite);
        writerTmp.Close();
    }

    public static void WriteStringDate(string logToWrite)
    {
        string stringToWrite = DateTime.Now.ToString("MM/dd/yy HH:mm:ss") + " - " + logToWrite;

        StreamWriter writerMain = new StreamWriter(pathMain, true);
        StreamWriter writerTmp = new StreamWriter(pathTmp, true);

        writerMain.WriteLine(stringToWrite);
        writerMain.Close();

        writerTmp.WriteLine(stringToWrite);
        writerTmp.Close();
    }

    public static void ClearFile()
    {
        StreamWriter writer = new StreamWriter(pathTmp);
        writer.Flush();
        writer.Close();
    }
}