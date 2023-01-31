using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminApp;

public class AppInfo
{
    private static readonly AppInfo instance = new AppInfo();
    public int userId;
    public string baseAdress = "http://192.168.0.103:58799/api/";
    public AppInfo()
    {
    }
    public static AppInfo GetInstance()
    {
        return instance;
    }
}
