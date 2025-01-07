using System.Globalization;

namespace PlannerBusConductoresN_MAUI;

public class Misc_Objects {
    #region[GeoCalculation]
    public static double CalcSpeed(double lat1, double lon1, TimeSpan time1, double lat2, double lon2, TimeSpan time2) {
        double distance = ArcInMeters(lat1, lon1, lat2, lon2);
        double time = (time2 - time1).TotalSeconds;
        double speed = distance / time;
        return speed;
    }

    public static double DegreeBearing(double lat1, double lon1, double lat2, double lon2) {
        double dLon = ToRad(lon2 - lon1);
        double dPhi = Math.Log(Math.Tan((ToRad(lat2) / 2) + (Math.PI / 4)) / Math.Tan((ToRad(lat1) / 2) + (Math.PI / 4)));
        if (Math.Abs(dLon) > Math.PI) {
            dLon = dLon > 0 ? -((2 * Math.PI) - dLon) : ((2 * Math.PI) + dLon);
        }
        return ToBearing(Math.Atan2(dLon, dPhi));
    }

    public static double ArcInMeters(double lat0, double lon0, double lat1, double lon1) {
        const double earthRadius = 6372797.560856;
        return earthRadius * ArcInRadians(lat0, lon0, lat1, lon1);
    }

    private static double ToRad(double degrees) {
        return degrees * (Math.PI / 180);
    }

    private static double ToDegrees(double radians) {
        return radians * 180 / Math.PI;
    }

    private static double ToBearing(double radians) {
        return (ToDegrees(radians) + 360) % 360;
    }

    private static double ArcInRadians(double lat0, double lon0, double lat1, double lon1) {
        double latitudeArc = DegToRad(lat0 - lat1);
        double longitudeArc = DegToRad(lon0 - lon1);
        double latitudeH = Math.Sin(latitudeArc * 0.5);
        latitudeH *= latitudeH;
        double lontitudeH = Math.Sin(longitudeArc * 0.5);
        lontitudeH *= lontitudeH;
        double tmp = Math.Cos(DegToRad(lat0)) * Math.Cos(DegToRad(lat1));
        return 2.0 * Math.Asin(Math.Sqrt(latitudeH + tmp * lontitudeH));
    }

    private static double DegToRad(double x) {
        return x * Math.PI / 180;
    }
    #endregion

    #region[TextFormatting]
    public static string AllSortedOut() {
        return $"Coordenadas:\t({OrganizePos(Constantes.latitude)}, {OrganizePos(Constantes.longitude)})" +
            $"\nGrados:\t\t\t{DecToMult(Constantes.degrees ?? 0.0)}" +
            $"\nTiempo:\t\t\t{Constantes.timestamp.ToString("c").Remove(8)}" +
            $"\nVelocidad:\t\t{Math.Round(Constantes.speed ?? 0.0, 3)} k/h" +
            $"\nDistancia:\t\t{Math.Round(Constantes.metres ?? 0.0, 3)} m";
    }

    private static string OrganizePos(double value) {
        return value.ToString("0.0000000", CultureInfo.InvariantCulture);
    }

    private static string DecToMult(double degrees) {
        double minutes = (degrees - Math.Truncate(degrees)) * 60;
        double seconds = (minutes - Math.Truncate(minutes)) * 60;
        return $"{AddZero((double)Math.Truncate(degrees))}º {AddZero((double)Math.Truncate(minutes))}' {AddZero((double)Math.Truncate(seconds))}''";
    }

    private static string AddZero(double value) {
        return value < 10 ? $"0{value}" : $"{value}";
    }
    #endregion
}