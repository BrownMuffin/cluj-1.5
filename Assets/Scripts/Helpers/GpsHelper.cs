using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GpsHelper
{
    public static float DistanceTo(GpsPosition localPosition, GpsPosition targetPosition)
    {
        double rlat1 = Mathf.PI * localPosition.Latitude / 180;
        double rlat2 = Mathf.PI * targetPosition.Latitude / 180;
        double theta = localPosition.Longitude - targetPosition.Longitude;
        double rtheta = Mathf.PI * theta / 180;
        double dist = 
            Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
            Math.Cos(rlat2) * Math.Cos(rtheta);

        dist = Math.Acos(dist);
        dist = dist * 180 / Mathf.PI;
        dist = dist * 60 * 1.853159616f;

        return (float)dist;
    }
    
    public static string DistanceToString(float distance)
    {
        if (distance < 10f)
            return Mathf.RoundToInt(distance * 1000) + " M";

        return Mathf.RoundToInt(distance) + " KM";
    }

    public static float AngleTo(GpsPosition localPosition, GpsPosition targetPosition)
    {
        localPosition.Latitude *= Mathf.Deg2Rad;
        targetPosition.Latitude *= Mathf.Deg2Rad;
        localPosition.Longitude *= Mathf.Deg2Rad;
        targetPosition.Longitude *= Mathf.Deg2Rad;

        double dLon = (targetPosition.Longitude - localPosition.Longitude);
        double y = Math.Sin(dLon) * Math.Cos(targetPosition.Latitude);
        double x = (Math.Cos(localPosition.Latitude) * Math.Sin(targetPosition.Latitude)) - 
            (Math.Sin(localPosition.Latitude) * Math.Cos(targetPosition.Latitude) * Math.Cos(dLon));
        double brng = Math.Atan2(y, x);
        brng = Mathf.Rad2Deg * brng;
        brng = (brng + 360) % 360;
        brng = 360 - brng;

        return (float)brng;
    }
}

[Serializable]
public struct GpsPosition
{
    public float Latitude;
    public float Longitude;

    public GpsPosition(float lat, float lon)
    {
        Latitude = lat;
        Longitude = lon;
    }
}
