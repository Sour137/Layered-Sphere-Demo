using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PrecipitationTrigModel
{
    int maxY;
    int worldY;
    double offsetA;
    double offsetB;
    double magnitudeA;
    double magnitudeB;
    int constant;
    int adjustingExpo;

    /// <summary>
    /// Default constructor. Can be used if maxY will be set later.
    /// </summary>
    public PrecipitationTrigModel()
    {
        maxY = 0;
        worldY = 180;
        offsetA = (Math.PI / 90.0);
        offsetB = (Math.PI / 30.0);
        magnitudeA = -800;
        magnitudeB = -200;
        constant = 1060;
        adjustingExpo = 19;
    }

    /// <summary>
    /// Complicated constructor. Can be used if maxY is already known.
    /// </summary>
    /// <param name="in_maxY">Specified limit for y-values on the tile grid.</param>
    public PrecipitationTrigModel(int in_maxY)
    {
        maxY = in_maxY;
        worldY = 180;
        offsetA = (Math.PI / 90.0);
        offsetB = (Math.PI / 30.0);
        magnitudeA = -1800;//-800;
        magnitudeB = -1000;//-200;
        constant = 1100;//1060;
        adjustingExpo = 19;
    }

    /// <summary>
    /// Complicated constructor. Parameterizes all vars.
    /// </summary>
    /// <param name="in_maxY">Specified limit of y-values on the tile grid.</param>
    /// <param name="in_worldY">Specified world y-value limit. 180 for a sphere, and 360 for a torus.</param>
    /// <param name="in_offsetA">Specified offset for the center of the curve.</param>
    /// <param name="in_offsetB">Specified offset for the tropical dry zones.</param>
    /// <param name="in_magnitudeA">Specified magnitude of the 'a' function component.</param>
    /// <param name="in_magnitudeB">Specified magnitude of the 'b' function component.</param>
    /// <param name="in_const">Specified magnitude of the 'c' function component.</param>
    /// <param name="in_adjExpo">Specified exponent for the 'a' function trig component. 19, and other default values for the other vars aligns the tropical dry zones to about 30 degress north or south.</param>
    public PrecipitationTrigModel(int in_maxY, int in_worldY, double in_offsetA, double in_offsetB, double in_magnitudeA, double in_magnitudeB, int in_const, int in_adjExpo)
    {
        maxY = in_maxY;
        worldY = in_worldY;
        offsetA = in_offsetA;
        offsetB = in_offsetB;
        magnitudeA = in_magnitudeA;
        magnitudeB = in_magnitudeB;
        constant = in_const;
        adjustingExpo = in_adjExpo;
    }

    /// <summary>
    /// Calculates the value. Is the core function of this class.
    /// </summary>
    /// <param name="in_y">Specified y value to calculate for. Adjusts value to the world coordinate system.</param>
    /// <param name="in_maxY">Specified limit for y-values on the tile grid.</param>
    /// <returns>A precipitation quantity in MILLIMETERS.</returns>
    public float Get_PrecipTrigModelValue(float in_y, int in_maxY)
    {
        float toRet = 0;

        float calcVal = AdjustY(in_y);

        float funkA = Calculate_PTMFunkA(calcVal);
        float funkB = Calculate_PTMFunkB(calcVal);

        toRet = funkA + funkB + constant;

        if (toRet < 0)
            toRet = 1;


        //if (in_y == 90)
        //    Console.WriteLine("PTM value of " + toRet + " = " + funkA + " + " + funkB + " + " + constant);

        return toRet;
    }

    /// <summary>
    /// Adjusts the tile grid y coordinate to a spherical y coordinate.
    /// </summary>
    /// <param name="in_y">Specified tile grid y value.</param>
    /// <returns>A spherical y coordinate.</returns>
    private float AdjustY(float in_y)
    {
        float toRet = 0.0f;

        float divy = in_y / maxY;
        toRet = divy * worldY;

        return toRet;
    }

    /// <summary>
    /// The first component of the core function. From default values:
    /// -800 * cos((pi * x) / 90)^19
    /// </summary>
    /// <param name="in_value">Specified y value to calculate for. In the example above, we can say x = y.</param>
    /// <returns>The result of the first calculation of the core function component 'a'.</returns>
    private float Calculate_PTMFunkA(float in_value)
    {
        float toRet = 0.0f;

        double inner = in_value * offsetA;
        double triggy = Math.Cos(inner);
        //Console.WriteLine("Trig answer = " + triggy + " and an inner value of " + inner + " and an in_value of " + in_value);
        //Console.ReadLine();

        double trigRaised = Math.Pow(triggy, adjustingExpo);
        double answer = magnitudeA * trigRaised;

        //Console.WriteLine("answer = " + answer + " = " + magnitudeA + " * " + trigRaised);
        //Console.ReadLine();

        toRet = (float)(answer);

        return toRet;
    }

    /// <summary>
    /// The second component of the core function. From default values:
    /// -200 * cos((pi * x) / 30)
    /// </summary>
    /// <param name="in_value">Specified y value. From the example above we can say x = y.</param>
    /// <returns>The result of the calculation of the second component of the core function.</returns>
    private float Calculate_PTMFunkB(float in_value)
    {
        float toRet = 0.0f;

        double inner = in_value * offsetB;
        double triggy = Math.Cos(inner);
        double answer = magnitudeB * triggy;

        toRet = (float)(answer);

        return toRet;
    }

    /// <summary>
    /// Simple return for the var maxY.
    /// </summary>
    /// <returns>The var maxY</returns>
    public int Get_maxY() { return maxY; }

    /// <summary>
    /// Simple assignment of the var maxY. Allows for use of the non-default constructors.
    /// </summary>
    /// <param name="in_maxY">Specified value to assign to maxY</param>
    public void Set_maxY(int in_maxY) { maxY = in_maxY; }
}

