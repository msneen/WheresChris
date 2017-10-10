using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace WheresChris.UITest
{
    [TestFixture(Platform.Android)]
    [TestFixture(Platform.iOS)]
    public class Tests
    {
        IApp app;
        Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
            //app = ConfigureApp.Android.StartApp();
        }

        [Test]
        public void AppLaunches()
        {
            app.Screenshot("First screen.");
            //app.Repl();
        }

        [Test]
        public void AcceptsName()
        {
            app.EnterText(x=>x.Marked("Nickname"),"Mike");
            app.Tap(x=>x.Marked("SaveButton"));
        }
    }
}

