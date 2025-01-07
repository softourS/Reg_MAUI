using System.Text;

namespace PlannerBusConductoresN_MAUI;

public static class Constantes {
    public static double latitude;
    public static double longitude;
    public static double? degrees;
    public static TimeSpan timestamp;
    public static double? speed;
    public static double? metres;

    public static double latitude_last;
    public static double longitude_last;

    public static string userMessage = "";
    public static Int32 id_pasajero;
    public static string id_device = "";
    public static string tokepush = "";

    public static string NOTIFICATION_ID = "";
    public static string CHANNEL_ID = "";
    public static string CHANNEL_NAME = "";

    //public static List<QRCodeb> qrcodes = new List<QRCodeb>();

    public static string ver = "v. 2.5.4";
    public static string user = "cross";
    public static string pass = "platform";
    public static string googlemapsurl = "https://maps.googleapis.com/maps/api/directions/";
    public static string urlSoft = "";
    public static string urlSigR = "";
    public static string urlTrans = "";
    public static string urlVTC = "";
    public static string urlTAD = "";
    public static string urlTaller = "";
    public static string urlLocat = "";
    public static string connError = "Compruebe su conexión   y reinicie la aplicación";
    public static string kmError = "*Kilómetros totales incorrectos.   Introduzca la candidad correcta   de kilómetros iniciales y finales";
    public static string datesError = "*Fecha inicial es mayor que la fecha final";
    public static string dateError = "*Fecha y hora introducidas son incorrectas";

    public enum TipoLector { NFC, CAM, EXT }

    public enum Domains { aizpurua, avsa, buñol, denibus, jonander, lamarina, laserranica, roymar, therpasa, transvia, turiabus, demo }

    public static Dictionary<string, string> domainCompany = new Dictionary<string, string> {
        { Domains.aizpurua.ToString (), "aizpurua" },
        { Domains.avsa.ToString (), "avsa" },
        { Domains.buñol.ToString (), "bunyol" },
        { Domains.denibus.ToString (), "denibus" },
        { Domains.jonander.ToString (), "jonander" },
        { Domains.lamarina.ToString (), "lamarina" },
        { Domains.laserranica.ToString (), "laserranica" },
        { Domains.roymar.ToString (), "roymar" },
        { Domains.therpasa.ToString (), "therpasa" },
        { Domains.transvia.ToString (), "transvia" },
        { Domains.turiabus.ToString (), "turiabus" },
        { Domains.demo.ToString (), "demotad" }
    };

    public static Dictionary<string, string> nameCompany = new Dictionary<string, string> {
        { Domains.aizpurua.ToString (), "Aizpurua" },
        { Domains.avsa.ToString (), "Avsa" },
        { Domains.buñol.ToString (), "Buñol" },
        { Domains.denibus.ToString (), "Denibus" },
        { Domains.jonander.ToString (), "Jon Ander" },
        { Domains.lamarina.ToString (), "La Marina" },
        { Domains.laserranica.ToString (), "La Serranica" },
        { Domains.roymar.ToString (), "Roymar" },
        { Domains.therpasa.ToString (), "Therpasa" },
        { Domains.transvia.ToString (), "Transvia" },
        { Domains.turiabus.ToString (), "Turiabus" },
        { Domains.demo.ToString (), "Demo" }
    };

    public static Dictionary<string, string> errorCodes = new Dictionary<string, string> {
        { "00", "Ok" },
        { "51", "El NIF comunicado no puede gestionar servicios." },
        { "52", "El NIF del comunicado no puede gestionar servicios para esa matrícula." },
        { "53", "EL NIF del arrendador no es correcto." },
        { "54", "El Nif del arrendatario no es correcto." },
        { "55", "El formato de la matrícula no es correcto." },
        { "56", "El arrendador no es titular de la autorización de esa matrícula." },
        { "57", "La fecha del contrato debe ser anterior a la fecha prevista de inicio." },
        { "58", "La provincia del contrato no es conrrecta." },
        { "59", "La provincia inicio no es correcta." },
        { "60", "La provincia fin no es correcta." },
        { "61", "La provincia del lugar más lejano no es correcta." },
        { "62", "El municipio del contrato no es correcto." },
        { "63", "El municipio inicio no es correcto." },
        { "64", "El municipio fin no es correcto." },
        { "65", "El municipio del lugar más lejano no es correcto." },
        { "66", "La CCAA de origen y destino son iguales. Debe comunicar provincia y municipio más lejano." },
        { "67", "El identificador del servicio es erróneo o el servicio ya está anulado." },
        { "68", "El identificador del servicio es erróneo o el servicio ya está iniciado." },
        { "69", "Error al insertar servicio." },
        { "70", "Error en el SW al confrimar el servicio." },
        { "79", "La fecha prevista de inicio debe ser anterior o igual a la fecha fin." },
        { "82", "Existe otro servicio posterior y no se puede confirmar el servicio indicado" },
        { "84", "El NIF comunicado no puede gestionar servicios" },
        { "86", "Error al confirmar el servicio" },
        { "96", "No se ha podido realizar la comunicación." },
        { "97", "Error no especificado." },
        { "98", "error de comunicación intentos > 3." },
        { "99", "error al crear el montaje." },
        { "100", "error loginusu no pertenece a un empleado" }
    };

    public static string Base64Encode(string plainText) {
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        plainText = Convert.ToBase64String(plainTextBytes);
        plainText = plainText.Insert(2, "5");
        plainText = plainText.Insert(plainText.Length - 3, "7");
        return plainText;
    }

    public static string Base64Decode(string base64EncodedData) {
        byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}