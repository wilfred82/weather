using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Xml.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;
using Sumit.Webpart.Weather.Common;
using Sumit.Webpart.Weather.Entity;

namespace Sumit.Webpart.Weather.Weather
{
    [ToolboxItemAttribute(false)]
    public class Weather : Microsoft.SharePoint.WebPartPages.WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/Sumit.Webpart.Weather/Weather/WeatherUserControl.ascx";

        #region WebPart Properties

        public bool _autoLoc = true;
        [WebBrowsable(true),
        WebDisplayName("Automatically Detect Location"),
        Personalizable(PersonalizationScope.Shared),
        WebPartStorage(Storage.Shared),
        WebDescription("Check if want to detect the location automatically"),
        Category("Weather WebPart Settings")]
        public bool AutoLoc
        {
            get
            {
                return _autoLoc;
            }
            set
            {
                _autoLoc = value;
            }
        }

        public string _cityName = "Agra, India";
        [WebBrowsable(true),
        WebDisplayName("City , State , Country"),
        Personalizable(PersonalizationScope.Shared),
        WebPartStorage(Storage.Shared),
        WebDescription("Enter the City name"),
        Category("Weather WebPart Settings")]
        public string CityName
        {
            get
            {
                return _cityName;
            }
            set
            {
                _cityName = value;
            }
        }


        public bool _condition = true;
        [WebBrowsable(true),
        WebDisplayName("Display Condition"),
        Personalizable(PersonalizationScope.Shared),
        WebPartStorage(Storage.Shared),
        WebDescription("Check if want to display Condition"),
        Category("Weather WebPart Settings")]
        public bool Condition
        {
            get
            {
                return _condition;
            }
            set
            {
                _condition = value;
            }
        }


        public bool _conditionImage = true;
        [WebBrowsable(true),
        WebDisplayName("Display Condition Image"),
        Personalizable(PersonalizationScope.Shared),
        WebPartStorage(Storage.Shared),
        WebDescription("Check if want to display Condition Image"),
        Category("Weather WebPart Settings")]
        public bool ConditionImage
        {
            get
            {
                return _conditionImage;
            }
            set
            {
                _conditionImage = value;
            }
        }

        public bool _high = true;
        [WebBrowsable(true),
        WebDisplayName("Display High Temperature"),
        Personalizable(PersonalizationScope.Shared),
        WebPartStorage(Storage.Shared),
        WebDescription("Check if want to display High Temperature"),
        Category("Weather WebPart Settings")]
        public bool High
        {
            get
            {
                return _high;
            }
            set
            {
                _high = value;
            }
        }

        public bool _low = true;
        [WebBrowsable(true),
        WebDisplayName("Display Low Temperature"),
        Personalizable(PersonalizationScope.Shared),
        WebPartStorage(Storage.Shared),
        WebDescription("Check if want to display Low Temperature"),
        Category("Weather WebPart Settings")]
        public bool Low
        {
            get
            {
                return _low;
            }
            set
            {
                _low = value;
            }
        }

        public Enums.TempUnitType _unitTemperature = Enums.TempUnitType.Celsius;
        [WebBrowsable(true),
        WebDisplayName("Unit For Temperature"),
        Personalizable(PersonalizationScope.Shared),
        WebPartStorage(Storage.Shared),
        WebDescription("Select the temperature unit"),
        Category("Weather WebPart Settings")]
        public Enums.TempUnitType UnitTemperature
        {
            get
            {
                return _unitTemperature;
            }
            set
            {
                _unitTemperature = value;
            }
        }

        public bool _humidity = false;
        [WebBrowsable(true),
        WebDisplayName("Display Humidity"),
        Personalizable(PersonalizationScope.Shared),
        WebPartStorage(Storage.Shared),
        WebDescription("Check if want to display Humidity"),
        Category("Weather WebPart Settings")]
        public bool Humidity
        {
            get
            {
                return _humidity;
            }
            set
            {
                _humidity = value;
            }
        }

        public bool _wind = false;
        [WebBrowsable(true),
        WebDisplayName("Display Wind"),
        Personalizable(PersonalizationScope.Shared),
        WebPartStorage(Storage.Shared),
        WebDescription("Check if want to display Wind"),
        Category("Weather WebPart Settings")]
        public bool Wind
        {
            get
            {
                return _wind;
            }
            set
            {
                _wind = value;
            }
        }

        public bool _updateInfo = true;
        [WebBrowsable(true),
        WebDisplayName("Show last update info"),
        Personalizable(PersonalizationScope.Shared),
        WebPartStorage(Storage.Shared),
        WebDescription("Check if want to show last update info"),
        Category("Weather WebPart Settings")]
        public bool UpdateInfo
        {
            get
            {
                return _updateInfo;
            }
            set
            {
                _updateInfo = value;
            }
        }

        public DisplayError LogError { get; set; }

        #endregion


        protected override void CreateChildControls()
        {
            try
            {
                LogError = new DisplayError();
                LogError.isError = false;

                if (this.AutoLoc)
                {
                    //Save the City Value to SP DB
                    SaveAutoLoc();
                }

                //Get the weather profile from the properties entered
                WeatherProfile _profile = GetWeatherProfile();

                Control control = Page.LoadControl(_ascxPath);
                ((WeatherUserControl)control)._weatherProfile = _profile;
                ((WeatherUserControl)control)._displayError = LogError;
                Controls.Add(control);

                //If any error occurs
                if (LogError.isError && !string.IsNullOrEmpty(LogError.ErrorMessage))
                {
                    Label Errorlbl = new Label();
                    Errorlbl.Text = LogError.ErrorMessage;
                    Errorlbl.CssClass = "colr";
                    this.Controls.Add(Errorlbl);
                    LogError.isError = false;
                    LogError.ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw (new SPException(ex.Message));
            }
        }

        /// <summary>
        /// Gets the weather profile 
        /// </summary>
        /// <returns></returns>
        private WeatherProfile GetWeatherProfile()
        {
            WeatherProfile _profile = new WeatherProfile();
            _profile.CityName = this.CityName;
            _profile.isCondition = this.Condition;
            _profile.isConditionImage = this.ConditionImage;
            _profile.isHighTemperature = this.High;
            _profile.isHumidity = this.Humidity;
            _profile.isLowTemprature = this.Low;
            _profile.UnitTemperature = this.UnitTemperature;
            _profile.isUpdateInfo = this.UpdateInfo;
            _profile.isWind = this.Wind;
            return _profile;
        }

        /// <summary>
        /// Save the auto located location in the webpart settings
        /// </summary>
        private void SaveAutoLoc()
        {
            String[] Location = new String[4];

            Location = GetLocation();

            using (SPSite objSite = new SPSite(SPContext.Current.Site.Url))
            {
                using (SPWeb objWeb = objSite.OpenWeb())
                {
                    SPFile objPage = objWeb.GetFile(HttpContext.Current.Request.Url.ToString());
                    SPLimitedWebPartManager mgr = objPage.GetLimitedWebPartManager(PersonalizationScope.Shared);
                    System.Web.UI.WebControls.WebParts.WebPart objWebPart = mgr.WebParts[this.ID];

                    if (objWebPart != null)
                    {
                        if (Location[1] == null && Location[2] == null)
                            ((Sumit.Webpart.Weather.Weather.Weather)(objWebPart.WebBrowsableObject)).CityName = null;
                        else
                            ((Sumit.Webpart.Weather.Weather.Weather)(objWebPart.WebBrowsableObject)).CityName = Location[1] + " , " + Location[2];

                        mgr.SaveChanges(objWebPart);
                    }
                }
            }
        }


        /// <summary>
        /// Gets the location of the client, retrieved from the URL http://api.hostip.info/
        /// </summary>
        /// <returns></returns>
        private string[] GetLocation()
        {
            string[] Location = new String[4];
            string url = "http://api.hostip.info/";
            XDocument xDoc = null;

            try
            {
                //get the XML from the URL
                xDoc = XDocument.Load(url);
            }
            catch (Exception ex)
            {
                LogError.ErrorMessage = "Could not retrieve location from http://api.hostip.info/ ";
                LogError.isError = true;
            }

            if (xDoc != null || xDoc.Root != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xDoc.ToString());

                foreach (XmlNode objNode in xmlDoc.ChildNodes[0].ChildNodes)
                {
                    if (objNode.Name.ToLower().Equals("gml:featuremember"))
                    {
                        foreach (XmlNode NodeHostIP in objNode.ChildNodes)
                        {
                            if (NodeHostIP.Name.ToLower().Equals("hostip"))
                            {
                                foreach (XmlNode NodeInfo in NodeHostIP.ChildNodes)
                                {
                                    if (NodeInfo.Name.ToLower().Equals("ip")) { Location[0] = NodeInfo.InnerText.ToString(); }
                                    else if (NodeInfo.Name.ToLower().Equals("gml:name"))
                                    {
                                        if (NodeInfo.InnerText.ToString().ToLower().Contains("unknown"))
                                            Location[1] = null;
                                        else
                                            Location[1] = NodeInfo.InnerText.ToString();
                                    }
                                    else if (NodeInfo.Name.ToLower().Equals("countryname"))
                                    {
                                        if (NodeInfo.InnerText.ToString().ToLower().Contains("unknown"))
                                            Location[2] = null;
                                        else
                                            Location[2] = NodeInfo.InnerText.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Location;
        }
    }
}
