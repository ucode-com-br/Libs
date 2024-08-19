using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Browser.Dom;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using UCode.Extensions;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using static UCode.Scrapper.Browser.Startup;

namespace UCode.Scrapper.Browser
{
    public struct Window
    {
        public int Width { get; set; }
        public int Height { get; set; }


        public bool IsMaximized { get; set; }

        public bool IsMinimized
        {
            get; set;
        }
    }

    public class Startup: IDisposable
    {
        public delegate void ReconfigureChromeOptions(ref ChromeOptions chromeOptions);
        public delegate void ReconfigureProxy(ref Proxy proxy);

        private Proxy? __proxy;
        public Proxy? Proxy
        {
            get => __proxy; set => __proxy = value;
        }

        private ChromeOptions __chromeOptions;
        public ChromeOptions ChromeOptions
        {
            get => __chromeOptions;
            set => __chromeOptions = value;
        }

        #region Arguments
        private Window _window = new () { Width = 1920, Height = 1080, IsMaximized = true, IsMinimized = false };
        public Window Window
        {
            get
            {
                return this._window;
            }
            private set
            {
                this._window = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private UnhandledPromptBehavior _unhandledPromptBehavior = UnhandledPromptBehavior.Accept;
        public UnhandledPromptBehavior UnhandledPromptBehavior
        {
            get
            {
                return this._unhandledPromptBehavior;
            }
            set
            {
                this._unhandledPromptBehavior = value;
                this.ChromeOptionsChanged = true;
            }
        }

        private bool _headless = true;
        public bool Headless
        {
            get
            {
                return this._headless;
            }
            private set
            {
                this._headless = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private bool _disableWebSecurity = true;
        public bool DisableWebSecurity
        {
            get
            {
                return this._disableWebSecurity;
            }
            private set
            {
                this._disableWebSecurity = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private bool _disableIsolateOrigins = true;
        public bool DisableIsolateOrigins
        {
            get
            {
                return this._disableIsolateOrigins;
            }
            private set
            {
                this._disableIsolateOrigins = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private bool _disableSitePerProcess = true;
        public bool DisableSitePerProcess
        {
            get
            {
                return this._disableSitePerProcess;
            }
            private set
            {
                this._disableSitePerProcess = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private bool _allowRunningInsecureContent = true;
        public bool AllowRunningInsecureContent
        {
            get
            {
                return this._allowRunningInsecureContent;
            }
            private set
            {
                this._allowRunningInsecureContent = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private bool _disableSandbox = true;
        public bool DisableSandbox
        {
            get
            {
                return this._disableSandbox;
            }
            private set
            {
                this._disableSandbox = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private bool _disableGPU = true;
        public bool DisableGPU
        {
            get
            {
                return this._disableGPU;
            }
            private set
            {
                this._disableGPU = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private string _lang = "en-US,en;q=0.9";
        public string Lang
        {
            get
            {
                return this._lang;
            }
            private set
            {
                this._lang = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private bool _ignoreCertificateErrors = true;
        public bool IgnoreCertificateErrors
        {
            get
            {
                return this._ignoreCertificateErrors;
            }
            private set
            {
                this._ignoreCertificateErrors = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private bool _audioMute = true;
        public bool AudioMute
        {
            get
            {
                return this._audioMute;
            }
            private set
            {
                this._audioMute = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private List<string> _arguments = new List<string>() {
            //"--headless=new",
            /////"--proxy-server=${proxyAddress}:${proxyPort}",
            //"--disable-web-security",
            //"--disable-features=IsolateOrigins,site-per-process",
            //"--allow-running-insecure-content",
            "--disable-blink-features=AutomationControlled",
            //"--no-sandbox",
            //"--mute-audio",
            "--no-zygote",
            "--no-xshm",
            //"--window-size=1920,1080",
            "--no-first-run",
            "--no-default-browser-check",
            "--disable-dev-shm-usage",
            ///"--disable-gpu",
            "--enable-webgl",
            //"--ignore-certificate-errors",
            //"--lang=en-US,en;q=0.9",
            "--password-store=basic",
            //"--disable-gpu-sandbox",
            "--disable-software-rasterizer",
            "--disable-background-timer-throttling",
            "--disable-backgrounding-occluded-windows",
            "--disable-renderer-backgrounding",
            "--disable-infobars",
            "--disable-breakpad",
            "--disable-canvas-aa",
            "--disable-2d-canvas-clip-aa",
            "--disable-gl-drawing-for-tests",
            "--enable-low-end-device-mode",
            ///"--no-sandbox"

        };
        public List<string> OtherArguments
        {
            get
            {
                return this._arguments;
            }
            private set
            {
                this._arguments = value;

                this.ChromeOptionsChanged = true;
            }
        }

        private bool _enableDownloads = true;
        public bool EnableDownloads
        {
            get => this._enableDownloads;
            private set
            {
                this._enableDownloads = value;
                this.ChromeOptionsChanged = true;
            }
        }

        private PageLoadStrategy _pageLoadStrategy = PageLoadStrategy.Normal;
        public PageLoadStrategy PageLoadStrategy
        {
            get => _pageLoadStrategy;
            private set
            {
                this._pageLoadStrategy = value;
                this.ChromeOptionsChanged = true;
            }
        }
        #endregion Arguments

        private ChromeDriver? _webDriver;
        private readonly DriverManager _driverManager;
        private bool _DisposedValue;

        private void BuildArgumentsChromeOptions()
        {
            if (this.ChromeOptions == null)
            {
                this.ChromeOptions = new ChromeOptions();

                this.Proxy = this.ChromeOptions.Proxy;
            }


            var result = new List<string>();

            result.Add($"--headless={(this.Headless ? "new" : "false")}");

            if (this._disableWebSecurity)
            {
                result.Add("--disable-web-security");
            }

            if (this.DisableIsolateOrigins || this.DisableSitePerProcess)
            {
                if (this.DisableIsolateOrigins == this.DisableSitePerProcess)
                {
                    result.Add("--disable-features=IsolateOrigins,site-per-process");
                }
                else
                {
                    if (this.DisableIsolateOrigins)
                    {
                        result.Add("--disable-features=IsolateOrigins");
                    }
                    else if (this.DisableSitePerProcess)
                    {
                        result.Add("--disable-features=site-per-process");
                    }
                }
            }

            if (this.AllowRunningInsecureContent)
            {
                result.Add("--allow-running-insecure-content");
            }

            if (this.AllowRunningInsecureContent)
            {
                result.Add($"--window-size={Window.Width},{Window.Height}");
            }

            if (this.DisableGPU)
            {
                result.Add("--disable-gpu");
            }

            if (this.DisableSandbox)
            {
                result.Add("--no-sandbox");

                if (this.DisableGPU)
                {
                    result.Add("--disable-gpu-sandbox");
                }
            }

            if (this.IgnoreCertificateErrors)
            {
                result.Add("--ignore-certificate-errors");
            }

            if (!string.IsNullOrWhiteSpace(this.Lang))
            {
                result.Add($"--lang={this.Lang}");
            }


            if (this.AudioMute)
            {
                result.Add("--mute-audio");
            }

            result.AddRange(this.OtherArguments);



            foreach (var argString in this.ChromeOptions.Arguments.ToArray())
            {
                this.ChromeOptions.AddExcludedArgument(argString);
            }


            this.ChromeOptions.AddArguments(result);

            this.ChromeOptions.UnhandledPromptBehavior = this.UnhandledPromptBehavior;
            this.ChromeOptions.AcceptInsecureCertificates = this.IgnoreCertificateErrors;
            this.ChromeOptions.EnableDownloads = this.EnableDownloads;
            this.ChromeOptions.PageLoadStrategy = this.PageLoadStrategy;
            this.ChromeOptions.Proxy = this.Proxy;
        }


        

        private bool _chromeOptionsChanged;
        private readonly SemaphoreSlim _chromeOptionsChangedSemaphore = new(0, 1);
        /// <summary>
        /// If chrome config is changed, rebuild driver
        /// </summary>
        public bool ChromeOptionsChanged
        {
            get
            {
                if (this._webDriver == null)
                {
                    return true;
                }

                return this._chromeOptionsChanged;
            }
            private set
            {
                this.ChromeOptionsChangingBegin();

                if (this._webDriver == null)
                {
                    this._chromeOptionsChanged = true;
                }
                else
                {
                    this._chromeOptionsChanged = value;
                }

                this.ChromeOptionsChangingEnd();
            }
        }
        public void ChromeOptionsChangingBegin()
        {
            this._chromeOptionsChangedSemaphore.Wait();
        }

        public void ChromeOptionsChangingEnd()
        {
            if(this._chromeOptionsChangedSemaphore.CurrentCount > 0)
                this._chromeOptionsChangedSemaphore.Release();
        }



        public Startup(string baseDirectory = null, ReconfigureChromeOptions? reconfigureChromeOptions = null, ReconfigureProxy? reconfigureProxy = null)
        {
            var baseDir = System.IO.Path.Combine(baseDirectory ?? System.IO.Path.GetTempPath(), "UCode-Scrapper-Browser");

            if (!System.IO.Directory.Exists(baseDir))
            {
                System.IO.Directory.CreateDirectory(baseDir);
            }

            this._driverManager = new DriverManager(baseDir);

            //var chromeDriverService = ChromeDriverService.CreateDefaultService(baseDir);


            this._driverManager.SetUpDriver(new ChromeConfig() { });


            this.BuildArgumentsChromeOptions();

            if (reconfigureChromeOptions != null)
                this.Reconfigure(reconfigureChromeOptions);

            if (reconfigureProxy != null)
                this.Reconfigure(reconfigureProxy);


            /*IDevTools devTools = _webDriver as IDevTools;

            var session = devTools.GetDevToolsSession();

            session.Domains.Network.EnableFetchForAllPatterns().Wait();

            session.DevToolsEventReceived += this.Session_DevToolsEventReceived;*/


            /*var nav = _webDriver.Navigate();

            nav.GoToUrl("");

            //contains(@value,'Generate')
            var element = _webDriver.FindElement(By.XPath("//input[@type='submit' and contains(@id,'btnConsultarHCaptcha')]"));

            element.Click();*/

        }


        /// <summary>
        /// Try refrresh chrome driver
        /// </summary>
        /// <returns></returns>
        public bool TryRefreshDriver()
        {
            try
            {
                this.ChromeOptionsChangingBegin();

                if (this.ChromeOptionsChanged)
                {
                    this.BuildArgumentsChromeOptions();

                    this._webDriver = new ChromeDriver(this.ChromeOptions);

                    this._chromeOptionsChanged = false;
                    this.ChromeOptionsChangingEnd();
                    return true;
                }
            }
            finally
            {
                this.ChromeOptionsChangingEnd();
            }

            return false;
        }

        

        /// <summary>
        /// Reconfigure chrome options
        /// </summary>
        /// <param name="configureOptions"></param>
        public void Reconfigure(ReconfigureChromeOptions configureOptions)
        {
            var chromeOptions = this.ChromeOptions;

            configureOptions.Invoke(ref chromeOptions);

            this.ChromeOptions = chromeOptions;

            this.ChromeOptionsChanged = true;
        }

        /// <summary>
        /// Reconfigure proxy
        /// </summary>
        /// <param name="configureOptions"></param>
        public void Reconfigure(ReconfigureProxy configureOptions)
        {
            var proxy = this.Proxy ?? new Proxy();

            configureOptions.Invoke(ref proxy);

            this.Proxy = proxy;

            this.ChromeOptionsChanged = true;
        }


        public (IWebDriver Driver, INavigation Navigation) CreateDriverNavigator()
        {
            //var driver = new ChromeDriver();
            _ = this.TryRefreshDriver();

            var navigate = this._webDriver!.Navigate();


            (IWebDriver Driver, INavigation Navigation) result = (_webDriver, navigate);

            return result;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!this._DisposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    this._chromeOptionsChangedSemaphore.Dispose();
                    this._webDriver?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this._DisposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
