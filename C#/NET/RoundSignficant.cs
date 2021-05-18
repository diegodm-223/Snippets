/*
*   2021/05/18
*   Author: Diego Delgado
*
*   This method allows to set a maximum number of significant digits to a number.
*/   

using System;
using System.Globalization;
using System.Linq;

/// <summary>
/// Round a double to a given amount of significant digits. 
/// </summary>
/// <example>
///     Given 2 significant digits and 4 decimal positions: 
///      1.0569  ==>   1.0600
///     26.2310  ==>  26.0000
///      0.1052  ==>   0.1050
///           0  ==>   0.0000
/// </example>
/// <remarks>
///     A maximum of <paramref name="decimalPositions"/> will be considered. So, for example, 
///     the value 0.00001 will become 0 for 4 decimal positions. This value is rounded, so 0.00009
///     will return 0.0001.
/// </remarks>
/// <param name="value">Number to be rounded.</param>
/// <param name="significantDigits">Number of significant digits that the result will have.</param>
/// <param name="decimalPositions">Number of decimal positions that will be considered from the given value. </param>
/// <returns>The given value rounded to have a maximum of significant digits and a maximum decimal positions</returns>
static double RoundSignificant(double value, int significantDigits, int decimalPositions = 4)
{
    value = Math.Round(value, decimalPositions);
    int intPartLength = (int)Math.Floor(Math.Log10(Math.Abs(value))); // lenth of the integer part. 

    double tempValue = value;   // Aux double which contains the value minus the significant digits found. 
    double finalValue = 0;      // Double with the significant digits found. 
    string tempValueSci = "";   // tempValue as string with scientific notation format. 
    int digit = 0;
    int exp = 0;
    for (int i = 0; i < significantDigits; i++)
    {
        if (i == significantDigits - 1)
        {
            // Last digit. Round it,
            tempValueSci = tempValue.ToString("0.E+0",CultureInfo.InvariantCulture);
        }
        else
        {
            // Scientific notation like 0.0000E+00
            string sciFormat = "0." + string.Concat(Enumerable.Repeat("0", intPartLength+decimalPositions)) + "E+00";                    
            tempValueSci = tempValue.ToString(sciFormat,CultureInfo.InvariantCulture);
        }
        digit = Int32.Parse(tempValueSci.Split('E')[0].Split('.')[0]);
        exp = Int32.Parse(tempValueSci.Split('E')[1]);
        finalValue += digit * Math.Pow(10, exp);
        tempValue -= digit * Math.Pow(10, exp);
    }
    return finalValue;
}
    

