using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class Write_to_file : MonoBehaviour
{
    public static void write(string file_path, ref List<float> time, ref List<Configuration> configurations)
    {
        File.Delete(file_path);
        StreamWriter output_file = new StreamWriter(file_path);

        string str;
        
        for(int l = 0; l < time.Count; l++)
        {
            str = "";
            str += time[l].ToString("0.0000000", CultureInfo.CreateSpecificCulture("en-GB"));
            str += ", ";
            str += configurations[l].q1.ToString("0.0000000", CultureInfo.CreateSpecificCulture("en-GB"));
            str += ", ";
            str += configurations[l].q2.ToString("0.0000000", CultureInfo.CreateSpecificCulture("en-GB"));
            str += ", ";
            str += configurations[l].q3.ToString("0.0000000", CultureInfo.CreateSpecificCulture("en-GB"));
            str += ", ";
            str += configurations[l].q4.ToString("0.0000000", CultureInfo.CreateSpecificCulture("en-GB"));
            str += ", ";
            str += configurations[l].grabber_claw.ToString("0.0000000", CultureInfo.CreateSpecificCulture("en-GB"));
            str += "\n";
            
            output_file.Write(str);
        }

        output_file.Close();
    }
}
